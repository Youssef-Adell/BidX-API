namespace BidX.DataAccess.Entites;

public class Auction
{
    public int Id { get; set; }

    public required string ProductName { get; set; }

    public required string ProductDescription { get; set; }

    public required ProductCondition ProductCondition { get; set; }

    public required string ThumbnailUrl { get; set; }

    public decimal StartingPrice { get; set; }

    public decimal MinBidIncrement { get; set; }

    public DateTimeOffset StartTime { get; set; }

    public DateTimeOffset EndTime { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public int CityId { get; set; }
    public City? City { get; set; }

    public int AuctioneerId { get; set; }
    public User? Auctioneer { get; set; }

    public int? WinnerId { get; set; }
    public User? Winner { get; set; }

    public ICollection<ProductImage>? ProductImages { get; set; }

    public ICollection<Bid>? Bids { get; set; }

    public bool IsActive
    {
        get => EndTime.CompareTo(DateTimeOffset.UtcNow) > 0;
    }
}