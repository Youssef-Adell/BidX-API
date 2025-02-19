namespace BidX.DataAccess.Entites;

public class Notification
{
    public int Id { get; set; }

    public required string Message { get; set; }

    public RedirectTo RedirectTo { get; set; }

    public int? RedirectId { get; set; } // Not all redirections need Id

    public bool IsRead { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public int IssuerId { get; set; }
    public User? Issuer { get; set; }

    public ICollection<NotificationRecipient>? NotificationRecipients { get; set; }
}