using EmotionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmotionService.Infrastructure.Persistence.Configurations;

public sealed class MusicDetailConfiguration : IEntityTypeConfiguration<MusicDetail>
{
    public void Configure(EntityTypeBuilder<MusicDetail> builder)
    {
        builder
            .ToTable("MusicDetails");
        builder
            .HasKey(x => x.MediaItemId);
        builder
            .Property(x => x.Artist)
            .HasMaxLength(150)
            .IsRequired();
        builder
            .Property(x => x.Album)
            .HasMaxLength(150);
        builder
            .Property(x => x.Genre)
            .HasMaxLength(100)
            .IsRequired();
        builder
            .Property(x => x.Description)
            .HasMaxLength(2000);
        builder
            .Property(x => x.Publisher)
            .HasMaxLength(150);
        builder
            .Property(x => x.Language)
            .HasMaxLength(50);
        builder.HasOne(x => x.MediaItem)
            .WithOne()
            .HasForeignKey<MusicDetail>(x => x.MediaItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
