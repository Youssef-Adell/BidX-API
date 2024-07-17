using System.ComponentModel.DataAnnotations;

namespace BidUp.BusinessLogic.DTOs.ReviewsDTOs;

public class UpdateReviewRequest
{
    [Required]
    [Range(1, 5)]
    public int Rating { get; init; }
    public string? Comment { get; init; }
}
