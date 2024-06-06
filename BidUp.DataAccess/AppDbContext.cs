namespace BidUp.DataAccess;

using BidUp.DataAccess.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public required DbSet<City> Cities { get; set; }
    public required DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>().ToTable("Users", "security")
            .HasIndex(u => u.RefreshToken) //to improve the search performance while getting the user associated to the refreshtoken
            .HasDatabaseName("RefreshTokenIndex");
        builder.Entity<IdentityRole<int>>().ToTable("Roles", "security");
        builder.Entity<IdentityUserRole<int>>().ToTable("UserRoles", "security");
        builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims", "security");
        builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins", "security");
        builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims", "security");
        builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens", "security");
    }
}

