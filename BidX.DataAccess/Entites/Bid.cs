namespace BidX.DataAccess.Entites;

public class Bid
{
    public int Id { get; set; }

    public decimal Amount { get; set; }

    public bool IsAccepted { get; set; }

    public DateTimeOffset PlacedAt { get; set; } = DateTimeOffset.UtcNow;

    public int AuctionId { get; set; }
    public Auction? Auction { get; set; }

    public int BidderId { get; set; }
    public User? Bidder { get; set; }
}