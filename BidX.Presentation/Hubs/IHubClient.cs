using BidX.BusinessLogic.DTOs.AuctionDTOs;
using BidX.BusinessLogic.DTOs.BidDTOs;
using BidX.BusinessLogic.DTOs.ChatDTOs;
using BidX.BusinessLogic.DTOs.CommonDTOs;

namespace BidXesentation.Hubs;

public interface IHubClient
{
    Task BidCreated(BidResponse createdBid); // Triggerd only for clients who currently in a specific auction room
    Task BidAccepted(BidResponse acceptedBid); // Triggerd only for clients who currently in a specific auction room

    Task AuctionCreated(AuctionResponse createdAuction); // Triggerd for clients who currently in the Feed room
    Task AuctionDeleted(AuctionDeletedResponse deletedAuctionId);  // Triggerd for for clients who currently in the Feed room
    Task AuctionEnded(AuctionEndedResponse endedAuction);  // Triggerd for clients who currently in the Feed room
    Task AuctionPriceUpdated(AuctionPriceUpdatedResponse auctionIdWithNewPrice);  // Triggerd for clients who currently in the Feed room

    Task MessageReceived(MessageResponse receivedMessage); // Triggerd for sender & receiver who currently in a specific chat room
    Task MessagesSeen(); // Triggerd for the sender client who currently in a specific chat room
    Task UserStatusChanged(UserStatusResponse userStatus); // Triggerd for any client currently in a chat room with this user 
    Task MessageReceivedNotification(); // Triggerd for any client got a new message

    Task ErrorOccurred(ErrorResponse error); // Triggerd for the caller client only if there is an error
}
