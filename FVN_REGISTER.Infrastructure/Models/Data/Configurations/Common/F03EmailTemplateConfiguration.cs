
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Common
{
    public class F03EmailTemplateConfiguration : IEntityTypeConfiguration<F03EmailTemplate>
    {
        public void Configure(EntityTypeBuilder<F03EmailTemplate> entity)
        {
            entity.ToTable("F03EmailTemplates");
            entity.HasKey(e => e.Id).HasName("PK_F03EmailTemplates");

            // Index cho Code để tra cứu nhanh khi gửi email
            entity.HasIndex(e => e.Code, "IX_EmailTemplate_Code").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Subject).HasMaxLength(255).IsRequired();

            // Cấu hình Body là nvarchar(max) để chứa HTML
            entity.Property(e => e.Body).HasColumnType("nvarchar(max)").IsRequired();

            // Audit defaults
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        }
    }
}
