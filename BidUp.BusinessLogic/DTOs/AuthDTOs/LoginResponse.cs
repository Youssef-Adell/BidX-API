namespace BidUp.BusinessLogic.DTOs.AuthDTOs;

public class LoginResponse
{
    public required UserInfo User { get; set; }
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}

public class UserInfo
{
    public required int Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public string? ProfilePictureUrl { get; init; }
    public required string Role { get; init; }
}
