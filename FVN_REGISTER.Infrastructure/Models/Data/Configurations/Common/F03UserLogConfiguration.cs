
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Common
{
    public class F03UserLogConfiguration : IEntityTypeConfiguration<F03UserLog>
    {
        public void Configure(EntityTypeBuilder<F03UserLog> entity)
        {
            entity.ToTable("F03UserLogs");
            entity.HasKey(e => e.Id).HasName("PK_F03UserLogs");

            // Index để tìm kiếm log theo User và thời gian
            entity.HasIndex(e => e.UserId, "IX_UserLog_UserId");
            entity.HasIndex(e => e.CreatedAt, "IX_UserLog_CreatedAt");

            // Property constraints
            entity.Property(e => e.LastSeen).HasMaxLength(255).IsRequired();
            entity.Property(e => e.LastSeenUrl).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ApplicationName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ApplicationVersion).HasMaxLength(20).IsRequired();
            entity.Property(e => e.WorkstationName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.WorkstationUser).HasMaxLength(100).IsRequired();

            // Mặc định thời gian
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        }
    }
}
