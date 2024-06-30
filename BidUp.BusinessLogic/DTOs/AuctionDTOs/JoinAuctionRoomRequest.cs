using System.ComponentModel.DataAnnotations;

namespace BidUp.BusinessLogic.DTOs.AuctionDTOs;

public class JoinAuctionRoomRequest
{
    [Required]
    public int AuctionId { get; init; }
}
