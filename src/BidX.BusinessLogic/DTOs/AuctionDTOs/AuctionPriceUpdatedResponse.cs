using System.ComponentModel.DataAnnotations;

namespace BidX.BusinessLogic.DTOs.AuctionDTOs;

public class AuctionPriceUpdatedResponse
{
    [Required]
    public int AuctionId { get; init; }

    [Required]
    public decimal NewPrice { get; init; }
}
