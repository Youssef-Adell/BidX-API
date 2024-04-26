using System.ComponentModel.DataAnnotations;

namespace BidUp.BusinessLogic.DTOs.AuthDTOs;

public class ResetPasswordRequest
{
    [Required]
    public required string UserId { get; set; }

    [Required]
    public required string Token { get; set; }

    [Required]
    public required string NewPassword { get; set; }
}