using EmotionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmotionService.Infrastructure.Persistence.EntityConfigs;

public sealed class ExperienceMoodConfiguration
    : IEntityTypeConfiguration<ExperienceMood>
{
    public void Configure(EntityTypeBuilder<ExperienceMood> builder)
    {
        builder.ToTable(
            "ExperienceMoods",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_ExperienceMoods_UserWeight",
                    "[UserWeight] >= 1.00 AND [UserWeight] <= 5.00");
            });

        builder.HasKey(x => new
        {
            x.ExperienceId,
            x.MoodId
        });

        builder.Property(x => x.UserWeight)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.HasOne(x => x.Experience)
            .WithMany(x => x.ExperienceMoods)
            .HasForeignKey(x => x.ExperienceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Mood)
            .WithMany()
            .HasForeignKey(x => x.MoodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.MoodId);
    }
}