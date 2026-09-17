using FVN_REGISTER.Core.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Common;

public sealed class ApprovalSnapshotConfiguration : IEntityTypeConfiguration<F03ApprovalSnapshot>
{
    public void Configure(EntityTypeBuilder<F03ApprovalSnapshot> builder)
    {
        builder.ToTable("F03ApprovalSnapshots");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RequestId).IsRequired();
        builder.Property(x => x.RequestType).IsRequired();
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.HasMany(x => x.Steps)
            .WithOne()
            .HasForeignKey("SnapshotId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
