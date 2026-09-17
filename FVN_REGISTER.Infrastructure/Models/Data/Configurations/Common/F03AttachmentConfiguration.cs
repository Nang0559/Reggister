
using FVN_REGISTER.Core.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Common
{
    public class F03AttachmentConfiguration : IEntityTypeConfiguration<F03Attachment>
    {
        public void Configure(EntityTypeBuilder<F03Attachment> builder)
        {
            builder.ToTable("F03Attachment");
            builder.HasKey(x => x.FileId);

            // Không FK cứng tới F03LeaveDay/F03OTRequest — vì RequestId trỏ tới nhiều bảng khác nhau tùy Module
            builder.HasIndex(x => new { x.Module, x.RequestId });   // index tăng tốc query load attachment theo đơn

            builder.Property(x => x.FileName).HasMaxLength(200);
            builder.Property(x => x.FilePath).HasMaxLength(255);
            builder.Property(x => x.FileExtension).HasMaxLength(10);
        }
    }
}
