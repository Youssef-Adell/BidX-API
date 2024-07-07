namespace BidUp.DataAccess.Entites;

// Join entity handles the many-to-many relationship between User and Chat which can indeed streamline
// the creation of chats and associating them with users without needing to retrieve the full User entities (like in IntiateChat method)
public class UserChat
{
    public int UserId { get; set; }
    public int ChatId { get; set; }
    public User? User { get; set; }
    public Chat? Chat { get; set; }
}
