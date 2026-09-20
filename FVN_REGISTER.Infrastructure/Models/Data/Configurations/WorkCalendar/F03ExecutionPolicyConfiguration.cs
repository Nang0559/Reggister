using FVN_REGISTER.Core.Entities.WorkCalendar;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.WorkCalendar;
public sealed class F03ExecutionPolicyConfiguration : IEntityTypeConfiguration<F03ExecutionPolicy>
{
    public void Configure(EntityTypeBuilder<F03ExecutionPolicy> b)
    {
        b.ToTable("F03ExecutionPolicies"); b.HasKey(x=>x.Id); b.Property(x=>x.ModuleCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.LastModifiedSource).HasMaxLength(50);
        b.HasIndex(x=>x.ModuleCode).IsUnique();
    }
}