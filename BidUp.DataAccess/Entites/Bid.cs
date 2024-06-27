namespace BidUp.DataAccess.Entites;

public class Bid
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime BidTime { get; set; } = DateTime.UtcNow;
    public int AuctionId { get; set; }
    public int BidderId { get; set; }

    public Auction? Auction { get; set; }
    public User? Bidder { get; set; }
}