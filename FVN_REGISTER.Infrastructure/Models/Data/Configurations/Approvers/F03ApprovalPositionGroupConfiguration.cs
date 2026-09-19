using FVN_REGISTER.Core.Entities.Approvers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Approvers;

public sealed class F03ApprovalPositionGroupConfiguration : IEntityTypeConfiguration<F03ApprovalPositionGroup>
{
    public void Configure(EntityTypeBuilder<F03ApprovalPositionGroup> entity)
    {
        entity.ToTable("F03ApprovalPositionGroups");
        entity.HasKey(e => e.Id);

        entity.HasIndex(e => e.PositionCode, "UX_F03ApprovalPositionGroups_PositionCode")
            .IsUnique();

        entity.Property(e => e.PositionCode).IsRequired().HasMaxLength(20);
        entity.Property(e => e.ApprovalGroupCode).IsRequired().HasMaxLength(30);
        entity.Property(e => e.ApprovalGroupName).IsRequired().HasMaxLength(100);
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        entity.Property(e => e.IsActive).HasDefaultValue(true);
    }
}
