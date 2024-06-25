namespace BidUp.DataAccess.Entites;
public class Auction
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal StartingPrice { get; set; }
    public decimal MinBidIncrement { get; set; }
    public int CategoryId { get; set; }
    public int CityId { get; set; }
    public int AuctioneerId { get; set; }
    public int? WinnerId { get; set; }
    public int? HighestBidId { get; set; }

    public required Product Product { get; set; }
    public Category? Category { get; set; }
    public City? City { get; set; }
    public User? Auctioneer { get; set; }
    public User? Winner { get; set; }
    public Bid? HighestBid { get; set; }
    public ICollection<Bid> Bids { get; } = new List<Bid>();

    public decimal CurrentPrice { get => HighestBid is not null ? HighestBid.Amount : StartingPrice; }
    public bool IsActive { get => EndTime.CompareTo(DateTime.UtcNow) > 0; }
    public void SetTime(long durationInSeconds)
    {
        StartTime = DateTime.UtcNow;
        EndTime = StartTime.AddSeconds(durationInSeconds);
    }
}