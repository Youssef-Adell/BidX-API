namespace BidUp.BusinessLogic.DTOs.AuctionDTOs;

public class AuctionResponse
{
    public int Id { get; init; }
    public required string ProductName { get; init; }
    public required string ThumbnailUrl { get; set; }
    public decimal CurrentPrice { get; init; }
    public DateTime EndTime { get; init; }
    public int CategoryId { get; init; }
}
