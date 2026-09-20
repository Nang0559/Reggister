using FVN_REGISTER.Core.Entities.Equipment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Equipment;

public sealed class F03EquipmentImportRowConfiguration : IEntityTypeConfiguration<F03EquipmentImportRow>
{
    public void Configure(EntityTypeBuilder<F03EquipmentImportRow> b)
    {
        b.ToTable("F03EquipmentImportRows");
        b.HasKey(x => x.Id);

        b.Property(x => x.RawJson).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.ErrorMessage).HasMaxLength(2000);
        b.Property(x => x.IsActive).HasDefaultValue(true);
        b.Property(x => x.CreatedAt).HasColumnType("datetime2(0)");
        b.Property(x => x.ModifiedAt).HasColumnType("datetime2(0)");

        b.HasIndex(x => new { x.BatchId, x.Status })
            .HasDatabaseName("IX_F03EquipmentImportRows_Batch_Status");

        b.HasOne<F03EquipmentImportBatch>()
            .WithMany()
            .HasForeignKey(x => x.BatchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}