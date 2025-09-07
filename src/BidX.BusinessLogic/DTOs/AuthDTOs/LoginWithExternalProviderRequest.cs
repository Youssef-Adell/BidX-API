using System.ComponentModel.DataAnnotations;

namespace BidX.BusinessLogic.DTOs.AuthDTOs;

public class LoginWithExternalProviderRequest
{
    [Required]
    public required string Provider { get; init; }

    [Required]
    public required string IdToken { get; init; }
}
