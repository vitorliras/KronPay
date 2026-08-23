using Domain.Entities.Notifications;
using KronPay.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Mappings.Notifications;

public sealed class NotificationPreferenceMap : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("notification_preferences");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.EmailOnCritical)
            .HasColumnName("email_on_critical")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.EmailOnImportant)
            .HasColumnName("email_on_important")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.EmailOnInformative)
            .HasColumnName("email_on_informative")
            .HasDefaultValue(false)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.UserId).IsUnique();
    }
}
