using BidX.BusinessLogic.DTOs.AuctionDTOs;
using BidX.DataAccess.Entites;

namespace BidX.BusinessLogic.Mappings;

public static class AuctionMappingExtensions
{
    public static Auction ToAuctionEntity(this CreateAuctionRequest request, int auctioneerId)
    {
        return new Auction
        {
            ProductName = request.ProductName,
            ProductDescription = request.ProductDescription,
            ProductCondition = request.ProductCondition,
            StartingPrice = request.StartingPrice,
            MinBidIncrement = request.MinBidIncrement,
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddSeconds(request.DurationInSeconds),
            CategoryId = request.CategoryId,
            CityId = request.CityId,
            AuctioneerId = auctioneerId,
            ThumbnailUrl = string.Empty // will be re setted by the AuctionService
        };
    }

    public static AuctionResponse ToAuctionResponse(this Auction auction)
    {
        return new AuctionResponse
        {
            Id = auction.Id,
            ProductName = auction.ProductName,
            ProductCondition = auction.ProductCondition,
            ThumbnailUrl = auction.ThumbnailUrl,
            CurrentPrice = auction.StartingPrice,
            EndTime = auction.EndTime,
            CategoryId = auction.CategoryId,
            CityId = auction.CityId
        };
    }

    public static IQueryable<AuctionResponse> ProjectToAuctionResponse(this IQueryable<Auction> query)
    {
        return query.Select(a => new AuctionResponse
        {
            Id = a.Id,
            ProductName = a.ProductName,
            ProductCondition = a.ProductCondition,
            ThumbnailUrl = a.ThumbnailUrl,
            CurrentPrice = a.WinnerId.HasValue ?
                a.Bids!.Where(b => b.IsAccepted).Select(b => b.Amount).Single()
                :
                a.Bids!.OrderByDescending(b => b.Amount)
                    .Select(b => (decimal?)b.Amount)
                    .FirstOrDefault() ?? a.StartingPrice,
            EndTime = a.EndTime,
            CategoryId = a.CategoryId,
            CityId = a.CityId
        });
    }

    public static IQueryable<AuctionDetailsResponse> ProjectToAuctionDetailsResponse(this IQueryable<Auction> query)
    {
        return query.Select(a => new AuctionDetailsResponse
        {
            Id = a.Id,
            ProductName = a.ProductName,
            ProductDescription = a.ProductDescription,
            ProductCondition = a.ProductCondition,
            MinBidIncrement = a.MinBidIncrement,
            CurrentPrice = a.WinnerId.HasValue ?
                a.Bids!.Where(b => b.IsAccepted).Select(b => b.Amount).Single()
                :
                a.Bids!.OrderByDescending(b => b.Amount)
                    .Select(b => (decimal?)b.Amount)
                    .FirstOrDefault() ?? a.StartingPrice,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Category = a.Category!.Name,
            City = a.City!.Name,
            ProductImages = a.ProductImages!.Select(i => i.Url),
            Auctioneer = new Auctioneer
            {
                Id = a.Auctioneer!.Id,
                FullName = a.Auctioneer.FullName,
                ProfilePictureUrl = a.Auctioneer.ProfilePictureUrl,
                AverageRating = a.Auctioneer.AverageRating
            },
            WinnerId = a.WinnerId
        });
    }

    public static IQueryable<AuctionUserHasBidOnResponse> ProjectToAuctionUserHasBidOnResponse(this IQueryable<Auction> query, int userId)
    {
        return query.Select(a => new AuctionUserHasBidOnResponse
        {
            Id = a.Id,
            ProductName = a.ProductName,
            ThumbnailUrl = a.ThumbnailUrl,
            CurrentPrice = a.WinnerId.HasValue ?
                a.Bids!.Where(b => b.IsAccepted).Select(b => b.Amount).Single()
                :
                a.Bids!.OrderByDescending(b => b.Amount)
                    .Select(b => (decimal?)b.Amount)
                    .FirstOrDefault() ?? a.StartingPrice,
            EndTime = a.EndTime,
            IsActive = a.EndTime > DateTimeOffset.UtcNow,
            IsUserWon = a.IsActive ? null : a.WinnerId == userId
        });
    }
}
