using System.ComponentModel.DataAnnotations;

namespace BidUp.BusinessLogic.DTOs.BidDTOs;

public class AcceptBidRequest
{
    [Required]
    public int BidId { get; init; }
}
