using BidX.BusinessLogic.DTOs.BidDTOs;
using BidX.DataAccess.Entites;

namespace BidX.BusinessLogic.Mappings;

public static class BidMappingExtensions
{
    public static Bid ToBidEntity(this BidRequest request, int bidderId)
    {
        return new Bid
        {
            Amount = request.Amount,
            AuctionId = request.AuctionId,
            PlacedAt = DateTimeOffset.UtcNow,
            BidderId = bidderId,
            IsAccepted = false,
        };
    }

    public static BidResponse ToBidResponse(this Bid bid)
    {
        return new BidResponse
        {
            Id = bid.Id,
            Amount = bid.Amount,
            IsAccepted = bid.IsAccepted,
            PlacedAt = bid.PlacedAt,
            AuctionId = bid.AuctionId,
            Bidder = new Bidder
            {
                Id = bid.BidderId,
                FullName = bid.Bidder!.FullName,
                ProfilePictureUrl = bid.Bidder.ProfilePictureUrl,
                AverageRating = bid.Bidder.AverageRating
            }
        };
    }
    public static BidResponse ToBidResponse(this Bid bid, string bidderFullName, string? bidderProfilePictureUrl, decimal bidderAverageRating)
    {
        return new BidResponse
        {
            Id = bid.Id,
            Amount = bid.Amount,
            IsAccepted = bid.IsAccepted,
            PlacedAt = bid.PlacedAt,
            AuctionId = bid.AuctionId,
            Bidder = new Bidder
            {
                Id = bid.BidderId,
                FullName = bidderFullName,
                ProfilePictureUrl = bidderProfilePictureUrl,
                AverageRating = bidderAverageRating
            }
        };
    }

    public static IQueryable<BidResponse> ProjectToBidResponse(this IQueryable<Bid> query)
    {
        return query.Select(b => new BidResponse
        {
            Id = b.Id,
            Amount = b.Amount,
            IsAccepted = b.IsAccepted,
            PlacedAt = b.PlacedAt,
            AuctionId = b.AuctionId,
            Bidder = new Bidder
            {
                Id = b.Bidder!.Id,
                FullName = b.Bidder.FullName,
                ProfilePictureUrl = b.Bidder.ProfilePictureUrl,
                AverageRating = b.Bidder.AverageRating
            }
        });
    }
}
