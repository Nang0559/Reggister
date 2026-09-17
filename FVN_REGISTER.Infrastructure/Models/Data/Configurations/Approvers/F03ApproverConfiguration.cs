
using FVN_REGISTER.Core.Entities.Approvers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FVN_REGISTER.Core.Extensions;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Approvers
{
    public class F03ApproverConfiguration : IEntityTypeConfiguration<F03Approver>
    {
        public void Configure(EntityTypeBuilder<F03Approver> entity)
        {
            entity.ToTable("F03Approvers");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => new { e.RequestType, e.ApproveForDeptCode }, "IX_Approver_Lookup");
            entity.HasIndex(e => e.UserId, "IX_Approver_UserId");

            entity.HasOne(d => d.User)
                  .WithMany()
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.RequestType)
                  .HasConversion(
                      v => v.ToCode(),
                      v => v.ToRequestModule())
                  .IsRequired()
                  .HasMaxLength(20);

            entity.Property(e => e.ApproverCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ApproveForDeptCode).IsRequired().HasMaxLength(20);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        }
    }
}
