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
using Microsoft.EntityFrameworkCore;

public class FVNWEBAPPContext : DbContext
{
    public FVNWEBAPPContext(DbContextOptions<FVNWEBAPPContext> options) : base(options) { }
    public DbSet<F03User> Users { get; set; } public DbSet<F03Function> Functions { get; set; } public DbSet<F03Permission> Permissions { get; set; } public DbSet<F03UserFunction> UserFunctions { get; set; } public DbSet<F03UserSession> UserSessions { get; set; }
    public DbSet<F03Employee> Employees { get; set; } public DbSet<F03Department> Departments { get; set; } public DbSet<F03Position> Positions { get; set; } public DbSet<F03Gender> Genders { get; set; }
    public DbSet<F03Approver> Approvers { get; set; } public DbSet<F03ApprovalStep> ApprovalSteps { get; set; } public DbSet<F03ApprovalSnapshot> ApprovalSnapshots { get; set; } public DbSet<F03ApprovalHistory> ApprovalHistories { get; set; }
    public DbSet<F03LeaveBalance> LeaveBalances { get; set; } public DbSet<F03LeaveType> LeaveTypes { get; set; } public DbSet<F03LeaveDay> LeaveDays { get; set; } public DbSet<F03LeaveDayDetail> LeaveDayDetails { get; set; } public DbSet<F03AttendanceStaging> AttendanceStagings { get; set; }
    public DbSet<F03OTRequest> OvertimeRequests { get; set; } public DbSet<F03OTEmployee> OvertimeEmployees { get; set; } public DbSet<F03OTLimitRule> OvertimeLimitRules { get; set; } public DbSet<F03OTReasonCode> OvertimeReasonCodes { get; set; } public DbSet<F03OTType> OTTypes { get; set; }
    public DbSet<F03TripRequest> TripRequests { get; set; } public DbSet<F03StagingTrip> StagingTrips { get; set; }
    public DbSet<F03EquipmentAsset> EquipmentAssets { get; set; } public DbSet<F03EquipmentRequest> EquipmentRequests { get; set; } public DbSet<F03EquipmentRepairHistory> EquipmentRepairHistories { get; set; }
    public DbSet<F03StagingDepartment> StagingDepartments { get; set; } public DbSet<F03StagingEmployee> StagingEmployees { get; set; } public DbSet<F03StagingLeaveType> StagingLeaveTypes { get; set; } public DbSet<F03StagingOTType> StagingOTTypes { get; set; } public DbSet<F03StagingPosition> StagingPositions { get; set; } public DbSet<F03SyncReviewFlag> SyncReviewFlags { get; set; } public DbSet<HrmLeaveTypeChangeLog> HrmLeaveTypeChangeLogs { get; set; }
    public DbSet<F03AppNotification> AppNotifications { get; set; } public DbSet<F03AuditLog> AuditLogs { get; set; } public DbSet<F03Attachment> Attachments { get; set; } public DbSet<F03EmailQueue> EmailQueues { get; set; } public DbSet<F03EmailLog> EmailLogs { get; set; } public DbSet<F03EmailTemplate> EmailTemplates { get; set; } public DbSet<F03EmailProfile> EmailProfiles { get; set; }
    public DbSet<F03BusinessRule> BusinessRules { get; set; } public DbSet<F03CompanyHoliday> CompanyHolidays { get; set; } public DbSet<F03WorkYear> WorkYears { get; set; } public DbSet<F03EscalationLog> EscalationLogs { get; set; } public DbSet<F03EscalationRule> EscalationRules { get; set; } public DbSet<F03UserLog> UserLogs { get; set; }
    public DbSet<VF03EmployeeApprover> VEmployeeApprovers { get; set; } public DbSet<VF03employee> VF03Employees { get; set; } public DbSet<vF03EmployeeAttendance> VF03EmployeeAttendances { get; set; } public DbSet<VF03LeaveRequest> VF03LeaveRequests { get; set; } public DbSet<VF03LeaveRequestDetail> VF03LeaveRequestDetails { get; set; } public DbSet<VF03leaveType> VF03LeaveTypes { get; set; } public DbSet<VF03OTRequest> VF03OTRequests { get; set; } public DbSet<VF03OTRequestDetail> VF03OTRequestDetails { get; set; } public DbSet<VF03OTSummary> VF03OTSummaries { get; set; } public DbSet<VF03LeaveBalance> VF03LeaveBalances { get; set; } public DbSet<VF03user> VF03Users { get; set; } public DbSet<VwCurrentlyPresentEmployee> VwCurrentlyPresentEmployees { get; set; } public DbSet<VwShiftCheckInOut> VwShiftCheckInOuts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.Entity<VwShiftCheckInOut>(entity => { entity.HasNoKey(); entity.ToView("VwShiftCheckInOut"); });
    }
    public override int SaveChanges() { ApplyAuditInfo(); return base.SaveChanges(); }
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) { ApplyAuditInfo(); return base.SaveChangesAsync(cancellationToken); }
    private void ApplyAuditInfo() { foreach (var entry in ChangeTracker.Entries<BaseAuditEntity>()) { if (entry.State == EntityState.Added) entry.Entity.CreatedAt = DateTime.Now; else if (entry.State == EntityState.Modified) entry.Entity.ModifiedAt = DateTime.Now; } }
}
