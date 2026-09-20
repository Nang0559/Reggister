
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Common
{
    public class F03WorkYearConfiguration : IEntityTypeConfiguration<F03WorkYear>
    {
        public void Configure(EntityTypeBuilder<F03WorkYear> entity)
        {
            entity.ToTable("F03WorkYears");
            entity.HasKey(e => e.Id).HasName("PK_F03WorkYears");

            // Index: Đảm bảo không trùng lặp năm làm việc
            entity.HasIndex(e => e.WorkYear, "IX_F03WorkYears_Year").IsUnique();

            // Cấu hình các trường bắt buộc
            entity.Property(e => e.WorkYear).IsRequired();
            entity.Property(e => e.StartDate).HasColumnType("date").IsRequired();
            entity.Property(e => e.EndDate).HasColumnType("date").IsRequired();

            // Audit defaults
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        }
    }
}
