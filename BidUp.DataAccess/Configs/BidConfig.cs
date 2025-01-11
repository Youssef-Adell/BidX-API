using BidUp.DataAccess.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BidUp.DataAccess.Configs;

public class BidConfig : IEntityTypeConfiguration<Bid>
{

    public void Configure(EntityTypeBuilder<Bid> builder)
    {
        builder.Property(b => b.Amount)
            .HasPrecision(18, 0);

        builder.HasIndex(b => new { b.AuctionId, b.Amount })
            .IsDescending();
    }

}
