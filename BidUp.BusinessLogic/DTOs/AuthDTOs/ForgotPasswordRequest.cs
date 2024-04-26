using System.ComponentModel.DataAnnotations;

namespace BidUp.BusinessLogic.DTOs.AuthDTOs;

public class ForgotPasswordRequest
{
    [EmailAddress]
    public required string Email { get; set; }
}
