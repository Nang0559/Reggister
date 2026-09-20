using FVN_REGISTER.Core.Entities.WorkCalendar;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.WorkCalendar;
public sealed class F03ExecutionReconciliationConfiguration : IEntityTypeConfiguration<F03ExecutionReconciliation>
{
    public void Configure(EntityTypeBuilder<F03ExecutionReconciliation> b)
    {
        b.ToTable("F03ExecutionReconciliations"); b.HasKey(x=>x.Id);
        b.Property(x=>x.ModuleCode).HasMaxLength(50).IsRequired(); b.Property(x=>x.SourceType).HasMaxLength(50).IsRequired(); b.Property(x=>x.ParticipantId).HasMaxLength(100);
        b.Property(x=>x.SourceId).HasMaxLength(100).IsRequired();
        b.Property(x=>x.PlannedState).HasMaxLength(50); b.Property(x=>x.ActualState).HasMaxLength(50);
        b.Property(x=>x.ReconciliationStatus).HasMaxLength(50).IsRequired(); b.Property(x=>x.DetailJson).HasColumnType("nvarchar(max)");
        b.HasIndex(x=>new{x.ModuleCode,x.SourceType,x.SourceId,x.ParticipantId,x.EmployeeId,x.WorkDate}).IsUnique();
        b.HasIndex(x=>new{x.EmployeeId,x.WorkDate,x.ReconciliationStatus}); b.HasIndex(x=>x.ActionId);
    }
}