using EmotionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmotionService.Infrastructure.Persistence.Configurations;

public sealed class GameDetailConfiguration : IEntityTypeConfiguration<GameDetail>
{
    public void Configure(EntityTypeBuilder<GameDetail> builder)
    {
        builder
            .ToTable("GameDetails");
        builder
            .HasKey(x => x.MediaItemId);
        builder
            .Property(x => x.Developer)
            .HasMaxLength(150)
            .IsRequired();
        builder
            .Property(x => x.Publisher)
            .HasMaxLength(150)
            .IsRequired();
        builder
            .Property(x => x.Genre)
            .HasMaxLength(100)
            .IsRequired();
        builder
            .Property(x => x.Platform)
            .HasMaxLength(150)
            .IsRequired();
        builder
            .Property(x => x.Description)
            .HasMaxLength(3000)
            .IsRequired();
        builder
            .Property(x => x.AgeRating)
            .HasMaxLength(30);
        builder
            .Property(x => x.GameMode)
            .HasMaxLength(100);
        builder
            .Property(x => x.Engine)
            .HasMaxLength(100);
        builder.HasOne(x => x.MediaItem)
            .WithOne()
            .HasForeignKey<GameDetail>(x => x.MediaItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
