using System.ComponentModel.DataAnnotations;

namespace BidUp.BusinessLogic.DTOs.AuctionDTOs;

public class AuctionDeletedOrEndedResponse
{
    [Required]
    public int AuctionId { get; init; }
}
