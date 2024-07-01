using System.ComponentModel.DataAnnotations;

namespace BidUp.BusinessLogic.DTOs.AuctionDTOs;

public class AuctionDeletedResponse
{
    [Required]
    public int AuctionId { get; init; }
}
