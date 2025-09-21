using MediatR;

namespace BidX.BusinessLogic.Events;

public class Event : INotification
{
    public Guid Id { get; init; } = Guid.NewGuid();
}
