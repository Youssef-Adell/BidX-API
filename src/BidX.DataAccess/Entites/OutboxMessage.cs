namespace BidX.DataAccess.Entites;

public class OutboxMessage
{
    public Guid Id { get; init; }
    public required string Type { get; init; }
    public required string Content { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public string? Error { get; set; }
}
