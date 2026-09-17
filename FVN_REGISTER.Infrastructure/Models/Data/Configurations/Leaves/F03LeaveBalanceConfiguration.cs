using FVN_REGISTER.Infrastructure.Models.Entities.Leaves;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Leaves
{
    public class F03LeaveBalanceConfiguration : IEntityTypeConfiguration<F03LeaveBalance>
    {
        public void Configure(EntityTypeBuilder<F03LeaveBalance> entity)
        {
            entity.ToTable("F03LeaveBalances");
            entity.HasKey(e => e.Id).HasName("PK_F03LeaveBalances");

            // Index quan trọng: Tăng tốc độ kiểm tra phép khi làm đơn
            entity.HasIndex(e => new { e.EmployeeCode, e.WorkYear }, "IX_F03LeaveBalance_Employee_Year")
                  .IsUnique();

            // Property constraints
            entity.Property(e => e.EmployeeCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TotalDays).HasColumnType("decimal(5,2)").HasDefaultValue(0);

            // Audit defaults
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
        }
    }
}
