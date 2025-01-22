using System.ComponentModel.DataAnnotations;

namespace BidX.BusinessLogic.DTOs.BidDTOs;

public class AcceptBidRequest
{
    [Required]
    public int BidId { get; init; }
}
