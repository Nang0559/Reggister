using FVN_REGISTER.Core.Entities.WorkCalendar;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.WorkCalendar;

public sealed class F03ActionPolicyConfiguration : IEntityTypeConfiguration<F03ActionPolicy>
{
    public void Configure(EntityTypeBuilder<F03ActionPolicy> entity)
    {
        entity.ToTable("F03ActionPolicies");
        entity.HasKey(x => x.Id);
        entity.HasIndex(x => new { x.ModuleCode, x.ActionType }).IsUnique();
        entity.Property(x => x.ModuleCode).HasMaxLength(50).IsRequired();
        entity.Property(x => x.ActionType).HasMaxLength(100).IsRequired();
        entity.Property(x => x.TitleTemplate).HasMaxLength(200);
        entity.Property(x => x.SummaryTemplate).HasMaxLength(1000);
    }
}
