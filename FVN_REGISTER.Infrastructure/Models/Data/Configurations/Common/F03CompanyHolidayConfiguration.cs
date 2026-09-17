using FVN_REGISTER.Infrastructure.Models.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Common
{
    public class F03CompanyHolidayConfiguration : IEntityTypeConfiguration<F03CompanyHoliday>
    {
        public void Configure(EntityTypeBuilder<F03CompanyHoliday> entity)
        {
            entity.ToTable("F03CompanyHolidays");
            entity.HasKey(e => e.Id).HasName("PK_F03CompanyHolidays");

            // Index: Giúp tra cứu lịch nghỉ theo năm hoặc ngày cụ thể rất nhanh
            entity.HasIndex(e => e.HolidayDate, "IX_Holiday_Date").IsUnique();
            entity.HasIndex(e => e.Year, "IX_Holiday_Year");

            entity.Property(e => e.HolidayDate).HasColumnType("date").IsRequired();
            entity.Property(e => e.Description).HasMaxLength(200).IsRequired();

            // Audit defaults
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        }
    }
}
