using System.ComponentModel.DataAnnotations;

namespace BidUp.BusinessLogic.DTOs.ChatDTOs;

public class CreateChatRequest
{
    [Required]
    public int ParticipantId { get; init; }
}
