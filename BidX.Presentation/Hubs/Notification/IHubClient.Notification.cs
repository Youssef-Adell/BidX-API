using BidX.BusinessLogic.DTOs.NotificationDTOs;

namespace BidX.Presentation.Hubs;

public partial interface IHubClient
{
    /// <summary>
    /// Triggerd for any client got a new notification
    /// </summary>
    Task UnreadNotificationsCountChanged(UnreadNotificationsCountResponse response);
}
