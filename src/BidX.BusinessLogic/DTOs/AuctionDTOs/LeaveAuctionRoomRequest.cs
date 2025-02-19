using System.ComponentModel.DataAnnotations;

namespace BidX.BusinessLogic.DTOs.AuctionDTOs;

public class LeaveAuctionRoomRequest
{
    [Required]
    public int AuctionId { get; init; }

}
