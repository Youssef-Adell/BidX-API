using System.ComponentModel.DataAnnotations;

namespace BidX.BusinessLogic.DTOs.AuthDTOs;

public class ConfirmEmailRequest
{
    [Required]
    public required int UserId { get; init; }

    [Required]
    public required string Token { get; init; }
}
