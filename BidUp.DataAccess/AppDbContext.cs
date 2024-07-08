namespace BidUp.DataAccess;

using BidUp.DataAccess.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public required DbSet<City> Cities { get; set; }
    public required DbSet<Category> Categories { get; set; }
    public required DbSet<Auction> Auctions { get; set; }
    public required DbSet<Bid> Bids { get; set; }
    public required DbSet<Chat> Chats { get; set; }
    public required DbSet<UserChat> UserChats { get; set; }
    public required DbSet<Message> Messages { get; set; }
    public required DbSet<Review> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>().ToTable("User", "security")
            .HasIndex(u => u.RefreshToken) //to improve the search performance while getting the user associated to the refreshtoken
            .HasDatabaseName("RefreshTokenIndex");
        builder.Entity<IdentityRole<int>>()
            .ToTable("Role", "security");
        builder.Entity<IdentityUserRole<int>>()
            .ToTable("UserRole", "security");
        builder.Entity<IdentityUserClaim<int>>()
            .ToTable("UserClaim", "security");
        builder.Entity<IdentityUserLogin<int>>()
            .ToTable("UserLogin", "security");
        builder.Entity<IdentityRoleClaim<int>>()
            .ToTable("RoleClaim", "security");
        builder.Entity<IdentityUserToken<int>>()
            .ToTable("UserToken", "security");

        builder.Entity<Auction>()
            .HasMany(a => a.Bids)
            .WithOne(b => b.Auction)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Auction>()
            .HasOne(a => a.HighestBid)
            .WithOne()
            .HasForeignKey<Auction>(a => a.HighestBidId);
        builder.Entity<Auction>()
            .HasOne(a => a.Product)
            .WithOne()
            .HasForeignKey<Product>(p => p.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Product>()
            .Property(p => p.Condition)
            .HasConversion(
                conditionObj => conditionObj.ToString(),
                conditionColumn => Enum.Parse<ProductCondition>(conditionColumn));
        builder.Entity<Product>()
            .HasMany(p => p.Images)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Chat>()
            .HasMany(c => c.Users)
            .WithMany(u => u.Chats)
            .UsingEntity<UserChat>();

        builder.Entity<User>()
            .HasMany(u => u.ReviewsWritten)
            .WithOne(r => r.Reviewer)
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<User>()
            .HasMany(u => u.ReviewsReceived)
            .WithOne(r => r.Reviewee)
            .HasForeignKey(r => r.RevieweeId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // I removed this convition to make the onDelete uses its default value which is NoAction instead of using CascadeDelete as a default for all required relationships
        configurationBuilder.Conventions.Remove<CascadeDeleteConvention>();

        // I removed this convention too because if i didnt, the CascadeDeleteConvention will be aplied to any DbSet exists, although I removed CascadeDeleteConvention above (i think it is a bug in EF, i may open an issue in github about it later)
        configurationBuilder.Conventions.Remove<TableNameFromDbSetConvention>();
    }
}

