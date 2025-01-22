using System.ComponentModel.DataAnnotations;

namespace BidX.BusinessLogic.DTOs.ChatDTOs;

public class CreateChatRequest
{
    [Required]
    public int ParticipantId { get; init; }
}
