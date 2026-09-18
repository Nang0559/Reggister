
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Securities
{
    public class F03UserFunctionConfiguration : IEntityTypeConfiguration<F03UserFunction>
    {
        public void Configure(EntityTypeBuilder<F03UserFunction> entity)
        {
            entity.ToTable("F03UserFunctions");
            entity.HasKey(e => e.Id);

            // Index tăng tốc độ truy vấn quyền của User
            entity.HasIndex(e => e.IdUser, "IX_UserFunction_UserId");
            entity.HasIndex(e => e.IdFunction, "IX_UserFunction_FunctionId");
            entity.HasIndex(e => new { e.IdUser, e.IdFunction }, "IX_UserFunction_User_Function").IsUnique();

            // Thiết lập các quan hệ
            entity.HasOne(d => d.User)
                  .WithMany(p => p.F03userFunctions)
                  .HasForeignKey(d => d.IdUser)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Permission)
                  .WithMany(p => p.F03userFunctions)
                  .HasForeignKey(d => d.IdPermission)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Function)
                  .WithMany(p => p.UserFunctions) // Đảm bảo đã khai báo collection này trong F03Function
                  .HasForeignKey(d => d.IdFunction)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
