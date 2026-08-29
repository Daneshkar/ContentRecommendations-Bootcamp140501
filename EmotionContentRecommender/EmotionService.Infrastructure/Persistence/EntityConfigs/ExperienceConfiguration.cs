using EmotionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmotionService.Infrastructure.Persistence.EntityConfigs;

public sealed class ExperienceConfiguration
    : IEntityTypeConfiguration<Experience>
{
    public void Configure(EntityTypeBuilder<Experience> builder)
    {
        builder.ToTable("Experiences");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.MediaItemId)
            .IsRequired();

        builder.Property(x => x.Note)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("datetime2")
            .IsRequired(false);

        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => x.MediaItemId);

        builder.HasIndex(x => x.CreatedAt);

        builder.HasOne(x => x.MediaItem)
            .WithMany()
            .HasForeignKey(x => x.MediaItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.ExperienceMoods)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.ExperienceThemes)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}