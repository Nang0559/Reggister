USE [FVN_REGISTER];
GO
SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON;
GO
/* Current EF Core model - dbo/F03* tables. */
IF OBJECT_ID('dbo.F03Permissions','U') IS NULL CREATE TABLE dbo.F03Permissions(
 Id int IDENTITY PRIMARY KEY, IsActive bit NULL DEFAULT 1, CreatedBy int NOT NULL DEFAULT 0, LastModifiedSource nvarchar(50), CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(), ModifiedBy int NULL, ModifiedAt datetime2(0) NULL, PermissionCode int NOT NULL, PermissionName nvarchar(100) NOT NULL, Detail nvarchar(500) NOT NULL);
IF OBJECT_ID('dbo.F03Functions','U') IS NULL CREATE TABLE dbo.F03Functions(
 Id int IDENTITY PRIMARY KEY, IsActive bit NULL DEFAULT 1, CreatedBy int NOT NULL DEFAULT 0, LastModifiedSource nvarchar(50), CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(), ModifiedBy int NULL, ModifiedAt datetime2(0) NULL, FunctionCode int NOT NULL, FunctionName nvarchar(100) NOT NULL, Detail nvarchar(500) NOT NULL);
IF OBJECT_ID('dbo.F03Users','U') IS NULL CREATE TABLE dbo.F03Users(
 Id int IDENTITY PRIMARY KEY, IsActive bit NULL DEFAULT 1, CreatedBy int NOT NULL DEFAULT 0, LastModifiedSource nvarchar(50), CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(), ModifiedBy int NULL, ModifiedAt datetime2(0) NULL, Password nvarchar(255) NOT NULL, Avatar nvarchar(255) NULL, EmployeeCode nvarchar(50) NOT NULL, FullName nvarchar(100) NULL, PermissionCode int NOT NULL, LastLogin datetime2(0) NULL, LockoutEnable bit NOT NULL DEFAULT 1, LockoutEndDate datetime2(0) NULL, NumLoginFailed int NOT NULL DEFAULT 0, LevelApprove int NOT NULL DEFAULT 0, DeptCode nvarchar(20) NULL, Cvcode nvarchar(20) NULL);
