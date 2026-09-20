using FVN_REGISTER.Core.Entities.WorkCalendar;
using FVN_REGISTER.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.WorkCalendar;

public sealed class F03CalendarModulePolicyConfiguration : IEntityTypeConfiguration<F03CalendarModulePolicy>
{
    public void Configure(EntityTypeBuilder<F03CalendarModulePolicy> entity)
    {
        entity.ToTable("F03CalendarModulePolicies");
        entity.HasKey(x => x.Id);
        entity.HasIndex(x => x.ModuleCode).IsUnique();
        entity.Property(x => x.ModuleCode).HasMaxLength(50).IsRequired();
        entity.Property(x => x.DisplayMode).HasConversion<byte>().HasDefaultValue(CalendarDisplayMode.MarkerAndSummary);
        entity.Property(x => x.NoteMode).HasConversion<byte>().HasDefaultValue(CalendarNoteMode.AlertsOnly);
        entity.Property(x => x.ConfirmationMode).HasConversion<byte>().HasDefaultValue(CalendarConfirmationMode.None);
        entity.Property(x => x.ReconciliationMode).HasConversion<byte>().HasDefaultValue(CalendarReconciliationMode.None);
        entity.Property(x => x.SummaryTemplate).HasMaxLength(500);
        entity.Property(x => x.DetailTemplate).HasMaxLength(500);
    }
}
