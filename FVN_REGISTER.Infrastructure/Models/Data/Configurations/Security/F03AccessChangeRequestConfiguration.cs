using FVN_REGISTER.Core.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Security;

public sealed class F03AccessChangeRequestConfiguration : IEntityTypeConfiguration<F03AccessChangeRequest>
{
    public void Configure(EntityTypeBuilder<F03AccessChangeRequest> builder)
    {
        builder.ToTable("F03AccessChangeRequests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BusinessModule).HasConversion<int>().IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.RequesterEmployeeCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.OldEmployeeCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.NewEmployeeCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.DeptCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.NewPositionCode).HasMaxLength(20);
        builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        builder.Property(x => x.RequestedFunctionCodesJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.EquipmentAssetIdsJson).HasColumnType("nvarchar(max)");
        builder.HasIndex(x => new { x.BusinessModule, x.NewEmployeeCode, x.Status });
        builder.HasIndex(x => new { x.OldEmployeeCode, x.Status });
    }
}
