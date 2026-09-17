
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.HR
{
    public class F03DepartmentConfiguration : IEntityTypeConfiguration<F03Department>
    {
        public void Configure(EntityTypeBuilder<F03Department> entity)
        {
            entity.ToTable("F03Departments");
            entity.HasKey(e => e.Id).HasName("PK_F03Departments");

            // Index duy nhất cho mã phòng ban để tối ưu tra cứu
            entity.HasIndex(e => e.DeptCode, "IX_F03Department_Code").IsUnique();

            // Cấu hình thuộc tính
            entity.Property(e => e.DeptCode).HasMaxLength(20).IsRequired();
            entity.Property(e => e.DeptName).HasMaxLength(100).IsRequired();

            // Audit defaults
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
        }
    }
}
