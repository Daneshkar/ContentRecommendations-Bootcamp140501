using EmotionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmotionService.Infrastructure.Persistence.Configurations;

public sealed class BookDetailConfiguration : IEntityTypeConfiguration<BookDetail>
{
    public void Configure(EntityTypeBuilder<BookDetail> builder)
    {
        builder
            .ToTable("BookDetails");
        builder
            .HasKey(x => x.MediaItemId);
        builder
            .Property(x => x.Author)
            .HasMaxLength(200)
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
            .Property(x => x.ISBN)
            .HasMaxLength(20)
            .IsRequired();
        builder
            .Property(x => x.Language)
            .HasMaxLength(50)
            .IsRequired();
        builder
            .Property(x => x.Description)
            .HasMaxLength(3000)
            .IsRequired();
        builder
            .Property(x => x.Edition)
            .HasMaxLength(100);
        builder.HasOne(x => x.MediaItem)
            .WithOne()
            .HasForeignKey<BookDetail>(x => x.MediaItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
