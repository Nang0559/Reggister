using FVN_REGISTER.Core.Entities.WorkCalendar;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.WorkCalendar;
public sealed class F03ExecutionConfirmationConfiguration : IEntityTypeConfiguration<F03ExecutionConfirmation>
{
    public void Configure(EntityTypeBuilder<F03ExecutionConfirmation> b)
    {
        b.ToTable("F03ExecutionConfirmations"); b.HasKey(x=>x.Id);
        b.Property(x=>x.ModuleCode).HasMaxLength(50).IsRequired(); b.Property(x=>x.SourceType).HasMaxLength(50).IsRequired(); b.Property(x=>x.ParticipantId).HasMaxLength(100); b.Property(x=>x.SourceId).HasMaxLength(100).IsRequired();
        b.Property(x=>x.Decision).HasMaxLength(50); b.Property(x=>x.Status).HasMaxLength(50).IsRequired();
        b.Property(x=>x.Comment).HasMaxLength(2000); b.Property(x=>x.ReviewNote).HasMaxLength(2000);
        b.HasIndex(x=>x.ReconciliationId).IsUnique(); b.HasIndex(x=>new{x.EmployeeId,x.Status,x.WorkDate});
    }
}