namespace BidUp.BusinessLogic.DTOs.ReviewsDTOs;

public class ReviewResponse
{
    public int Id { get; init; }
    public int ReviewerId { get; init; }
    public int RevieweeId { get; init; }
    public int Rating { get; init; }
    public string? Comment { get; init; }
}
