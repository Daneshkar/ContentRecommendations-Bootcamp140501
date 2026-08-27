using EmotionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmotionService.Infrastructure.Persistence.Configurations;

public sealed class ItemThemeWeightConfiguration
    : IEntityTypeConfiguration<ItemThemeWeight>
{
    public void Configure(
        EntityTypeBuilder<ItemThemeWeight> builder)
    {
        builder.ToTable("ItemThemeWeights");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id);

        builder.Property(x => x.WeightValue)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(x => x.ExperienceCount)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.MediaItemId,
            x.ThemeId
        }).IsUnique();

        builder.HasOne(x => x.MediaItem)
            .WithMany()
            .HasForeignKey(x => x.MediaItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Theme)
            .WithMany()
            .HasForeignKey(x => x.ThemeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}