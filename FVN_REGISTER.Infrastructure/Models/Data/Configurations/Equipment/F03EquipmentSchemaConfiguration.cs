using FVN_REGISTER.Core.Entities.Equipment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Equipment;

public sealed class F03EquipmentSchemaConfiguration : IEntityTypeConfiguration<F03EquipmentSchema>
{
    public void Configure(EntityTypeBuilder<F03EquipmentSchema> b)
    {
        b.ToTable("F03EquipmentSchemas");
        b.HasKey(x => x.Id);
        b.Property(x => x.DeptCode).HasMaxLength(20).IsRequired();
        b.Property(x => x.SchemaName).HasMaxLength(150).IsRequired();
        b.Property(x => x.Version).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.IsActive).HasDefaultValue(true);

        b.HasIndex(x => new { x.DeptCode, x.Version })
            .IsUnique()
            .HasDatabaseName("UX_F03EquipmentSchemas_Dept_Version");

        b.HasIndex(x => new { x.DeptCode, x.Status })
            .HasDatabaseName("IX_F03EquipmentSchemas_Dept_Status");
    }
}
