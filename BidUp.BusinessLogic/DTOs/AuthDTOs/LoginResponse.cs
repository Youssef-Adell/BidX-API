namespace BidUp.BusinessLogic.DTOs.AuthDTOs;

public class LoginResponse
{
    public required int UserId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string Role { get; init; }
    public required string AccessToken { get; set; }
    public required double ExpiresIn { get; set; }
}
