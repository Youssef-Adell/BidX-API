using BidUp.DataAccess.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BidUp.DataAccess.Configs;

public class AuctionConfig : IEntityTypeConfiguration<Auction>
{
    public void Configure(EntityTypeBuilder<Auction> builder)
    {
        builder.ToTable(t =>
            t.HasCheckConstraint(
                "CK_Auction_ProductCondition",
                "ProductCondition IN ('New', 'Used')"));

        builder.Property(a => a.ProductCondition)
            .HasConversion(
                conditionEnum => conditionEnum.ToString(),
                conditionColumn => Enum.Parse<ProductCondition>(conditionColumn));


        builder.Property(a => a.StartingPrice)
        .HasPrecision(18, 0);

        builder.Property(a => a.MinBidIncrement)
        .HasPrecision(18, 0);

        builder.HasIndex(a => a.EndTime);

        builder.HasMany(a => a.Bids)
              .WithOne(b => b.Auction)
              .IsRequired()
              .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.ProductImages)
            .WithOne(i => i.Auction)
            .OnDelete(DeleteBehavior.Cascade);
    }

}
