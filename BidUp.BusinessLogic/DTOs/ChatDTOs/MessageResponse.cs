using System.Text.Json.Serialization;

namespace BidUp.BusinessLogic.DTOs.ChatDTOs;

public class MessageResponse
{
    public int Id { get; init; }
    public int ChatId { get; init; }
    public int SenderId { get; init; }
    public required string Content { get; init; }
    public DateTime SentAt { get; init; }
    public bool Seen { get; init; }

    [JsonIgnore] // We need it only in the hun to be able send a notifiction to the receiver but the frontend does not need it
    public int ReceiverId { get; init; }
}
