namespace BidX.BusinessLogic.DTOs.AuthDTOs;

public class ExternalUserInfo
{
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Picture { get; set; }
}
