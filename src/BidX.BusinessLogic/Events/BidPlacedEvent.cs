using MediatR;

namespace BidX.BusinessLogic.Events;

public class BidPlacedEvent : INotification
{
    public required decimal BidAmount { get; init; }
    public required int BidderId { get; init; }
    public required int AuctionId { get; init; }
    public required string AuctionProductName { get; init; }
    public required int AuctioneerId { get; init; }
    public required int? PreviousHighBidderId { get; init; }
}