using FVN_REGISTER.Infrastructure.Models.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Common
{
    public class F03AuditLogConfiguration : IEntityTypeConfiguration<F03AuditLog>
    {
        public void Configure(EntityTypeBuilder<F03AuditLog> entity)
        {
            entity.ToTable("F03AuditLogs");
            entity.HasKey(e => e.Id).HasName("PK_F03AuditLogs");

            // Index để lọc log theo User hoặc Action cực nhanh
            entity.HasIndex(e => e.UserId, "IX_AuditLog_UserId");
            entity.HasIndex(e => e.Action, "IX_AuditLog_Action");
            entity.HasIndex(e => e.CreatedAt, "IX_AuditLog_CreatedAt");

            entity.Property(e => e.Action).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(255);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        }
    }
}
