using EmotionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmotionService.Infrastructure.Persistence.Configurations;

public class MediaItemConfiguration
    : IEntityTypeConfiguration<MediaItem>
{
    public void Configure(EntityTypeBuilder<MediaItem> builder)
    {
        builder.ToTable("MediaItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Description);

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(500);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.ReleaseDate);

        builder.HasOne(x => x.ItemType)
            .WithMany()
            .HasForeignKey(x => x.ItemTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);
    }
}