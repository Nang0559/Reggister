using FVN_REGISTER.Core.Entities.Equipment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Equipment;

public sealed class F03EquipmentAssetConfiguration : IEntityTypeConfiguration<F03EquipmentAsset>
{
    public void Configure(EntityTypeBuilder<F03EquipmentAsset> b)
    {
        b.ToTable("F03EquipmentAssets"); b.HasKey(x => x.Id);
        b.Property(x => x.EquipmentCode).HasMaxLength(30).IsRequired();
        b.Property(x => x.EquipmentName).HasMaxLength(250).IsRequired();
        b.Property(x => x.Specification).HasMaxLength(1000); b.Property(x => x.SerialNumber).HasMaxLength(100);
        b.Property(x => x.AssetCode).HasMaxLength(50); b.Property(x => x.PurchasePrice).HasColumnType("decimal(18,2)");
        b.Property(x => x.DeptCode).HasMaxLength(20).IsRequired(); b.Property(x => x.Location).HasMaxLength(250);
        b.Property(x => x.QrToken).HasMaxLength(128).IsRequired(); b.Property(x => x.Note).HasMaxLength(1000);
        b.HasIndex(x => x.EquipmentCode).IsUnique(); b.HasIndex(x => x.QrToken).IsUnique(); b.HasIndex(x => x.DeptCode);
        b.Property(x => x.IsQrActive).HasDefaultValue(false); b.Property(x => x.IsActive).HasDefaultValue(true);
        b.HasMany(x => x.RepairHistory).WithOne(x => x.Asset).HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Requests).WithOne(x => x.Asset).HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);
    }
}
