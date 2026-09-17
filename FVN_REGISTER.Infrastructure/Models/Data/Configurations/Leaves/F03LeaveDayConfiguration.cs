
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Leaves
{
    public class F03LeaveDayConfiguration : IEntityTypeConfiguration<F03LeaveDay>
    {
        public void Configure(EntityTypeBuilder<F03LeaveDay> entity)
        {
            entity.ToTable("F03LeaveDays");
            entity.HasKey(e => e.Id).HasName("PK_F03LeaveDays");

            // Index để tăng tốc độ load lịch
            entity.HasIndex(e => e.StartTime, "IX_F03LeaveDay_StartTime");
            entity.HasIndex(e => e.EmployeeCode, "IX_F03LeaveDay_EmployeeCode");

            // Properties
            entity.Property(e => e.EmployeeCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LeaveReason).HasMaxLength(500).IsRequired();
            entity.Property(e => e.TotalDay).HasColumnType("decimal(5,2)");
            entity.Property(e => e.TotalLeaveDay).HasColumnType("decimal(5,2)");
            entity.Property(e => e.LeaveTypeCode).HasMaxLength(10);

            // Enum -> Int
            entity.Property(e => e.RequestStatus)
                  .HasConversion<int>()
                  .HasDefaultValue(ApprovalStatus.Draft);

            // Audit & Defaults
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
        }
    }
}
