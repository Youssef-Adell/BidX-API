namespace BidUp.BusinessLogic.DTOs.ReviewsDTOs;

public class MyReviewResponse
{
    public int Id { get; init; }
    public decimal Rating { get; init; }
    public string? Comment { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
