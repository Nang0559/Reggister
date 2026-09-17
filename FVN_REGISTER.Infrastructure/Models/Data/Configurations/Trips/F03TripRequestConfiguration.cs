using FVN_REGISTER.Core.Entities.Trips;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Trips;

public sealed class F03TripRequestConfiguration : IEntityTypeConfiguration<F03TripRequest>
{
    public void Configure(EntityTypeBuilder<F03TripRequest> builder)
    {
        builder.ToTable("F03TripRequests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TripCode).HasMaxLength(30).IsRequired();
        builder.Property(x => x.EmployeeCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.DeptCode).HasMaxLength(20);
        builder.Property(x => x.Destination).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Purpose).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.CustomerOrPartner).HasMaxLength(250);
        builder.Property(x => x.TransportMethod).HasMaxLength(100);
        builder.Property(x => x.CompanionEmployeeCodes).HasMaxLength(500);
        builder.Property(x => x.EstimatedCost).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Accommodation).HasMaxLength(500);
        builder.Property(x => x.Note).HasMaxLength(1000);
        builder.HasIndex(x => x.TripCode).IsUnique();
        builder.HasIndex(x => new { x.EmployeeCode, x.StartDate, x.EndDate });
    }
}
