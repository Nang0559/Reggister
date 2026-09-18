using FVN_REGISTER.Core.Entities.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Views;

public sealed class VF03LeaveBalanceConfiguration : IEntityTypeConfiguration<VF03LeaveBalance>
{
    public void Configure(EntityTypeBuilder<VF03LeaveBalance> entity)
    {
        entity.HasNoKey().ToView("vF03LeaveBalance");

        entity.Property(e => e.EmployeeCode).HasMaxLength(50);
        entity.Property(e => e.EmployeeName).HasMaxLength(50);
        entity.Property(e => e.DeptCode).HasMaxLength(30);
        entity.Property(e => e.DeptName).HasMaxLength(64);
        entity.Property(e => e.GenderName).HasMaxLength(50);

        entity.Property(e => e.TotalEntitledLeave).HasColumnType("decimal(18, 2)");
        entity.Property(e => e.TotalDaysOff).HasColumnType("decimal(18, 2)");
        entity.Property(e => e.LeaveDaysUsed).HasColumnType("decimal(18, 2)");
        entity.Property(e => e.RemainingLeave).HasColumnType("decimal(18, 2)");

        entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
    }
}
