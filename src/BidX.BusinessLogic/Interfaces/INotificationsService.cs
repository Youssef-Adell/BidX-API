using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.DTOs.NotificationDTOs;
using BidX.BusinessLogic.DTOs.QueryParamsDTOs;
using BidX.BusinessLogic.Events;

namespace BidX.BusinessLogic.Interfaces;

public interface INotificationsService
{
    Task<Page<NotificationResponse>> GetUserNotifications(int userId, NotificationsQueryParams queryParams);
    Task MarkNotificationAsRead(int callerId, int notificationId);
    Task MarkAllNotificationsAsRead(int callerId);
    Task NotifyUserWithUnreadNotificationsCount(int userId);
    Task SendPlacedBidNotifications(BidPlacedEvent evt);
    Task SendAcceptedBidNotifications(int auctionId, string auctionTitle, int winnerId, int auctioneerId, IEnumerable<int> biddersIds);
}
