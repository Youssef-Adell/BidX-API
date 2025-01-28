using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.DTOs.NotificationDTOs;
using BidX.BusinessLogic.DTOs.QueryParamsDTOs;

namespace BidX.BusinessLogic.Interfaces;

public interface INotificationsService
{
    Task<Page<NotificationResponse>> GetUserNotifications(int userId, NotificationsQueryParams queryParams);
    Task MarkNotificationAsRead(int callerId, int notificationId);
    Task NotifyUserWithUnreadNotificationsCount(int userId);
    Task NotifyNewBid(int auctionId, string auctionTitle, decimal bidAmount, int bidderId, int auctioneerId, int? previousHighestBidderId);
}
