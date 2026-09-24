using FVN_REGISTER.Core.Entities.Equipment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Equipment;

public sealed class F03EquipmentFieldDefinitionConfiguration : IEntityTypeConfiguration<F03EquipmentFieldDefinition>
{
    public void Configure(EntityTypeBuilder<F03EquipmentFieldDefinition> b)
    {
        b.ToTable("F03EquipmentFieldDefinitions");
        b.HasKey(x => x.Id);
        b.Property(x => x.SchemaId).IsRequired();
        b.Property(x => x.DeptCode).HasMaxLength(20).IsRequired();
        b.Property(x => x.FieldKey).HasMaxLength(60).IsRequired();
        b.Property(x => x.FieldLabel).HasMaxLength(150).IsRequired();
        b.Property(x => x.DataType).HasMaxLength(20).IsRequired();
        b.Property(x => x.MaxLength);
        b.Property(x => x.DefaultValue).HasMaxLength(500);
        b.Property(x => x.OptionsJson).HasMaxLength(2000);
        b.Property(x => x.IsActive).HasDefaultValue(true);
        b.HasOne(x => x.Schema).WithMany(x => x.Fields).HasForeignKey(x => x.SchemaId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => new { x.SchemaId, x.FieldKey }).IsUnique().HasDatabaseName("UX_F03EquipmentFieldDefinitions_Schema_FieldKey");
    }
}