/* Standardize BaseAuditEntity PK columns for security entities. */
IF OBJECT_ID(N'dbo.F03Permissions',N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.F03Permissions',N'Id') IS NULL
   AND COL_LENGTH(N'dbo.F03Permissions',N'IdPermission') IS NOT NULL
    EXEC sp_rename N'dbo.F03Permissions.IdPermission', N'Id', N'COLUMN';
IF OBJECT_ID(N'dbo.F03Functions',N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.F03Functions',N'Id') IS NULL
   AND COL_LENGTH(N'dbo.F03Functions',N'IdFunction') IS NOT NULL
    EXEC sp_rename N'dbo.F03Functions.IdFunction', N'Id', N'COLUMN';
IF OBJECT_ID(N'dbo.F03Users',N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.F03Users',N'Id') IS NULL
   AND COL_LENGTH(N'dbo.F03Users',N'IdUser') IS NOT NULL
    EXEC sp_rename N'dbo.F03Users.IdUser', N'Id', N'COLUMN';

/* Canonical Work Calendar / annual leave upgrades. */
IF OBJECT_ID(N'dbo.F03CompanyHoliday',N'U') IS NOT NULL AND COL_LENGTH(N'dbo.F03CompanyHoliday',N'TinhPhep') IS NULL AND COL_LENGTH(N'dbo.F03CompanyHoliday',N'IsPaidLeave') IS NOT NULL
    EXEC sys.sp_rename N'dbo.F03CompanyHoliday.IsPaidLeave', N'TinhPhep', N'COLUMN';
IF OBJECT_ID(N'dbo.F03LeaveBalances',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03LeaveBalances',N'BaseLeaveDays') IS NULL ALTER TABLE dbo.F03LeaveBalances ADD BaseLeaveDays decimal(5,2) NOT NULL CONSTRAINT DF_F03LeaveBalances_BaseLeaveDays DEFAULT 12;
    IF COL_LENGTH(N'dbo.F03LeaveBalances',N'SeniorityLeaveDays') IS NULL ALTER TABLE dbo.F03LeaveBalances ADD SeniorityLeaveDays decimal(5,2) NOT NULL CONSTRAINT DF_F03LeaveBalances_SeniorityLeaveDays DEFAULT 0;
    IF COL_LENGTH(N'dbo.F03LeaveBalances',N'YearsOfService') IS NULL ALTER TABLE dbo.F03LeaveBalances ADD YearsOfService int NOT NULL CONSTRAINT DF_F03LeaveBalances_YearsOfService DEFAULT 0;
    IF COL_LENGTH(N'dbo.F03LeaveBalances',N'CalculatedAt') IS NULL ALTER TABLE dbo.F03LeaveBalances ADD CalculatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03LeaveBalances_CalculatedAt DEFAULT GETDATE();
END;

/* Safe upgrades for existing databases. New installs already receive these widths above. */
IF COL_LENGTH(N'dbo.F03Employees',N'TotalLeaveDays') IS NOT NULL
    ALTER TABLE dbo.F03Employees ALTER COLUMN TotalLeaveDays decimal(10,2) NOT NULL;

IF COL_LENGTH(N'dbo.F03StagingEmployee',N'TotalLeaveDays') IS NOT NULL
    ALTER TABLE dbo.F03StagingEmployee ALTER COLUMN TotalLeaveDays decimal(10,2) NULL;

IF COL_LENGTH(N'dbo.F03OTTypes',N'RateMultiplier') IS NOT NULL
    ALTER TABLE dbo.F03OTTypes ALTER COLUMN RateMultiplier decimal(5,2) NOT NULL;

IF COL_LENGTH(N'dbo.F03OTEmployees',N'OTRateMultiplier') IS NOT NULL
    ALTER TABLE dbo.F03OTEmployees ALTER COLUMN OTRateMultiplier decimal(5,2) NOT NULL;

IF COL_LENGTH(N'dbo.F03StagingOTType',N'RateMultiplier') IS NOT NULL
    ALTER TABLE dbo.F03StagingOTType ALTER COLUMN RateMultiplier decimal(5,2) NULL;

/* Normalize common entities that now inherit BaseAuditEntity. */
IF OBJECT_ID(N'dbo.F03Attachment',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03Attachment',N'Id') IS NULL AND COL_LENGTH(N'dbo.F03Attachment',N'FileId') IS NOT NULL
    BEGIN
        EXEC sp_rename N'dbo.F03Attachment.FileId', N'Id', N'COLUMN';
    END;
    IF COL_LENGTH(N'dbo.F03Attachment',N'IsActive') IS NULL ALTER TABLE dbo.F03Attachment ADD IsActive bit NULL CONSTRAINT DF_F03Attachment_IsActive DEFAULT 1;
    IF COL_LENGTH(N'dbo.F03Attachment',N'CreatedBy') IS NULL ALTER TABLE dbo.F03Attachment ADD CreatedBy int NOT NULL CONSTRAINT DF_F03Attachment_CreatedBy DEFAULT 0;
    IF COL_LENGTH(N'dbo.F03Attachment',N'LastModifiedSource') IS NULL ALTER TABLE dbo.F03Attachment ADD LastModifiedSource nvarchar(50) NULL;
    IF COL_LENGTH(N'dbo.F03Attachment',N'CreatedAt') IS NULL ALTER TABLE dbo.F03Attachment ADD CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03Attachment_CreatedAt DEFAULT GETDATE();
    IF COL_LENGTH(N'dbo.F03Attachment',N'ModifiedBy') IS NULL ALTER TABLE dbo.F03Attachment ADD ModifiedBy int NULL;
    IF COL_LENGTH(N'dbo.F03Attachment',N'ModifiedAt') IS NULL ALTER TABLE dbo.F03Attachment ADD ModifiedAt datetime2(0) NULL;
    UPDATE dbo.F03Attachment SET CreatedBy=ISNULL(CreatedBy,0), CreatedAt=ISNULL(CreatedAt,GETDATE());
    ALTER TABLE dbo.F03Attachment ALTER COLUMN CreatedBy int NOT NULL;
    ALTER TABLE dbo.F03Attachment ALTER COLUMN CreatedAt datetime2(0) NOT NULL;
END;

IF OBJECT_ID(N'dbo.F03EmailLogs',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03EmailLogs',N'IsActive') IS NULL ALTER TABLE dbo.F03EmailLogs ADD IsActive bit NULL CONSTRAINT DF_F03EmailLogs_IsActive DEFAULT 1;
    IF COL_LENGTH(N'dbo.F03EmailLogs',N'CreatedBy') IS NULL ALTER TABLE dbo.F03EmailLogs ADD CreatedBy int NOT NULL CONSTRAINT DF_F03EmailLogs_CreatedBy DEFAULT 0;
    IF COL_LENGTH(N'dbo.F03EmailLogs',N'LastModifiedSource') IS NULL ALTER TABLE dbo.F03EmailLogs ADD LastModifiedSource nvarchar(50) NULL;
    IF COL_LENGTH(N'dbo.F03EmailLogs',N'CreatedAt') IS NULL ALTER TABLE dbo.F03EmailLogs ADD CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03EmailLogs_CreatedAt DEFAULT GETDATE();
    IF COL_LENGTH(N'dbo.F03EmailLogs',N'ModifiedBy') IS NULL ALTER TABLE dbo.F03EmailLogs ADD ModifiedBy int NULL;
    IF COL_LENGTH(N'dbo.F03EmailLogs',N'ModifiedAt') IS NULL ALTER TABLE dbo.F03EmailLogs ADD ModifiedAt datetime2(0) NULL;
END;

IF OBJECT_ID(N'dbo.F03UserLogs',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03UserLogs',N'IsActive') IS NULL ALTER TABLE dbo.F03UserLogs ADD IsActive bit NULL CONSTRAINT DF_F03UserLogs_IsActive DEFAULT 1;
    IF COL_LENGTH(N'dbo.F03UserLogs',N'CreatedBy') IS NULL ALTER TABLE dbo.F03UserLogs ADD CreatedBy int NOT NULL CONSTRAINT DF_F03UserLogs_CreatedBy DEFAULT 0;
    IF COL_LENGTH(N'dbo.F03UserLogs',N'LastModifiedSource') IS NULL ALTER TABLE dbo.F03UserLogs ADD LastModifiedSource nvarchar(50) NULL;
    IF COL_LENGTH(N'dbo.F03UserLogs',N'CreatedAt') IS NULL ALTER TABLE dbo.F03UserLogs ADD CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03UserLogs_CreatedAt DEFAULT GETDATE();
    IF COL_LENGTH(N'dbo.F03UserLogs',N'ModifiedBy') IS NULL ALTER TABLE dbo.F03UserLogs ADD ModifiedBy int NULL;
    IF COL_LENGTH(N'dbo.F03UserLogs',N'ModifiedAt') IS NULL ALTER TABLE dbo.F03UserLogs ADD ModifiedAt datetime2(0) NULL;
END;

/* Normalize approval snapshot/reminder entities to BaseAuditEntity. */
IF OBJECT_ID(N'dbo.F03ApprovalSnapshots',N'U') IS NOT NULL
BEGIN
 IF COL_LENGTH(N'dbo.F03ApprovalSnapshots',N'IsActive') IS NULL ALTER TABLE dbo.F03ApprovalSnapshots ADD IsActive bit NULL CONSTRAINT DF_F03ApprovalSnapshots_IsActive DEFAULT 1;
 IF COL_LENGTH(N'dbo.F03ApprovalSnapshots',N'CreatedBy') IS NULL ALTER TABLE dbo.F03ApprovalSnapshots ADD CreatedBy int NOT NULL CONSTRAINT DF_F03ApprovalSnapshots_CreatedBy DEFAULT 0;
 IF COL_LENGTH(N'dbo.F03ApprovalSnapshots',N'LastModifiedSource') IS NULL ALTER TABLE dbo.F03ApprovalSnapshots ADD LastModifiedSource nvarchar(50) NULL;
 IF COL_LENGTH(N'dbo.F03ApprovalSnapshots',N'CreatedAt') IS NULL ALTER TABLE dbo.F03ApprovalSnapshots ADD CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03ApprovalSnapshots_CreatedAt DEFAULT GETUTCDATE();
 IF COL_LENGTH(N'dbo.F03ApprovalSnapshots',N'ModifiedBy') IS NULL ALTER TABLE dbo.F03ApprovalSnapshots ADD ModifiedBy int NULL;
 IF COL_LENGTH(N'dbo.F03ApprovalSnapshots',N'ModifiedAt') IS NULL ALTER TABLE dbo.F03ApprovalSnapshots ADD ModifiedAt datetime2(0) NULL;
END;
IF OBJECT_ID(N'dbo.F03ApprovalStepSnapshots',N'U') IS NOT NULL
BEGIN
 IF COL_LENGTH(N'dbo.F03ApprovalStepSnapshots',N'IsActive') IS NULL ALTER TABLE dbo.F03ApprovalStepSnapshots ADD IsActive bit NULL CONSTRAINT DF_F03ApprovalStepSnapshots_IsActive DEFAULT 1;
 IF COL_LENGTH(N'dbo.F03ApprovalStepSnapshots',N'CreatedBy') IS NULL ALTER TABLE dbo.F03ApprovalStepSnapshots ADD CreatedBy int NOT NULL CONSTRAINT DF_F03ApprovalStepSnapshots_CreatedBy DEFAULT 0;
 IF COL_LENGTH(N'dbo.F03ApprovalStepSnapshots',N'LastModifiedSource') IS NULL ALTER TABLE dbo.F03ApprovalStepSnapshots ADD LastModifiedSource nvarchar(50) NULL;
 IF COL_LENGTH(N'dbo.F03ApprovalStepSnapshots',N'CreatedAt') IS NULL ALTER TABLE dbo.F03ApprovalStepSnapshots ADD CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03ApprovalStepSnapshots_CreatedAt DEFAULT GETUTCDATE();
 IF COL_LENGTH(N'dbo.F03ApprovalStepSnapshots',N'ModifiedBy') IS NULL ALTER TABLE dbo.F03ApprovalStepSnapshots ADD ModifiedBy int NULL;
 IF COL_LENGTH(N'dbo.F03ApprovalStepSnapshots',N'ModifiedAt') IS NULL ALTER TABLE dbo.F03ApprovalStepSnapshots ADD ModifiedAt datetime2(0) NULL;
END;
IF OBJECT_ID(N'dbo.F03ApprovalReminderLog',N'U') IS NOT NULL
BEGIN
 IF COL_LENGTH(N'dbo.F03ApprovalReminderLog',N'IsActive') IS NULL ALTER TABLE dbo.F03ApprovalReminderLog ADD IsActive bit NULL CONSTRAINT DF_F03ApprovalReminderLog_IsActive DEFAULT 1;
 IF COL_LENGTH(N'dbo.F03ApprovalReminderLog',N'CreatedBy') IS NULL ALTER TABLE dbo.F03ApprovalReminderLog ADD CreatedBy int NOT NULL CONSTRAINT DF_F03ApprovalReminderLog_CreatedBy DEFAULT 0;
 IF COL_LENGTH(N'dbo.F03ApprovalReminderLog',N'LastModifiedSource') IS NULL ALTER TABLE dbo.F03ApprovalReminderLog ADD LastModifiedSource nvarchar(50) NULL;
 IF COL_LENGTH(N'dbo.F03ApprovalReminderLog',N'CreatedAt') IS NULL ALTER TABLE dbo.F03ApprovalReminderLog ADD CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03ApprovalReminderLog_CreatedAt DEFAULT GETUTCDATE();
 IF COL_LENGTH(N'dbo.F03ApprovalReminderLog',N'ModifiedBy') IS NULL ALTER TABLE dbo.F03ApprovalReminderLog ADD ModifiedBy int NULL;
 IF COL_LENGTH(N'dbo.F03ApprovalReminderLog',N'ModifiedAt') IS NULL ALTER TABLE dbo.F03ApprovalReminderLog ADD ModifiedAt datetime2(0) NULL;
END;

/* Canonical BaseAuditEntity PK: Id is the single inherited identity key.
   Never drop Id from BaseAuditEntity-backed security tables. */
IF OBJECT_ID('dbo.F03HrmUserRoleRules','U') IS NULL CREATE TABLE dbo.F03HrmUserRoleRules(
    Id int IDENTITY PRIMARY KEY,
    IsActive bit NOT NULL DEFAULT 1,
    CreatedBy int NOT NULL DEFAULT 0,
    LastModifiedSource nvarchar(50) NULL,
    CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),
    ModifiedBy int NULL,
    ModifiedAt datetime2(0) NULL,
    DeptCode nvarchar(20) NULL,
    PositionCode nvarchar(20) NULL,
    PermissionCode int NOT NULL,
    Priority int NOT NULL DEFAULT 100,
    Note nvarchar(500) NULL
);

/* Upgrade existing BaseAudit-backed security tables. */
IF OBJECT_ID(N'dbo.F03UserFunctions',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03UserFunctions',N'Id') IS NULL AND COL_LENGTH(N'dbo.F03UserFunctions',N'IdUserFunction') IS NOT NULL
        EXEC sp_rename N'dbo.F03UserFunctions.IdUserFunction', N'Id', N'COLUMN';
    IF COL_LENGTH(N'dbo.F03UserFunctions',N'IsActive') IS NULL ALTER TABLE dbo.F03UserFunctions ADD IsActive bit NULL CONSTRAINT DF_F03UserFunctions_IsActive DEFAULT 1;
    IF COL_LENGTH(N'dbo.F03UserFunctions',N'CreatedBy') IS NULL ALTER TABLE dbo.F03UserFunctions ADD CreatedBy int NOT NULL CONSTRAINT DF_F03UserFunctions_CreatedBy DEFAULT 0;
    IF COL_LENGTH(N'dbo.F03UserFunctions',N'LastModifiedSource') IS NULL ALTER TABLE dbo.F03UserFunctions ADD LastModifiedSource nvarchar(50) NULL;
    IF COL_LENGTH(N'dbo.F03UserFunctions',N'CreatedAt') IS NULL ALTER TABLE dbo.F03UserFunctions ADD CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03UserFunctions_CreatedAt DEFAULT GETDATE();
    IF COL_LENGTH(N'dbo.F03UserFunctions',N'ModifiedBy') IS NULL ALTER TABLE dbo.F03UserFunctions ADD ModifiedBy int NULL;
    IF COL_LENGTH(N'dbo.F03UserFunctions',N'ModifiedAt') IS NULL ALTER TABLE dbo.F03UserFunctions ADD ModifiedAt datetime2(0) NULL;
END;

IF OBJECT_ID(N'dbo.F03UserSessions',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03UserSessions',N'IsActive') IS NULL ALTER TABLE dbo.F03UserSessions ADD IsActive bit NULL CONSTRAINT DF_F03UserSessions_IsActive DEFAULT 1;
    IF COL_LENGTH(N'dbo.F03UserSessions',N'CreatedBy') IS NULL ALTER TABLE dbo.F03UserSessions ADD CreatedBy int NOT NULL CONSTRAINT DF_F03UserSessions_CreatedBy DEFAULT 0;
    IF COL_LENGTH(N'dbo.F03UserSessions',N'LastModifiedSource') IS NULL ALTER TABLE dbo.F03UserSessions ADD LastModifiedSource nvarchar(50) NULL;
    IF COL_LENGTH(N'dbo.F03UserSessions',N'CreatedAt') IS NULL ALTER TABLE dbo.F03UserSessions ADD CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03UserSessions_CreatedAt DEFAULT GETUTCDATE();
    IF COL_LENGTH(N'dbo.F03UserSessions',N'ModifiedBy') IS NULL ALTER TABLE dbo.F03UserSessions ADD ModifiedBy int NULL;
    IF COL_LENGTH(N'dbo.F03UserSessions',N'ModifiedAt') IS NULL ALTER TABLE dbo.F03UserSessions ADD ModifiedAt datetime2(0) NULL;
END;

IF OBJECT_ID('dbo.F03UserFunctions','U') IS NULL CREATE TABLE dbo.F03UserFunctions(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50) NULL,CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,IdUser int NOT NULL,IdPermission int NOT NULL,IdFunction int NOT NULL);
IF OBJECT_ID('dbo.F03UserSessions','U') IS NULL CREATE TABLE dbo.F03UserSessions(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50) NULL,CreatedAt datetime2(0) NOT NULL DEFAULT GETUTCDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,UserId int NOT NULL,DeviceType nvarchar(10) NOT NULL,DeviceId nvarchar(100) NOT NULL,DeviceName nvarchar(100) NULL,JwtToken nvarchar(max) NULL,ExpiresAt datetime2(0) NULL,SignalRConnectionId nvarchar(100) NULL,LastSeenAt datetime2(0) NULL,RevokedAt datetime2(0) NULL,RememberMe bit NOT NULL DEFAULT 0);

IF OBJECT_ID('dbo.F03Departments','U') IS NULL CREATE TABLE dbo.F03Departments(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,DeptCode nvarchar(20) NOT NULL,DeptName nvarchar(100) NOT NULL,ParentDeptCode nvarchar(50) NULL,DisplayPriority int NULL,ShowInReport bit NOT NULL DEFAULT 1);
IF OBJECT_ID('dbo.F03Positions','U') IS NULL CREATE TABLE dbo.F03Positions(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,PositionCode nvarchar(20) NOT NULL,PositionName nvarchar(100) NOT NULL,IsApprove bit NOT NULL DEFAULT 0,IsAllowApprove bit NOT NULL DEFAULT 0,DefaultApproveLevel int NULL);
IF OBJECT_ID('dbo.F03Genders','U') IS NULL CREATE TABLE dbo.F03Genders(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,GenderCode nvarchar(10) NOT NULL,GenderName nvarchar(50) NOT NULL);
IF OBJECT_ID('dbo.F03Employees','U') IS NULL CREATE TABLE dbo.F03Employees(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,EmployeeCode nvarchar(50) NOT NULL,EmployeeName nvarchar(100) NOT NULL,DeptCode nvarchar(20) NOT NULL,PositionCode nvarchar(20) NOT NULL,BirthDate datetime2(0) NULL,GenderCode int NULL,EmailAddress nvarchar(100) NOT NULL,PhoneNumber nvarchar(20) NULL,FirstWorkingDate datetime2(0) NULL,EndWorkingDate datetime2(0) NULL,TotalLeaveDays decimal(10,2) NOT NULL DEFAULT 0,EmployeeNo int NULL,LevelApprove int NULL);
IF OBJECT_ID('dbo.F03WorkYear','U') IS NULL AND OBJECT_ID('dbo.F03WorkYears','U') IS NULL
    CREATE TABLE dbo.F03WorkYear(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,WorkYear int NOT NULL,StartDate date NOT NULL,EndDate date NOT NULL,Remark nvarchar(500) NULL);
IF OBJECT_ID('dbo.F03CompanyHoliday','U') IS NULL AND OBJECT_ID('dbo.F03CompanyHolidays','U') IS NULL
    CREATE TABLE dbo.F03CompanyHoliday(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,HolidayDate date NOT NULL,Description nvarchar(200) NOT NULL,Year int NOT NULL,TinhPhep bit NOT NULL DEFAULT 1);
IF OBJECT_ID(N'dbo.F03WorkYears',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.F03WorkYear',N'U') IS NULL
    EXEC sys.sp_rename N'dbo.F03WorkYears', N'F03WorkYear';
IF OBJECT_ID(N'dbo.F03CompanyHolidays',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.F03CompanyHoliday',N'U') IS NULL
    EXEC sys.sp_rename N'dbo.F03CompanyHolidays', N'F03CompanyHoliday';

IF OBJECT_ID('dbo.F03LeaveType','U') IS NULL CREATE TABLE dbo.F03LeaveType(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,LeaveTypeCode nvarchar(50) NOT NULL,LeaveTypeName nvarchar(200) NOT NULL,LeaveTypeName2 nvarchar(200) NULL,IsCountedAsLeave bit NOT NULL DEFAULT 0,HRMCode nvarchar(50) NULL);
IF OBJECT_ID('dbo.F03LeaveBalances','U') IS NULL CREATE TABLE dbo.F03LeaveBalances(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,EmployeeCode nvarchar(50) NOT NULL,WorkYear int NOT NULL,BaseLeaveDays decimal(5,2) NOT NULL DEFAULT 12,SeniorityLeaveDays decimal(5,2) NOT NULL DEFAULT 0,TotalDays decimal(5,2) NOT NULL DEFAULT 12,YearsOfService int NOT NULL DEFAULT 0,CalculatedAt datetime2(0) NOT NULL DEFAULT GETDATE());
IF OBJECT_ID('dbo.F03LeaveDays','U') IS NULL CREATE TABLE dbo.F03LeaveDays(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,EmployeeCode nvarchar(50) NOT NULL,DeptCode nvarchar(20) NULL,RequestStatus int NOT NULL DEFAULT 0,WorkYear int NOT NULL,StartTime datetime2(0) NOT NULL,EndTime datetime2(0) NOT NULL,TotalDay decimal(5,2) NOT NULL,RegisterId int NOT NULL,LeaveReason nvarchar(500) NOT NULL,Sync bit NULL,LastSync datetime2(0) NULL,LeaveTypeCode nvarchar(10) NULL,TotalLeaveDay decimal(5,2) NULL);
IF OBJECT_ID('dbo.F03LeaveDayDetails','U') IS NULL CREATE TABLE dbo.F03LeaveDayDetails(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50) NULL,CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,LeaveDaysId int NOT NULL,LeaveDate date NOT NULL,LeaveTypeCode nvarchar(10) NOT NULL,LeaveTypeName nvarchar(100) NULL,IsCountedAsLeave bit NOT NULL DEFAULT 0,IsHalfDay bit NOT NULL DEFAULT 0,HalfDayOption nvarchar(20) NULL,DayValue decimal(5,2) NOT NULL);
IF OBJECT_ID(N'dbo.F03LeaveDayDetails',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03LeaveDayDetails',N'IsActive') IS NULL ALTER TABLE dbo.F03LeaveDayDetails ADD IsActive bit NULL CONSTRAINT DF_F03LeaveDayDetails_IsActive DEFAULT 1;
    IF COL_LENGTH(N'dbo.F03LeaveDayDetails',N'LastModifiedSource') IS NULL ALTER TABLE dbo.F03LeaveDayDetails ADD LastModifiedSource nvarchar(50) NULL;
    IF COL_LENGTH(N'dbo.F03LeaveDayDetails',N'ModifiedBy') IS NULL ALTER TABLE dbo.F03LeaveDayDetails ADD ModifiedBy int NULL;
    IF COL_LENGTH(N'dbo.F03LeaveDayDetails',N'ModifiedAt') IS NULL ALTER TABLE dbo.F03LeaveDayDetails ADD ModifiedAt datetime2(0) NULL;
    UPDATE dbo.F03LeaveDayDetails SET CreatedAt=ISNULL(CreatedAt,GETDATE()), CreatedBy=ISNULL(CreatedBy,0);
    ALTER TABLE dbo.F03LeaveDayDetails ALTER COLUMN CreatedAt datetime2(0) NOT NULL;
    ALTER TABLE dbo.F03LeaveDayDetails ALTER COLUMN CreatedBy int NOT NULL;
END;
IF OBJECT_ID('dbo.F03AttendanceStaging','U') IS NULL CREATE TABLE dbo.F03AttendanceStaging(Id int IDENTITY PRIMARY KEY,WorkDate datetime2(0) NOT NULL,EmployeeCode nvarchar(50) NOT NULL,DeptCode nvarchar(20) NULL,DeptName nvarchar(100) NULL,FullName nvarchar(100) NULL,CheckInText nvarchar(max) NULL,CheckOutText nvarchar(max) NULL,CheckInDateTime datetime2(0) NULL,CheckOutDateTime datetime2(0) NULL,ShiftCode nvarchar(20) NULL,ShiftName nvarchar(100) NULL,ShiftAbbr nvarchar(10) NULL,ShiftCategory int NULL,OtHours decimal(5,2) NULL,TotalHours decimal(5,2) NULL,IsHoliday bit NOT NULL DEFAULT 0,HolidayType nvarchar(50) NULL,ShiftType nvarchar(20) NULL,SyncedAt datetime2(0) NOT NULL DEFAULT GETDATE());

IF OBJECT_ID('dbo.F03OTTypes','U') IS NULL CREATE TABLE dbo.F03OTTypes(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,OTTypeCode nvarchar(50) NOT NULL,OTTypeName nvarchar(200) NOT NULL,OTTypeName2 nvarchar(200) NULL,RateMultiplier decimal(5,2) NOT NULL DEFAULT 1.50,HRMCode nvarchar(50) NULL);
IF OBJECT_ID('dbo.F03OTRequests','U') IS NULL CREATE TABLE dbo.F03OTRequests(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,EmployeeCode nvarchar(50) NOT NULL,DeptCode nvarchar(20) NULL,RequestStatus int NOT NULL DEFAULT 0,OTCode nvarchar(20) NOT NULL,CreatedByEmail nvarchar(100) NULL,ScopeType nvarchar(20) NOT NULL,OTDate datetime2(0) NOT NULL,StartTime datetime2(0) NOT NULL,EndTime datetime2(0) NOT NULL,PlannedHours decimal(5,2) NOT NULL,TotalOTHours decimal(5,2) NOT NULL,OTTypeCode nvarchar(20) NOT NULL,OTReasonSummary nvarchar(500) NULL);
IF OBJECT_ID('dbo.F03OTEmployees','U') IS NULL CREATE TABLE dbo.F03OTEmployees(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,OTRequestId int NOT NULL,EmployeeCode nvarchar(50) NOT NULL,EmployeeName nvarchar(100) NULL,DeptCode nvarchar(20) NULL,DeptName nvarchar(100) NULL,CvCode nvarchar(10) NULL,OTTypeCode nvarchar(20) NULL,StartTime datetime2(0) NULL,EndTime datetime2(0) NULL,OTHours decimal(5,2) NOT NULL,ActualHours decimal(5,2) NULL,ActualStartTime datetime2(0) NULL,ActualEndTime datetime2(0) NULL,OTReasonCategoryCode nvarchar(10) NULL,OTReasonDetail nvarchar(500) NULL,OTRateMultiplier decimal(5,2) NOT NULL DEFAULT 1.50,ValidationStatus int NOT NULL DEFAULT 0,ValidationMessage nvarchar(255) NULL,Note nvarchar(500) NULL);
IF OBJECT_ID('dbo.F03OTLimitRules','U') IS NULL CREATE TABLE dbo.F03OTLimitRules(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,LimitType nvarchar(20) NOT NULL,LimitValue decimal(5,2) NOT NULL,PositionCode nvarchar(20) NULL,DeptCode nvarchar(20) NULL,LimitHours decimal(5,2) NOT NULL,Description nvarchar(500) NULL);
IF OBJECT_ID('dbo.F03OTCodes','U') IS NULL CREATE TABLE dbo.F03OTCodes(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,ReasonCode nvarchar(10) NOT NULL,DisplayName nvarchar(100) NOT NULL,Description nvarchar(255) NULL,DisplayOrder int NOT NULL DEFAULT 0);

IF OBJECT_ID('dbo.F03TripRequests','U') IS NULL CREATE TABLE dbo.F03TripRequests(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,EmployeeCode nvarchar(50) NOT NULL,DeptCode nvarchar(20) NULL,RequestStatus int NOT NULL DEFAULT 0,TripCode nvarchar(30) NOT NULL,StartDate datetime2(0) NOT NULL,EndDate datetime2(0) NOT NULL,Destination nvarchar(250) NOT NULL,Purpose nvarchar(1000) NOT NULL,CustomerOrPartner nvarchar(250) NULL,TransportMethod nvarchar(100) NULL,CompanionEmployeeCodes nvarchar(500) NULL,EstimatedCost decimal(18,2) NULL,Accommodation nvarchar(500) NULL,Note nvarchar(1000) NULL);
IF OBJECT_ID('dbo.F03StagingTrips','U') IS NULL CREATE TABLE dbo.F03StagingTrips(Id int IDENTITY PRIMARY KEY,EmployeeCode nvarchar(50) NULL,StartDate datetime2(0) NULL,EndDate datetime2(0) NULL,Destination nvarchar(255) NULL,Purpose nvarchar(500) NULL,IsProcessed bit NOT NULL DEFAULT 0,ErrorMessage nvarchar(1000) NULL,CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),CreatedBy nvarchar(100) NULL);

IF OBJECT_ID('dbo.F03EquipmentAssets','U') IS NULL CREATE TABLE dbo.F03EquipmentAssets(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,EquipmentCode nvarchar(30) NOT NULL,EquipmentName nvarchar(250) NOT NULL,Specification nvarchar(1000) NULL,SerialNumber nvarchar(100) NULL,AssetCode nvarchar(50) NULL,PurchasePrice decimal(18,2) NOT NULL,PurchaseDate datetime2(0) NOT NULL,ExpectedDepreciationDate datetime2(0) NOT NULL,DeptCode nvarchar(20) NOT NULL,Location nvarchar(250) NULL,QrToken nvarchar(128) NOT NULL,IsQrActive bit NOT NULL DEFAULT 0,Note nvarchar(1000) NULL);
IF OBJECT_ID('dbo.F03EquipmentRequests','U') IS NULL CREATE TABLE dbo.F03EquipmentRequests(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,EmployeeCode nvarchar(50) NOT NULL,DeptCode nvarchar(20) NULL,RequestStatus int NOT NULL DEFAULT 0,RequestKind int NOT NULL,AssetId int NULL,SelectedApproverCode nvarchar(50) NOT NULL,QrToken nvarchar(128) NOT NULL,OperatorUserId int NOT NULL,EquipmentName nvarchar(250) NOT NULL,Specification nvarchar(1000) NULL,SerialNumber nvarchar(100) NULL,AssetCode nvarchar(50) NULL,PurchasePrice decimal(18,2) NOT NULL,PurchaseDate datetime2(0) NULL,ExpectedDepreciationDate datetime2(0) NULL,Location nvarchar(250) NULL,Note nvarchar(1000) NULL,RepairDate datetime2(0) NULL,RepairContent nvarchar(1000) NULL,RepairVendor nvarchar(250) NULL,RepairCost decimal(18,2) NULL,RepairResult nvarchar(1000) NULL);
IF OBJECT_ID('dbo.F03EquipmentRepairHistory','U') IS NULL CREATE TABLE dbo.F03EquipmentRepairHistory(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,AssetId int NOT NULL,RequestId int NOT NULL,RepairDate datetime2(0) NOT NULL,OperatorUserId int NOT NULL,RepairCost decimal(18,2) NULL,RepairContent nvarchar(1000) NOT NULL,RepairVendor nvarchar(250) NULL,RepairResult nvarchar(1000) NULL,Note nvarchar(1000) NULL,IsApproved bit NOT NULL DEFAULT 0);

IF OBJECT_ID('dbo.F03Approvers','U') IS NULL CREATE TABLE dbo.F03Approvers(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,UserId int NULL,RequestType nvarchar(20) NOT NULL,ApproverCode nvarchar(50) NOT NULL,PositionCode nvarchar(20) NULL,ApproverName nvarchar(100) NOT NULL,ApproverEmail nvarchar(100) NOT NULL,ApproverDeptCode nvarchar(20) NOT NULL,ApproverDeptName nvarchar(100) NOT NULL,ApproveForDeptCode nvarchar(20) NOT NULL,ApproveForDeptName nvarchar(100) NOT NULL,Level int NOT NULL,RoleName nvarchar(50) NOT NULL);
IF OBJECT_ID('dbo.F03ApprovalSteps','U') IS NULL CREATE TABLE dbo.F03ApprovalSteps(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,RequestType nvarchar(20) NOT NULL,RequestId int NOT NULL,Level int NOT NULL,LevelName nvarchar(50) NULL,RoleName nvarchar(50) NOT NULL,ApproverCode nvarchar(50) NULL,ApproverName nvarchar(100) NULL,ApproverEmail nvarchar(100) NULL,Required bit NOT NULL DEFAULT 1,Approved bit NULL,ApprovedAt datetime2(0) NULL,Comment nvarchar(500) NULL,ReminderSent bit NOT NULL DEFAULT 0,IsOverriddenByAdmin bit NOT NULL DEFAULT 0,OverriddenByCode nvarchar(50) NULL,OverriddenByName nvarchar(100) NULL,OverriddenAt datetime2(0) NULL);
IF OBJECT_ID('dbo.F03ApprovalSnapshots','U') IS NULL CREATE TABLE dbo.F03ApprovalSnapshots(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50) NULL,CreatedAt datetime2(0) NOT NULL DEFAULT GETUTCDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,RequestId int NOT NULL,RequestType int NOT NULL);
IF OBJECT_ID('dbo.F03ApprovalStepSnapshots','U') IS NULL CREATE TABLE dbo.F03ApprovalStepSnapshots(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50) NULL,CreatedAt datetime2(0) NOT NULL DEFAULT GETUTCDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,SnapshotId int NOT NULL,Level int NOT NULL,ApproverCode nvarchar(20) NOT NULL,ApproverName nvarchar(100) NOT NULL,ApproverEmail nvarchar(max) NOT NULL,RoleName nvarchar(100) NOT NULL,IsRequired bit NOT NULL);
IF OBJECT_ID('dbo.ApprovalHistories','U') IS NULL CREATE TABLE dbo.ApprovalHistories(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,RequestType int NOT NULL,RequestId int NOT NULL,StepId int NOT NULL,IsOverriddenByAdmin bit NOT NULL DEFAULT 0,ApproverCode nvarchar(50) NOT NULL,ApproverName nvarchar(100) NOT NULL,OverriddenByCode nvarchar(50) NULL,OverriddenByName nvarchar(100) NULL,OverriddenAt datetime2(0) NULL,Decision int NOT NULL,Comment nvarchar(max) NULL,ActionAt datetime2(0) NOT NULL);
IF OBJECT_ID('dbo.F03ApprovalReminderLog','U') IS NULL CREATE TABLE dbo.F03ApprovalReminderLog(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50) NULL,CreatedAt datetime2(0) NOT NULL DEFAULT GETUTCDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,RequestType int NOT NULL,RequestId int NOT NULL,Level int NOT NULL,SentAt datetime2(0) NOT NULL DEFAULT GETDATE());

IF OBJECT_ID('dbo.F03Attachment','U') IS NULL CREATE TABLE dbo.F03Attachment(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50) NULL,CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,Module int NOT NULL,RequestId int NOT NULL,FileName nvarchar(200) NOT NULL,FilePath nvarchar(255) NOT NULL,FileExtension nvarchar(10) NULL,FileSize bigint NOT NULL DEFAULT 0);
IF OBJECT_ID('dbo.F03AppNotifications','U') IS NULL CREATE TABLE dbo.F03AppNotifications(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,UserId int NOT NULL,EmployeeCode nvarchar(50) NULL,RequestModule nvarchar(20) NOT NULL,Action nvarchar(20) NOT NULL,Title nvarchar(200) NOT NULL,Body nvarchar(1000) NULL,ActionUrl nvarchar(500) NULL,RelatedLeaveId int NULL,RelatedOTId int NULL,ApprovalLevel int NULL,IsHighPriority bit NOT NULL DEFAULT 0,Metadata nvarchar(2000) NULL,IsRead bit NOT NULL DEFAULT 0,ReadAt datetime2(0) NULL);
IF OBJECT_ID('dbo.F03EmailQueues','U') IS NULL CREATE TABLE dbo.F03EmailQueues(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,ToEmail nvarchar(255) NOT NULL,Subject nvarchar(255) NOT NULL,Body nvarchar(max) NOT NULL,TemplateCode nvarchar(50) NULL,Payload nvarchar(max) NULL,Status nvarchar(20) NOT NULL DEFAULT 'Pending',RetryCount int NOT NULL DEFAULT 0,MaxRetry int NOT NULL DEFAULT 3,ErrorMessage nvarchar(1000) NULL,SentAt datetime2(0) NULL);
IF OBJECT_ID('dbo.F03EmailLogs','U') IS NULL CREATE TABLE dbo.F03EmailLogs(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50) NULL,CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,QueueId int NULL,ToEmail nvarchar(255) NOT NULL,Subject nvarchar(255) NOT NULL,Status nvarchar(20) NOT NULL,SentAt datetime2(0) NOT NULL DEFAULT GETDATE(),ErrorMessage nvarchar(1000) NULL);
IF OBJECT_ID('dbo.F03EmailTemplates','U') IS NULL CREATE TABLE dbo.F03EmailTemplates(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,Code nvarchar(50) NOT NULL,Subject nvarchar(255) NOT NULL,Body nvarchar(max) NOT NULL,Description nvarchar(500) NULL);
IF OBJECT_ID('dbo.F03EmailProfiles','U') IS NULL CREATE TABLE dbo.F03EmailProfiles(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,ParentId int NOT NULL,IsGroup bit NOT NULL,Code nvarchar(20) NOT NULL,Name nvarchar(100) NOT NULL,NameEn nvarchar(100) NULL,EmailServerName nvarchar(100) NOT NULL,EmailServerType nvarchar(20) NOT NULL,EmailServerPort int NOT NULL,EmailServerEnableSsl bit NOT NULL,EmailAccountName nvarchar(100) NOT NULL,EmailAddress nvarchar(100) NOT NULL,EmailPassword nvarchar(255) NULL,SiteUrl nvarchar(255) NULL,Timestamp rowversion NOT NULL);
IF OBJECT_ID('dbo.F03BusinessRules','U') IS NULL CREATE TABLE dbo.F03BusinessRules(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,Module nvarchar(20) NOT NULL,Code nvarchar(50) NOT NULL,Name nvarchar(100) NOT NULL,ConfigJson nvarchar(max) NULL,HrmCode nvarchar(50) NULL);
IF OBJECT_ID('dbo.F03EscalationRules','U') IS NULL CREATE TABLE dbo.F03EscalationRules(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,RequestModule nvarchar(20) NOT NULL,Level int NOT NULL,DeptCode nvarchar(20) NULL,WarningHours decimal(5,2) NOT NULL,EscalateHours decimal(5,2) NOT NULL,DeadlineHour int NOT NULL);
IF OBJECT_ID('dbo.F03EscalationLogs','U') IS NULL CREATE TABLE dbo.F03EscalationLogs(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,RequestId int NOT NULL,RequestModule nvarchar(20) NOT NULL,Level int NOT NULL,Action nvarchar(50) NULL);
IF OBJECT_ID('dbo.F03AuditLogs','U') IS NULL CREATE TABLE dbo.F03AuditLogs(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50),CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,UserId int NULL,UserName nvarchar(100) NULL,Action nvarchar(100) NOT NULL,Description nvarchar(2000) NULL,IpAddress nvarchar(50) NULL,UserAgent nvarchar(255) NULL);
IF OBJECT_ID('dbo.F03UserLogs','U') IS NULL CREATE TABLE dbo.F03UserLogs(Id int IDENTITY PRIMARY KEY,IsActive bit NULL DEFAULT 1,CreatedBy int NOT NULL DEFAULT 0,LastModifiedSource nvarchar(50) NULL,CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),ModifiedBy int NULL,ModifiedAt datetime2(0) NULL,UserId int NOT NULL,LastSeen nvarchar(255) NOT NULL,LastSeenUrl nvarchar(500) NOT NULL,ApplicationName nvarchar(100) NOT NULL,ApplicationVersion nvarchar(20) NOT NULL,WorkstationName nvarchar(100) NOT NULL,WorkstationUser nvarchar(100) NOT NULL);

