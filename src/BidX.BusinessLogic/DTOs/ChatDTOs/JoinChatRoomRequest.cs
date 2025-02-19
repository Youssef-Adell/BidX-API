using System.ComponentModel.DataAnnotations;

namespace BidX.BusinessLogic.DTOs.ChatDTOs;

public class JoinChatRoomRequest
{
    [Required]
    public int ChatId { get; init; }
}
