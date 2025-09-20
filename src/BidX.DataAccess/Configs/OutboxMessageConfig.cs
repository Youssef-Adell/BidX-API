using BidX.DataAccess.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BidX.DataAccess.Configs;

public class OutboxMessageConfig : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasIndex(m => new { m.CreatedAt, m.ProcessedAt })
            .IncludeProperties(m => new { m.Id, m.Type, m.Content })
            .HasFilter("[ProcessedAt] IS NULL");
    }

}
