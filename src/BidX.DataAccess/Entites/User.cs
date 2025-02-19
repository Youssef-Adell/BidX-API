using Microsoft.AspNetCore.Identity;

namespace BidX.DataAccess.Entites;

public class User : IdentityUser<int>
{
    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public string FullName { get; } = null!;

    public string? ProfilePictureUrl { get; set; }

    public bool IsOnline { get; set; }

    public decimal AverageRating { get; set; }

    public string? RefreshToken { get; set; }
}
