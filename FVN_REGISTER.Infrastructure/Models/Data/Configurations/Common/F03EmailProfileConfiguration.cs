using FVN_REGISTER.Core.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Common
{
    public class F03EmailProfileConfiguration : IEntityTypeConfiguration<F03EmailProfile>
    {
        public void Configure(EntityTypeBuilder<F03EmailProfile> entity)
        {
            entity.ToTable("F03EmailProfiles");
            entity.HasKey(e => e.Id).HasName("PK_F03EmailProfiles");

            entity.HasIndex(e => e.Code, "IX_EmailProfile_Code").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EmailServerName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EmailAddress).HasMaxLength(100).IsRequired();

            // Cấu hình concurrency token bằng Timestamp
            entity.Property(e => e.Timestamp).IsRowVersion();

            // Audit defaults
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        }
    }
}
