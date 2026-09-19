using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Securities;

public sealed class F03UserConfiguration : IEntityTypeConfiguration<F03User>
{
    public void Configure(EntityTypeBuilder<F03User> entity)
    {
        entity.ToTable("F03Users");
        entity.HasKey(e => e.Id);

        entity.HasIndex(e => e.EmployeeCode, "IX_User_EmployeeCode").IsUnique();

        entity.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(50);
        entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
        entity.Property(e => e.FullName).HasMaxLength(100);
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        entity.Property(e => e.IsActive).HasDefaultValue(true);
    }
}
