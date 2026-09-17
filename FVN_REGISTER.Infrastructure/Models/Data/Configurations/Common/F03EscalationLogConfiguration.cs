using FVN_REGISTER.Infrastructure.Models.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Common
{
    public class F03EscalationLogConfiguration : IEntityTypeConfiguration<F03EscalationLog>
    {
        public void Configure(EntityTypeBuilder<F03EscalationLog> entity)
        {
            entity.ToTable("F03EscalationLogs");
            entity.HasKey(e => e.Id).HasName("PK_F03EscalationLogs");

            // Index để truy vấn lịch sử của 1 đơn hàng cụ thể cực nhanh
            entity.HasIndex(e => new { e.RequestModule, e.RequestId }, "IX_EscalationLog_RequestLookup");

            // Chuyển Enum sang String trong DB
            entity.Property(e => e.RequestModule)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(e => e.Action).HasMaxLength(50);

            // Audit defaults
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        }
    }
}
