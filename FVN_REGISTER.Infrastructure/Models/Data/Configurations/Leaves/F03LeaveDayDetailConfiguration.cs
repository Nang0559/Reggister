using FVN_REGISTER.Infrastructure.Models.Entities.Leaves;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Leaves
{
    public class F03LeaveDayDetailConfiguration : IEntityTypeConfiguration<F03LeaveDayDetail>
    {
        public void Configure(EntityTypeBuilder<F03LeaveDayDetail> entity)
        {
            entity.ToTable("F03LeaveDayDetails");
            entity.HasKey(e => e.Id).HasName("PK_F03LeaveDayDetails");

            // Properties
            entity.Property(e => e.LeaveTypeCode).HasMaxLength(10).IsRequired();
            entity.Property(e => e.LeaveTypeName).HasMaxLength(100);
            entity.Property(e => e.DayValue).HasColumnType("decimal(5,2)");

            // Enum -> String
            entity.Property(e => e.HalfDayOption)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            // Relationship
            entity.HasOne(d => d.LeaveDay)
                  .WithMany(p => p.F03LeaveDayDetails)
                  .HasForeignKey(d => d.LeaveDaysId)
                  .HasConstraintName("FK_F03LeaveDayDetail_F03LeaveDay");
        }
    }
}
