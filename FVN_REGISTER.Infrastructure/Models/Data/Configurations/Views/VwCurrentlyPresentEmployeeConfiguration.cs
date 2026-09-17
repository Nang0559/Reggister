
using FVN_REGISTER.Core.Entities.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Views
{
    public class VwCurrentlyPresentEmployeeConfiguration : IEntityTypeConfiguration<VwCurrentlyPresentEmployee>
    {
        public void Configure(EntityTypeBuilder<VwCurrentlyPresentEmployee> entity)
        {
            entity.HasNoKey().ToView("vw_CurrentlyPresentEmployees");

            entity.Property(e => e.EmployeeId)
                .HasMaxLength(10)
                .HasColumnName("EmployeeID");

            entity.Property(e => e.FullName)
                .HasMaxLength(50)
                .IsFixedLength();

            entity.Property(e => e.DeptCode).HasMaxLength(30);
            entity.Property(e => e.DeptName).HasMaxLength(64);

            entity.Property(e => e.CheckInTime).HasColumnType("datetime");
        }
    }
}
