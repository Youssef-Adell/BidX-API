namespace BidUp.DataAccess.Entites;

public class Message
{
    public int Id { get; set; }
    public required string Content { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool Seen { get; set; }
    public int SenderId { get; set; }
    public int ChatId { get; set; }

    public User? Sender { get; set; }
    public Chat? Chat { get; set; }
}
