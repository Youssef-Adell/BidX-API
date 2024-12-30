namespace BidUp.BusinessLogic.DTOs.ReviewsDTOs;

public class ReviewResponse
{
    public int Id { get; init; }
    public decimal Rating { get; init; }
    public string? Comment { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public required Reviewer Reviewer { get; init; }
}

public class Reviewer
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? ProfilePictureUrl { get; init; }
}
