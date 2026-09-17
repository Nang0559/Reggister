using FVN_REGISTER.Core.Entities.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Views
{
    public class VF03OTSummaryConfiguration : IEntityTypeConfiguration<VF03OTSummary>
    {
        public void Configure(EntityTypeBuilder<VF03OTSummary> entity)
        {
            entity.HasNoKey().ToView("vF03OTSummary"); // ⚠️ đổi lại đúng tên view SQL thật

            entity.Property(e => e.EmployeeCode).HasMaxLength(50);
            entity.Property(e => e.EmployeeName).HasMaxLength(50);

            entity.Property(e => e.DeptCode).HasMaxLength(30);
            entity.Property(e => e.DeptName).HasMaxLength(64);

            entity.Property(e => e.TotalApprovedHours).HasColumnType("decimal(7, 2)");
            entity.Property(e => e.TotalPendingHours).HasColumnType("decimal(7, 2)");

            // TotalHours là computed property (=> trong C#), KHÔNG map vào EF Core
            entity.Ignore(e => e.TotalHours);
        }
    }
}