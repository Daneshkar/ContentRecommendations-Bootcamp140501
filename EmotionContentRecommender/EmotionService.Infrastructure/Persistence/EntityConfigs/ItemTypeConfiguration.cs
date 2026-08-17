using EmotionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class ItemTypeConfiguration
    : IEntityTypeConfiguration<ItemType>
{
    public void Configure(EntityTypeBuilder<ItemType> builder)
    {
        builder.ToTable("ItemTypes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.HasData(
           new
           {
               Id = 1,
               Name = "Movie",
               IsActive = true
           },
           new
           {
               Id = 2,
               Name = "Game",
               IsActive = true
           },
           new
           {
               Id = 3,
               Name = "Book",
               IsActive = true
           },
           new
           {
               Id = 4,
               Name = "Music",
               IsActive = true
           }
       );
    }
}