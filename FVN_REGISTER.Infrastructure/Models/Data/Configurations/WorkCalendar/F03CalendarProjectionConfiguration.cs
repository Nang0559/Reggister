using FVN_REGISTER.Core.Entities.WorkCalendar;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.WorkCalendar;

public sealed class F03CalendarProjectionConfiguration : IEntityTypeConfiguration<F03CalendarProjection>
{
    public void Configure(EntityTypeBuilder<F03CalendarProjection> entity)
    {
        entity.ToTable("F03CalendarProjection");
        entity.HasKey(x => x.Id);
        entity.HasIndex(x => new { x.EmployeeId, x.WorkDate, x.ModuleCode, x.SourceType, x.SourceId, x.ParticipantId }).IsUnique();
        entity.HasIndex(x => new { x.EmployeeId, x.WorkDate });
        entity.HasIndex(x => new { x.EmployeeId, x.WorkDate, x.RequiresAction });
        entity.Property(x => x.SourceType).HasMaxLength(50).IsRequired();
        entity.Property(x => x.ParticipantId).HasMaxLength(100);
        entity.Property(x => x.ModuleCode).HasMaxLength(50).IsRequired();
        entity.Property(x => x.SourceId).HasMaxLength(100).IsRequired();
        entity.Property(x => x.StatusCode).HasMaxLength(50).IsRequired();
        entity.Property(x => x.Marker).HasMaxLength(20);
        entity.Property(x => x.Summary).HasMaxLength(500);
        entity.Property(x => x.DetailRoute).HasMaxLength(500);
        entity.Property(x => x.CalculatedAt).HasDefaultValueSql("(getdate())");
        entity.HasOne<FVN_REGISTER.Core.Entities.HR.F03Employee>()
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
