namespace BidUp.BusinessLogic.DTOs.UserProfileDTOs;

public class UserProfileResponse
{
    public int Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? ProfilePictureUrl { get; init; }
    public decimal TotalRating { get; init; }
}
