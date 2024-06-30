using System.ComponentModel.DataAnnotations;

namespace BidUp.BusinessLogic.DTOs.AuctionDTOs;

public class AuctionPriceUpdatedResponse
{
    [Required]
    public int AuctionId { get; init; }

    [Required]
    public decimal NewPrice { get; init; }
}
