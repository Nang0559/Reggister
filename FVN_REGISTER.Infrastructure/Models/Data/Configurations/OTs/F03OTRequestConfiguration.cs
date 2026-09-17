
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.OTs
{
    public class F03OTRequestConfiguration : IEntityTypeConfiguration<F03OTRequest>
    {
        public void Configure(EntityTypeBuilder<F03OTRequest> entity)
        {
            entity.ToTable("F03OTRequests");
            entity.HasKey(e => e.Id).HasName("PK_F03OTRequests");

            // Index cho việc tìm kiếm đơn OT nhanh
            entity.HasIndex(e => e.OTCode, "IX_F03OTRequest_Code").IsUnique();
            entity.HasIndex(e => e.OTDate, "IX_F03OTRequest_Date");

            // Property constraints
            entity.Property(e => e.OTCode).HasMaxLength(20).IsRequired();
            entity.Property(e => e.DeptCode).HasMaxLength(20).IsRequired();
            entity.Property(e => e.OTTypeCode).HasMaxLength(20).IsRequired();

            // Enum conversions
            entity.Property(e => e.ScopeType).HasConversion<string>();
            entity.Property(e => e.RequestStatus).HasConversion<int>();

            // Audit defaults kế thừa từ BaseRequestEntity (thông qua DbContext)
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        }
    }
}
