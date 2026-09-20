using FVN_REGISTER.Core.Entities.Approvers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Approvers;

public sealed class F03ApprovalReminderLogConfiguration : IEntityTypeConfiguration<F03ApprovalReminderLog>
{
    public void Configure(EntityTypeBuilder<F03ApprovalReminderLog> b)
    {
        b.ToTable("F03ApprovalReminderLog");
        b.HasKey(x => x.Id);

        b.Property(x => x.IsActive).HasDefaultValue(true);
        b.Property(x => x.CreatedAt).HasColumnType("datetime2(0)");
        b.Property(x => x.ModifiedAt).HasColumnType("datetime2(0)");
        b.Property(x => x.SentAt).HasColumnType("datetime2(0)");
    }
}