using FVN_REGISTER.Core.Entities.Trips;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Trips;

public sealed class F03TripActualConfiguration : IEntityTypeConfiguration<F03TripActual>
{
    public void Configure(EntityTypeBuilder<F03TripActual> builder)
    {
        builder.ToTable("F03TripActual");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TripRequestId).IsRequired();
        builder.Property(x => x.TripCode).HasMaxLength(30).IsRequired();
        builder.Property(x => x.EmployeeCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Destination).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Purpose).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.CustomerOrPartner).HasMaxLength(250);
        builder.Property(x => x.TransportMethod).HasMaxLength(100);
        builder.Property(x => x.CompanionEmployeeCodes).HasMaxLength(500);
        builder.Property(x => x.Accommodation).HasMaxLength(500);
        builder.Property(x => x.Note).HasMaxLength(1000);
        builder.Property(x => x.ActualStatus).HasMaxLength(30).IsRequired();
        builder.Property(x => x.LastModifiedSource).HasMaxLength(50);

        builder.HasIndex(x => x.TripRequestId).IsUnique();
        builder.HasIndex(x => new { x.EmployeeCode, x.ActualStartDate, x.ActualEndDate });
        builder.HasIndex(x => new { x.EmployeeCode, x.ActualStartDate, x.ActualStatus });
    }
}
