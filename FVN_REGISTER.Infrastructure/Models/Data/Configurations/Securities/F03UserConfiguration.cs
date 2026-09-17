using FVN_REGISTER.Infrastructure.Models.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Securities
{
    public class F03UserConfiguration : IEntityTypeConfiguration<F03User>
    {
        public void Configure(EntityTypeBuilder<F03User> entity)
        {
            entity.ToTable("F03Users");
            entity.HasKey(e => e.IdUser);

            // Index cho EmployeeCode làm key tìm kiếm chính
            entity.HasIndex(e => e.EmployeeCode, "IX_User_EmployeeCode").IsUnique();

            entity.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Password).IsRequired().HasMaxLength(255);

            // Nếu bạn không dùng UserName làm đăng nhập nữa, có thể bỏ [Required]
            entity.Property(e => e.FullName).HasMaxLength(100);
        }
    }
}
