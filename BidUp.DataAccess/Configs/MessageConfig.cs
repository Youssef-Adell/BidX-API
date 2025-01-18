using BidUp.DataAccess.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BidUp.DataAccess.Configs;

public class MessageConfig : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable(t => t.HasTrigger("last_message_trigger"));

        builder.HasOne(m => m.Chat)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Recipient)
            .WithMany()
            .HasForeignKey(m => m.RecipientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Needed in GetUserChats for:
        //  - Counting unread received messages by filtering with (ChatId + RecipientId + IsRead)
        builder.HasIndex(m => new { m.ChatId, m.RecipientId, m.IsRead });

    }

}
