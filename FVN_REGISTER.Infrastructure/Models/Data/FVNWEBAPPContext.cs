namespace FVN_REGISTER.Infrastructure
{
    using System.Reflection;
    using FVN_REGISTER.Core.Entities;
    using FVN_REGISTER.Core.Entities.Approvers;
    using FVN_REGISTER.Core.Entities.Common;
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

        // --- Security ---
        public DbSet<F03User> Users { get; set; }
        public DbSet<F03Function> Functions { get; set; }
        public DbSet<F03Permission> Permissions { get; set; }
        public DbSet<F03UserFunction> UserFunctions { get; set; }
        public DbSet<F03UserSession> UserSessions { get; set; }

        // --- HR ---
        public DbSet<F03Employee> Employees { get; set; }
        public DbSet<F03Department> Departments { get; set; }
        public DbSet<F03Position> Positions { get; set; }
        public DbSet<F03Gender> Genders { get; set; }

        // --- Approvers ---
        public DbSet<F03Approver> Approvers { get; set; }
        public DbSet<F03ApprovalStep> ApprovalSteps { get; set; }
        public DbSet<F03ApprovalSnapshot> ApprovalSnapshots { get; set; }
        public DbSet<F03ApprovalHistory> ApprovalHistories { get; set; }

        // --- Leaves ---
        public DbSet<F03LeaveBalance> LeaveBalances { get; set; }
        public DbSet<F03LeaveType> LeaveTypes { get; set; }
        public DbSet<F03LeaveDay> LeaveDays { get; set; }
        public DbSet<F03LeaveDayDetail> LeaveDayDetails { get; set; }
        public DbSet<F03AttendanceStaging> AttendanceStagings { get; set; }

        // --- OT ---
        public DbSet<F03OTRequest> OvertimeRequests { get; set; }
        public DbSet<F03OTEmployee> OvertimeEmployees { get; set; }
        public DbSet<F03OTLimitRule> OvertimeLimitRules { get; set; }
        public DbSet<F03OTReasonCode> OvertimeReasonCodes { get; set; }
        public DbSet<F03OTType> OTTypes { get; set; }
        public DbSet<F03OTReasonCode> OTReasonCodes { get; set; }

        // --- Trips ---
        public DbSet<F03StagingTrip> StagingTrips { get; set; }

        // --- HRM Sync (staging + review) ---
        public DbSet<F03StagingDepartment> StagingDepartments { get; set; }
        public DbSet<F03StagingEmployee> StagingEmployees { get; set; }
        public DbSet<F03StagingLeaveType> StagingLeaveTypes { get; set; }
        public DbSet<F03StagingOTType> StagingOTTypes { get; set; }
        public DbSet<F03StagingPosition> StagingPositions { get; set; }
        public DbSet<F03SyncReviewFlag> SyncReviewFlags { get; set; }
        public DbSet<HrmLeaveTypeChangeLog> HrmLeaveTypeChangeLogs { get; set; }

        // --- Common & Infrastructure ---
        public DbSet<F03AppNotification> AppNotifications { get; set; }
        public DbSet<F03AuditLog> AuditLogs { get; set; }
        public DbSet<F03Attachment> Attachments { get; set; }
        public DbSet<F03EmailQueue> EmailQueues { get; set; }
        public DbSet<F03EmailLog> EmailLogs { get; set; }
        public DbSet<F03EmailTemplate> EmailTemplates { get; set; }
        public DbSet<F03EmailProfile> EmailProfiles { get; set; }
        public DbSet<F03BusinessRule> BusinessRules { get; set; }
        public DbSet<F03CompanyHoliday> CompanyHolidays { get; set; }
        public DbSet<F03WorkYear> WorkYears { get; set; }
        public DbSet<F03EscalationLog> EscalationLogs { get; set; }
        public DbSet<F03EscalationRule> EscalationRules { get; set; }
        public DbSet<F03UserLog> UserLogs { get; set; }

        // ================= VIEWS (Read-only) =================
        public DbSet<VF03EmployeeApprover> VEmployeeApprovers { get; set; }
        public DbSet<VF03employee> VF03Employees { get; set; }
        public DbSet<vF03EmployeeAttendance> VF03EmployeeAttendances { get; set; }
        public DbSet<VF03LeaveRequest> VF03LeaveRequests { get; set; }
        public DbSet<VF03LeaveRequestDetail> VF03LeaveRequestDetails { get; set; }
        public DbSet<VF03leaveType> VF03LeaveTypes { get; set; }
        public DbSet<VF03OTRequest> VF03OTRequests { get; set; }
        public DbSet<VF03OTRequestDetail> VF03OTRequestDetails { get; set; }
        public DbSet<VF03OTSummary> VF03OTSummaries { get; set; }
        public DbSet<VF03LeaveBalance> VF03LeaveBalances { get; set; }
        //public DbSet<VF03LeaveBalance> VF03LeaveBalances { get; set; }   // ← thêm, bị thiếu so với file thật có
        public DbSet<VF03user> VF03Users { get; set; }
        public DbSet<VwCurrentlyPresentEmployee> VwCurrentlyPresentEmployees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tự động load toàn bộ IEntityTypeConfiguration<T> trong assembly Infrastructure —
            // thêm Configuration mới không cần sửa dòng này.
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public override int SaveChanges()
        {
            ApplyAuditInfo();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditInfo();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyAuditInfo()
        {
            foreach (var entry in ChangeTracker.Entries<BaseAuditEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.Now;   // khớp convention DateTime.Now toàn hệ thống
                        break;
                    case EntityState.Modified:
                        entry.Entity.ModifiedAt = DateTime.Now;
                        break;
                }
            }
        }
    }
}