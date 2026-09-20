
using FVN_REGISTER.Core.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Common
{
    public class F03AppNotificationConfiguration : IEntityTypeConfiguration<F03AppNotification>
    {
        public void Configure(EntityTypeBuilder<F03AppNotification> entity)
        {
            entity.ToTable("F03AppNotifications");
            entity.HasKey(e => e.Id).HasName("PK_F03AppNotifications");

            entity.HasIndex(e => e.UserId, "IX_Notification_User");
            entity.HasIndex(e => e.IsRead, "IX_Notification_IsRead");
            entity.HasIndex(e => new { e.ActionId, e.UserId, e.IsRead, e.CreatedAt }, "IX_F03AppNotifications_ActionId");
            entity.HasIndex(e => new { e.UserId, e.IsRead, e.CreatedAt }, "IX_F03AppNotifications_UserUnread");

            entity.Property(e => e.RequestModule)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(e => e.Action)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Body).HasMaxLength(1000);
            entity.Property(e => e.ActionUrl).HasMaxLength(500);
            entity.Property(e => e.NotificationType).HasMaxLength(50);

            entity.HasOne<FVN_REGISTER.Core.Entities.WorkCalendar.F03ActionItem>()
                  .WithMany()
                  .HasForeignKey(e => e.ActionId)
                  .HasPrincipalKey(x => x.ActionId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        }
    }
}
