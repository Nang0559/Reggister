

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FVN_REGISTER.Infrastructure.Models.Entities.Views;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Views
{
    public class VF03OTRequestConfiguration : IEntityTypeConfiguration<VF03OTRequest>
    {
        public void Configure(EntityTypeBuilder<VF03OTRequest> entity)
        {
            entity.HasNoKey().ToView("vF03OTRequest"); // ⚠️ đổi lại đúng tên view SQL thật

            entity.Property(e => e.OTCode).HasMaxLength(50);

            entity.Property(e => e.EmployeeCode).HasMaxLength(50);
            entity.Property(e => e.EmployeeName).HasMaxLength(50);
            entity.Property(e => e.CreatedByEmail).HasMaxLength(100);

            entity.Property(e => e.DeptCode).HasMaxLength(30);
            entity.Property(e => e.DeptName).HasMaxLength(64);

            entity.Property(e => e.OTDate).HasColumnType("smalldatetime");

            entity.Property(e => e.PlannedHours).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TotalOTHours).HasColumnType("decimal(5, 2)");

            entity.Property(e => e.OTTypeCode).HasMaxLength(10);
            entity.Property(e => e.OTTypeName).HasMaxLength(200);
            entity.Property(e => e.RateMultiplier).HasColumnType("decimal(4, 2)");

            entity.Property(e => e.OTReasonSummary).HasMaxLength(500);
            entity.Property(e => e.ScopeType).HasMaxLength(20);
            entity.Property(e => e.RequestStatus).HasMaxLength(50);

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        }
    }
}
