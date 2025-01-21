namespace BidUp.DataAccess.Entites;

public class Chat
{
    public int Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public int Participant1Id { get; set; }
    public User? Participant1 { get; set; }

    public int Participant2Id { get; set; }
    public User? Participant2 { get; set; }

    public int? LastMessageId { get; set; }
    public Message? LastMessage { get; set; }

    public ICollection<Message>? Messages { get; set; }
}