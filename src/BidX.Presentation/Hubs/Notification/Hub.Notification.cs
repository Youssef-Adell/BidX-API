using BidX.BusinessLogic.DTOs.NotificationDTOs;
using Microsoft.AspNetCore.Authorization;

namespace BidX.Presentation.Hubs;

public partial class Hub
{
    [Authorize]
    public async Task MarkNotificationAsRead(MarkNotificationAsReadRequest request)
    {
        var userId = int.Parse(Context.UserIdentifier!);
        await notificationsService.MarkNotificationAsRead(userId, request.NotificationId);
    }

    [Authorize]
    public async Task MarkAllNotificationsAsRead()
    {
        var userId = int.Parse(Context.UserIdentifier!);
        await notificationsService.MarkAllNotificationsAsRead(userId);
    }
}
