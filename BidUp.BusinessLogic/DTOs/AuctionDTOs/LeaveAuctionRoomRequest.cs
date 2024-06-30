using System.ComponentModel.DataAnnotations;

namespace BidUp.BusinessLogic.DTOs.AuctionDTOs;

public class LeaveAuctionRoomRequest
{
    [Required]
    public int AuctionId { get; init; }

}
