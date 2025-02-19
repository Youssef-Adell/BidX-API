using System.ComponentModel.DataAnnotations;

namespace BidX.BusinessLogic.DTOs.AuthDTOs;

public class RegisterRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public required string FirstName { get; init; }

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public required string LastName { get; init; }

    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Password { get; init; }
}