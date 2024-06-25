using System.ComponentModel.DataAnnotations;

namespace BidUp.BusinessLogic.DTOs.BidDTOs;

public class BidRequest
{
    [Required]
    public int AuctionId { get; init; }

    [Required]
    [Range(1, ((double)decimal.MaxValue), ErrorMessage = "The Amount field is required and must be a positive number.")]
    public decimal Amount { get; init; }
}
