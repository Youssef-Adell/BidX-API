using BidX.BusinessLogic.DTOs.ChatDTOs;

namespace BidX.Presentation.Hubs;

public partial interface IHubClient
{
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
}
