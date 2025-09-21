using BidX.BusinessLogic.Events;
using BidX.BusinessLogic.Interfaces;
using MediatR;

namespace BidX.BusinessLogic.Handlers;

public class BidPlacedNotificationsHandler : INotificationHandler<BidPlacedEvent>
{
    private readonly INotificationsService notificationsService;

    public BidPlacedNotificationsHandler(INotificationsService notificationsService)
    {
        this.notificationsService = notificationsService;
    }

    public async Task Handle(BidPlacedEvent notification, CancellationToken cancellationToken)
    {
        await notificationsService.SendPlacedBidNotifications(notification);
    }

}
