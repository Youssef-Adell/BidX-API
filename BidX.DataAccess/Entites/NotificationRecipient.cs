namespace BidX.DataAccess.Entites;

public class NotificationRecipient
{
    public int RecipientId { get; set; }
    public int NotificationId { get; set; }

    public User? Recipient { get; set; }
    public Notification? Notification { get; set; }

    public bool IsRead { get; set; }
}
