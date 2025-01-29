using BidX.DataAccess.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BidX.DataAccess.Configs;

public class NotificationConfig : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(n => n.RedirectTo)
            .HasConversion(
                redirectToEnum => redirectToEnum.ToString(),
                redirectToColumn => Enum.Parse<RedirectTo>(redirectToColumn));
    }

}
