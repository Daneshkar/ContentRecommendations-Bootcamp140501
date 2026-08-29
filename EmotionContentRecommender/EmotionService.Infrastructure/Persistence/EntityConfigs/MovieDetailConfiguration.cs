using EmotionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmotionService.Infrastructure.Persistence.Configurations;

public sealed class MovieDetailConfiguration : IEntityTypeConfiguration<MovieDetail>
{
    public void Configure(EntityTypeBuilder<MovieDetail> builder)
    {
        builder
            .ToTable("MovieDetails");
        builder
            .HasKey(x => x.MediaItemId);
        builder
            .Property(x => x.Director)
            .HasMaxLength(150)
            .IsRequired();
        builder
            .Property(x => x.Genre)
            .HasMaxLength(100)
            .IsRequired();
        builder
            .Property(x => x.Synopsis)
            .HasMaxLength(3000)
            .IsRequired();
        builder
            .Property(x => x.Language)
            .HasMaxLength(50);
        builder
            .Property(x => x.Country)
            .HasMaxLength(100);
        builder
            .Property(x => x.AgeRating)
            .HasMaxLength(30);
        builder
            .Property(x => x.Cast)
            .HasMaxLength(1000);
        builder
            .Property(x => x.Studio)
            .HasMaxLength(150);
        builder.HasOne(x => x.MediaItem)
            .WithOne()
            .HasForeignKey<MovieDetail>(x => x.MediaItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
