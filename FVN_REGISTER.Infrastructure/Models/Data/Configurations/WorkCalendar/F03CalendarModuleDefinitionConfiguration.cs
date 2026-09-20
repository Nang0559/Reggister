using FVN_REGISTER.Core.Entities.WorkCalendar;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.WorkCalendar;

public sealed class F03CalendarModuleDefinitionConfiguration : IEntityTypeConfiguration<F03CalendarModuleDefinition>
{
    public void Configure(EntityTypeBuilder<F03CalendarModuleDefinition> entity)
    {
        entity.ToTable("F03CalendarModuleDefinitions");
        entity.HasKey(x => x.Id);
        entity.HasIndex(x => x.ModuleCode).IsUnique();
        entity.Property(x => x.ModuleCode).HasMaxLength(50).IsRequired();
        entity.Property(x => x.ModuleName).HasMaxLength(100).IsRequired();
    }
}
