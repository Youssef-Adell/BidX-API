namespace BidUp.BusinessLogic.DTOs.ReviewsDTOs;

public class ReviewResponse
{
    public int Id { get; init; }
    public int Rating { get; init; }
    public string? Comment { get; init; }
    public required Reviewer Reviewer { get; init; }
}

public class Reviewer
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? ProfilePictureUrl { get; init; }
}
