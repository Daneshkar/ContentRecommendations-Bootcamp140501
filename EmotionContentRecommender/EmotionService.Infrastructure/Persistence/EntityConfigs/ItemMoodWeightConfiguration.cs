using EmotionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmotionService.Infrastructure.Persistence.Configurations;

public sealed class ItemMoodWeightConfiguration
    : IEntityTypeConfiguration<ItemMoodWeight>
{
    public void Configure(
        EntityTypeBuilder<ItemMoodWeight> builder)
    {
        builder.ToTable("ItemMoodWeights");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

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
            x.MoodId
        }).IsUnique();

        builder.HasOne(x => x.MediaItem)
            .WithMany()
            .HasForeignKey(x => x.MediaItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Mood)
            .WithMany()
            .HasForeignKey(x => x.MoodId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}