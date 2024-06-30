using BidUp.BusinessLogic.DTOs.AuctionDTOs;
using BidUp.BusinessLogic.DTOs.BidDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;

namespace BidUp.Presentation.Hubs;

public interface IAppHubClient
{
    Task BidCreated(BidResponse createdBid); // Triggerd only for clients who currently in a specific auction room
    Task BidAccepted(BidResponse acceptedBid); // Triggerd only for clients who currently in a specific auction room
    Task AuctionCreated(AuctionResponse createdAuction); // Global event
    Task AuctionDeletedOrEnded(AuctionDeletedOrEndedResponse auctionDeletedOrEndedResponse);  // Global event
    Task AuctionPriceUpdated(AuctionPriceUpdatedResponse auctionPriceUpdatedResponse);  // Global event
    Task ErrorOccurred(ErrorResponse error); // Global event
}
