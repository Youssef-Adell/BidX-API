using System.ComponentModel.DataAnnotations;

namespace BidX.BusinessLogic.DTOs.ProfileDTOs;

public class ProfileUpdateRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public required string FirstName { get; init; }

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public required string LastName { get; init; }
}
