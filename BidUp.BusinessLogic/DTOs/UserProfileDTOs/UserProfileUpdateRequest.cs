using System.ComponentModel.DataAnnotations;

namespace BidUp.BusinessLogic.DTOs.UserProfileDTOs;

public class UserProfileUpdateRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public required string FirstName { get; init; }

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public required string LastName { get; init; }
}
