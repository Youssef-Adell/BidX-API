using System.ComponentModel.DataAnnotations;

namespace BidUp.BusinessLogic.DTOs.ChatDTOs;

public class MessageRequest
{
    [Required]
    public int ChatId { get; init; }

    [Required]
    public required string Message { get; init; }
}
