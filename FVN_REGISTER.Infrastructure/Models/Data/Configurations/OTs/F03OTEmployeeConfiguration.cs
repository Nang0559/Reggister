using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.OTs;

public sealed class F03OTEmployeeConfiguration : IEntityTypeConfiguration<F03OTEmployee>
{
    public void Configure(EntityTypeBuilder<F03OTEmployee> entity)
    {
        entity.ToTable("F03OTEmployees");
        entity.HasKey(e => e.Id).HasName("PK_F03OTEmployees");

        entity.HasIndex(e => e.OTRequestId, "IX_F03OTEmployee_RequestId");
        entity.HasIndex(e => e.EmployeeCode, "IX_F03OTEmployee_EmployeeCode");

        entity.Property(e => e.EmployeeCode).HasMaxLength(50).IsRequired();
        entity.Property(e => e.OTRateMultiplier).HasDefaultValue(1.5m);

        entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        entity.Property(e => e.IsActive).HasDefaultValue(true);

        entity.HasOne(d => d.OTRequest)
            .WithMany(p => p.Employees)
            .HasForeignKey(d => d.OTRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
