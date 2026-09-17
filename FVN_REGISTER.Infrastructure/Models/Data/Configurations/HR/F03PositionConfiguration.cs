using FVN_REGISTER.Infrastructure.Models.Entities.HR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.HR
{
    public class F03PositionConfiguration : IEntityTypeConfiguration<F03Position>
    {
        public void Configure(EntityTypeBuilder<F03Position> entity)
        {
            entity.ToTable("F03Positions");
            entity.HasKey(e => e.Id).HasName("PK_F03Positions");

            // Index duy nhất cho mã vị trí
            entity.HasIndex(e => e.PositionCode, "IX_F03Position_Code").IsUnique();

            // Cấu hình property
            entity.Property(e => e.PositionCode).HasMaxLength(20).IsRequired();
            entity.Property(e => e.PositionName).HasMaxLength(100).IsRequired();

            // Audit defaults
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsApprove).HasDefaultValue(false);
            entity.Property(e => e.IsAllowApprove).HasDefaultValue(false);
        }
    }
}
