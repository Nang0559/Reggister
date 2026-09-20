namespace FVN_REGISTER.Infrastructure;

using System.Reflection;
using FVN_REGISTER.Core.Entities;
using FVN_REGISTER.Core.Entities.Approvers;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Entities.Equipment;
using FVN_REGISTER.Core.Entities.HR;
using FVN_REGISTER.Core.Entities.HRM;
using FVN_REGISTER.Core.Entities.Leaves;
using FVN_REGISTER.Core.Entities.OT;
using FVN_REGISTER.Core.Entities.Security;
using FVN_REGISTER.Core.Entities.Trips;
using FVN_REGISTER.Core.Entities.Views;
using FVN_REGISTER.Core.Entities.WorkCalendar;
using Microsoft.EntityFrameworkCore;

public class FVNWEBAPPContext : DbContext
{
    public FVNWEBAPPContext(DbContextOptions<FVNWEBAPPContext> options) : base(options) { }
    public DbSet<F03User> Users { get; set; } public DbSet<F03Function> Functions { get; set; } public DbSet<F03Permission> Permissions { get; set; } public DbSet<F03UserFunction> UserFunctions { get; set; } public DbSet<F03UserSession> UserSessions { get; set; } public DbSet<F03Role> Roles { get; set; } public DbSet<F03RoleFunction> RoleFunctions { get; set; } public DbSet<F03UserRole> UserRoles { get; set; }
    public DbSet<F03Employee> Employees { get; set; } public DbSet<F03Department> Departments { get; set; } public DbSet<F03Position> Positions { get; set; } public DbSet<F03Gender> Genders { get; set; }
    public DbSet<F03Approver> Approvers { get; set; } public DbSet<F03ApprovalReminderLog> ApprovalReminderLogs { get; set; } public DbSet<F03ApprovalPolicy> ApprovalPolicies { get; set; } public DbSet<F03ApprovalSelection> ApprovalSelections { get; set; } public DbSet<F03ApprovalSnapshot> ApprovalSnapshots { get; set; } public DbSet<F03ApprovalHistory> ApprovalHistories { get; set; }
    public DbSet<F03LeaveBalance> LeaveBalances { get; set; } public DbSet<F03LeaveType> LeaveTypes { get; set; } public DbSet<F03LeaveDay> LeaveDays { get; set; } public DbSet<F03LeaveDayDetail> LeaveDayDetails { get; set; } public DbSet<F03AttendanceStaging> AttendanceStagings { get; set; }
    public DbSet<F03OTRequest> OvertimeRequests { get; set; } public DbSet<F03OTEmployee> OvertimeEmployees { get; set; } public DbSet<F03OTLimitRule> OvertimeLimitRules { get; set; } public DbSet<F03OTReasonCode> OvertimeReasonCodes { get; set; } public DbSet<F03OTType> OTTypes { get; set; }
    public DbSet<F03TripRequest> TripRequests { get; set; } public DbSet<F03StagingTrip> StagingTrips { get; set; }
    public DbSet<F03EquipmentAsset> EquipmentAssets { get; set; } public DbSet<F03EquipmentFieldDefinition> EquipmentFieldDefinitions { get; set; } public DbSet<F03EquipmentImportBatch> EquipmentImportBatches { get; set; } public DbSet<F03EquipmentImportRow> EquipmentImportRows { get; set; } public DbSet<F03EquipmentRequest> EquipmentRequests { get; set; } public DbSet<F03EquipmentRepairHistory> EquipmentRepairHistories { get; set; }
    public DbSet<F03StagingDepartment> StagingDepartments { get; set; } public DbSet<F03StagingEmployee> StagingEmployees { get; set; } public DbSet<F03StagingLeaveType> StagingLeaveTypes { get; set; } public DbSet<F03StagingOTType> StagingOTTypes { get; set; } public DbSet<F03StagingPosition> StagingPositions { get; set; } public DbSet<F03SyncReviewFlag> SyncReviewFlags { get; set; } public DbSet<HrmLeaveTypeChangeLog> HrmLeaveTypeChangeLogs { get; set; }
    public DbSet<F03AppNotification> AppNotifications { get; set; }
    public DbSet<F03CalendarModuleDefinition> CalendarModuleDefinitions { get; set; }
    public DbSet<F03CalendarModulePolicy> CalendarModulePolicies { get; set; }
    public DbSet<F03CalendarProjection> CalendarProjections { get; set; }
    public DbSet<F03ActionItem> ActionItems { get; set; }
    public DbSet<F03ExecutionPolicy> ExecutionPolicies { get; set; }
    public DbSet<F03ExecutionReconciliation> ExecutionReconciliations { get; set; }
    public DbSet<F03ExecutionConfirmation> ExecutionConfirmations { get; set; }
    public DbSet<F03ExecutionConfirmationEvidence> ExecutionConfirmationEvidence { get; set; }
    public DbSet<F03ExecutionReconciliationHistory> ExecutionReconciliationHistory { get; set; }
    public DbSet<F03ExecutionResolution> ExecutionResolutions { get; set; }
    public DbSet<F03ActionPolicy> ActionPolicies { get; set; } public DbSet<F03AuditLog> AuditLogs { get; set; } public DbSet<F03Attachment> Attachments { get; set; } public DbSet<F03EmailQueue> EmailQueues { get; set; } public DbSet<F03EmailLog> EmailLogs { get; set; } public DbSet<F03EmailTemplate> EmailTemplates { get; set; } public DbSet<F03EmailProfile> EmailProfiles { get; set; }
    public DbSet<F03PublicInformation> PublicInformations { get; set; } public DbSet<F03HrmUserRoleRule> HrmUserRoleRules { get; set; } public DbSet<F03BusinessRule> BusinessRules { get; set; } public DbSet<F03CompanyHoliday> CompanyHolidays { get; set; } public DbSet<F03WorkYear> WorkYears { get; set; } public DbSet<F03EscalationLog> EscalationLogs { get; set; } public DbSet<F03EscalationRule> EscalationRules { get; set; } public DbSet<F03UserLog> UserLogs { get; set; }
    public DbSet<VF03EmployeeApprover> VEmployeeApprovers { get; set; } public DbSet<VF03employee> VF03Employees { get; set; } public DbSet<vF03EmployeeAttendance> VF03EmployeeAttendances { get; set; } public DbSet<VF03LeaveRequest> VF03LeaveRequests { get; set; } public DbSet<VF03LeaveRequestDetail> VF03LeaveRequestDetails { get; set; } public DbSet<VF03leaveType> VF03LeaveTypes { get; set; } public DbSet<VF03OTRequest> VF03OTRequests { get; set; } public DbSet<VF03OTRequestDetail> VF03OTRequestDetails { get; set; } public DbSet<VF03OTSummary> VF03OTSummaries { get; set; } public DbSet<VF03LeaveBalance> VF03LeaveBalances { get; set; } public DbSet<VF03user> VF03Users { get; set; } public DbSet<VwCurrentlyPresentEmployee> VwCurrentlyPresentEmployees { get; set; } public DbSet<VwShiftCheckInOut> VwShiftCheckInOuts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.Entity<VwShiftCheckInOut>(entity => { entity.HasNoKey(); entity.ToView("VwShiftCheckInOut"); });


