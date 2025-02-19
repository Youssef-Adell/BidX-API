using BidX.BusinessLogic.DTOs.AuctionDTOs;
using BidX.BusinessLogic.DTOs.BidDTOs;
using BidX.BusinessLogic.DTOs.ChatDTOs;
using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.DTOs.NotificationDTOs;

namespace BidX.BusinessLogic.Interfaces;

public interface IRealTimeService
{
    Task SendMessageToChat(int chatId, MessageResponse message);
    Task MarkAllMessagesAsRead(int chatId, int readerId);
    Task MarkMessageAsRead(int chatId, int messageId);
    Task NotifyUserWithUnreadChatsCount(int userId, int unreadChatsCount);
    Task NotifyParticipantsWithUserStatus(IEnumerable<int> chatsIds, int userId, bool isOnline);

    Task NotifyUserWithUnreadNotificationsCount(int userId, int unreadNotificationsCount);

    Task SendPlacedBidToAuctionRoom(int auctionId, BidResponse bid);
    Task SendAcceptedBidToAuctionRoom(int auctionId, BidResponse bid);
    Task SendAuctionToFeed(AuctionResponse auction);
    Task DeleteAuctionFromFeed(int auctionId);
    Task UpdateAuctionPriceInFeed(int auctionId, decimal newPrice);
    Task MarkAuctionAsEndedInFeed(int auctionId, decimal finalPrice);

    Task SendErrorToUser(int userId, ErrorResponse error);
    Task SendErrorToUser(int userId, ErrorCode errorCode, IEnumerable<string> errorMessages);
}
