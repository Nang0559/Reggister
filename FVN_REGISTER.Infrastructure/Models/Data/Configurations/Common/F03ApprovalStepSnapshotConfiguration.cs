
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Common
{
    public class F03ApprovalStepSnapshotConfiguration : IEntityTypeConfiguration<F03ApprovalStepSnapshot>
    {
        public void Configure(EntityTypeBuilder<F03ApprovalStepSnapshot> builder)
        {
            builder.ToTable("F03ApprovalStepSnapshots");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ApproverCode).IsRequired().HasMaxLength(20);
            builder.Property(x => x.ApproverName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.RoleName).IsRequired().HasMaxLength(100);

            // Liên kết ngược lại Snapshot cha
            builder.HasOne<F03ApprovalSnapshot>()
                   .WithMany(s => s.Steps)
                   .HasForeignKey("SnapshotId") // Shadow property
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
