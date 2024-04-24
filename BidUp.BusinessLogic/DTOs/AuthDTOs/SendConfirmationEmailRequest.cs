using System.ComponentModel.DataAnnotations;

namespace BidUp.BusinessLogic.DTOs.AuthDTOs;

public class SendConfirmationEmailRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }
}
