using AutoMapper;
using BidUp.BusinessLogic.DTOs.AuctionDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.Interfaces;
using BidUp.DataAccess;
using BidUp.DataAccess.Entites;
using Microsoft.EntityFrameworkCore;

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
