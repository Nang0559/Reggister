using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FVN_REGISTER.Infrastructure.Models.Entities.Views;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Views
{
    public class VF03OTRequestDetailConfiguration : IEntityTypeConfiguration<VF03OTRequestDetail>
    {
        public void Configure(EntityTypeBuilder<VF03OTRequestDetail> entity)
        {
            entity.HasNoKey().ToView("vF03OTRequestDetail"); // ⚠️ đổi lại đúng tên view SQL thật

            entity.Property(e => e.OTCode).HasMaxLength(50);

            entity.Property(e => e.EmployeeCode).HasMaxLength(50);
            entity.Property(e => e.EmployeeName).HasMaxLength(50);

            entity.Property(e => e.DeptCode).HasMaxLength(30);
            entity.Property(e => e.DeptName).HasMaxLength(64);

            entity.Property(e => e.StartTime).HasColumnType("time");
            entity.Property(e => e.EndTime).HasColumnType("time");
            entity.Property(e => e.ActualStartTime).HasColumnType("time");
            entity.Property(e => e.ActualEndTime).HasColumnType("time");

            entity.Property(e => e.OTHours).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.ActualHours).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.OTRateMultiplier).HasColumnType("decimal(4, 2)");

            entity.Property(e => e.CvCode).HasMaxLength(30).HasColumnName("CVCode");
            entity.Property(e => e.EmpOTTypeCode).HasMaxLength(10);

            entity.Property(e => e.ValidationStatus).HasMaxLength(20);
            entity.Property(e => e.ValidationMessage).HasMaxLength(500);
            entity.Property(e => e.Note).HasMaxLength(500);

            entity.Property(e => e.OTReasonCategoryCode).HasMaxLength(10);
            entity.Property(e => e.OTReasonDetail).HasMaxLength(500);

            entity.Property(e => e.RequestStatus).HasMaxLength(50);

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        }
    }
}