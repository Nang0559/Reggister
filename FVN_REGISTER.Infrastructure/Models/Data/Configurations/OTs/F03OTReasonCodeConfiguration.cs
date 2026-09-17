using FVN_REGISTER.Core.Entities.OT;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.OTs
{
    public class F03OTReasonCodeConfiguration : IEntityTypeConfiguration<F03OTReasonCode>
    {
        public void Configure(EntityTypeBuilder<F03OTReasonCode> entity)
        {
            entity.ToTable("F03OTReasonCodes");
            entity.HasKey(e => e.Id).HasName("PK_F03OTReasonCodes");

            // Index cho mã lý do để tránh trùng lặp
            entity.HasIndex(e => e.ReasonCode, "IX_F03OTReasonCode_Code").IsUnique();

            // Property constraints
            entity.Property(e => e.ReasonCode).HasMaxLength(10).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);

            // Audit defaults
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        }
    }
}
