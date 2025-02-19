using System.ComponentModel.DataAnnotations;

namespace BidX.BusinessLogic.DTOs.AuctionDTOs;

public class AuctionEndedResponse
{
    [Required]
    public int AuctionId { get; init; }

    [Required]
    public decimal FinalPrice { get; init; }
}
