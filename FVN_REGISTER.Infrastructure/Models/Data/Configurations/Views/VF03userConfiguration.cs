using FVN_REGISTER.Core.Entities.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;




namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Views
    {
        public class VF03userConfiguration : IEntityTypeConfiguration<VF03user>
        {
            public void Configure(EntityTypeBuilder<VF03user> entity)
            {
                entity.HasNoKey().ToView("vF03Users");

                entity.Property(e => e.UserName).HasMaxLength(50);
                entity.Property(e => e.Password).HasMaxLength(50);
                entity.Property(e => e.Avatar).HasMaxLength(255);

                entity.Property(e => e.EmployeeCode).HasMaxLength(50);
                entity.Property(e => e.EmployeeName).HasMaxLength(50);

                entity.Property(e => e.DeptCode).HasMaxLength(30);
                entity.Property(e => e.DeptName).HasMaxLength(64);

                entity.Property(e => e.GenderName).HasMaxLength(50);
                entity.Property(e => e.BirthDate).HasColumnType("datetime");

                entity.Property(e => e.EmailAddress).HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).HasMaxLength(50);

                entity.Property(e => e.PermissionName).HasMaxLength(50);

                entity.Property(e => e.LastLogin).HasColumnType("datetime");
                entity.Property(e => e.LockoutEndDate).HasColumnType("datetime");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");
                entity.Property(e => e.ModifiedAt).HasColumnType("datetime");

                entity.Property(e => e.Cvcode).HasMaxLength(30).HasColumnName("CVCode");
            }
        }
    }
