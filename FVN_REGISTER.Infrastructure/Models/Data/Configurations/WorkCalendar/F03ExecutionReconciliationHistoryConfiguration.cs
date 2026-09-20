using FVN_REGISTER.Core.Entities.WorkCalendar;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.WorkCalendar;

public sealed class F03ExecutionReconciliationHistoryConfiguration : IEntityTypeConfiguration<F03ExecutionReconciliationHistory>
{
    public void Configure(EntityTypeBuilder<F03ExecutionReconciliationHistory> b)
    {
        b.ToTable("F03ExecutionReconciliationHistory");
        b.HasKey(x => x.Id);
        b.Property(x => x.FromStatus).HasMaxLength(50);
        b.Property(x => x.ToStatus).HasMaxLength(50).IsRequired();
        b.Property(x => x.EventType).HasMaxLength(100).IsRequired();
        b.Property(x => x.Reason).HasMaxLength(2000);
        b.HasIndex(x => new { x.ReconciliationId, x.CreatedAt });
    }
}