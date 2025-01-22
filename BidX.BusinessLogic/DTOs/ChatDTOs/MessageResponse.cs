namespace BidX.BusinessLogic.DTOs.ChatDTOs;

public class MessageResponse
{
    public int Id { get; init; }
    public required string Content { get; init; }
    public DateTimeOffset SentAt { get; init; }
    public bool IsRead { get; init; }
    public int ChatId { get; init; }
    public int SenderId { get; init; }
    public int RecipientId { get; init; }
}
