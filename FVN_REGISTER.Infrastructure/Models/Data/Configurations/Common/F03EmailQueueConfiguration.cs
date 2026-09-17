using FVN_REGISTER.Core.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Common
{
    public class F03EmailQueueConfiguration : IEntityTypeConfiguration<F03EmailQueue>
    {
        public void Configure(EntityTypeBuilder<F03EmailQueue> entity)
        {
            entity.ToTable("F03EmailQueues");
            entity.HasKey(e => e.Id).HasName("PK_F03EmailQueues");

            // Index: Giúp Background Service lấy các email Pending nhanh nhất
            entity.HasIndex(e => e.Status, "IX_EmailQueue_Status");

            entity.Property(e => e.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(e => e.Body).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.Payload).HasColumnType("nvarchar(max)");

            // Mặc định
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.RetryCount).HasDefaultValue(0);
            entity.Property(e => e.MaxRetry).HasDefaultValue(3);
        }
    }
}
