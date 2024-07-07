using Microsoft.AspNetCore.Identity;

namespace BidUp.DataAccess.Entites;

public class User : IdentityUser<int>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? RefreshToken { get; set; }
    public ICollection<Chat> Chats { get; } = new List<Chat>();
}
