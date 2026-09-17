using FVN_REGISTER.Core.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Common
{
    public class F03EmailLogConfiguration : IEntityTypeConfiguration<F03EmailLog>
    {
        public void Configure(EntityTypeBuilder<F03EmailLog> entity)
        {
            entity.ToTable("F03EmailLogs");
            entity.HasKey(e => e.Id).HasName("PK_F03EmailLogs");

            // Index: Giúp tìm kiếm lịch sử gửi email theo địa chỉ email hoặc theo thời gian
            entity.HasIndex(e => e.ToEmail, "IX_EmailLog_ToEmail");
            entity.HasIndex(e => e.SentAt, "IX_EmailLog_SentAt");

            entity.Property(e => e.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(e => e.ToEmail).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Subject).HasMaxLength(255).IsRequired();
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
        }
    }
}
