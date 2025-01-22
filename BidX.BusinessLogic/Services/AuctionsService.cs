using AutoMapper;
using AutoMapper.QueryableExtensions;
using BidX.BusinessLogic.DTOs.AuctionDTOs;
using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.DTOs.QueryParamsDTOs;
using BidX.BusinessLogic.Interfaces;
using BidX.DataAccess;
using BidX.DataAccess.Entites;
using Microsoft.EntityFrameworkCore;

namespace BidX.BusinessLogic.Services;

public class AuctionsService : IAuctionsService
{
    private readonly AppDbContext appDbContext;
    private readonly ICloudService cloudService;
    private readonly IMapper mapper;

    public AuctionsService(AppDbContext appDbContext, ICloudService cloudService, IMapper mapper)
    {
        this.appDbContext = appDbContext;
        this.cloudService = cloudService;
        this.mapper = mapper;
    }

    public async Task<Page<AuctionResponse>> GetAuctions(AuctionsQueryParams queryParams)
    {
        // Build the query based on the parameters (Short circuit if a query param has no value)
        var auctionsQuery = appDbContext.Auctions
            .Where(a => (string.IsNullOrEmpty(queryParams.Search) || a.ProductName.Contains(queryParams.Search!)) && // I didn't add and index for ProductName because this query is non-sargable so it cannot efficiently use indexes (https://stackoverflow.com/a/4268107, https://stackoverflow.com/a/799616) consider creating Full-Text index later (https://shorturl.at/COl2f)
                        (queryParams.ProductCondition == null || a.ProductCondition == queryParams.ProductCondition) && // I didn't add an index for ProductCondition because it is a low selectivity column that has only 2 values (Used, New)
                        (queryParams.CategoryId == null || a.CategoryId == queryParams.CategoryId) &&
                        (queryParams.CityId == null || a.CityId == queryParams.CityId) &&
                        (queryParams.ActiveOnly == false || a.EndTime > DateTimeOffset.UtcNow));

        // Get the total count before pagination
        var totalCount = await auctionsQuery.CountAsync();
        if (totalCount == 0)
            return new Page<AuctionResponse>([], queryParams.Page, queryParams.PageSize, totalCount);

        // Get the list of auctions with pagination and mapping
        var auctions = await auctionsQuery
            // Get the last auctions first
            .OrderByDescending(a => a.Id)
            // Paginate
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ProjectTo<AuctionResponse>(mapper.ConfigurationProvider)
            .AsNoTracking()
            .ToListAsync();

        return new Page<AuctionResponse>(auctions, queryParams.Page, queryParams.PageSize, totalCount);
    }

    public async Task<Result<Page<AuctionResponse>>> GetUserAuctions(int userId, UserAuctionsQueryParams queryParams)
    {
        // Build the query based on the parameters
        var userAuctionsQuery = appDbContext.Auctions
            .Where(a => (a.AuctioneerId == userId) &&
                        (queryParams.ActiveOnly == false || a.EndTime > DateTimeOffset.UtcNow));

        // Get the total count before pagination
        var totalCount = await userAuctionsQuery.CountAsync();

        if (totalCount == 0) // This ensures that the method will execute only 2 queries at most.
        {
            var userExists = await appDbContext.Users.AnyAsync(a => a.Id == userId);
            if (!userExists)
                return Result<Page<AuctionResponse>>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["User not found."]);

            return Result<Page<AuctionResponse>>.Success(new([], queryParams.Page, queryParams.PageSize, totalCount));
        }

        // Get the list of auctions with pagination and mapping
        var userAuctions = await userAuctionsQuery
            // Get the newly added auctions first
            .OrderByDescending(a => a.Id)
            // Paginate
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ProjectTo<AuctionResponse>(mapper.ConfigurationProvider)
            .AsNoTracking()
            .ToListAsync();

