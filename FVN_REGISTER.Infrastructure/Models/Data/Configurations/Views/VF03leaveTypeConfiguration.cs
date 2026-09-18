using FVN_REGISTER.Core.Entities.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Views;

public sealed class VF03leaveTypeConfiguration : IEntityTypeConfiguration<VF03leaveType>
{
    public void Configure(EntityTypeBuilder<VF03leaveType> entity)
    {
        entity.HasNoKey().ToView("vF03leaveType");

        entity.Property(e => e.LeaveTypeCode).HasMaxLength(10);
        entity.Property(e => e.LeaveTypeName).HasMaxLength(200);
        entity.Property(e => e.LeaveTypeName2).HasMaxLength(200);
        entity.Property(e => e.CountedAsLeaveName).HasMaxLength(100);
        entity.Property(e => e.Hrmcode).HasMaxLength(50);

        entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
    }
}
