using BidX.DataAccess.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BidX.DataAccess.Configs;

public class NotificationRecipientConfig : IEntityTypeConfiguration<NotificationRecipient>
{
    public void Configure(EntityTypeBuilder<NotificationRecipient> builder)
    {
        // RecipientId comes first in the composite key to efficiently support: 
        //  - Fetching all notifications for a recipient (Where RecipientId = x)
        //  - Marking a notification as read (Where RecipientId = x AND NotificationId = y)
        builder.HasKey(nr => new { nr.RecipientId, nr.NotificationId });

        // To ensure that a recipient gets only one notification per event
        builder.HasIndex(nr => new { nr.RecipientId, nr.EventId })
            .IsUnique();

        builder.HasOne(nr => nr.Notification)
            .WithMany(n => n.NotificationRecipients)
            .HasForeignKey(nr => nr.NotificationId);

        builder.HasOne(nr => nr.Recipient)
            .WithMany()
            .HasForeignKey(nr => nr.RecipientId);
    }
}