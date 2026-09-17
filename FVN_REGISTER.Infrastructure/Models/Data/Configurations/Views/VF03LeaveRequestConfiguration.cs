using FVN_REGISTER.Core.Entities.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Views;

public sealed class VF03LeaveRequestConfiguration : IEntityTypeConfiguration<VF03LeaveRequest>
{
    public void Configure(EntityTypeBuilder<VF03LeaveRequest> entity)
    {
        entity.HasNoKey().ToView("vF03LeaveRequest");

        entity.Property(e => e.EmployeeCode).HasMaxLength(50);
        entity.Property(e => e.EmployeeName).HasMaxLength(50);
        entity.Property(e => e.GenderName).HasMaxLength(50);
        entity.Property(e => e.DeptCode).HasMaxLength(30);
        entity.Property(e => e.DeptName).HasMaxLength(64);
        entity.Property(e => e.EmailAddress).HasMaxLength(100);

        entity.Property(e => e.RegisterDate).HasColumnType("smalldatetime");
        entity.Property(e => e.StartDate).HasColumnType("smalldatetime");
        entity.Property(e => e.EndDate).HasColumnType("smalldatetime");
        entity.Property(e => e.TotalDay).HasColumnType("decimal(5, 2)");
        entity.Property(e => e.TotalLeaveDay).HasColumnType("decimal(5, 2)");
        entity.Property(e => e.LeaveTypeCode).HasMaxLength(10);
        entity.Property(e => e.LeaveTypeName).HasMaxLength(200);
        entity.Property(e => e.LeaveReason).HasMaxLength(500);
        entity.Property(e => e.RequestStatus).HasConversion<string>().HasMaxLength(50);
        entity.Property(e => e.PositionCode).HasMaxLength(30).HasColumnName("CVCode");
        entity.Property(e => e.PositionName).HasMaxLength(64).HasColumnName("CVName");
        entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
    }
}
