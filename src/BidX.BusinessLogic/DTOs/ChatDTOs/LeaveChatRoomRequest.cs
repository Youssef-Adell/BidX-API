using System.ComponentModel.DataAnnotations;

namespace BidX.BusinessLogic.DTOs.ChatDTOs;

public class LeaveChatRoomRequest
{
    [Required]
    public int ChatId { get; init; }
}
