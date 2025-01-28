using BidX.BusinessLogic.DTOs.NotificationDTOs;
using BidX.BusinessLogic.Interfaces;

namespace BidX.Presentation.Hubs;

public partial class Hub
{
    public async Task MarkNotificationAsRead(MarkNotificationAsReadRequest request)
    {
        var userId = int.Parse(Context.UserIdentifier!);
        await notificationsService.MarkNotificationAsRead(userId, request.NotificationId);
    }
}
