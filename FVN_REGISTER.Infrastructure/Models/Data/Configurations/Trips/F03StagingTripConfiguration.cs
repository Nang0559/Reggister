using FVN_REGISTER.Infrastructure.Models.Entities.Trips;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Trips
{
    public class F03StagingTripConfiguration : IEntityTypeConfiguration<F03StagingTrip>
    {
        public void Configure(EntityTypeBuilder<F03StagingTrip> entity)
        {
            entity.ToTable("F03StagingTrips");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.EmployeeCode).HasMaxLength(50);
            entity.Property(e => e.Destination).HasMaxLength(255);
            entity.Property(e => e.Purpose).HasMaxLength(500);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);

            // Index để quét các dòng chưa xử lý cực nhanh
            entity.HasIndex(e => e.IsProcessed, "IX_StagingTrip_IsProcessed");
        }
    }
}
