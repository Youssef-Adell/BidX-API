namespace BidUp.BusinessLogic.DTOs.UserProfileDTOs;

public class UserProfileResponse
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? ProfilePictureUrl { get; init; }
}
