using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Persistence.EntityConfigs;

public class OtpCodeConfig : IEntityTypeConfiguration<OtpCode>
{
    public void Configure(EntityTypeBuilder<OtpCode> builder)
    {
        builder.ToTable("OtpCodes");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
               .UseIdentityColumn();

        builder.Property(x => x.Mobile)
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(x => x.Code)
               .HasMaxLength(10)
               .IsRequired();

        builder.Property(x => x.ExpiresAt)
               .IsRequired();

        builder.Property(x => x.IsUsed)
               .IsRequired()
               .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.HasIndex(x => x.Mobile);
        builder.HasIndex(x => x.Code);
    }
}
