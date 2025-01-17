namespace BidUp.BusinessLogic.DTOs.ReviewsDTOs;

public class ReviewResponse
{
    public int Id { get; init; }
    public decimal Rating { get; init; }
    public string? Comment { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public required Reviewer Reviewer { get; init; }
}

public class Reviewer
{
    public int Id { get; init; }
    public required string FullName { get; init; }
    public string? ProfilePictureUrl { get; init; }
    public decimal AverageRating { get; init; }
}
