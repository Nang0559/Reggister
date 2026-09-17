using FVN_REGISTER.Core.Entities.Equipment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Equipment;

public sealed class F03EquipmentRequestConfiguration : IEntityTypeConfiguration<F03EquipmentRequest>
{
    public void Configure(EntityTypeBuilder<F03EquipmentRequest> b)
    {
        b.ToTable("F03EquipmentRequests"); b.HasKey(x => x.Id);
        b.Property(x => x.RequestKind).HasConversion<int>(); b.Property(x => x.RequestStatus).HasConversion<int>().HasDefaultValue(Core.Enums.ApprovalStatus.Draft);
        b.Property(x => x.EmployeeCode).HasMaxLength(50).IsRequired(); b.Property(x => x.DeptCode).HasMaxLength(20).IsRequired();
        b.Property(x => x.SelectedApproverCode).HasMaxLength(50).IsRequired(); b.Property(x => x.QrToken).HasMaxLength(128).IsRequired();
        b.Property(x => x.EquipmentName).HasMaxLength(250).IsRequired(); b.Property(x => x.Specification).HasMaxLength(1000);
        b.Property(x => x.SerialNumber).HasMaxLength(100); b.Property(x => x.AssetCode).HasMaxLength(50);
        b.Property(x => x.PurchasePrice).HasColumnType("decimal(18,2)"); b.Property(x => x.Location).HasMaxLength(250); b.Property(x => x.Note).HasMaxLength(1000);
        b.Property(x => x.RepairContent).HasMaxLength(1000); b.Property(x => x.RepairVendor).HasMaxLength(250); b.Property(x => x.RepairCost).HasColumnType("decimal(18,2)"); b.Property(x => x.RepairResult).HasMaxLength(1000);
        b.HasIndex(x => new { x.EmployeeCode, x.CreatedAt }); b.HasIndex(x => x.QrToken); b.HasIndex(x => new { x.RequestKind, x.RequestStatus });
        b.HasOne(x => x.Asset).WithMany(x => x.Requests).HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);
    }
}
