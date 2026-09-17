using FVN_REGISTER.Infrastructure.Models.Entities.OT;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.OTs
{
    public class F03OTEmployeeConfiguration : IEntityTypeConfiguration<F03OTEmployee>
    {
        public void Configure(EntityTypeBuilder<F03OTEmployee> entity)
        {
            entity.ToTable("F03OTeEmployees");
            entity.HasKey(e => e.Id).HasName("PK_F03OTeEmployees");

            // Index cho performance
            entity.HasIndex(e => e.OTRequestId, "IX_F03OTEmployee_RequestId");
            entity.HasIndex(e => e.EmployeeCode, "IX_F03OTEmployee_EmployeeCode");

            // Constraints
            entity.Property(e => e.EmployeeCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.OtRateMultiplier).HasDefaultValue(1.5m);

            // Audit & Defaults
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            // Mối quan hệ
            entity.HasOne(d => d.OvertimeRequest)
                  .WithMany(p => p.Employees)
                  .HasForeignKey(d => d.OTRequestId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
