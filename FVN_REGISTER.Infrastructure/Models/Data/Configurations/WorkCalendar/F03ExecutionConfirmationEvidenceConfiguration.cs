using FVN_REGISTER.Core.Entities.WorkCalendar;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.WorkCalendar;
public sealed class F03ExecutionConfirmationEvidenceConfiguration : IEntityTypeConfiguration<F03ExecutionConfirmationEvidence>
{
    public void Configure(EntityTypeBuilder<F03ExecutionConfirmationEvidence> b)
    {
        b.ToTable("F03ExecutionConfirmationEvidence"); b.HasKey(x=>x.Id);
        b.Property(x => x.LastModifiedSource).HasMaxLength(50);
        b.Property(x=>x.EvidenceType).HasMaxLength(50).IsRequired(); b.Property(x=>x.ReferenceNo).HasMaxLength(200);
        b.Property(x=>x.ExternalUrl).HasMaxLength(1000); b.Property(x=>x.Description).HasMaxLength(2000);
        b.Property(x=>x.ReviewStatus).HasMaxLength(50).IsRequired(); b.Property(x=>x.ReviewNote).HasMaxLength(2000);
        b.HasIndex(x=>new{x.ConfirmationId,x.ReviewStatus,x.SubmittedAt});
    }
}