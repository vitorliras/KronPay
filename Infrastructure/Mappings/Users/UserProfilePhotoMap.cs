using KronPay.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Mappings.Users;

public sealed class UserProfilePhotoMap : IEntityTypeConfiguration<UserProfilePhoto>
{
    public void Configure(EntityTypeBuilder<UserProfilePhoto> builder)
    {
        builder.ToTable("user_profile_photos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.Photo)
            .HasColumnName("photo")
            .IsRequired();

        builder.Property(x => x.ContentType)
            .HasColumnName("content_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime2(0)")
            .IsRequired();

        builder.HasIndex(x => x.UserId).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
