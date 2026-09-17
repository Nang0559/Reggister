
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Common
{
    public class F03EscalationRuleConfiguration : IEntityTypeConfiguration<F03EscalationRule>
    {
        public void Configure(EntityTypeBuilder<F03EscalationRule> entity)
        {
            entity.ToTable("F03EscalationRules");
            entity.HasKey(e => e.Id).HasName("PK_F03EscalationRules");

            // Index: Tối ưu khi hệ thống cần kiểm tra quy tắc cho phòng ban cụ thể
            entity.HasIndex(e => new { e.RequestModule, e.Level, e.DeptCode }, "IX_EscalationRule_Lookup");

            // Chuyển Enum sang String trong DB
            entity.Property(e => e.RequestModule)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(e => e.WarningHours).HasColumnType("decimal(5,2)");
            entity.Property(e => e.EscalateHours).HasColumnType("decimal(5,2)");

            // Audit defaults
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        }
    }
}
