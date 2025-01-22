using System.ComponentModel.DataAnnotations;

namespace BidX.BusinessLogic.DTOs.ChatDTOs;

public class SendMessageRequest
{
    [Required]
    public int ChatId { get; init; }

    [Required]
    public required string Message { get; init; }
}
