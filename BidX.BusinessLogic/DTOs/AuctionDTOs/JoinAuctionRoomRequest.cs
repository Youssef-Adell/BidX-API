using System.ComponentModel.DataAnnotations;

namespace BidX.BusinessLogic.DTOs.AuctionDTOs;

public class JoinAuctionRoomRequest
{
    [Required]
    public int AuctionId { get; init; }
}
