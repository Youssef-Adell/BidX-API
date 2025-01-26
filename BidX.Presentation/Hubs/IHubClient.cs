using BidX.BusinessLogic.DTOs.AuctionDTOs;
using BidX.BusinessLogic.DTOs.BidDTOs;
using BidX.BusinessLogic.DTOs.ChatDTOs;
using BidX.BusinessLogic.DTOs.CommonDTOs;

namespace BidX.Presentation.Hubs;

public interface IHubClient
{
    /// <summary>
    /// Triggerd only for clients who currently in a specific auction room
    /// </summary>
    Task BidPlaced(BidResponse response);

    /// <summary>
    /// Triggerd only for clients who currently in a specific auction room
    /// </summary>
    Task BidAccepted(BidResponse response);


    /// <summary>
    /// Triggerd for clients who currently in the Feed room
    /// </summary>
    Task AuctionCreated(AuctionResponse response);

    /// <summary>
    /// Triggerd for for clients who currently in the Feed room
    /// </summary>
    Task AuctionDeleted(AuctionDeletedResponse response);

    /// <summary>
    /// Triggerd for clients who currently in the Feed room
    /// </summary>
    Task AuctionEnded(AuctionEndedResponse response);

    /// <summary>
    /// Triggerd for clients who currently in the Feed room
    /// </summary>
    Task AuctionPriceUpdated(AuctionPriceUpdatedResponse response);


    /// <summary>
    /// Triggerd for sender and receiver who currently in a specific chat room
    /// </summary>
    Task MessageReceived(MessageResponse response);

    /// <summary>
    /// Triggerd for sender and receiver who currently in a specific chat room
    /// </summary>
    Task AllMessagesRead(AllMessagesReadResponse response);

    /// <summary>
    /// Triggerd for sender and receiver who currently in a specific chat room
    /// </summary>
    Task MessageRead(MessageReadResponse response);

    /// <summary>
    /// Triggerd for any client currently in a chat room with this user 
    /// </summary>
    Task UserStatusChanged(UserStatusResponse userStatus);

    /// <summary>
    /// Triggerd for any client got a new message
    /// </summary>
    Task UnreadChatsCountChanged(UnreadChatsCountResponse response);


    /// <summary>
    /// Triggerd for the caller client only if there is an error
    /// </summary>
    Task ErrorOccurred(ErrorResponse error);
}
