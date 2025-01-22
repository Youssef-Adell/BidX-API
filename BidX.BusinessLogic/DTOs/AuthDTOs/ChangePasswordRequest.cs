using System.ComponentModel.DataAnnotations;

namespace BidX.BusinessLogic.DTOs.AuthDTOs;

public class ChangePasswordRequest
{
    [Required] //[required] attribute makes the endpoint return badrequest error if the value is either empty string or null
    public required string CurrentPassword { get; set; } //required keyword makes the endpoint return badrequest error only if the value is null

    [Required]
    public required string NewPassword { get; set; }
}
