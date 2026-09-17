using FVN_REGISTER.Infrastructure.Models.Entities.HR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.HR
{
    public class F03GenderConfiguration : IEntityTypeConfiguration<F03Gender>
    {
        public void Configure(EntityTypeBuilder<F03Gender> entity)
        {
            entity.ToTable("F03Genders");
            entity.HasKey(e => e.Id).HasName("PK_F03Genders");

            // Index duy nhất cho mã giới tính
            entity.HasIndex(e => e.GenderCode, "IX_F03Gender_Code").IsUnique();

            // Property constraints
            entity.Property(e => e.GenderCode).HasMaxLength(10).IsRequired();
            entity.Property(e => e.GenderName).HasMaxLength(50).IsRequired();

            // Audit defaults kế thừa
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        }
    }
}
