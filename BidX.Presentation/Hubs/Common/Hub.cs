using BidX.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace BidX.Presentation.Hubs;

public partial class Hub : Hub<IHubClient>
{
    private readonly IBidsService bidsService;
    private readonly IChatsService chatsService;
    private readonly INotificationsService notificationsService;

    public Hub(IBidsService bidsService, IChatsService chatsService, INotificationsService notificationsService)
    {
        this.bidsService = bidsService;
        this.chatsService = chatsService;
        this.notificationsService = notificationsService;
    }

    public override async Task OnConnectedAsync()
    {
        if (int.TryParse(Context.UserIdentifier, out int userId))
        {
            await notificationsService.NotifyUserWithUnreadNotificationsCount(userId);
            await chatsService.NotifyUserWithUnreadChatsCount(userId);
            await chatsService.NotifyParticipantsWithUserStatus(userId, isOnline: true);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (int.TryParse(Context.UserIdentifier, out int userId))
        {
            await chatsService.NotifyParticipantsWithUserStatus(userId, isOnline: false);
        }

        await base.OnDisconnectedAsync(exception);
    }
}
