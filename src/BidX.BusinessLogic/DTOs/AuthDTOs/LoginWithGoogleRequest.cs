using System.ComponentModel.DataAnnotations;

namespace BidX.BusinessLogic.DTOs.AuthDTOs;

public class LoginWithGoogleRequest
{
    [Required]
    public required string IdToken {get; init;}
}