IF OBJECT_ID('dbo.F03StagingDepartment','U') IS NULL CREATE TABLE dbo.F03StagingDepartment(Id int IDENTITY PRIMARY KEY,EntityKey nvarchar(255) NOT NULL,Action int NOT NULL,IsProcessed bit NOT NULL DEFAULT 0,ErrorMessage nvarchar(1000) NULL,CreatedAt datetime2(0) NOT NULL,CreatedBy nvarchar(100) NULL,DeptName nvarchar(255) NOT NULL,ParentDeptCode nvarchar(50) NULL,DisplayPriority int NULL,ShowInReport bit NOT NULL DEFAULT 1);
IF OBJECT_ID('dbo.F03StagingEmployee','U') IS NULL CREATE TABLE dbo.F03StagingEmployee(Id int IDENTITY PRIMARY KEY,EntityKey nvarchar(255) NOT NULL,Action int NOT NULL,IsProcessed bit NOT NULL DEFAULT 0,ErrorMessage nvarchar(1000) NULL,CreatedAt datetime2(0) NOT NULL,CreatedBy nvarchar(100) NULL,EmployeeName nvarchar(255) NOT NULL,DeptCode nvarchar(20) NULL,PositionCode nvarchar(20) NULL,EmailAddress nvarchar(100) NOT NULL,PhoneNumber nvarchar(20) NULL,BirthDate datetime2(0) NULL,GenderCode int NULL,FirstWorkingDate datetime2(0) NULL,EndWorkingDate datetime2(0) NULL,TotalLeaveDays decimal(5,2) NULL,EmployeeNo int NULL);
IF OBJECT_ID('dbo.F03StagingLeaveType','U') IS NULL CREATE TABLE dbo.F03StagingLeaveType(Id int IDENTITY PRIMARY KEY,EntityKey nvarchar(255) NOT NULL,Action int NOT NULL,IsProcessed bit NOT NULL DEFAULT 0,ErrorMessage nvarchar(1000) NULL,CreatedAt datetime2(0) NOT NULL,CreatedBy nvarchar(100) NULL,LeaveTypeName nvarchar(255) NOT NULL,LeaveTypeName2 nvarchar(255) NULL,TinhPhep bit NOT NULL,HRMCode nvarchar(50) NULL);
IF OBJECT_ID('dbo.F03StagingOTType','U') IS NULL CREATE TABLE dbo.F03StagingOTType(Id int IDENTITY PRIMARY KEY,EntityKey nvarchar(255) NOT NULL,Action int NOT NULL,IsProcessed bit NOT NULL DEFAULT 0,ErrorMessage nvarchar(1000) NULL,CreatedAt datetime2(0) NOT NULL,CreatedBy nvarchar(100) NULL,OTTypeName nvarchar(255) NOT NULL,OTTypeName2 nvarchar(255) NULL,RateMultiplier decimal(3,1) NOT NULL,HRMCode nvarchar(50) NULL);
IF OBJECT_ID('dbo.F03StagingPosition','U') IS NULL CREATE TABLE dbo.F03StagingPosition(Id int IDENTITY PRIMARY KEY,EntityKey nvarchar(255) NOT NULL,Action int NOT NULL,IsProcessed bit NOT NULL DEFAULT 0,ErrorMessage nvarchar(1000) NULL,CreatedAt datetime2(0) NOT NULL,CreatedBy nvarchar(100) NULL,PositionName nvarchar(255) NOT NULL,DefaultApproveLevel int NULL);
IF OBJECT_ID('dbo.F03SyncReviewFlag','U') IS NULL CREATE TABLE dbo.F03SyncReviewFlag(Id int IDENTITY PRIMARY KEY,EntityType nvarchar(100) NOT NULL,EntityKey nvarchar(255) NOT NULL,FlagType nvarchar(100) NOT NULL,Message nvarchar(1000) NOT NULL,DetectedAt datetime2(0) NOT NULL DEFAULT GETDATE(),IsResolved bit NOT NULL DEFAULT 0,ResolvedAt datetime2(0) NULL,ResolvedBy nvarchar(100) NULL);
IF OBJECT_ID('dbo.HrmLeaveTypeChangeLogs','U') IS NULL CREATE TABLE dbo.HrmLeaveTypeChangeLogs(Id bigint IDENTITY PRIMARY KEY,LeaveTypeCode nvarchar(50) NOT NULL,LeaveTypeName nvarchar(200) NULL,LeaveTypeName2 nvarchar(200) NULL,TinhPhep bit NULL,HRMCode nvarchar(50) NULL,ActionType int NOT NULL,ChangedAt datetime2(0) NOT NULL,IsProcessed bit NOT NULL DEFAULT 0,ProcessedAt datetime2(0) NULL);
GO

