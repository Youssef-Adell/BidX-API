using BidX.DataAccess.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BidX.DataAccess.Configs;

public class ReviewConfig : IEntityTypeConfiguration<Review>
{

    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable(t => t.HasTrigger("average_rating_trigger")); // To metigate the error due to this breaking change https://shorturl.at/EfgxH

        builder.Property(r => r.Rating)
            .HasPrecision(2, 1); // 2 total digits at max includes 1 digit at right of the decimal point (ex: 3.5)

        builder.HasOne(u => u.Reviewer)
            .WithMany()
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict); // To avoid unconsistent AverageRating for Reviewees who the user review before

        builder.HasOne(u => u.Reviewee)
            .WithMany()
            .HasForeignKey(r => r.RevieweeId)
            .OnDelete(DeleteBehavior.Restrict);
    }

}
