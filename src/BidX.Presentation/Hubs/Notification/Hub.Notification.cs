using BidX.BusinessLogic.DTOs.NotificationDTOs;

namespace BidX.Presentation.Hubs;

public partial class Hub
{
    public async Task MarkNotificationAsRead(MarkNotificationAsReadRequest request)
    {
        var userId = int.Parse(Context.UserIdentifier!);
        await notificationsService.MarkNotificationAsRead(userId, request.NotificationId);
    }

    public async Task MarkAllNotificationsAsRead()
    {
        var userId = int.Parse(Context.UserIdentifier!);
        await notificationsService.MarkAllNotificationsAsRead(userId);
    }
}
