using System.ComponentModel.DataAnnotations;

namespace BidUp.BusinessLogic.DTOs.AuthDTOs;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Password { get; init; }
}