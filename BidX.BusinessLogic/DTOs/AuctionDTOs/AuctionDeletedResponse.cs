using System.ComponentModel.DataAnnotations;

namespace BidX.BusinessLogic.DTOs.AuctionDTOs;

public class AuctionDeletedResponse
{
    [Required]
    public int AuctionId { get; init; }
}
