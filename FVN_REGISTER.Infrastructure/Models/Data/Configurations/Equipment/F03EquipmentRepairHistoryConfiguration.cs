using FVN_REGISTER.Core.Entities.Equipment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Equipment;

public sealed class F03EquipmentRepairHistoryConfiguration : IEntityTypeConfiguration<F03EquipmentRepairHistory>
{
    public void Configure(EntityTypeBuilder<F03EquipmentRepairHistory> b)
    {
        b.ToTable("F03EquipmentRepairHistory"); b.HasKey(x => x.Id);
        b.Property(x => x.RepairContent).HasMaxLength(1000).IsRequired(); b.Property(x => x.RepairVendor).HasMaxLength(250);
        b.Property(x => x.RepairResult).HasMaxLength(1000); b.Property(x => x.Note).HasMaxLength(1000);
        b.Property(x => x.RepairCost).HasColumnType("decimal(18,2)"); b.HasIndex(x => new { x.AssetId, x.RepairDate });
        b.HasIndex(x => x.RequestId).IsUnique(); b.Property(x => x.IsApproved).HasDefaultValue(false);
    }
}
