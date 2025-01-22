using System.ComponentModel.DataAnnotations;

namespace BidX.BusinessLogic.DTOs.AuthDTOs;

public class ForgotPasswordRequest
{
    [EmailAddress]
    public required string Email { get; set; }
}
