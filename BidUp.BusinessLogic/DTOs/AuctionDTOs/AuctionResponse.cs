using BidUp.DataAccess.Entites;

namespace BidUp.BusinessLogic.DTOs.AuctionDTOs;

public class AuctionResponse
{
    public int Id { get; init; }
    public required string ProductName { get; init; }
    public ProductCondition ProductCondition { get; init; }
    public required string ThumbnailUrl { get; set; }
    public decimal CurrentPrice { get; init; }
    public DateTimeOffset EndTime { get; init; }
    public int CategoryId { get; init; }
    public int CityId { get; init; }
}
