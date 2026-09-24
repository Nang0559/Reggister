using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.OTs;

public sealed class F03OTLimitRuleConfiguration
    : IEntityTypeConfiguration<F03OTLimitRule>
{
    public void Configure(EntityTypeBuilder<F03OTLimitRule> entity)
    {
        entity.ToTable("F03OTLimitRules");

        entity.HasKey(e => e.Id)
            .HasName("PK_F03OTLimitRules");

        entity.HasIndex(
                e => new
                {
                    e.LimitType,
                    e.ScopeType,
                    e.ScopeCode,
                    e.EmployeeCode,
                    e.PositionCode,
                    e.DeptCode
                },
                "IX_OTLimitRule_Lookup");

        entity.Property(e => e.LimitType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // ScopeType:
        // Employee   = 1
        // Department = 2
        // Block      = 3
        entity.Property(e => e.ScopeType)
            .HasConversion<int>()
            .HasDefaultValue(OTLimitScopeType.Employee)
            // CLR default 0 means "not specified"; Employee=1 is a valid explicit value.
            .HasSentinel((OTLimitScopeType)0)
            .IsRequired();

        entity.Property(e => e.ScopeCode)
            .HasMaxLength(50);

        entity.Property(e => e.EmployeeCode)
            .HasMaxLength(50);

        entity.Property(e => e.PositionCode)
            .HasMaxLength(20);

        entity.Property(e => e.DeptCode)
            .HasMaxLength(20);

        entity.Property(e => e.Description)
            .HasMaxLength(500);

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("(getdate())");

        entity.Property(e => e.IsActive)
            .HasDefaultValue(true);
    }
}
