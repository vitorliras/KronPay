using Domain.Entities.Notifications;
using KronPay.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Mappings.Notifications;

public sealed class NotificationMap : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .IsRequired();

        builder.Property(x => x.Criticality)
            .HasColumnName("criticality")
            .IsRequired();

        builder.Property(x => x.MessageKey)
            .HasColumnName("message_key")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PayloadJson)
            .HasColumnName("payload_json")
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.RelatedEntityType)
            .HasColumnName("related_entity_type")
            .HasMaxLength(50);

        builder.Property(x => x.RelatedEntityId)
            .HasColumnName("related_entity_id");

        builder.Property(x => x.IsRead)
            .HasColumnName("is_read")
            .HasColumnType("bit")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.ReadAt)
            .HasColumnName("read_at");

        builder.Property(x => x.IsResolved)
            .HasColumnName("is_resolved")
            .HasColumnType("bit")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.ResolvedAt)
            .HasColumnName("resolved_at");

        builder.Property(x => x.IsArchived)
            .HasColumnName("is_archived")
            .HasColumnType("bit")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.ArchivedAt)
            .HasColumnName("archived_at");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime2(0)")
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UserId, x.IsArchived, x.IsRead });
        builder.HasIndex(x => new { x.UserId, x.IsResolved, x.Criticality });
        builder.HasIndex(x => new { x.UserId, x.IsArchived, x.ArchivedAt });
    }
}
