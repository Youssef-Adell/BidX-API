using System.ComponentModel.DataAnnotations;

namespace BidX.BusinessLogic.DTOs.ReviewsDTOs;

public class AddReviewRequest
{
    [Required]
    [Range(1, 5)]
    public decimal Rating { get; init; }
    public string? Comment { get; init; }
}
