namespace BidUp.DataAccess;

using System.Reflection;
using BidUp.DataAccess.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

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
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Entity<Chat>()
            .HasMany(c => c.Users)
            .WithMany(u => u.Chats)
            .UsingEntity<UserChat>();
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // I removed this Convention to make the onDelete uses its default value which is NoAction instead of using CascadeDelete as a default for all required relationships
        configurationBuilder.Conventions.Remove<CascadeDeleteConvention>();

        // I removed this Convention too because if i didnt, the CascadeDeleteConvention will be aplied to any DbSet exists, although I removed CascadeDeleteConvention above (i think it is a bug in EF, i may open an issue in github about it later)
        configurationBuilder.Conventions.Remove<TableNameFromDbSetConvention>();
    }
}

