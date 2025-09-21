using MediatR;

namespace BidX.BusinessLogic.Events;

public class BidAcceptedEvent : Event
{
    public int WinnerId { get; set; }
    public int AuctionId { get; set; }
    public string AuctionProductName { get; set; } = string.Empty;
    public int AuctioneerId { get; set; }
    public List<int> BiddersIds { get; set; } = new();
}