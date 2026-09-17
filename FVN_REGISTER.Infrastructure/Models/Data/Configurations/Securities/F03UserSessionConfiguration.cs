using FVN_REGISTER.Infrastructure.Models.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Securities
{
    public class F03UserSessionConfiguration : IEntityTypeConfiguration<F03UserSession>
    {
        public void Configure(EntityTypeBuilder<F03UserSession> entity)
        {
            entity.ToTable("F03UserSessions");
            entity.HasKey(e => e.Id);

            // Index cực quan trọng: tìm session của 1 user hoặc kiểm tra 1 thiết bị
            entity.HasIndex(e => e.UserId, "IX_Session_UserId");
            entity.HasIndex(e => e.DeviceId, "IX_Session_DeviceId");

            entity.Property(e => e.DeviceType).IsRequired().HasMaxLength(10);
            entity.Property(e => e.DeviceId).IsRequired().HasMaxLength(100);

            // Ràng buộc mối quan hệ
            entity.HasOne(d => d.User)
                  .WithMany() // Nếu không cần ICollection trong F03User
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.Cascade); // Xóa User -> Xóa sạch session
        }
    }
}
