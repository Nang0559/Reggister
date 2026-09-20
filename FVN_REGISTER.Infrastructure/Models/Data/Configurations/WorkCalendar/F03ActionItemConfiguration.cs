using FVN_REGISTER.Core.Entities.WorkCalendar;
using FVN_REGISTER.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.WorkCalendar;

public sealed class F03ActionItemConfiguration : IEntityTypeConfiguration<F03ActionItem>
{
    public void Configure(EntityTypeBuilder<F03ActionItem> entity)
    {
        entity.ToTable("F03ActionItems");
        entity.HasKey(x => x.Id);
        entity.HasIndex(x => x.ActionId).IsUnique();
        entity.HasIndex(x => new { x.ModuleCode, x.SourceId, x.ActionType, x.AssignedToEmployeeId })
            .IsUnique()
            .HasFilter("[Status] IN (0, 10)");
        entity.HasIndex(x => new { x.AssignedToUserId, x.Status, x.Priority, x.DueAt });
        entity.HasIndex(x => new { x.AssignedToEmployeeId, x.Status, x.Priority, x.DueAt });
        entity.HasIndex(x => new { x.ModuleCode, x.SourceId, x.ActionType });
        entity.Property(x => x.ActionId).HasDefaultValueSql("(newsequentialid())");
        entity.Property(x => x.ModuleCode).HasMaxLength(50).IsRequired();
        entity.Property(x => x.SourceId).HasMaxLength(100).IsRequired();
        entity.Property(x => x.ActionType).HasMaxLength(100).IsRequired();
        entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
        entity.Property(x => x.Summary).HasMaxLength(1000);
        entity.Property(x => x.Status).HasConversion<byte>().HasDefaultValue(ActionItemStatus.Open);
        entity.Property(x => x.DetailRoute).HasMaxLength(500);
        entity.Property(x => x.ReferenceNo).HasMaxLength(100);
        entity.HasOne<FVN_REGISTER.Core.Entities.HR.F03Employee>()
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<FVN_REGISTER.Core.Entities.HR.F03Employee>()
            .WithMany()
            .HasForeignKey(x => x.AssignedToEmployeeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_F03ActionItems_AssignedEmployee");
        entity.HasOne<FVN_REGISTER.Core.Entities.Security.F03User>()
            .WithMany()
            .HasForeignKey(x => x.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
