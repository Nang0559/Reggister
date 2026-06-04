using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Contract.Models;

public partial class FVNWEBAPPContext : DbContext
{
    public FVNWEBAPPContext()
    {
    }

    public FVNWEBAPPContext(DbContextOptions<FVNWEBAPPContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<CompanyHoliday> CompanyHolidays { get; set; }

    public virtual DbSet<DebugTable> DebugTables { get; set; }

    public virtual DbSet<EmailLog> EmailLogs { get; set; }

    public virtual DbSet<EmailQueue> EmailQueues { get; set; }

    public virtual DbSet<EmailTemplate> EmailTemplates { get; set; }

    public virtual DbSet<EmployeeAttendanceView> EmployeeAttendanceViews { get; set; }

    public virtual DbSet<EscalationLog> EscalationLogs { get; set; }

    public virtual DbSet<EscalationRule> EscalationRules { get; set; }

    public virtual DbSet<F03cv> F03cvs { get; set; }

    public virtual DbSet<F03department> F03departments { get; set; }

    public virtual DbSet<F03emailProfile> F03emailProfiles { get; set; }

    public virtual DbSet<F03employee> F03employees { get; set; }

    public virtual DbSet<F03employeeTmp> F03employeeTmps { get; set; }

    public virtual DbSet<F03function> F03functions { get; set; }

    public virtual DbSet<F03gender> F03genders { get; set; }

    public virtual DbSet<F03leaveDay> F03leaveDays { get; set; }

    public virtual DbSet<F03leaveDayDetail> F03leaveDayDetails { get; set; }

    public virtual DbSet<F03leaveDaysApprover> F03leaveDaysApprovers { get; set; }

    public virtual DbSet<F03leaveDaysAttachment> F03leaveDaysAttachments { get; set; }

    public virtual DbSet<F03leaveType> F03leaveTypes { get; set; }

    public virtual DbSet<F03levelApprove> F03levelApproves { get; set; }

    public virtual DbSet<F03permission> F03permissions { get; set; }

    public virtual DbSet<F03registerType> F03registerTypes { get; set; }

    public virtual DbSet<F03tongPhep> F03tongPheps { get; set; }

    public virtual DbSet<F03user> F03users { get; set; }

    public virtual DbSet<F03userFunction> F03userFunctions { get; set; }

    public virtual DbSet<F03userLog> F03userLogs { get; set; }

    public virtual DbSet<F03workYear> F03workYears { get; set; }

    public virtual DbSet<TmpPhepton> TmpPheptons { get; set; }

    public virtual DbSet<VEmployeeApprover> VEmployeeApprovers { get; set; }

    public virtual DbSet<VF03employee> VF03employees { get; set; }

    public virtual DbSet<VF03leaveDay> VF03leaveDays { get; set; }

    public virtual DbSet<VF03leaveDayDetail> VF03leaveDayDetails { get; set; }

    public virtual DbSet<VF03leaveDays1> VF03leaveDays1s { get; set; }

    public virtual DbSet<VF03leaveDaysApprover> VF03leaveDaysApprovers { get; set; }

    public virtual DbSet<VF03leaveDaysIntermediate> VF03leaveDaysIntermediates { get; set; }

    public virtual DbSet<VF03leaveType> VF03leaveTypes { get; set; }

    public virtual DbSet<VF03phepTon> VF03phepTons { get; set; }

    public virtual DbSet<VF03phepTon1> VF03phepTon1s { get; set; }

    public virtual DbSet<VF03user> VF03users { get; set; }

    public virtual DbSet<VF03users1> VF03users1s { get; set; }

    public virtual DbSet<VwCurrentlyPresentEmployee> VwCurrentlyPresentEmployees { get; set; }

    public virtual DbSet<VwShiftCheckInOut> VwShiftCheckInOuts { get; set; }
    public virtual DbSet<UserSession> UserSessions { get; set; }
    public virtual DbSet<AppNotification> AppNotifications { get; set; }
    // ===== OT (Tăng ca) =====
    public virtual DbSet<F03OTRequest> F03OTRequests { get; set; }
    public virtual DbSet<F03OTEmployee> F03OTEmployees { get; set; }
    public virtual DbSet<F03OTApprover> F03OTApprovers { get; set; }
    public virtual DbSet<F03OTLimitRule> F03OTLimitRules { get; set; }

    // Views
    public virtual DbSet<VF03OTRequest> VF03OTRequests { get; set; }
    public virtual DbSet<VF03OTSummary> VF03OTSummaries { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=192.168.200.10\\WEBAPPDB;Initial Catalog=FVNWEBAPP;User ID=sa;Password=Fcc@dmin;TrustServerCertificate=True;MultipleActiveResultSets=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AuditLog__3214EC07EF97494C");

            entity.HasIndex(e => e.Action, "IX_AuditLogs_Action");

            entity.HasIndex(e => e.CreatedAt, "IX_AuditLogs_CreatedAt");

            entity.HasIndex(e => e.UserId, "IX_AuditLogs_UserId");

            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UserAgent).HasMaxLength(255);
            entity.Property(e => e.UserName).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_AuditLogs_User");
        });

        modelBuilder.Entity<CompanyHoliday>(entity =>
        {
            entity.ToTable("CompanyHoliday");

            entity.Property(e => e.Description).HasMaxLength(50);
            entity.Property(e => e.HolidayDate).HasColumnType("smalldatetime");
            entity.Property(e => e.TinhPhep).HasDefaultValue(0);
        });

        modelBuilder.Entity<DebugTable>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("DebugTable");

            entity.Property(e => e.EmployeeCode).HasMaxLength(50);
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.LastDknma)
                .HasMaxLength(50)
                .HasColumnName("LastDKNMa");
            entity.Property(e => e.LeaveTypeCode).HasMaxLength(10);
            entity.Property(e => e.NewDknma)
                .HasMaxLength(50)
                .HasColumnName("NewDKNMa");
            entity.Property(e => e.Nvma)
                .HasMaxLength(50)
                .HasColumnName("NVMa");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<EmailLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__EmailLog__3214EC070EEF1E35");

            entity.ToTable("EmailLog");

            entity.Property(e => e.SentAt).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Subject).HasMaxLength(255);
            entity.Property(e => e.ToEmail).HasMaxLength(255);
        });

        modelBuilder.Entity<EmailQueue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__EmailQue__3214EC07BE0F0B0A");

            entity.ToTable("EmailQueue");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.MaxRetry).HasDefaultValue(3);
            entity.Property(e => e.RetryCount).HasDefaultValue(0);
            entity.Property(e => e.SentAt).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Subject).HasMaxLength(255);
            entity.Property(e => e.TemplateCode).HasMaxLength(50);
            entity.Property(e => e.ToEmail).HasMaxLength(255);
        });

        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__EmailTem__3214EC077991E259");

            entity.ToTable("EmailTemplate");

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Subject).HasMaxLength(255);
        });

        modelBuilder.Entity<EmployeeAttendanceView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("EmployeeAttendanceView");

            entity.Property(e => e.CheckInTime).HasColumnType("datetime");
            entity.Property(e => e.CheckOutTime).HasColumnType("datetime");
            entity.Property(e => e.EmployeeId)
                .HasMaxLength(10)
                .HasColumnName("EmployeeID");
            entity.Property(e => e.FullName)
                .HasMaxLength(50)
                .IsFixedLength();
        });

        modelBuilder.Entity<EscalationLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Escalati__3214EC07F92AB3EA");

            entity.Property(e => e.Action).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<EscalationRule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Escalati__3214EC07BADCAA58");

            entity.Property(e => e.DeptCode).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LeaveTypeCode).HasMaxLength(50);
        });

        modelBuilder.Entity<F03cv>(entity =>
        {
            entity.ToTable("F03CV");

            entity.HasIndex(e => e.Cvcode, "IX_B20DmCV_Ma_Bp")
                .IsUnique()
                .HasFillFactor(90);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasDefaultValue(-1);
            entity.Property(e => e.Cvcode)
                .HasMaxLength(30)
                .HasColumnName("CVCode");
            entity.Property(e => e.Cvname)
                .HasMaxLength(64)
                .HasDefaultValue("")
                .HasColumnName("CVName");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy).HasDefaultValue(-1);
        });

        modelBuilder.Entity<F03department>(entity =>
        {
            entity.ToTable("F03Department");

            entity.HasIndex(e => e.DeptCode, "IX_B20DmBp_Ma_Bp")
                .IsUnique()
                .HasFillFactor(90);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasDefaultValue(-1);
            entity.Property(e => e.DeptCode).HasMaxLength(30);
            entity.Property(e => e.DeptName)
                .HasMaxLength(64)
                .HasDefaultValue("");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy).HasDefaultValue(-1);
        });

        modelBuilder.Entity<F03emailProfile>(entity =>
        {
            entity.ToTable("F03EmailProfile");

            entity.Property(e => e.Code).HasMaxLength(30);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasDefaultValue(-1);
            entity.Property(e => e.EmailAccountName).HasMaxLength(192);
            entity.Property(e => e.EmailAddress).HasMaxLength(192);
            entity.Property(e => e.EmailPassword).HasMaxLength(192);
            entity.Property(e => e.EmailServerEnableSsl).HasColumnName("EmailServerEnable_SSL");
            entity.Property(e => e.EmailServerName).HasMaxLength(192);
            entity.Property(e => e.EmailServerPort).HasDefaultValue(25);
            entity.Property(e => e.EmailServerType)
                .HasMaxLength(192)
                .HasDefaultValue("SMTP");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy).HasDefaultValue(-1);
            entity.Property(e => e.Name).HasMaxLength(192);
            entity.Property(e => e.Name2)
                .HasMaxLength(192)
                .HasDefaultValue("");
            entity.Property(e => e.ParentId).HasDefaultValue(-1);
            entity.Property(e => e.SiteUrl)
                .HasMaxLength(500)
                .HasColumnName("SiteURL");
            entity.Property(e => e.Timestamp)
                .IsRowVersion()
                .IsConcurrencyToken()
                .HasColumnName("timestamp");
        });

        modelBuilder.Entity<F03employee>(entity =>
        {
            entity.HasKey(e => e.Id).IsClustered(false);

            entity.ToTable("F03Employee", tb => tb.HasTrigger("tgr_F03Employee_InsertUpdate_User"));

            entity.HasIndex(e => e.EmployeeCode, "IX_F03Employee_Code")
                .IsUnique()
                .IsClustered()
                .HasFillFactor(90);

            entity.Property(e => e.BirthDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasDefaultValue(-1);
            entity.Property(e => e.Cvcode)
                .HasMaxLength(50)
                .HasColumnName("CVCode");
            entity.Property(e => e.DeptCode)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.EmailAddress)
                .HasMaxLength(100)
                .HasDefaultValue("pc1@fcc-vn.com.vn");
            entity.Property(e => e.EmployeeCode).HasMaxLength(30);
            entity.Property(e => e.EmployeeName)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.EndWorkingDate).HasColumnType("datetime");
            entity.Property(e => e.FirstWorkingDate).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy).HasDefaultValue(-1);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.TongPhep).HasColumnType("numeric(18, 2)");
        });

        modelBuilder.Entity<F03employeeTmp>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("F03EmployeeTMP", tb => tb.HasTrigger("tg_TMP_to_Epp"));

            entity.Property(e => e.BirthDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Cvcode)
                .HasMaxLength(50)
                .HasColumnName("CVCode");
            entity.Property(e => e.DeptCode).HasMaxLength(50);
            entity.Property(e => e.EmailAddress).HasMaxLength(100);
            entity.Property(e => e.EmployeeCode).HasMaxLength(30);
            entity.Property(e => e.EmployeeName).HasMaxLength(50);
            entity.Property(e => e.EndWorkingDate).HasColumnType("datetime");
            entity.Property(e => e.FirstWorkingDate).HasColumnType("datetime");
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.TongPhep).HasColumnType("numeric(18, 2)");
        });

        modelBuilder.Entity<F03function>(entity =>
        {
            entity.HasKey(e => e.IdFunction).HasName("PK_F03UserFunction");

            entity.ToTable("F03Function");

            entity.HasIndex(e => e.FunctionCode, "UQ__F03UserF__E037746C889EE601").IsUnique();

            entity.Property(e => e.Detail)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.FunctionName).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Timestamps)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("timestamps");
        });

        modelBuilder.Entity<F03gender>(entity =>
        {
            entity.HasKey(e => e.Id).IsClustered(false);

            entity.ToTable("F03Gender");

            entity.HasIndex(e => e.GenderCode, "IX_F03Gender_Code")
                .IsUnique()
                .IsClustered()
                .HasFillFactor(90);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasDefaultValue(-1);
            entity.Property(e => e.GenderCode)
                .HasMaxLength(30)
                .HasDefaultValue("");
            entity.Property(e => e.GenderName)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy).HasDefaultValue(-1);
        });

        modelBuilder.Entity<F03leaveDay>(entity =>
        {
            entity.ToTable("F03LeaveDays", tb => tb.HasTrigger("trg_InsertIntoHRM"));

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasDefaultValue(-1);
            entity.Property(e => e.EmployeeCode).HasMaxLength(50);
            entity.Property(e => e.EndDate).HasColumnType("smalldatetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastSync)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LeaveReason).HasMaxLength(500);
            entity.Property(e => e.LeaveTypeCode).HasMaxLength(10);
            entity.Property(e => e.Level1ApproveCode).HasMaxLength(30);
            entity.Property(e => e.Level1ApproveEmail).HasMaxLength(50);
            entity.Property(e => e.Level1ApproveName).HasMaxLength(50);
            entity.Property(e => e.Level1ApproveTime).HasColumnType("datetime");
            entity.Property(e => e.Level1Comment).HasMaxLength(500);
            entity.Property(e => e.Level2ApproveCode).HasMaxLength(30);
            entity.Property(e => e.Level2ApproveEmail).HasMaxLength(50);
            entity.Property(e => e.Level2ApproveName).HasMaxLength(50);
            entity.Property(e => e.Level2ApproveTime).HasColumnType("datetime");
            entity.Property(e => e.Level2Comment).HasMaxLength(500);
            entity.Property(e => e.Level3ApproveCode).HasMaxLength(30);
            entity.Property(e => e.Level3ApproveEmail).HasMaxLength(50);
            entity.Property(e => e.Level3ApproveName).HasMaxLength(50);
            entity.Property(e => e.Level3ApproveTime).HasColumnType("datetime");
            entity.Property(e => e.Level3Comment).HasMaxLength(200);
            entity.Property(e => e.ModifiedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy).HasDefaultValue(-1);
            entity.Property(e => e.NotifiedLv1At).HasColumnType("datetime");
            entity.Property(e => e.NotifiedLv2At).HasColumnType("datetime");
            entity.Property(e => e.NotifiedLv3At).HasColumnType("datetime");
            entity.Property(e => e.RegisterDate).HasColumnType("smalldatetime");
            entity.Property(e => e.RequestStatus).HasMaxLength(50);
            entity.Property(e => e.StartDate).HasColumnType("smalldatetime");
            entity.Property(e => e.Sync).HasDefaultValue(false);
            entity.Property(e => e.TotalDay).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TotalLeaveDay).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.WorkYear).HasDefaultValueSql("(datepart(year,getdate()))");
        });

        modelBuilder.Entity<F03leaveDayDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__F03Leave__3214EC07CC8E96FC");

            entity.ToTable("F03LeaveDayDetails");

            entity.HasIndex(e => e.LeaveDate, "IX_F03LeaveDayDetails_LeaveDate");

            entity.HasIndex(e => e.LeaveDaysId, "IX_F03LeaveDayDetails_RequestId");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DayValue).HasColumnType("decimal(3, 1)");
            entity.Property(e => e.HalfDayOption).HasMaxLength(10);
            entity.Property(e => e.LeaveTypeCode).HasMaxLength(10);
            entity.Property(e => e.LeaveTypeName).HasMaxLength(100);

            entity.HasOne(d => d.LeaveDays).WithMany(p => p.F03leaveDayDetails)
                .HasForeignKey(d => d.LeaveDaysId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_F03LeaveDayDetails_F03LeaveDays");
        });

        modelBuilder.Entity<F03leaveDaysApprover>(entity =>
        {
            entity.ToTable("F03LeaveDaysApprover");

            entity.Property(e => e.ApproveLevel).HasDefaultValueSql("('')");
            entity.Property(e => e.ApproveLevel1Email).HasMaxLength(255);
            entity.Property(e => e.ApproveLevel1Name).HasMaxLength(255);
            entity.Property(e => e.ApproveLevel2Email).HasMaxLength(255);
            entity.Property(e => e.ApproveLevel2Name).HasMaxLength(255);
            entity.Property(e => e.ApproveLevel3Email).HasMaxLength(255);
            entity.Property(e => e.ApproveLevel3Name).HasMaxLength(255);
            entity.Property(e => e.ApproveLevelCode).HasMaxLength(30);
            entity.Property(e => e.ApproveLevelEmail).HasMaxLength(255);
            entity.Property(e => e.ApproveLevelName)
                .HasMaxLength(255)
                .HasDefaultValue("");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasDefaultValue(-1);
            entity.Property(e => e.DeptCode).HasMaxLength(30);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy).HasDefaultValue(-1);
        });

        modelBuilder.Entity<F03leaveDaysAttachment>(entity =>
        {
            entity.HasKey(e => e.FileId).HasName("PK_F03FormFile");

            entity.ToTable("F03LeaveDaysAttachments");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasDefaultValue(-1);
            entity.Property(e => e.FileName).HasMaxLength(200);
            entity.Property(e => e.FilePath).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<F03leaveType>(entity =>
        {
            entity.HasKey(e => e.LeaveTypeId);

            entity.ToTable("F03LeaveType");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasDefaultValue(-1);
            entity.Property(e => e.Hrmcode)
                .HasMaxLength(50)
                .HasDefaultValue("")
                .HasColumnName("HRMCode");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LeaveTypeCode)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.LeaveTypeName)
                .HasMaxLength(200)
                .HasDefaultValue("");
            entity.Property(e => e.LeaveTypeName2)
                .HasMaxLength(200)
                .HasDefaultValue("");
            entity.Property(e => e.ModifiedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy).HasDefaultValue(-1);
            entity.Property(e => e.TinhPhep).HasDefaultValue(false);
        });

        modelBuilder.Entity<F03levelApprove>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("F03LevelApprove");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.LevelId).HasColumnName("LevelID");
            entity.Property(e => e.LevelName).HasMaxLength(50);
        });

        modelBuilder.Entity<F03permission>(entity =>
        {
            entity.HasKey(e => e.IdPermission).HasName("PK__permissi__5180B3BFE43D88CB");

            entity.ToTable("F03Permissions");

            entity.HasIndex(e => e.PermissionCode, "UQ__F03Permi__91FE5750BB44D8EB").IsUnique();

            entity.Property(e => e.Detail)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PermissionName).HasMaxLength(50);
            entity.Property(e => e.Timestamps)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("timestamps");
        });

        modelBuilder.Entity<F03registerType>(entity =>
        {
            entity.HasKey(e => e.RegisterId);

            entity.ToTable("F03RegisterType");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasDefaultValue(-1);
            entity.Property(e => e.HrmregisterCode)
                .HasMaxLength(50)
                .HasDefaultValue("")
                .HasColumnName("HRMRegisterCode");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy).HasDefaultValue(-1);
            entity.Property(e => e.RegisterName)
                .HasMaxLength(200)
                .HasDefaultValue("");
            entity.Property(e => e.TotalDay).HasColumnType("numeric(18, 2)");
        });

        modelBuilder.Entity<F03tongPhep>(entity =>
        {
            entity.ToTable("F03TongPhep");

            entity.HasIndex(e => new { e.EmployeeCode, e.WorkYear }, "UQ__F03TongP__2D246E3FE5A0C38B").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasDefaultValue(-1);
            entity.Property(e => e.EmployeeCode).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy).HasDefaultValue(-1);
            entity.Property(e => e.TongPhep).HasColumnType("numeric(18, 2)");
        });

        modelBuilder.Entity<F03user>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("PK_F03Admins");

            entity.ToTable("F03Users");

            entity.Property(e => e.Avatar)
                .HasMaxLength(255)
                .HasDefaultValue("/Content/images/avatar/avatar-default.jpg");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasDefaultValue(-1);
            entity.Property(e => e.Cvcode)
                .HasMaxLength(50)
                .HasColumnName("CVCode");
            entity.Property(e => e.DeptCode).HasMaxLength(50);
            entity.Property(e => e.EmployeeCode).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastLogin).HasColumnType("datetime");
            entity.Property(e => e.LockoutEnable).HasDefaultValue(false);
            entity.Property(e => e.LockoutEndDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy).HasDefaultValue(-1);
            entity.Property(e => e.NumLoginFailed).HasDefaultValue(0);
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.PermissionCode).HasDefaultValue(-1);
            entity.Property(e => e.UserName).HasMaxLength(50);

            entity.HasOne(d => d.PermissionCodeNavigation).WithMany(p => p.F03users)
                .HasForeignKey(d => d.PermissionCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__admins__id_permi__46E78A0C");
        });

        modelBuilder.Entity<F03userFunction>(entity =>
        {
            entity.HasKey(e => new { e.IdUser, e.IdFunction }).HasName("PK_UserFunction");

            entity.ToTable("F03UserFunctions");

            entity.HasOne(d => d.IdPermissionNavigation).WithMany(p => p.F03userFunctions)
                .HasForeignKey(d => d.IdPermission)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserFunction_Permission");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.F03userFunctions)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserFunction_User");
        });

        modelBuilder.Entity<F03userLog>(entity =>
        {
            entity.HasKey(e => e.LogId);

            entity.ToTable("F03UserLog");

            entity.Property(e => e.ApplicationName)
                .HasMaxLength(30)
                .HasDefaultValue("");
            entity.Property(e => e.ApplicationVerion)
                .HasMaxLength(30)
                .HasDefaultValue("");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LastSeen)
                .HasMaxLength(100)
                .HasColumnName("Last_Seen");
            entity.Property(e => e.LastSeenUrl)
                .HasMaxLength(100)
                .HasColumnName("Last_Seen_Url");
            entity.Property(e => e.WorkstationName)
                .HasMaxLength(30)
                .HasDefaultValue("");
            entity.Property(e => e.WorkstationUser)
                .HasMaxLength(30)
                .HasDefaultValue("");
        });

        modelBuilder.Entity<F03workYear>(entity =>
        {
            entity.ToTable("F03WorkYear");

            entity.HasIndex(e => e.WorkYear, "UQ__F03WorkY__2404B77335E9A685").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasDefaultValue(-1);
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedAt)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy).HasDefaultValue(-1);
            entity.Property(e => e.Remark)
                .HasMaxLength(500)
                .HasDefaultValue("");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TmpPhepton>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TMP_PHEPTON");

            entity.Property(e => e.Mnv)
                .HasMaxLength(50)
                .HasColumnName("MNV");
            entity.Property(e => e.Pt)
                .HasColumnType("numeric(18, 0)")
                .HasColumnName("PT");
            entity.Property(e => e.Ttp).HasColumnName("TTP");
        });

        modelBuilder.Entity<VEmployeeApprover>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vEmployeeApprover");

            entity.Property(e => e.ApproveLevelCode).HasMaxLength(30);
            entity.Property(e => e.ApproverEmail).HasMaxLength(255);
            entity.Property(e => e.ApproverName).HasMaxLength(255);
            entity.Property(e => e.DeptCode).HasMaxLength(30);
            entity.Property(e => e.DeptName).HasMaxLength(64);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<VF03employee>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vF03Employee");

            entity.Property(e => e.BirthDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Cvcode)
                .HasMaxLength(30)
                .HasColumnName("CVCode");
            entity.Property(e => e.Cvname)
                .HasMaxLength(64)
                .HasColumnName("CVName");
            entity.Property(e => e.DeptCode).HasMaxLength(30);
            entity.Property(e => e.DeptName).HasMaxLength(64);
            entity.Property(e => e.EmailAddress).HasMaxLength(100);
            entity.Property(e => e.EmployeeCode).HasMaxLength(30);
            entity.Property(e => e.EmployeeName).HasMaxLength(50);
            entity.Property(e => e.EndWorkingDate).HasColumnType("datetime");
            entity.Property(e => e.FirstWorkingDate).HasColumnType("datetime");
            entity.Property(e => e.GenderName).HasMaxLength(50);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.TongPhep).HasColumnType("numeric(18, 2)");
        });

        modelBuilder.Entity<VF03leaveDay>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vF03LeaveDays");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedAtText).HasMaxLength(4000);
            entity.Property(e => e.Cvcode)
                .HasMaxLength(30)
                .HasColumnName("CVCode");
            entity.Property(e => e.Cvname)
                .HasMaxLength(64)
                .HasColumnName("CVName");
            entity.Property(e => e.DeptCode).HasMaxLength(30);
            entity.Property(e => e.DeptName).HasMaxLength(64);
            entity.Property(e => e.EmailAddress).HasMaxLength(100);
            entity.Property(e => e.EmployeeCode).HasMaxLength(50);
            entity.Property(e => e.EmployeeName).HasMaxLength(50);
            entity.Property(e => e.EndDate).HasColumnType("smalldatetime");
            entity.Property(e => e.GenderName).HasMaxLength(50);
            entity.Property(e => e.LastSync).HasColumnType("datetime");
            entity.Property(e => e.LeaveDay).HasMaxLength(4000);
            entity.Property(e => e.LeaveReason).HasMaxLength(500);
            entity.Property(e => e.Level1ApproveCode).HasMaxLength(30);
            entity.Property(e => e.Level1ApproveEmail).HasMaxLength(50);
            entity.Property(e => e.Level1ApproveName).HasMaxLength(50);
            entity.Property(e => e.Level1ApproveTime).HasColumnType("datetime");
            entity.Property(e => e.Level1ApproveTimeText).HasMaxLength(4000);
            entity.Property(e => e.Level1Comment).HasMaxLength(500);
            entity.Property(e => e.Level1IsApproveText).HasMaxLength(9);
            entity.Property(e => e.Level2ApproveCode).HasMaxLength(30);
            entity.Property(e => e.Level2ApproveEmail).HasMaxLength(50);
            entity.Property(e => e.Level2ApproveName).HasMaxLength(50);
            entity.Property(e => e.Level2ApproveTime).HasColumnType("datetime");
            entity.Property(e => e.Level2ApproveTimeText).HasMaxLength(4000);
            entity.Property(e => e.Level2Comment).HasMaxLength(500);
            entity.Property(e => e.Level2IsApproveText).HasMaxLength(9);
            entity.Property(e => e.Level3ApproveCode).HasMaxLength(30);
            entity.Property(e => e.Level3ApproveEmail).HasMaxLength(50);
            entity.Property(e => e.Level3ApproveName).HasMaxLength(50);
            entity.Property(e => e.Level3ApproveTime).HasColumnType("datetime");
            entity.Property(e => e.Level3Comment).HasMaxLength(200);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.PhepTon).HasColumnType("numeric(38, 1)");
            entity.Property(e => e.RegisterDate).HasColumnType("smalldatetime");
            entity.Property(e => e.RequestStatus).HasMaxLength(50);
            entity.Property(e => e.StartDate).HasColumnType("smalldatetime");
            entity.Property(e => e.TongPhep).HasColumnType("numeric(18, 2)");
            entity.Property(e => e.TotalDay).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TotalLeaveDay).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<VF03leaveDayDetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vF03LeaveDayDetails");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DayValue).HasColumnType("decimal(3, 1)");
            entity.Property(e => e.DeptCode).HasMaxLength(30);
            entity.Property(e => e.DeptName).HasMaxLength(64);
            entity.Property(e => e.EmailAddress).HasMaxLength(100);
            entity.Property(e => e.EmployeeCode).HasMaxLength(50);
            entity.Property(e => e.EmployeeName).HasMaxLength(50);
            entity.Property(e => e.EndDate).HasColumnType("smalldatetime");
            entity.Property(e => e.HalfDayOption).HasMaxLength(10);
            entity.Property(e => e.LeaveDateText).HasMaxLength(4000);
            entity.Property(e => e.LeaveTypeCode).HasMaxLength(10);
            entity.Property(e => e.LeaveTypeName).HasMaxLength(100);
            entity.Property(e => e.Level1ApproveName).HasMaxLength(50);
            entity.Property(e => e.Level2ApproveName).HasMaxLength(50);
            entity.Property(e => e.Level3ApproveName).HasMaxLength(50);
            entity.Property(e => e.RequestStatus).HasMaxLength(50);
            entity.Property(e => e.StartDate).HasColumnType("smalldatetime");
        });

        modelBuilder.Entity<VF03leaveDays1>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vF03LeaveDays1");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedAtText).HasMaxLength(4000);
            entity.Property(e => e.Cvcode)
                .HasMaxLength(30)
                .HasColumnName("CVCode");
            entity.Property(e => e.Cvname)
                .HasMaxLength(64)
                .HasColumnName("CVName");
            entity.Property(e => e.DeptCode).HasMaxLength(30);
            entity.Property(e => e.DeptName).HasMaxLength(64);
            entity.Property(e => e.EmailAddress).HasMaxLength(100);
            entity.Property(e => e.EmployeeCode).HasMaxLength(50);
            entity.Property(e => e.EmployeeName).HasMaxLength(50);
            entity.Property(e => e.EndDate).HasColumnType("smalldatetime");
            entity.Property(e => e.GenderName).HasMaxLength(50);
            entity.Property(e => e.HalfDayOption).HasMaxLength(10);
            entity.Property(e => e.Hrmcode)
                .HasMaxLength(50)
                .HasColumnName("HRMCode");
            entity.Property(e => e.LeaveDayText).HasMaxLength(4000);
            entity.Property(e => e.LeaveDurationText).HasMaxLength(16);
            entity.Property(e => e.LeaveReason).HasMaxLength(50);
            entity.Property(e => e.LeaveTypeCode).HasMaxLength(10);
            entity.Property(e => e.LeaveTypeName).HasMaxLength(200);
            entity.Property(e => e.Level1ApproveEmail).HasMaxLength(50);
            entity.Property(e => e.Level1ApproveName).HasMaxLength(50);
            entity.Property(e => e.Level1ApproveTime).HasColumnType("datetime");
            entity.Property(e => e.Level1ApproveTimeText).HasMaxLength(4000);
            entity.Property(e => e.Level1Comment).HasMaxLength(500);
            entity.Property(e => e.Level1IsApproveText).HasMaxLength(9);
            entity.Property(e => e.Level2ApproveEmail).HasMaxLength(50);
            entity.Property(e => e.Level2ApproveName).HasMaxLength(50);
            entity.Property(e => e.Level2ApproveTime).HasColumnType("datetime");
            entity.Property(e => e.Level2ApproveTimeText).HasMaxLength(4000);
            entity.Property(e => e.Level2Comment).HasMaxLength(500);
            entity.Property(e => e.Level2IsApproveText).HasMaxLength(9);
            entity.Property(e => e.Level3ApproveEmail).HasMaxLength(50);
            entity.Property(e => e.Level3ApproveName).HasMaxLength(50);
            entity.Property(e => e.Level3ApproveTime).HasColumnType("datetime");
            entity.Property(e => e.Level3Comment).HasMaxLength(200);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.RegisterDate).HasColumnType("smalldatetime");
            entity.Property(e => e.RequestStatus).HasMaxLength(50);
            entity.Property(e => e.StartDate).HasColumnType("smalldatetime");
            entity.Property(e => e.TotalLeaveDay).HasColumnType("numeric(18, 2)");
            entity.Property(e => e.TotalPaidLeaveDay).HasColumnType("numeric(18, 2)");
            entity.Property(e => e.TotalUnpaidLeaveDay).HasColumnType("numeric(18, 2)");
        });

        modelBuilder.Entity<VF03leaveDaysApprover>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vF03LeaveDaysApprover");

            entity.Property(e => e.ApproveLevelCode).HasMaxLength(30);
            entity.Property(e => e.ApproveLevelEmail).HasMaxLength(255);
            entity.Property(e => e.ApproveLevelName).HasMaxLength(255);
            entity.Property(e => e.DeptCode).HasMaxLength(30);
            entity.Property(e => e.DeptName).HasMaxLength(64);
            entity.Property(e => e.LevelName).HasMaxLength(50);
        });

        modelBuilder.Entity<VF03leaveDaysIntermediate>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vF03LeaveDaysIntermediate");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedAtText).HasMaxLength(4000);
            entity.Property(e => e.Cvcode)
                .HasMaxLength(30)
                .HasColumnName("CVCode");
            entity.Property(e => e.Cvname)
                .HasMaxLength(64)
                .HasColumnName("CVName");
            entity.Property(e => e.DeptCode).HasMaxLength(30);
            entity.Property(e => e.DeptName).HasMaxLength(64);
            entity.Property(e => e.EmailAddress).HasMaxLength(100);
            entity.Property(e => e.EmployeeCode).HasMaxLength(50);
            entity.Property(e => e.EmployeeName).HasMaxLength(50);
            entity.Property(e => e.EndDate).HasColumnType("smalldatetime");
            entity.Property(e => e.GenderName).HasMaxLength(50);
            entity.Property(e => e.LastSync).HasColumnType("datetime");
            entity.Property(e => e.LeaveDay).HasMaxLength(4000);
            entity.Property(e => e.LeaveReason).HasMaxLength(500);
            entity.Property(e => e.Level1ApproveCode).HasMaxLength(30);
            entity.Property(e => e.Level1ApproveEmail).HasMaxLength(50);
            entity.Property(e => e.Level1ApproveName).HasMaxLength(50);
            entity.Property(e => e.Level1ApproveTime).HasColumnType("datetime");
            entity.Property(e => e.Level1ApproveTimeText).HasMaxLength(4000);
            entity.Property(e => e.Level1Comment).HasMaxLength(500);
            entity.Property(e => e.Level1IsApproveText).HasMaxLength(9);
            entity.Property(e => e.Level2ApproveCode).HasMaxLength(30);
            entity.Property(e => e.Level2ApproveEmail).HasMaxLength(50);
            entity.Property(e => e.Level2ApproveName).HasMaxLength(50);
            entity.Property(e => e.Level2ApproveTime).HasColumnType("datetime");
            entity.Property(e => e.Level2ApproveTimeText).HasMaxLength(4000);
            entity.Property(e => e.Level2Comment).HasMaxLength(500);
            entity.Property(e => e.Level2IsApproveText).HasMaxLength(9);
            entity.Property(e => e.Level3ApproveCode).HasMaxLength(30);
            entity.Property(e => e.Level3ApproveEmail).HasMaxLength(50);
            entity.Property(e => e.Level3ApproveName).HasMaxLength(50);
            entity.Property(e => e.Level3ApproveTime).HasColumnType("datetime");
            entity.Property(e => e.Level3Comment).HasMaxLength(200);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.RegisterDate).HasColumnType("smalldatetime");
            entity.Property(e => e.RequestStatus).HasMaxLength(50);
            entity.Property(e => e.StartDate).HasColumnType("smalldatetime");
            entity.Property(e => e.TotalDay).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TotalLeaveDay).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<VF03leaveType>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vF03LeaveType");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Hrmcode)
                .HasMaxLength(50)
                .HasColumnName("HRMCode");
            entity.Property(e => e.LeaveTypeCode).HasMaxLength(50);
            entity.Property(e => e.LeaveTypeId).ValueGeneratedOnAdd();
            entity.Property(e => e.LeaveTypeName).HasMaxLength(200);
            entity.Property(e => e.LeaveTypeName2).HasMaxLength(200);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.TinhPhepText).HasMaxLength(5);
        });

        modelBuilder.Entity<VF03phepTon>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vF03PhepTon");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DeptCode).HasMaxLength(30);
            entity.Property(e => e.DeptName).HasMaxLength(64);
            entity.Property(e => e.EmployeeCode).HasMaxLength(50);
            entity.Property(e => e.EmployeeName).HasMaxLength(50);
            entity.Property(e => e.GenderName).HasMaxLength(50);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.PhepTon).HasColumnType("numeric(38, 1)");
            entity.Property(e => e.SoNgayNghiPhep).HasColumnType("decimal(38, 1)");
            entity.Property(e => e.TongPhep).HasColumnType("numeric(18, 2)");
            entity.Property(e => e.TongSoNgayNghi).HasColumnType("decimal(38, 1)");
        });

        modelBuilder.Entity<VF03phepTon1>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vF03PhepTon1");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DeptCode).HasMaxLength(30);
            entity.Property(e => e.DeptName).HasMaxLength(64);
            entity.Property(e => e.EmployeeCode).HasMaxLength(50);
            entity.Property(e => e.EmployeeName).HasMaxLength(50);
            entity.Property(e => e.GenderName).HasMaxLength(50);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.PhepTon).HasColumnType("numeric(38, 2)");
            entity.Property(e => e.SoNgayNghiPhep).HasColumnType("numeric(38, 2)");
            entity.Property(e => e.TongPhep).HasColumnType("numeric(18, 2)");
            entity.Property(e => e.TongSoNgayNghi).HasColumnType("numeric(38, 2)");
        });

        modelBuilder.Entity<VF03user>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vF03Users");

            entity.Property(e => e.Avatar).HasMaxLength(255);
            entity.Property(e => e.BirthDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Cvcode)
                .HasMaxLength(30)
                .HasColumnName("CVCode");
            entity.Property(e => e.DeptCode).HasMaxLength(30);
            entity.Property(e => e.DeptName).HasMaxLength(64);
            entity.Property(e => e.EmailAddress).HasMaxLength(100);
            entity.Property(e => e.EmployeeCode).HasMaxLength(50);
            entity.Property(e => e.EmployeeName).HasMaxLength(50);
            entity.Property(e => e.GenderName).HasMaxLength(50);
            entity.Property(e => e.LastLogin).HasColumnType("datetime");
            entity.Property(e => e.LockoutEndDate).HasColumnType("datetime");
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.PermissionName).HasMaxLength(50);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        modelBuilder.Entity<VF03users1>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vF03Users1");

            entity.Property(e => e.ApproveLevelName).HasMaxLength(50);
            entity.Property(e => e.Avatar).HasMaxLength(255);
            entity.Property(e => e.BirthDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Cvcode)
                .HasMaxLength(30)
                .HasColumnName("CVCode");
            entity.Property(e => e.DeptCode).HasMaxLength(30);
            entity.Property(e => e.DeptName).HasMaxLength(64);
            entity.Property(e => e.EmailAddress).HasMaxLength(100);
            entity.Property(e => e.EmployeeCode).HasMaxLength(50);
            entity.Property(e => e.EmployeeName).HasMaxLength(50);
            entity.Property(e => e.GenderName).HasMaxLength(50);
            entity.Property(e => e.LastLogin).HasColumnType("datetime");
            entity.Property(e => e.LockoutEndDate).HasColumnType("datetime");
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.PermissionName).HasMaxLength(50);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        modelBuilder.Entity<VwCurrentlyPresentEmployee>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_CurrentlyPresentEmployees");

            entity.Property(e => e.CheckInTime).HasColumnType("datetime");
            entity.Property(e => e.DeptName).HasMaxLength(64);
            entity.Property(e => e.EmployeeId)
                .HasMaxLength(10)
                .HasColumnName("EmployeeID");
            entity.Property(e => e.FullName)
                .HasMaxLength(50)
                .IsFixedLength();
        });

        modelBuilder.Entity<VwShiftCheckInOut>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_ShiftCheckInOut");

            entity.Property(e => e.CheckInTime).HasColumnType("datetime");
            entity.Property(e => e.CheckOutTime).HasColumnType("datetime");
            entity.Property(e => e.EmployeeId)
                .HasMaxLength(10)
                .HasColumnName("EmployeeID");
            entity.Property(e => e.FullName)
                .HasMaxLength(50)
                .IsFixedLength();
        });
        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.ToTable("UserSession");

            entity.HasKey(e => e.Id);

            // 1. Cấu hình các cột chuỗi dạng VARCHAR (Không chứa Unicode)
            entity.Property(e => e.DeviceType)
                  .HasMaxLength(20)
                  .HasColumnType("varchar")
                  .IsRequired(); // NOT NULL

            entity.Property(e => e.DeviceId)
                  .HasMaxLength(200)
                  .HasColumnType("varchar")
                  .IsRequired(); // NOT NULL

            entity.Property(e => e.JwtToken)
                  .HasMaxLength(2000)
                  .HasColumnType("varchar"); // NULL

            entity.Property(e => e.SignalRConnectionId)
                  .HasMaxLength(200)
                  .HasColumnType("varchar"); // NULL

            // 2. Cấu hình các cột chuỗi dạng NVARCHAR (Có chứa Unicode)
            entity.Property(e => e.DeviceName)
                  .HasMaxLength(100); // NULL

            // 3. Cấu hình trạng thái hoạt động (Khớp 100% tên IsActive)
            entity.Property(e => e.IsActive)
                  .HasColumnName("IsActive")
                  .HasColumnType("bit")
                  .HasDefaultValueSql("((1))")
                  .IsRequired(); // NOT NULL

            // 4. Cấu hình các cột ngày tháng
            entity.Property(e => e.CreatedAt)
                  .HasColumnType("datetime")
                  .HasDefaultValueSql("(getdate())")
                  .IsRequired(); // NOT NULL

            entity.Property(e => e.LastSeenAt)
                  .HasColumnType("datetime"); // NULL

            entity.Property(e => e.RevokedAt)
                  .HasColumnType("datetime"); // NULL

            // 5. Cấu hình các INDEX (Đặc biệt là Filtered Index)
            // Index thông thường phục vụ query tìm session active của một user
            entity.HasIndex(e => new { e.UserId, e.IsActive }, "IX_UserSession_UserId_IsActive");

            // UNIQUE INDEX có kèm điều kiện WHERE IsActive = 1 dưới SQL (Filtered Index)
            entity.HasIndex(e => e.DeviceId, "IX_UserSession_DeviceId")
                  .IsUnique()
                  .HasFilter("[IsActive] = 1");

            // 6. Cấu hình khóa ngoại trỏ chính xác đến bảng F03Users trường IdUser
            entity.HasOne(d => d.User)
                  .WithMany() // Để trống nếu class F03user không cần quản lý danh sách UserSessions
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK_UserSession_User");
        });
        modelBuilder.Entity<AppNotification>(entity =>
        {
            entity.ToTable("AppNotification");
            entity.HasKey(e => e.Id).HasName("PK_AppNotification");
            entity.HasIndex(e => new { e.UserId, e.IsRead }, "IX_AppNotification_UserId_IsRead");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.EmployeeCode).HasMaxLength(50);
            entity.Property(e => e.NotificationType).HasMaxLength(50).HasDefaultValue("SYSTEM");
            entity.Property(e => e.Title).HasMaxLength(200).HasDefaultValue("");
            entity.Property(e => e.ActionUrl).HasMaxLength(500);
            entity.Property(e => e.ReadAt).HasColumnType("datetime");
        });
        // ===== F03OTRequest =====
        modelBuilder.Entity<F03OTRequest>(entity =>
        {
            entity.ToTable("F03OTRequest");

            entity.Property(e => e.OTCode).HasMaxLength(50);
            entity.Property(e => e.EmployeeCode).HasMaxLength(50);
            entity.Property(e => e.DeptCode).HasMaxLength(30);
            entity.Property(e => e.OTTypeCode).HasMaxLength(20);
            entity.Property(e => e.OTReason).HasMaxLength(500);
            entity.Property(e => e.RequestStatus).HasMaxLength(50);
            entity.Property(e => e.ScopeType).HasMaxLength(20);
            entity.Property(e => e.TotalOTHours).HasColumnType("decimal(5,2)");
            entity.Property(e => e.PlannedHours).HasColumnType("decimal(5,2)");

            entity.Property(e => e.Level3ApproveEmail).HasMaxLength(100);
            entity.Property(e => e.Level3ApproveCode).HasMaxLength(30);
            entity.Property(e => e.Level3ApproveName).HasMaxLength(100);
            entity.Property(e => e.Level3ApproveTime).HasColumnType("datetime");
            entity.Property(e => e.Level3Comment).HasMaxLength(500);

            entity.Property(e => e.Level5ApproveEmail).HasMaxLength(100);
            entity.Property(e => e.Level5ApproveCode).HasMaxLength(30);
            entity.Property(e => e.Level5ApproveName).HasMaxLength(100);
            entity.Property(e => e.Level5ApproveTime).HasColumnType("datetime");
            entity.Property(e => e.Level5Comment).HasMaxLength(500);

            entity.Property(e => e.Level6ApproveEmail).HasMaxLength(100);
            entity.Property(e => e.Level6ApproveCode).HasMaxLength(30);
            entity.Property(e => e.Level6ApproveName).HasMaxLength(100);
            entity.Property(e => e.Level6ApproveTime).HasColumnType("datetime");
            entity.Property(e => e.Level6Comment).HasMaxLength(500);

            entity.Property(e => e.Level7ApproveEmail).HasMaxLength(100);
            entity.Property(e => e.Level7ApproveCode).HasMaxLength(30);
            entity.Property(e => e.Level7ApproveName).HasMaxLength(100);
            entity.Property(e => e.Level7ApproveTime).HasColumnType("datetime");
            entity.Property(e => e.Level7Comment).HasMaxLength(500);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasDefaultValue(-1);
            entity.Property(e => e.ModifiedBy).HasDefaultValue(-1);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasMany(e => e.F03OTEmployees)
                .WithOne(e => e.OTRequest)
                .HasForeignKey(e => e.OTRequestId)
                .HasConstraintName("FK_F03OTEmployee_F03OTRequest");
        });

        // ===== F03OTEmployee =====
        modelBuilder.Entity<F03OTEmployee>(entity =>
        {
            entity.ToTable("F03OTEmployee");

            entity.Property(e => e.EmployeeCode).HasMaxLength(50);
            entity.Property(e => e.EmployeeName).HasMaxLength(100);
            entity.Property(e => e.DeptCode).HasMaxLength(30);
            entity.Property(e => e.DeptName).HasMaxLength(64);
            entity.Property(e => e.CvCode).HasMaxLength(30);
            entity.Property(e => e.OTTypeCode).HasMaxLength(20);
            entity.Property(e => e.OTHours).HasColumnType("decimal(5,2)");
            entity.Property(e => e.ActualHours).HasColumnType("decimal(5,2)");
            entity.Property(e => e.OTRateMultiplier).HasColumnType("decimal(3,1)");
            entity.Property(e => e.ValidationStatus).HasMaxLength(20);
            entity.Property(e => e.ValidationMessage).HasMaxLength(500);
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        // ===== F03OTApprover =====
        modelBuilder.Entity<F03OTApprover>(entity =>
        {
            entity.ToTable("F03OTApprover");

            entity.Property(e => e.DeptCode).HasMaxLength(30);
            entity.Property(e => e.DeptName).HasMaxLength(64);
            entity.Property(e => e.ApproverCode).HasMaxLength(30);
            entity.Property(e => e.ApproverName).HasMaxLength(100);
            entity.Property(e => e.ApproverEmail).HasMaxLength(100);
            entity.Property(e => e.RoleName).HasMaxLength(100);
            entity.Property(e => e.LevelName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasDefaultValue(-1);
            entity.Property(e => e.ModifiedBy).HasDefaultValue(-1);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        // ===== F03OTLimitRule =====
        modelBuilder.Entity<F03OTLimitRule>(entity =>
        {
            entity.ToTable("F03OTLimitRule");

            entity.Property(e => e.LimitType).HasMaxLength(20);
            entity.Property(e => e.LimitValue).HasColumnType("decimal(6,1)");
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        // ===== VF03OTRequest (View) =====
        modelBuilder.Entity<VF03OTRequest>(entity =>
        {
            entity.HasNoKey().ToView("vF03OTRequest");

            entity.Property(e => e.OTCode).HasMaxLength(50);
            entity.Property(e => e.EmployeeCode).HasMaxLength(50);
            entity.Property(e => e.DeptCode).HasMaxLength(30);
            entity.Property(e => e.DeptName).HasMaxLength(64);
            entity.Property(e => e.OTTypeCode).HasMaxLength(20);
            entity.Property(e => e.OTTypeName).HasMaxLength(100);
            entity.Property(e => e.RequestStatus).HasMaxLength(50);
            entity.Property(e => e.TotalOTHours).HasColumnType("decimal(5,2)");
            entity.Property(e => e.ScopeType).HasMaxLength(20);
            entity.Property(e => e.Reason).HasMaxLength(500);

            entity.Property(e => e.Level3ApproveEmail).HasMaxLength(100);
            entity.Property(e => e.Level3ApproveName).HasMaxLength(100);
            entity.Property(e => e.Level3ApproveTime).HasColumnType("datetime");

            entity.Property(e => e.Level5ApproveEmail).HasMaxLength(100);
            entity.Property(e => e.Level5ApproveName).HasMaxLength(100);
            entity.Property(e => e.Level5ApproveTime).HasColumnType("datetime");

            entity.Property(e => e.Level6ApproveEmail).HasMaxLength(100);
            entity.Property(e => e.Level6ApproveName).HasMaxLength(100);
            entity.Property(e => e.Level6ApproveTime).HasColumnType("datetime");

            entity.Property(e => e.Level7ApproveEmail).HasMaxLength(100);
            entity.Property(e => e.Level7ApproveName).HasMaxLength(100);
            entity.Property(e => e.Level7ApproveTime).HasColumnType("datetime");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedByEmail).HasMaxLength(100);
        });

        // ===== VF03OTSummary (View) =====
        modelBuilder.Entity<VF03OTSummary>(entity =>
        {
            entity.HasNoKey().ToView("vF03OTSummary");

            entity.Property(e => e.EmployeeCode).HasMaxLength(50);
            entity.Property(e => e.EmployeeName).HasMaxLength(100);
            entity.Property(e => e.DeptCode).HasMaxLength(30);
            entity.Property(e => e.TotalApprovedHours).HasColumnType("decimal(6,1)");
            entity.Property(e => e.TotalPendingHours).HasColumnType("decimal(6,1)");
        });

    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
