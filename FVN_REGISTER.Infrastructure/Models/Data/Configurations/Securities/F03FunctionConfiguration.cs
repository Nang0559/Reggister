using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Securities;

public sealed class F03FunctionConfiguration : IEntityTypeConfiguration<F03Function>
{
    public void Configure(EntityTypeBuilder<F03Function> entity)
    {
        entity.ToTable("F03Functions");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("IdFunction");

        entity.HasIndex(e => e.FunctionCode, "IX_Function_Code").IsUnique();

        entity.Property(e => e.FunctionName).IsRequired().HasMaxLength(100);
        entity.Property(e => e.Detail).HasMaxLength(500);
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        entity.Property(e => e.IsActive).HasDefaultValue(true);
    }
}
