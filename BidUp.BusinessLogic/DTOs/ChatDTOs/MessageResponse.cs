using System.Text.Json.Serialization;

namespace BidUp.BusinessLogic.DTOs.ChatDTOs;

public class MessageResponse
{
    public int Id { get; init; }
    public required string Content { get; init; }
    public DateTime SentAt { get; init; }
    public int SenderId { get; init; }
    public int ChatId { get; init; }

    [JsonIgnore] // We need it only in the hub to be able to notify the receiver that he received a message but the client not interest in it 
    public int ReceiverId { get; init; }
}
