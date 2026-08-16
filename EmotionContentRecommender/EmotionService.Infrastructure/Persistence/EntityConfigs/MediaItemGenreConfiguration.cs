using EmotionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmotionService.Infrastructure.Persistence.Configurations;

public sealed class MediaItemGenreConfiguration
    : IEntityTypeConfiguration<MediaItemGenre>
{
    public void Configure(
        EntityTypeBuilder<MediaItemGenre> builder)
    {
        builder.ToTable("MediaItemGenres");

        builder.HasKey(x => new
        {
            x.MediaItemId,
            x.GenreId
        });

        builder.HasOne(x => x.MediaItem)
            .WithMany()
            .HasForeignKey(x => x.MediaItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Genre)
            .WithMany()
            .HasForeignKey(x => x.GenreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}