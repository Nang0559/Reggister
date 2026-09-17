using FVN_REGISTER.Infrastructure.Models.Entities.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Views
{
    public class vF03EmployeeAttendanceConfiguration : IEntityTypeConfiguration<vF03EmployeeAttendance>
    {
        public void Configure(EntityTypeBuilder<vF03EmployeeAttendance> entity)
        {
            entity.HasNoKey().ToView("vF03EmployeeAttendance"); // ⚠️ đổi lại đúng tên view thật của bạn

            entity.Property(e => e.CheckInTime).HasColumnType("datetime");
            entity.Property(e => e.CheckOutTime).HasColumnType("datetime");
            entity.Property(e => e.EmployeeId).HasMaxLength(10).HasColumnName("EmployeeID");
            entity.Property(e => e.FullName).HasMaxLength(50).IsFixedLength();
        }
    }
}
