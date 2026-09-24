using FVN_REGISTER.Core.Entities.Equipment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Equipment;

public sealed class F03EquipmentAssignmentHistoryConfiguration : IEntityTypeConfiguration<F03EquipmentAssignmentHistory>
{
    public void Configure(EntityTypeBuilder<F03EquipmentAssignmentHistory> b)
    {
        b.ToTable("F03EquipmentAssignmentHistory");
        b.HasKey(x => x.Id);
        b.Property(x => x.PreviousResponsibleEmployeeCode).HasMaxLength(50);
        b.Property(x => x.NewResponsibleEmployeeCode).HasMaxLength(50);
        b.Property(x => x.PreviousApproverEmployeeCode).HasMaxLength(50);
        b.Property(x => x.NewApproverEmployeeCode).HasMaxLength(50);
        b.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        b.HasIndex(x => x.AssetId);
        b.HasIndex(x => x.HandoverAt);
        b.HasOne(x => x.Asset).WithMany(x => x.AssignmentHistory)
            .HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);
    }
}
