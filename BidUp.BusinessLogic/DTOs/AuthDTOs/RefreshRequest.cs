using System.ComponentModel.DataAnnotations;

namespace BidUp.BusinessLogic.DTOs.AuthDTOs;

public class RefreshRequest
{
    [Required]
    public required string RefreshToken { get; set; }
}