/* Upgrade existing installations for HRM master/staging fields. */
IF OBJECT_ID(N'dbo.F03Departments',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03Departments',N'ParentDeptCode') IS NULL ALTER TABLE dbo.F03Departments ADD ParentDeptCode nvarchar(50) NULL;
    IF COL_LENGTH(N'dbo.F03Departments',N'DisplayPriority') IS NULL ALTER TABLE dbo.F03Departments ADD DisplayPriority int NULL;
    IF COL_LENGTH(N'dbo.F03Departments',N'ShowInReport') IS NULL ALTER TABLE dbo.F03Departments ADD ShowInReport bit NOT NULL CONSTRAINT DF_F03Departments_ShowInReport DEFAULT 1;
END;
IF OBJECT_ID(N'dbo.F03Employees',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03Employees',N'BirthDate') IS NULL ALTER TABLE dbo.F03Employees ADD BirthDate datetime2(0) NULL;
    IF COL_LENGTH(N'dbo.F03Employees',N'GenderCode') IS NULL ALTER TABLE dbo.F03Employees ADD GenderCode int NULL;
    IF COL_LENGTH(N'dbo.F03Employees',N'FirstWorkingDate') IS NULL ALTER TABLE dbo.F03Employees ADD FirstWorkingDate datetime2(0) NULL;
    IF COL_LENGTH(N'dbo.F03Employees',N'EndWorkingDate') IS NULL ALTER TABLE dbo.F03Employees ADD EndWorkingDate datetime2(0) NULL;
    IF COL_LENGTH(N'dbo.F03Employees',N'TotalLeaveDays') IS NULL ALTER TABLE dbo.F03Employees ADD TotalLeaveDays decimal(10,2) NOT NULL CONSTRAINT DF_F03Employees_TotalLeaveDays DEFAULT 0;
    IF COL_LENGTH(N'dbo.F03Employees',N'EmployeeNo') IS NULL ALTER TABLE dbo.F03Employees ADD EmployeeNo int NULL;
END;
IF OBJECT_ID(N'dbo.F03LeaveType',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03LeaveType',N'LeaveTypeName2') IS NULL ALTER TABLE dbo.F03LeaveType ADD LeaveTypeName2 nvarchar(200) NULL;
    IF COL_LENGTH(N'dbo.F03LeaveType',N'IsCountedAsLeave') IS NULL ALTER TABLE dbo.F03LeaveType ADD IsCountedAsLeave bit NOT NULL CONSTRAINT DF_F03LeaveType_IsCountedAsLeave DEFAULT 0;
    IF COL_LENGTH(N'dbo.F03LeaveType',N'HRMCode') IS NULL ALTER TABLE dbo.F03LeaveType ADD HRMCode nvarchar(50) NULL;
END;
IF OBJECT_ID(N'dbo.F03StagingLeaveType',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03StagingLeaveType',N'TinhPhep') IS NULL ALTER TABLE dbo.F03StagingLeaveType ADD TinhPhep bit NOT NULL CONSTRAINT DF_F03StagingLeaveType_TinhPhep DEFAULT 0;
END;
IF OBJECT_ID(N'dbo.F03StagingEmployee',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03StagingEmployee',N'TotalLeaveDays') IS NOT NULL
        ALTER TABLE dbo.F03StagingEmployee ALTER COLUMN TotalLeaveDays decimal(10,2) NULL;
END;
IF OBJECT_ID(N'dbo.F03StagingOTType',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03StagingOTType',N'RateMultiplier') IS NOT NULL
        ALTER TABLE dbo.F03StagingOTType ALTER COLUMN RateMultiplier decimal(5,2) NOT NULL;
END;
IF OBJECT_ID(N'dbo.F03OTTypes',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03OTTypes',N'RateMultiplier') IS NOT NULL
        ALTER TABLE dbo.F03OTTypes ALTER COLUMN RateMultiplier decimal(5,2) NOT NULL;
END;

/* Upgrade existing installations for HRM master staging fields. */
IF OBJECT_ID(N'dbo.F03StagingDepartment',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03StagingDepartment',N'ParentDeptCode') IS NULL ALTER TABLE dbo.F03StagingDepartment ADD ParentDeptCode nvarchar(50) NULL;
    IF COL_LENGTH(N'dbo.F03StagingDepartment',N'DisplayPriority') IS NULL ALTER TABLE dbo.F03StagingDepartment ADD DisplayPriority int NULL;
    IF COL_LENGTH(N'dbo.F03StagingDepartment',N'ShowInReport') IS NULL ALTER TABLE dbo.F03StagingDepartment ADD ShowInReport bit NOT NULL CONSTRAINT DF_F03StagingDepartment_ShowInReport DEFAULT 1;
END;
IF OBJECT_ID(N'dbo.F03StagingEmployee',N'U') IS NOT NULL
    IF COL_LENGTH(N'dbo.F03StagingEmployee',N'EmployeeNo') IS NULL ALTER TABLE dbo.F03StagingEmployee ADD EmployeeNo int NULL;
GO


/*
  HRM SHIFT MASTER
  Source:
    HRM.dbo.tblca
    HRM.dbo.CC_LichTrinhCa
    HRM.dbo.tblNhanVien.NVLichTrinhCa
    HRM.dbo.CC_LichTrinhVaoRa.Loai

  FVN_REGISTER owns these local copies and uses them for attendance
  shift resolution. HRM remains READ ONLY.
*/
IF OBJECT_ID('dbo.F03Shifts','U') IS NULL CREATE TABLE dbo.F03Shifts(
    Id int IDENTITY PRIMARY KEY,
    IsActive bit NOT NULL DEFAULT 1,
    CreatedBy int NOT NULL DEFAULT 0,
    LastModifiedSource nvarchar(50) NULL,
    CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),
    ModifiedBy int NULL,
    ModifiedAt datetime2(0) NULL,
    HrmCode nvarchar(20) NOT NULL,
    ShiftCode nvarchar(20) NOT NULL,
    ShiftName nvarchar(100) NOT NULL,
    ShiftAbbr nvarchar(10) NULL,
    StartTime time(0) NOT NULL,
    Break1Start time(0) NULL,
    Break1End time(0) NULL,
    Break2Start time(0) NULL,
    Break2End time(0) NULL,
    Break3Start time(0) NULL,
    Break3End time(0) NULL,
    EndTime time(0) NOT NULL,
    LateCalcTime time(0) NULL,
    OTRateBase smallint NOT NULL DEFAULT 0,
    OTRateTC smallint NOT NULL DEFAULT 0,
    OTUnit tinyint NOT NULL DEFAULT 1,
    MidBreakMinutes smallint NOT NULL DEFAULT 0,
    RegularMinutes smallint NOT NULL DEFAULT 0,
    DailyOTThresholdMinutes int NOT NULL DEFAULT 0,
    DailyOTTCThresholdMinutes int NOT NULL DEFAULT 0,
    LateThresholdMinutes tinyint NOT NULL DEFAULT 0,
    EarlyLeaveThresholdMinutes tinyint NOT NULL DEFAULT 0,
    ScanBeforeMinutes smallint NOT NULL DEFAULT 240,
    ScanAfterMinutes smallint NOT NULL DEFAULT 240,
    AttendanceUnit int NOT NULL DEFAULT 1,
    AllowSundayOT bit NOT NULL DEFAULT 0,
    AllowHolidayOT bit NOT NULL DEFAULT 0,
    ShiftType tinyint NOT NULL DEFAULT 0,
    DepartmentScope nvarchar(1000) NULL,
    RestDayType tinyint NOT NULL DEFAULT 0,
    IgnoreAbsence bit NOT NULL DEFAULT 1,
    ScheduleInOutType tinyint NOT NULL DEFAULT 0,
    SplitOTAfterShift bit NOT NULL DEFAULT 0,
    CountBreakAsWork bit NULL,
    CountToTotalWork bit NULL,
    AllowOutside bit NOT NULL DEFAULT 0,
    AllowEarlyCheckIn bit NOT NULL DEFAULT 0,
    ShiftGroup nvarchar(50) NULL
);

IF OBJECT_ID('dbo.F03ShiftSchedules','U') IS NULL CREATE TABLE dbo.F03ShiftSchedules(
    Id int IDENTITY PRIMARY KEY,
    IsActive bit NOT NULL DEFAULT 1,
    CreatedBy int NOT NULL DEFAULT 0,
    LastModifiedSource nvarchar(50) NULL,
    CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),
    ModifiedBy int NULL,
    ModifiedAt datetime2(0) NULL,
    ScheduleCode nvarchar(50) NOT NULL,
    ScheduleName nvarchar(200) NOT NULL,
    IsMonthly bit NOT NULL DEFAULT 0,
    HrmCode nvarchar(50) NOT NULL
);

IF OBJECT_ID('dbo.F03ShiftScheduleDays','U') IS NULL CREATE TABLE dbo.F03ShiftScheduleDays(
    Id int IDENTITY PRIMARY KEY,
    IsActive bit NOT NULL DEFAULT 1,
    CreatedBy int NOT NULL DEFAULT 0,
    LastModifiedSource nvarchar(50) NULL,
    CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),
    ModifiedBy int NULL,
    ModifiedAt datetime2(0) NULL,
    ScheduleCode nvarchar(50) NOT NULL,
    DayNo tinyint NOT NULL,
    ShiftCode nvarchar(20) NOT NULL
);

IF OBJECT_ID('dbo.F03EmployeeShiftSchedules','U') IS NULL CREATE TABLE dbo.F03EmployeeShiftSchedules(
    Id int IDENTITY PRIMARY KEY,
    IsActive bit NOT NULL DEFAULT 1,
    CreatedBy int NOT NULL DEFAULT 0,
    LastModifiedSource nvarchar(50) NULL,
    CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),
    ModifiedBy int NULL,
    ModifiedAt datetime2(0) NULL,
    EmployeeCode nvarchar(50) NOT NULL,
    ScheduleCode nvarchar(50) NULL,
    ScheduleType nvarchar(20) NULL,
    HrmEmployeeNo int NULL,
    ValidFrom date NULL,
    ValidTo date NULL
);

IF OBJECT_ID('dbo.F03HrmShiftReference','U') IS NULL CREATE TABLE dbo.F03HrmShiftReference(
    Id bigint IDENTITY PRIMARY KEY,
    IsActive bit NOT NULL DEFAULT 1,
    CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),
    SyncedAt datetime2(0) NOT NULL DEFAULT GETDATE(),
    EmployeeCode nvarchar(50) NOT NULL,
    WorkDate date NOT NULL,
    HrmScheduleCode nvarchar(50) NULL,
    HrmShiftCode nvarchar(20) NULL,
    HrmShiftAbbr nvarchar(10) NULL,
    HrmCheckIn datetime2(0) NULL,
    HrmCheckOut datetime2(0) NULL,
    SourceUpdatedAt datetime2(0) NULL
);

GO
