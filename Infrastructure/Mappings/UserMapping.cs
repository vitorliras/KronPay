using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class UserMapping : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.OwnsOne(x => x.Name, name =>
        {
            name.Property(n => n.Value)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(60);
        });

        builder.OwnsOne(x => x.Username, username =>
        {
            username.Property(u => u.Value)
                .HasColumnName("username")
                .IsRequired()
                .HasMaxLength(20);

            username.HasIndex(u => u.Value)
                .IsUnique();
        });

        builder.OwnsOne(x => x.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("email")
                .IsRequired()
                .HasMaxLength(80);

            email.HasIndex(e => e.Value).IsUnique();
        });

        builder.OwnsOne(x => x.Cpf, cpf =>
        {
            cpf.Property(c => c.Value)
                .HasColumnName("cpf")
                .IsRequired()
                .HasMaxLength(14);

            cpf.HasIndex(c => c.Value)
                .IsUnique();
        });

        builder.OwnsOne(x => x.Phone, phone =>
        {
            phone.Property(p => p.Value)
                .HasColumnName("phone")
                .HasMaxLength(20);
        });

        builder.Property(x => x.PasswordHash)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.UserType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.LastAccessAt)
            .IsRequired(false);
    }
}
