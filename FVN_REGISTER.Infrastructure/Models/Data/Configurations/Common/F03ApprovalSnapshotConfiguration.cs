using FVN_REGISTER.Infrastructure.Models.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Common
{
    public class ApprovalSnapshotConfiguration : IEntityTypeConfiguration<F03ApprovalSnapshot>
    {
        public void Configure(EntityTypeBuilder<F03ApprovalSnapshot> builder)
        {
            builder.ToTable("F03ApprovalSnapshots"); // Đồng bộ với [Table]
            builder.HasKey(x => x.Id);

            builder.Property(x => x.RequestId).IsRequired();
            builder.Property(x => x.Module).IsRequired();
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Cấu hình quan hệ 1-n tường minh
            builder.HasMany(x => x.Steps)
                   .WithOne()
                   .HasForeignKey("SnapshotId")
                   .OnDelete(DeleteBehavior.Cascade); // Cấu hình xóa ở đây cũng được
        }
    }
}