        var response = new Page<AuctionResponse>(userAuctions, queryParams.Page, queryParams.PageSize, totalCount);
        return Result<Page<AuctionResponse>>.Success(response);
    }

    public async Task<Result<Page<AuctionUserHasBidOnResponse>>> GetAuctionsUserHasBidOn(int userId, AuctionsUserHasBidOnQueryParams queryParams)
    {
        // Build the query based on the parameters
        var auctionsUserHasBidOnQuery = appDbContext.Auctions
            .Where(a =>
                queryParams.WonOnly
                    ? a.WinnerId == userId
                    : a.Bids!.Any(b => b.BidderId == userId) &&
                      (!queryParams.ActiveOnly || a.EndTime > DateTimeOffset.UtcNow)
            );

        // Get the total count before pagination
        var totalCount = await auctionsUserHasBidOnQuery.CountAsync();

        if (totalCount == 0)
        {
            var userExists = await appDbContext.Users.AnyAsync(a => a.Id == userId);
            if (!userExists)
                return Result<Page<AuctionUserHasBidOnResponse>>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["User not found."]);

            return Result<Page<AuctionUserHasBidOnResponse>>.Success(new([], queryParams.Page, queryParams.PageSize, totalCount));
        }

        // Get the list of auctions with pagination and mapping
        var auctionsUserHasBidOn = await auctionsUserHasBidOnQuery
            // Get the newly added auctions first
            .OrderByDescending(a => a.Id)
            // Paginate
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ProjectTo<AuctionUserHasBidOnResponse>(mapper.ConfigurationProvider)
            .AsNoTracking()
            .ToListAsync();

        auctionsUserHasBidOn.ForEach(a => a.IsUserWon = a.IsActive ? null : a.WInnerId == userId);

        var response = new Page<AuctionUserHasBidOnResponse>(auctionsUserHasBidOn, queryParams.Page, queryParams.PageSize, totalCount);
        return Result<Page<AuctionUserHasBidOnResponse>>.Success(response);
    }

    public async Task<Result<AuctionDetailsResponse>> GetAuction(int auctionId)
    {
        var auctionResponse = await appDbContext.Auctions
            .Include(a => a.ProductImages)
            .Include(a => a.Category)
            .Include(a => a.City)
            .Include(a => a.Auctioneer)
            .ProjectTo<AuctionDetailsResponse>(mapper.ConfigurationProvider)
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == auctionId);

        if (auctionResponse is null)
            return Result<AuctionDetailsResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Auction not found."]);

        return Result<AuctionDetailsResponse>.Success(auctionResponse);
    }

    public async Task<Result<AuctionResponse>> CreateAuction(int callerId, CreateAuctionRequest request, IEnumerable<Stream> productImages)
    {
        var auction = mapper.Map<CreateAuctionRequest, Auction>(request, o => o.Items["AuctioneerId"] = callerId);

        var validationResult = await ValidateCategoryAndCity(request.CategoryId, request.CityId);
        if (!validationResult.Succeeded)
            return Result<AuctionResponse>.Failure(validationResult.Error!);

        var assigningResult = await AssignImagesToAuction(auction, productImages);
        if (!assigningResult.Succeeded)
            return Result<AuctionResponse>.Failure(assigningResult.Error!);

        appDbContext.Add(auction);
        await appDbContext.SaveChangesAsync();

        var response = mapper.Map<Auction, AuctionResponse>(auction);
        return Result<AuctionResponse>.Success(response);
    }

    public async Task<Result> DeleteAuction(int callerId, int auctionId)
    {
        var noOfRowsAffected = await appDbContext.Auctions
            .Where(a => a.Id == auctionId && a.AuctioneerId == callerId)
            .ExecuteDeleteAsync();

        if (noOfRowsAffected <= 0)
            return Result.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Auction not found."]);

        return Result.Success();
    }


    private async Task<Result> ValidateCategoryAndCity(int categoryId, int cityId)
    {
        // Multiple active operations on the same context instance are not supportet
        // so i cant do these 2 calls concurently using Task.WhenAll but I can combine them into a single query like this
        var result = await appDbContext.Categories
            .Where(c => c.Id == categoryId && !c.IsDeleted)
            .Select(c => new
            {
                CategoryExists = true,
                CityExists = appDbContext.Cities.Any(ci => ci.Id == cityId)
            })
            .AsNoTracking()
            .FirstOrDefaultAsync();

        var errors = new List<string>();

        if (result == null || !result.CategoryExists)
            errors.Add("Invalid category id.");

        if (result == null || !result.CityExists)
            errors.Add("Invalid city id.");

        if (errors.Count > 0)
            return Result.Failure(ErrorCode.USER_INPUT_INVALID, errors);

        return Result.Success();
    }

    private async Task<Result> AssignImagesToAuction(Auction auction, IEnumerable<Stream> productImages)
    {
        // Upload and assign product thumbnail  
        var thumbnailUploadResult = await cloudService.UploadThumbnail(productImages.First());
        if (!thumbnailUploadResult.Succeeded)
            return Result.Failure(thumbnailUploadResult.Error!);

        auction.ThumbnailUrl = thumbnailUploadResult.Response!.FileUrl;

        // Upload and assign product images
        var imagesUploadResult = await cloudService.UploadImages(productImages);
        if (!imagesUploadResult.Succeeded)
            return Result.Failure(imagesUploadResult.Error!);

        auction.ProductImages = imagesUploadResult.Response!
                                .Select(response => new ProductImage { Id = response.FileId, Url = response.FileUrl })
                                .ToList();

        return Result.Success();
    }
}
