using FVN_REGISTER.Core.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Security;

public sealed class F03HrmUserRoleRuleConfiguration : IEntityTypeConfiguration<F03HrmUserRoleRule>
{
    public void Configure(EntityTypeBuilder<F03HrmUserRoleRule> b)
    {
        b.ToTable("F03HrmUserRoleRules");
        b.HasKey(x => x.Id);

        b.Property(x => x.DeptCode).HasMaxLength(20);
        b.Property(x => x.PositionCode).HasMaxLength(20);
        b.Property(x => x.Note).HasMaxLength(500);
        b.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        b.Property(x => x.CreatedAt).HasColumnType("datetime2(0)");
        b.Property(x => x.ModifiedAt).HasColumnType("datetime2(0)");
    }
}