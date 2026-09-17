
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Leaves
{
    public class F03AttendanceStagingConfiguration : IEntityTypeConfiguration<F03AttendanceStaging>
    {
        public void Configure(EntityTypeBuilder<F03AttendanceStaging> entity)
        {
            entity.ToTable("F03AttendanceStaging");
            entity.HasKey(e => e.Id);

            // Index cực kỳ quan trọng cho báo cáo và đồng bộ
            entity.HasIndex(e => e.WorkDate, "IX_F03AttendanceStaging_WorkDate");
            entity.HasIndex(e => e.EmployeeCode, "IX_F03AttendanceStaging_EmployeeCode");

            // Các thiết lập độ dài để tối ưu không gian lưu trữ
            entity.Property(e => e.EmployeeCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SyncedAt).HasDefaultValueSql("(getdate())");
        }
    }
}
