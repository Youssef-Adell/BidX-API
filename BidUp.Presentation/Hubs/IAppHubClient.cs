using BidUp.BusinessLogic.DTOs.AuctionDTOs;
using BidUp.BusinessLogic.DTOs.BidDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;

namespace BidUp.Presentation.Hubs;

public interface IAppHubClient
{
    Task AuctionCreated(AuctionResponse createdAuction);
    Task AuctionDeletedOrEnded(AuctionDeletedOrEndedResponse auctionDeletedOrEndedResponse);
    Task AuctionPriceUpdated(AuctionPriceUpdatedResponse auctionPriceUpdatedResponse);
    Task BidCreated(BidResponse createdBid);
    Task BidAccepted(BidResponse acceptedBid);
    Task ErrorOccurred(ErrorResponse error);
}
