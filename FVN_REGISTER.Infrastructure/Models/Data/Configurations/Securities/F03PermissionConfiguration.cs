using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Securities;

public sealed class F03PermissionConfiguration : IEntityTypeConfiguration<F03Permission>
{
    public void Configure(EntityTypeBuilder<F03Permission> entity)
    {
        entity.ToTable("F03Permissions");
        entity.HasKey(e => e.Id);

        entity.HasIndex(e => e.PermissionCode, "IX_Permission_Code").IsUnique();

        entity.Property(e => e.PermissionName).IsRequired().HasMaxLength(100);
        entity.Property(e => e.Detail).HasMaxLength(500);
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        entity.Property(e => e.IsActive).HasDefaultValue(true);
    }
}
