using BidUp.BusinessLogic.DTOs.AuctionDTOs;
using BidUp.BusinessLogic.DTOs.BidDTOs;
using BidUp.BusinessLogic.DTOs.ChatDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;

namespace BidUp.Presentation.Hubs;

public interface IAppHubClient
{
    Task BidCreated(BidResponse createdBid); // Triggerd only for clients who currently in a specific auction room
    Task BidAccepted(BidResponse acceptedBid); // Triggerd only for clients who currently in a specific auction room

    Task AuctionCreated(AuctionResponse createdAuction); // Triggerd for all connected clients
    Task AuctionDeleted(AuctionDeletedResponse deletedAuctionId);  // Triggerd for all connected clients
    Task AuctionEnded(AuctionEndedResponse endedAuctionId);  // Triggerd for all connected clients
    Task AuctionPriceUpdated(AuctionPriceUpdatedResponse auctionIdWithNewPrice);  // Triggerd for all connected clients

    Task MessageReceived(MessageResponse messageResponse);

    Task ErrorOccurred(ErrorResponse error); // Triggerd for the caller client only if there is an error
}
