using FVN_REGISTER.Core.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Securities;

public sealed class F03SecurityFunctionRegistryItemConfiguration : IEntityTypeConfiguration<F03SecurityFunctionRegistryItem>
{
    public void Configure(EntityTypeBuilder<F03SecurityFunctionRegistryItem> builder)
    {
        builder.ToTable("F03SecurityFunctionRegistry");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.FunctionKey).IsUnique();
        builder.Property(x => x.FunctionKey).IsRequired().HasMaxLength(150);
        builder.Property(x => x.DefinitionName).IsRequired().HasMaxLength(150);
        builder.Property(x => x.LifecycleStatus).IsRequired().HasMaxLength(30);
        builder.Property(x => x.SourceType).IsRequired().HasMaxLength(30);
        builder.Property(x => x.DefinitionHash).IsRequired().HasMaxLength(128);
        builder.Property(x => x.FirstDiscoveredAt).HasColumnType("datetime2");
        builder.Property(x => x.LastSeenAt).HasColumnType("datetime2");
        builder.Property(x => x.ResolvedAt).HasColumnType("datetime2");
    }
}
