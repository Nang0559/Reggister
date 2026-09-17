
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Leaves
{
    public class F03StagingLeaveConfiguration : IEntityTypeConfiguration<F03StagingLeave>
    {
        public void Configure(EntityTypeBuilder<F03StagingLeave> entity)
        {
            entity.ToTable("F03StagingLeaves");
            entity.HasKey(e => e.Id).HasName("PK_F03StagingLeaves");

            // Index: Giúp lọc nhanh các dòng chưa xử lý
            entity.HasIndex(e => e.IsProcessed, "IX_Staging_IsProcessed");

            entity.Property(e => e.EmployeeCode).HasMaxLength(50);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
        }
    }
}
