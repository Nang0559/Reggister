
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.HR
{
   
        public class F03EmployeeConfiguration : IEntityTypeConfiguration<F03Employee>
        {
            public void Configure(EntityTypeBuilder<F03Employee> builder)
            {
                builder.HasIndex(e => e.EmployeeCode).IsUnique();

                // FK: Employee.PositionCode -> Position.PositionCode
                builder.HasOne(e => e.Position)
                       .WithMany(p => p.Employees)
                       .HasForeignKey(e => e.PositionCode)
                       .HasPrincipalKey(p => p.PositionCode)   // vì PositionCode không phải PK (PK là Id từ BaseAuditEntity)
                       .OnDelete(DeleteBehavior.Restrict);      // không cho xóa Position nếu còn Employee dùng

                // FK: Employee.DeptCode -> Department.DeptCode (nếu áp dụng luôn)
                builder.HasOne(e => e.Department)
                       .WithMany()
                       .HasForeignKey(e => e.DeptCode)
                       .HasPrincipalKey(d => d.DeptCode)
                       .OnDelete(DeleteBehavior.Restrict);
            }
        }
    
}
