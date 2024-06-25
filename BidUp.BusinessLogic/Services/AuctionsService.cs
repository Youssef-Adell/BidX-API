using AutoMapper;
using BidUp.BusinessLogic.DTOs.AuctionDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.DTOs.QueryParamsDTOs;
using BidUp.BusinessLogic.Interfaces;
using BidUp.DataAccess;
using BidUp.DataAccess.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BidUp.BusinessLogic.Services;

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
        var filterdAuctions = appDbContext.Auctions
            .Include(a => a.Product)
            .Include(a => a.HighestBid)
            // Search & Filter (Short circuit if a query param has no value)
            .Where(a => (queryParams.Search.IsNullOrEmpty() || a.Product.Name.ToLower().Contains(queryParams.Search!)) && // I wont add and index for Product.Name because this query is non-sargable so it cannot efficiently use indexes (https://stackoverflow.com/a/4268107, https://stackoverflow.com/a/799616)
                        (queryParams.CategoryId == null || a.CategoryId == queryParams.CategoryId) &&
                        (queryParams.CityId == null || a.CityId == queryParams.CityId) &&
                        (queryParams.ProductCondition == null || a.Product.Condition == queryParams.ProductCondition) &&
                        (queryParams.ActiveOnly == false || a.EndTime > DateTime.UtcNow)); // I think adding an index for Auction.EndTime not worth because the small tables rarely benefit from indexs in addition to it slow the writting operations

        // Use the above query to get the total count of filterd auctions before applying the pagination
        var totalAuctionsCount = await filterdAuctions.CountAsync();

        var auctions = await filterdAuctions
            // Get the last auctions first
            .OrderByDescending(a => a.Id)
            // Paginate
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .AsNoTracking()
            .ToListAsync();

        var auctionsResponses = mapper.Map<IEnumerable<AuctionResponse>>(auctions);

        var response = new Page<AuctionResponse>(auctionsResponses, queryParams.Page, queryParams.PageSize, totalAuctionsCount);

        return response;
    }

    public async Task<AppResult<AuctionDetailsResponse>> GetAuction(int auctionId)
    {
        var auction = await appDbContext.Auctions
            .Include(a => a.Product)
            .ThenInclude(p => p.Images)
            .Include(a => a.Category)
            .Include(a => a.City)
            .Include(a => a.Auctioneer)
            .Include(a => a.HighestBid)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == auctionId);

        if (auction is null)
            return AppResult<AuctionDetailsResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Auction not found."]);

        var response = mapper.Map<AuctionDetailsResponse>(auction);

        return AppResult<AuctionDetailsResponse>.Success(response);
    }

    public async Task<AppResult<AuctionResponse>> CreateAuction(int currentUserId, CreateAuctionRequest createAuctionRequest, IEnumerable<Stream> productImages)
    {
        var validationResult = await ValidateCategoryAndCity(createAuctionRequest.CategoryId, createAuctionRequest.CityId);
        if (!validationResult.Succeeded)
            return AppResult<AuctionResponse>.Failure(validationResult.Error!.ErrorCode, validationResult.Error.ErrorMessages);

        var auction = mapper.Map<CreateAuctionRequest, Auction>(createAuctionRequest);
        auction.AuctioneerId = currentUserId;
        auction.SetTime(createAuctionRequest.DurationInSeconds);

        var assigningResult = await AssignImagesToAuction(auction, productImages);
        if (!assigningResult.Succeeded)
            return AppResult<AuctionResponse>.Failure(assigningResult.Error!.ErrorCode, assigningResult.Error.ErrorMessages);

        appDbContext.Add(auction);
        await appDbContext.SaveChangesAsync();

        var response = mapper.Map<Auction, AuctionResponse>(auction);
        return AppResult<AuctionResponse>.Success(response);
    }

    public async Task<AppResult> DeleteAuction(int currentUserId, int auctionId)
    {
        var noOfRowsAffected = await appDbContext.Auctions
            .Where(a => a.Id == auctionId && a.AuctioneerId == currentUserId)
            .ExecuteDeleteAsync();

        if (noOfRowsAffected <= 0)
            return AppResult.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Auction not found."]);

        return AppResult.Success();
    }


    private async Task<AppResult> ValidateCategoryAndCity(int categoryId, int cityId)
    {
        // Multiple active operations on the same context instance are not supported so i cant do these 2 calls concurently using Task.WhenAll 
        var isValidCategory = await appDbContext.Categories.AnyAsync(c => c.Id == categoryId && !c.IsDeleted);
        var isValidCity = await appDbContext.Cities.AnyAsync(c => c.Id == cityId);

        if (!isValidCategory)
            return AppResult.Failure(ErrorCode.USER_INPUT_INVALID, ["Invalid category id."]);

        if (!isValidCity)
            return AppResult.Failure(ErrorCode.USER_INPUT_INVALID, ["Invalid city id."]);

        return AppResult.Success();
    }

    private async Task<AppResult> AssignImagesToAuction(Auction auction, IEnumerable<Stream> productImages)
    {
        // Upload and assign product images
        var imagesUploadResult = await cloudService.UploadImages(productImages);

        if (!imagesUploadResult.Succeeded)
            return AppResult.Failure(imagesUploadResult.Error!.ErrorCode, imagesUploadResult.Error.ErrorMessages);

        foreach (var result in imagesUploadResult.Response!)
        {
            var image = new ProductImage { Id = result.FileId, Url = result.FileUrl };
            auction.Product.Images.Add(image);
        };

        // Upload and assign product thumbnail
        var thumbnailUploadResult = await cloudService.UploadThumbnail(productImages.First());

        if (!thumbnailUploadResult.Succeeded)
            return AppResult.Failure(thumbnailUploadResult.Error!.ErrorCode, thumbnailUploadResult.Error.ErrorMessages);

        auction.Product.ThumbnailUrl = thumbnailUploadResult.Response!.FileUrl;

        return AppResult.Success();
    }
}
