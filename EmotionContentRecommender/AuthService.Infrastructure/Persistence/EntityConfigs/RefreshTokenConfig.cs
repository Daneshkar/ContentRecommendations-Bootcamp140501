using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Persistence.EntityConfigs;

public class RefreshTokenConfig : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
               .UseIdentityColumn();

        builder.Property(x => x.Token)
               .HasMaxLength(500)
               .IsRequired();

        builder.Property(x => x.ExpiresAt).IsRequired();

        builder.Property(x => x.IsRevoked)
               .IsRequired()
               .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);

        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Token).IsUnique();
        builder.HasIndex(x => x.UserId);
    }
}
