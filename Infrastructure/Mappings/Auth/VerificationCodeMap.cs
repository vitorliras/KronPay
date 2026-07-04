using Domain.Entities.Auth;
using KronPay.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Mappings.Auth;

public sealed class VerificationCodeMap : IEntityTypeConfiguration<VerificationCode>
{
    public void Configure(EntityTypeBuilder<VerificationCode> builder)
    {
        builder.ToTable("verification_codes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.CodeHash)
            .HasColumnName("code_hash")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Purpose)
            .HasColumnName("purpose")
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("datetime2(0)")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime2(0)")
            .IsRequired();

        builder.Property(x => x.Used)
            .HasColumnName("used")
            .IsRequired();

        builder.Property(x => x.AttemptsCount)
            .HasColumnName("attempts_count")
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UserId, x.Purpose, x.CreatedAt });
    }
}
