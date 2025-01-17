using BidUp.DataAccess.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BidUp.DataAccess.Configs;

public class ReviewConfig : IEntityTypeConfiguration<Review>
{

    public void Configure(EntityTypeBuilder<Review> builder)
    {
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
