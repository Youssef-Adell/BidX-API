using System.ComponentModel.DataAnnotations;

namespace BidUp.BusinessLogic.DTOs.AuthDTOs;

public class RefreshRequest
{
    // This property must be required otherwise the consumer can enter null as a value and get the users who have null value for RefreshToken field in the db
    [Required]
    public required string RefreshToken { get; set; }
}
