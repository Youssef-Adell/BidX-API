namespace BidUp.DataAccess.Entites;

public class Review
{
    public int Id { get; set; }

    public decimal Rating { get; set; }

    public string? Comment { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public int ReviewerId { get; set; }
    public User? Reviewer { get; set; }

    public int RevieweeId { get; set; }
    public User? Reviewee { get; set; }
}
