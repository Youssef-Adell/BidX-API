namespace BidUp.BusinessLogic.DTOs.BidDTOs;

public class BidResponse
{
    public int Id { get; init; }
    public decimal Amount { get; init; }
    public bool IsAccepted { get; init; }
    public DateTimeOffset PlacedAt { get; init; }
    public int AuctionId { get; init; }
    public required Bidder Bidder { get; init; }
}

public class Bidder
{
    public int Id { get; init; }
    public required string FullName { get; init; }
    public string? ProfilePictureUrl { get; init; }
    public decimal AverageRating { get; init; }
}