using System.ComponentModel.DataAnnotations;

namespace BidUp.BusinessLogic.DTOs.AuthDTOs;

public class RefreshRequest
{
    public string? RefreshToken { get; set; }
}
