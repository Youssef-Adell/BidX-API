namespace BidUp.BusinessLogic.DTOs.AuctionDTOs;

public class AuctionUserHasBidOnResponse
{
    public int Id { get; init; }
    public required string ProductName { get; init; }
    public required string ThumbnailUrl { get; set; }
    public decimal CurrentPrice { get; init; }
    public DateTime EndTime { get; init; }
    public bool IsActive { get; init; }
    public bool? IsUserWon { get; set; }
}
