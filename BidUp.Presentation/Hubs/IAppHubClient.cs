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

    Task MessageReceived(MessageResponse receivedMessage); // Triggerd for sender & receiver who currently in a specific chat room
    Task MessagesSeen(); // Triggerd for the sender client who currently in a specific chat room
    Task UserStatusChanged(UserStatusResponse userStatus); // Triggerd for any client currently in a chat room with this user 
    Task MessageReceivedNotification(); // Triggerd for any client got a new message

    Task ErrorOccurred(ErrorResponse error); // Triggerd for the caller client only if there is an error
}
