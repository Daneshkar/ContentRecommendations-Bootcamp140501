using EmotionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmotionService.Infrastructure.Persistence.EntityConfigs;

public sealed class ExperienceThemeConfiguration
    : IEntityTypeConfiguration<ExperienceTheme>
{
    public void Configure(EntityTypeBuilder<ExperienceTheme> builder)
    {
        builder.ToTable(
            "ExperienceThemes",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_ExperienceThemes_UserWeight",
                    "[UserWeight] >= 1.00 AND [UserWeight] <= 5.00");
            });

        builder.HasKey(x => new
        {
            x.ExperienceId,
            x.ThemeId
        });

        builder.Property(x => x.UserWeight)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.HasOne(x => x.Experience)
            .WithMany(x => x.ExperienceThemes)
            .HasForeignKey(x => x.ExperienceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Theme)
            .WithMany()
            .HasForeignKey(x => x.ThemeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ThemeId);
    }
}