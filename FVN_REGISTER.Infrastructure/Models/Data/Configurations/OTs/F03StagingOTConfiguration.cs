using FVN_REGISTER.Infrastructure.Models.Entities.OT;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.OTs
{
    public class F03StagingOTConfiguration : IEntityTypeConfiguration<F03StagingOT>
    {
        public void Configure(EntityTypeBuilder<F03StagingOT> entity)
        {
            entity.ToTable("F03StagingOTs");
            entity.HasKey(e => e.Id).HasName("PK_F03StagingOTs");

            // Index quan trọng để Job xử lý import quét dữ liệu nhanh chóng
            entity.HasIndex(e => e.IsProcessed, "IX_StagingOT_IsProcessed");

            // Cấu hình độ dài để bảo mật dữ liệu
            entity.Property(e => e.EmployeeCode).HasMaxLength(50);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);

            // Kiểu dữ liệu decimal cho giờ OT để tránh sai số
            entity.Property(e => e.Hours).HasColumnType("decimal(5,2)");

            // Defaults
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsProcessed).HasDefaultValue(false);
        }
    }
}
