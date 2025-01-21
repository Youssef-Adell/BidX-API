using System.ComponentModel.DataAnnotations;

namespace BidUp.BusinessLogic.DTOs.ChatDTOs;

public class LeaveChatRoomRequest
{
    [Required]
    public int ChatId { get; init; }
}
