namespace BidUp.BusinessLogic.DTOs.BidDTOs;

public class BidResponse
{
    public int Id { get; init; }
    public decimal Amount { get; init; }
    public DateTime BidTime { get; init; }
    public int AuctionId { get; init; }
    public required Bidder Bidder { get; init; }
}

public class Bidder
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? ProfilePictureUrl { get; init; }
}