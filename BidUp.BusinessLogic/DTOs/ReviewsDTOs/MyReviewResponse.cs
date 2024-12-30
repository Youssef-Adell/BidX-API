namespace BidUp.BusinessLogic.DTOs.ReviewsDTOs;

public class MyReviewResponse
{
    public int Id { get; init; }
    public decimal Rating { get; init; }
    public string? Comment { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
