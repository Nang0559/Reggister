using FVN_REGISTER.Core.Entities.Equipment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FVN_REGISTER.Infrastructure.Models.Data.Configurations.Equipment;

public sealed class F03EquipmentInspectionTemplateConfiguration : IEntityTypeConfiguration<F03EquipmentInspectionTemplate>
{
    public void Configure(EntityTypeBuilder<F03EquipmentInspectionTemplate> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.DeptCode, x.TemplateCode, x.Version }).IsUnique();
        b.Property(x => x.Status).HasMaxLength(20);
        b.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.TemplateId).OnDelete(DeleteBehavior.Cascade);
    }
}
public sealed class F03EquipmentInspectionItemConfiguration : IEntityTypeConfiguration<F03EquipmentInspectionItem>
{
    public void Configure(EntityTypeBuilder<F03EquipmentInspectionItem> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TemplateId, x.ItemCode }).IsUnique();
        b.Property(x => x.MinValue).HasPrecision(18, 4);
        b.Property(x => x.MaxValue).HasPrecision(18, 4);
    }
}
public sealed class F03EquipmentInspectionAssignmentConfiguration : IEntityTypeConfiguration<F03EquipmentInspectionAssignment>
{
    public void Configure(EntityTypeBuilder<F03EquipmentInspectionAssignment> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.EquipmentId, x.TemplateId, x.EffectiveFrom, x.IsActive });
        b.HasOne(x => x.Equipment).WithMany(x => x.InspectionAssignments).HasForeignKey(x => x.EquipmentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Template).WithMany(x => x.Assignments).HasForeignKey(x => x.TemplateId).OnDelete(DeleteBehavior.Restrict);
    }
}
public sealed class F03EquipmentInspectionTaskConfiguration : IEntityTypeConfiguration<F03EquipmentInspectionTask>
{
    public void Configure(EntityTypeBuilder<F03EquipmentInspectionTask> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.AssignmentId, x.ScheduledDate }).IsUnique();
        b.HasIndex(x => x.ActionId).IsUnique().HasFilter("[ActionId] IS NOT NULL");
        b.HasIndex(x => x.ApprovalActionId).IsUnique().HasFilter("[ApprovalActionId] IS NOT NULL");
        b.HasIndex(x => new { x.InspectorEmployeeCode, x.Status, x.DueAt });
        b.HasOne(x => x.Equipment).WithMany(x => x.InspectionTasks).HasForeignKey(x => x.EquipmentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Template).WithMany().HasForeignKey(x => x.TemplateId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<F03EquipmentInspectionAssignment>().WithMany().HasForeignKey(x => x.AssignmentId).OnDelete(DeleteBehavior.Restrict);
    }
}
public sealed class F03EquipmentInspectionItemResultConfiguration : IEntityTypeConfiguration<F03EquipmentInspectionItemResult>
{
    public void Configure(EntityTypeBuilder<F03EquipmentInspectionItemResult> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TaskId, x.ItemId }).IsUnique();
        b.Property(x => x.ValueNumber).HasPrecision(18, 4);
    }
}
public sealed class F03EquipmentInspectionEvidenceConfiguration : IEntityTypeConfiguration<F03EquipmentInspectionEvidence>
{
    public void Configure(EntityTypeBuilder<F03EquipmentInspectionEvidence> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TaskId, x.ItemResultId });
        b.HasOne(x => x.Task).WithMany(x => x.Evidence).HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.Cascade);
    }
}