        modelBuilder.Entity<F03OTLimitRule>(entity =>
        {
            // dbo.F03OTLimitRules stores LimitType as nvarchar (e.g. Monthly, Yearly, Special),
            // while the domain model exposes OTLimitType as an enum.
            // Explicit conversion prevents EF from materializing the string column as Int32.
            entity.Property(x => x.LimitType)
                .HasConversion<string>()
                .HasMaxLength(40);

            entity.Property(x => x.ScopeType)
                .HasConversion<int>()
                .HasDefaultValue(FVN_REGISTER.Core.Enums.OTLimitScopeType.Employee);
        });

        modelBuilder.Entity<F03Position>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PositionCode)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasIndex(x => x.PositionCode)
                .IsUnique();
        });
        modelBuilder.Entity<F03ApprovalPolicy>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PositionCode)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasIndex(x => new
            {
                x.RequestType,
                x.PositionCode,
                x.Level
            })
            .IsUnique();

            entity.HasOne<F03Position>()
                .WithMany()
                .HasForeignKey(x => x.PositionCode)
                .HasPrincipalKey(x => x.PositionCode)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<F03ApprovalSelection>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.RequestType, x.RequestId, x.Level }).IsUnique();
        });

        modelBuilder.Entity<F03RoleFunction>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.IdRole, x.IdFunction }).IsUnique();
            entity.HasOne(x => x.Role).WithMany(x => x.RoleFunctions).HasForeignKey(x => x.IdRole).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Function).WithMany(x => x.RoleFunctions).HasForeignKey(x => x.IdFunction).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<F03UserRole>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.IdUser, x.IdRole }).IsUnique();
            entity.HasOne(x => x.User).WithMany(x => x.UserRoles).HasForeignKey(x => x.IdUser).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Role).WithMany(x => x.UserRoles).HasForeignKey(x => x.IdRole).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<F03Role>(entity => entity.HasIndex(x => x.RoleCode).IsUnique());
    }

    public override int SaveChanges() { ApplyAuditInfo(); return base.SaveChanges(); }
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) { ApplyAuditInfo(); return base.SaveChangesAsync(cancellationToken); }
    private void ApplyAuditInfo()
    {
        foreach (var entry in ChangeTracker.Entries<BaseAuditEntity>())
        {
            if (entry.State == EntityState.Added) entry.Entity.CreatedAt = DateTime.Now;
            else if (entry.State == EntityState.Modified) entry.Entity.ModifiedAt = DateTime.Now;
        }
    }
}