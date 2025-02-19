namespace BidX.DataAccess.Entites;

public class Message
{
    public int Id { get; set; }

    public required string Content { get; set; }

    public bool IsRead { get; set; }

    public DateTimeOffset SentAt { get; set; }

    public int SenderId { get; set; }
    public User? Sender { get; set; }

    public int RecipientId { get; set; }
    public User? Recipient { get; set; }

    public int ChatId { get; set; }
    public Chat? Chat { get; set; }
}
