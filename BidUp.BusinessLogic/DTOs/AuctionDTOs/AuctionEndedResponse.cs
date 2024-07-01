using System.ComponentModel.DataAnnotations;

namespace BidUp.BusinessLogic.DTOs.AuctionDTOs;

public class AuctionEndedResponse
{
    [Required]
    public int AuctionId { get; init; }
}
