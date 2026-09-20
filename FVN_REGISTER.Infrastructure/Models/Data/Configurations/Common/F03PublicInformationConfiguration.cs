using FVN_REGISTER.Core.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Common;

public sealed class F03PublicInformationConfiguration : IEntityTypeConfiguration<F03PublicInformation>
{
    public void Configure(EntityTypeBuilder<F03PublicInformation> b)
    {
        b.ToTable("F03PublicInformation");
        b.HasKey(x => x.Id);

        b.Property(x => x.Type).HasMaxLength(30).IsRequired();
        b.Property(x => x.Title).HasMaxLength(300).IsRequired();
        b.Property(x => x.Summary).HasMaxLength(1000);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.AttachmentUrl).HasMaxLength(1000);
        b.Property(x => x.IsActive).HasDefaultValue(true);
        b.Property(x => x.CreatedAt).HasColumnType("datetime2(0)");
        b.Property(x => x.ModifiedAt).HasColumnType("datetime2(0)");
        b.HasIndex(x => new { x.Status, x.IsImportant, x.PublishedAt })
            .HasDatabaseName("IX_F03PublicInformation_Published");
    }
}