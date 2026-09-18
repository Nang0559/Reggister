using FVN_REGISTER.Core.Entities.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Views;

public sealed class VF03LeaveRequestDetailConfiguration : IEntityTypeConfiguration<VF03LeaveRequestDetail>
{
    public void Configure(EntityTypeBuilder<VF03LeaveRequestDetail> entity)
    {
        // This entity maps to a SQL view and does not have a real EF primary key.
        // RowId is a view-generated row identifier, not a persisted entity key.
        entity.HasNoKey().ToView("vF03LeaveRequestDetail");

        entity.Property(e => e.EmployeeCode).HasMaxLength(50);
        entity.Property(e => e.EmployeeName).HasMaxLength(50);
        entity.Property(e => e.DeptCode).HasMaxLength(30);
        entity.Property(e => e.DeptName).HasMaxLength(64);

        entity.Property(e => e.RequestStatus)
            .HasConversion<string>()
            .HasMaxLength(50);

        entity.Property(e => e.StartDate).HasColumnType("smalldatetime");
        entity.Property(e => e.EndDate).HasColumnType("smalldatetime");
        entity.Property(e => e.LeaveDate).HasColumnType("date");

        entity.Property(e => e.LeaveTypeCode).HasMaxLength(10);
        entity.Property(e => e.LeaveTypeName).HasMaxLength(200);
        entity.Property(e => e.HalfDayOption).HasMaxLength(20);

        entity.Property(e => e.DayValue).HasColumnType("decimal(5, 2)");
        entity.Property(e => e.DetailCreatedAt).HasColumnType("datetime");
    }
}
