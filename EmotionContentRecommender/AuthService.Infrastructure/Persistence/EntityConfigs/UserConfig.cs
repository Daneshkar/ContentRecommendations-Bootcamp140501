using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Persistence.EntityConfigs;

public class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Username)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.Email)
               .HasMaxLength(255);

        builder.Property(x => x.Mobile)
               .HasMaxLength(20);

        builder.Property(x => x.PasswordHash)
               .IsRequired();

        builder.Property(x => x.Role)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.Avatar)
               .HasColumnType("nvarchar(max)");

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.HasIndex(x => x.Username).IsUnique();
        builder.HasIndex(x => x.Email)
               .IsUnique()
               .HasFilter("[Email] IS NOT NULL");
    }
}
