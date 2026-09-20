using FVN_REGISTER.Core.Entities.Equipment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Equipment;

public sealed class F03EquipmentImportBatchConfiguration : IEntityTypeConfiguration<F03EquipmentImportBatch>
{
    public void Configure(EntityTypeBuilder<F03EquipmentImportBatch> b)
    {
        b.ToTable("F03EquipmentImportBatches");
        b.HasKey(x => x.Id);

        b.Property(x => x.DeptCode).HasMaxLength(20).IsRequired();
        b.Property(x => x.FileName).HasMaxLength(260).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.IsActive).HasDefaultValue(true);
        b.Property(x => x.CreatedAt).HasColumnType("datetime2(0)");
        b.Property(x => x.ModifiedAt).HasColumnType("datetime2(0)");
    }
}