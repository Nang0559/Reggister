using FVN_REGISTER.Core.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Security;

public sealed class F03ManagedScopeConfiguration : IEntityTypeConfiguration<F03ManagedScope>
{
    public void Configure(EntityTypeBuilder<F03ManagedScope> b)
    {
        b.ToTable("F03ManagedScopes");
        b.HasKey(x => x.Id);
        b.Property(x => x.EmployeeCode).IsRequired().HasMaxLength(50);
        b.Property(x => x.NodeType).IsRequired().HasMaxLength(30);
        b.Property(x => x.NodeCode).HasMaxLength(50);
        b.Property(x => x.FactoryCode).HasMaxLength(50);
        b.Property(x => x.DeptCode).HasMaxLength(20);
        b.Property(x => x.SubDepartmentCode).HasMaxLength(20);
        b.Property(x => x.Remark).HasMaxLength(500);
        b.Property(x => x.CreatedAt).HasColumnType("datetime2(0)");
        b.Property(x => x.ModifiedAt).HasColumnType("datetime2(0)");
        b.HasIndex(x => new { x.EmployeeCode, x.IsActive });
        b.HasIndex(x => new { x.NodeType, x.NodeCode, x.DeptCode, x.SubDepartmentCode, x.IsActive });
    }
}
