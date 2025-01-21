using System.ComponentModel.DataAnnotations;

namespace BidUp.BusinessLogic.DTOs.ChatDTOs;

public class JoinChatRoomRequest
{
    [Required]
    public int ChatId { get; init; }
}
