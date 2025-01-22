namespace BidX.BusinessLogic.DTOs.ProfileDTOs;

public class ProfileResponse
{
    public int Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? ProfilePictureUrl { get; init; }
    public decimal AverageRating { get; init; }
}
