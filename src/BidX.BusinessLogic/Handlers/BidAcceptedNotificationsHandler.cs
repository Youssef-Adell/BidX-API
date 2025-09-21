using BidX.BusinessLogic.Events;
using BidX.BusinessLogic.Interfaces;
using MediatR;

namespace BidX.BusinessLogic.Handlers;

public class BidAcceptedNotificationsHandler : INotificationHandler<BidAcceptedEvent>
{
    private readonly INotificationsService notificationsService;

    public BidAcceptedNotificationsHandler(INotificationsService notificationsService)
    {
        this.notificationsService = notificationsService;
    }

    public async Task Handle(BidAcceptedEvent notification, CancellationToken cancellationToken)
    {
        await notificationsService.SendAcceptedBidNotifications(notification);
    }
}
