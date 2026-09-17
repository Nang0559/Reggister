using FVN_REGISTER.Infrastructure.Models.Entities.OT;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.OTs
{
    public class F03OTLimitRuleConfiguration : IEntityTypeConfiguration<F03OTLimitRule>
    {
        public void Configure(EntityTypeBuilder<F03OTLimitRule> entity)
        {
            entity.ToTable("F03OTLimitRules");
            entity.HasKey(e => e.Id).HasName("PK_F03OTLimitRules");

            // Index để tối ưu truy vấn quy tắc áp dụng cho Nhân viên/Bộ phận
            entity.HasIndex(e => new { e.LimitType, e.CvCode, e.DeptCode }, "IX_OTLimitRule_Lookup");

            // Property constraints
            entity.Property(e => e.LimitType)
                  .HasConversion<string>() // Lưu vào SQL là 'Daily', 'Weekly'...
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(e => e.CvCode).HasMaxLength(20);
            entity.Property(e => e.DeptCode).HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(500);

            // Audit defaults
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        }
    }
}
