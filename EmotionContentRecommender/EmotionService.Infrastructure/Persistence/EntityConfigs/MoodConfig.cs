using EmotionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmotionService.Infrastructure.Persistence.EntityConfigs;

public class MoodConfig : IEntityTypeConfiguration<Mood>
{
    public void Configure(EntityTypeBuilder<Mood> builder)
    {
        builder.ToTable("Moods");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.Description)
               .HasColumnType("text");

        builder.Property(x => x.IsActive)
               .IsRequired()
               .HasDefaultValue(true);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}
