using BidUp.DataAccess.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BidUp.DataAccess.Configs;

public class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User", "security")
           .HasIndex(u => u.RefreshToken); //to improve the search performance while getting the user associated to the refreshtoken

        builder.Property(u => u.FullName)
            .HasComputedColumnSql("[FirstName] + ' ' + [LastName]", stored: true);

        builder.Property(u => u.AverageRating)
            .HasPrecision(2, 1);

    }

}
