using EmotionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmotionService.Infrastructure.Persistence.EntityConfigs;

public sealed class PendingMediaItemWeightUpdateConfiguration
    : IEntityTypeConfiguration<PendingMediaItemWeightUpdate>
{
    public void Configure(
        EntityTypeBuilder<PendingMediaItemWeightUpdate> builder)
    {
        builder.ToTable(
            "PendingMediaItemWeightUpdates",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_PendingMediaItemWeightUpdates_Version",
                    "[Version] > 0");
            });

        builder.HasKey(x => x.MediaItemId);

        builder.Property(x => x.RequestedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.Version)
            .IsRequired();

        builder.HasIndex(x => x.RequestedAt);

        builder.HasOne(x => x.MediaItem)
            .WithMany()
            .HasForeignKey(x => x.MediaItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
