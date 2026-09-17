using FVN_REGISTER.Infrastructure.Models.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Common
{
    public class F03BusinessRuleConfiguration : IEntityTypeConfiguration<F03BusinessRule>
    {
        public void Configure(EntityTypeBuilder<F03BusinessRule> entity)
        {
            entity.ToTable("F03BusinessRules");
            entity.HasKey(e => e.Id);

            // Index để tìm kiếm nhanh theo Module và Code
            entity.HasIndex(e => new { e.Module, e.Code }, "IX_BusinessRule_Module_Code").IsUnique();

            entity.Property(e => e.Module)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        }
    }
}
