
using FVN_REGISTER.Core.Entities.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Views
{
    public class VF03employeeConfiguration : IEntityTypeConfiguration<VF03employee>
    {
        public void Configure(EntityTypeBuilder<VF03employee> entity)
        {
            entity.HasNoKey().ToView("vF03Employee");

            entity.Property(e => e.BirthDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Cvcode).HasMaxLength(30).HasColumnName("CVCode");
            entity.Property(e => e.Cvname).HasMaxLength(64).HasColumnName("CVName");
            entity.Property(e => e.DeptCode).HasMaxLength(30);
            entity.Property(e => e.DeptName).HasMaxLength(64);
            entity.Property(e => e.EmailAddress).HasMaxLength(100);
            entity.Property(e => e.EmployeeCode).HasMaxLength(30);
            entity.Property(e => e.EmployeeName).HasMaxLength(50);
            entity.Property(e => e.EndWorkingDate).HasColumnType("datetime");
            entity.Property(e => e.FirstWorkingDate).HasColumnType("datetime");
            entity.Property(e => e.GenderName).HasMaxLength(50);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.TongPhep).HasColumnType("numeric(18, 2)");
        }
    }
}
