
using FVN_REGISTER.Core.Entities.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Views
{
    public class VEmployeeApproverConfiguration : IEntityTypeConfiguration<VF03EmployeeApprover>
    {
        public void Configure(EntityTypeBuilder<VF03EmployeeApprover> entity)
        {
            entity.HasNoKey().ToView("vEmployeeApprover");

            entity.Property(e => e.ApproveLevelCode).HasMaxLength(30);
            entity.Property(e => e.ApproverEmail).HasMaxLength(255);
            entity.Property(e => e.ApproverName).HasMaxLength(255);
            entity.Property(e => e.DeptCode).HasMaxLength(30);
            entity.Property(e => e.DeptName).HasMaxLength(64);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        }
    }
}
