using Domain.Entities.DataRetention;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Mappings.DataRetention;

public sealed class DataRetentionPurgeRunMap : IEntityTypeConfiguration<DataRetentionPurgeRun>
{
    public void Configure(EntityTypeBuilder<DataRetentionPurgeRun> builder)
    {
        builder.ToTable("data_retention_purge_runs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.LastRunAt)
            .HasColumnName("last_run_at")
            .HasColumnType("datetime2(0)")
            .IsRequired();
    }
}
