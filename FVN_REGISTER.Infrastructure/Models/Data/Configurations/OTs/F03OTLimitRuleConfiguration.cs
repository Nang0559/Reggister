using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.OTs;

public sealed class F03OTLimitRuleConfiguration : IEntityTypeConfiguration<F03OTLimitRule>
{
    public void Configure(EntityTypeBuilder<F03OTLimitRule> entity)
    {
        entity.ToTable("F03OTLimitRules");
        entity.HasKey(e => e.Id).HasName("PK_F03OTLimitRules");

        entity.HasIndex(e => new { e.LimitType, e.PositionCode, e.DeptCode }, "IX_OTLimitRule_Lookup");

        entity.Property(e => e.LimitType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        entity.Property(e => e.PositionCode).HasMaxLength(20);
        entity.Property(e => e.DeptCode).HasMaxLength(20);
        entity.Property(e => e.Description).HasMaxLength(500);

        entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        entity.Property(e => e.IsActive).HasDefaultValue(true);
    }
}
