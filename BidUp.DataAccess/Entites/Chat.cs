namespace BidUp.DataAccess.Entites;

public class Chat
{
    public int Id { get; set; }
    public ICollection<Message> Messages { get; } = new List<Message>();
    public ICollection<User> Users { get; set; } = new List<User>();
}
