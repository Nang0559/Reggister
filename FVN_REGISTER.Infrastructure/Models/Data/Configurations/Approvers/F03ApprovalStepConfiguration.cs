
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FVN_REGISTER.Core.Extensions;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Approvers
{
    public class F03ApprovalStepConfiguration : IEntityTypeConfiguration<F03ApprovalStep>
    {
        public void Configure(EntityTypeBuilder<F03ApprovalStep> entity)
        {
            entity.ToTable("F03ApprovalSteps");
            entity.HasKey(e => e.Id);

            // Index để truy vấn nhanh trạng thái duyệt của một lá đơn
            // VD: "Cho tôi biết đơn LEAVE 105 đang dừng ở bước nào?"
            entity.HasIndex(e => new { e.RequestType, e.RequestId }, "IX_ApprovalStep_Request");

            // Index cho việc chạy background job gửi reminder
            entity.HasIndex(e => new { e.Approved, e.ReminderSent }, "IX_ApprovalStep_PendingReminder");

            entity.Property(e => e.RequestType)
                  .HasConversion(
                      v => v.ToCode(),
                      v => v.ToRequestModule())
                  .IsRequired()
                  .HasMaxLength(20);

            entity.Property(e => e.RoleName).IsRequired().HasMaxLength(50);

            // Audit Defaults
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        }
    }
}
