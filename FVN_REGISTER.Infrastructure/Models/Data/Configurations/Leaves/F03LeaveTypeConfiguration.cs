using FVN_REGISTER.Core.Entities.Leaves;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Leaves;

public sealed class F03LeaveTypeConfiguration : IEntityTypeConfiguration<F03LeaveType>
{
    public void Configure(EntityTypeBuilder<F03LeaveType> entity)
    {
        entity.ToTable("F03LeaveType");
        entity.HasKey(e => e.Id).HasName("PK_F03LeaveType");
        entity.HasIndex(e => e.LeaveTypeCode, "IX_F03LeaveType_Code").IsUnique();

        entity.Property(e => e.LeaveTypeCode).HasMaxLength(50).IsRequired();
        entity.Property(e => e.LeaveTypeName).HasMaxLength(200).IsRequired();
        entity.Property(e => e.LeaveTypeName2).HasMaxLength(200);
        entity.Property(e => e.HRMCode).HasMaxLength(50).HasColumnName("HRMCode");
        entity.Property(e => e.IsCountedAsLeave).HasDefaultValue(false);

        entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        entity.Property(e => e.IsActive).HasDefaultValue(true);
    }
}
