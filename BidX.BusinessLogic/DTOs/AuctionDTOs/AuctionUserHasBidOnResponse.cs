using System.Text.Json.Serialization;

namespace BidX.BusinessLogic.DTOs.AuctionDTOs;

public class AuctionUserHasBidOnResponse
{
    public int Id { get; init; }
    public required string ProductName { get; init; }
    public required string ThumbnailUrl { get; set; }
    public decimal CurrentPrice { get; init; }
    public DateTimeOffset EndTime { get; init; }
    public bool IsActive { get; init; }
    public bool? IsUserWon { get; set; }

    [JsonIgnore] // Used to calcute IsUserWon at the app-side
    public int? WInnerId { get; init; }
}
