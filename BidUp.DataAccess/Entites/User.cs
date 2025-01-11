using Microsoft.AspNetCore.Identity;

namespace BidUp.DataAccess.Entites;

public class User : IdentityUser<int>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string FullName { get; } = null!;
    public string? ProfilePictureUrl { get; set; }
    public decimal TotalRating { get; set; }
    public string? RefreshToken { get; set; }
    public bool IsOnline { get; set; }
    public ICollection<Chat> Chats { get; } = new List<Chat>();
    public ICollection<Review> ReviewsWritten { get; } = new List<Review>();
    public ICollection<Review> ReviewsReceived { get; } = new List<Review>();
}
