USE [FVN_REGISTER];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/*
  Security Function Registry
  --------------------------
  FunctionCode remains the legacy numeric permission identity.
  FunctionKey is the stable application identity used by discovery.
  Discovery never grants/revokes a RoleFunction and never hard-deletes a function.
*/

IF COL_LENGTH(N'dbo.F03Functions', N'FunctionKey') IS NULL
    ALTER TABLE dbo.F03Functions ADD FunctionKey nvarchar(150) NULL;
IF COL_LENGTH(N'dbo.F03Functions', N'ModuleCode') IS NULL
    ALTER TABLE dbo.F03Functions ADD ModuleCode nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.F03Functions', N'ActionCode') IS NULL
    ALTER TABLE dbo.F03Functions ADD ActionCode nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.F03Functions', N'ScopeCode') IS NULL
    ALTER TABLE dbo.F03Functions ADD ScopeCode nvarchar(30) NULL;
IF COL_LENGTH(N'dbo.F03Functions', N'LifecycleStatus') IS NULL
    ALTER TABLE dbo.F03Functions ADD LifecycleStatus nvarchar(30) NOT NULL CONSTRAINT DF_F03Functions_LifecycleStatus DEFAULT N'Active';
IF COL_LENGTH(N'dbo.F03Functions', N'SourceType') IS NULL
    ALTER TABLE dbo.F03Functions ADD SourceType nvarchar(30) NOT NULL CONSTRAINT DF_F03Functions_SourceType DEFAULT N'Legacy';
IF COL_LENGTH(N'dbo.F03Functions', N'LastSeenAt') IS NULL
    ALTER TABLE dbo.F03Functions ADD LastSeenAt datetime2(0) NULL;
IF COL_LENGTH(N'dbo.F03Functions', N'ReplacementFunctionKey') IS NULL
    ALTER TABLE dbo.F03Functions ADD ReplacementFunctionKey nvarchar(150) NULL;

/* Backfill stable keys for all existing permission codes. Unknown legacy codes remain identifiable. */
UPDATE f SET FunctionKey = CASE f.FunctionCode
    WHEN 2701 THEN N'Dashboard.View'
    WHEN 2001 THEN N'Leave.View'
    WHEN 2002 THEN N'Leave.Create'
    WHEN 2003 THEN N'Leave.Edit'
    WHEN 2004 THEN N'Leave.Cancel'
    WHEN 2005 THEN N'Leave.Approve'
    WHEN 2006 THEN N'Leave.Export'
    WHEN 2101 THEN N'OT.View'
    WHEN 2102 THEN N'OT.Create'
    WHEN 2103 THEN N'OT.Edit'
    WHEN 2104 THEN N'OT.Cancel'
    WHEN 2105 THEN N'OT.Approve'
    WHEN 2106 THEN N'OT.Reconcile'
    WHEN 2107 THEN N'OT.Export'
    WHEN 2201 THEN N'Trip.View'
    WHEN 2202 THEN N'Trip.Create'
    WHEN 2203 THEN N'Trip.Edit'
    WHEN 2204 THEN N'Trip.Cancel'
    WHEN 2205 THEN N'Trip.Approve'
    WHEN 2206 THEN N'Trip.Export'
    WHEN 2301 THEN N'Equipment.View'
    WHEN 2302 THEN N'Equipment.Create'
    WHEN 2303 THEN N'Equipment.Edit'
    WHEN 2304 THEN N'Equipment.Repair'
    WHEN 2305 THEN N'Equipment.Approve'
    WHEN 2306 THEN N'Equipment.Import'
    WHEN 2307 THEN N'Equipment.Export'
    WHEN 2308 THEN N'Equipment.Cancel'
    WHEN 2309 THEN N'Equipment.Assign'
    WHEN 2310 THEN N'Equipment.Transfer'
    WHEN 2311 THEN N'Equipment.Return'
    WHEN 2312 THEN N'Equipment.Liquidate'
    WHEN 2313 THEN N'Equipment.QR'
    WHEN 2314 THEN N'Equipment.History'
    WHEN 2315 THEN N'Equipment.InspectionManage'
    WHEN 2316 THEN N'Equipment.InspectionExecute'
    WHEN 2317 THEN N'Equipment.InspectionApprove'
    WHEN 2318 THEN N'Equipment.InspectionReport'
    WHEN 2401 THEN N'UserManagement.View'
    WHEN 2402 THEN N'UserManagement.Create'
    WHEN 2403 THEN N'UserManagement.Edit'
    WHEN 2404 THEN N'UserManagement.Lock'
    WHEN 2405 THEN N'UserManagement.ResetPassword'
    WHEN 2406 THEN N'UserManagement.AssignPermission'
    WHEN 2407 THEN N'UserManagement.ManageTwoFactor'
    WHEN 2501 THEN N'HrmSync.ViewStatus'
    WHEN 2502 THEN N'HrmSync.Sync'
    WHEN 2503 THEN N'HrmSync.Review'
    WHEN 2504 THEN N'HrmSync.Retry'
    WHEN 2601 THEN N'Security.View'
    WHEN 2602 THEN N'Security.ManageRoles'
    WHEN 2603 THEN N'Security.ManageFunctions'
    WHEN 2604 THEN N'Security.Audit'
    WHEN 2801 THEN N'PublicInformation.Manage'
    WHEN 2802 THEN N'Execution.Review'
    WHEN 2803 THEN N'Payroll.View'
    WHEN 2804 THEN N'Payroll.Prepare'
    WHEN 2805 THEN N'Payroll.Lock'
    WHEN 2806 THEN N'Payroll.Export'
    WHEN 2807 THEN N'PublicForm.Manage'
    WHEN 2808 THEN N'PublicForm.SubmissionView'
    WHEN 2809 THEN N'PublicForm.Export'
    WHEN 2901 THEN N'Attendance.View'
    WHEN 2902 THEN N'Attendance.Export'
    WHEN 2911 THEN N'Attendance.Calculate'
    WHEN 2912 THEN N'Attendance.Feedback'
    WHEN 3001 THEN N'Department.View'
    WHEN 3002 THEN N'Department.Manage'
    WHEN 3011 THEN N'Employee.View'
    WHEN 3012 THEN N'Employee.Manage'
    WHEN 3021 THEN N'LeaveType.View'
    WHEN 3022 THEN N'LeaveType.Manage'
    WHEN 3031 THEN N'Approver.View'
    WHEN 3032 THEN N'Approver.Manage'
    WHEN 3041 THEN N'WorkCalendar.View'
    WHEN 3042 THEN N'WorkCalendar.Manage'
    WHEN 3043 THEN N'Calendar.View'
    WHEN 3051 THEN N'DepartmentStatus.View'
    WHEN 3061 THEN N'OTLimit.Manage'
    WHEN 3071 THEN N'ApprovalPolicy.Manage'
    WHEN 3072 THEN N'HrmUserRoleRule.Manage'
    WHEN 3081 THEN N'EmailQueue.Manage'
    WHEN 3082 THEN N'EmailTemplate.Manage'
    WHEN 3091 THEN N'SecurityAccessChange.View'
    WHEN 3092 THEN N'SecurityAccessChange.Create'
    WHEN 3093 THEN N'SecurityAccessChange.Approve'
    WHEN 3094 THEN N'SecurityAccessChange.Execute'
    ELSE N'Legacy.' + CONVERT(nvarchar(20), f.FunctionCode)
END
WHERE NULLIF(LTRIM(RTRIM(FunctionKey)), N'') IS NULL;

UPDATE dbo.F03Functions SET LastSeenAt = ISNULL(LastSeenAt, CreatedAt) WHERE LastSeenAt IS NULL;
UPDATE dbo.F03Functions SET ScopeCode = N'Own' WHERE ScopeCode IS NULL;

IF EXISTS (SELECT 1 FROM dbo.F03Functions GROUP BY FunctionKey HAVING COUNT(*) > 1)
    THROW 51460, N'F03Functions có FunctionKey trùng; cần xử lý trước khi tạo unique index.', 1;

ALTER TABLE dbo.F03Functions ALTER COLUMN FunctionKey nvarchar(150) NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.F03Functions') AND name = N'UX_F03Functions_FunctionKey')
    CREATE UNIQUE INDEX UX_F03Functions_FunctionKey ON dbo.F03Functions(FunctionKey);

IF OBJECT_ID(N'dbo.F03SecurityFunctionRegistry',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03SecurityFunctionRegistry
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03SecurityFunctionRegistry PRIMARY KEY,
        IsActive bit NULL CONSTRAINT DF_F03SecurityFunctionRegistry_IsActive DEFAULT 1,
        CreatedBy int NOT NULL CONSTRAINT DF_F03SecurityFunctionRegistry_CreatedBy DEFAULT 0,
        LastModifiedSource nvarchar(50) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03SecurityFunctionRegistry_CreatedAt DEFAULT GETDATE(),
        ModifiedBy int NULL,
        ModifiedAt datetime2(0) NULL,
        FunctionKey nvarchar(150) NOT NULL,
        FunctionCode int NOT NULL,
        DefinitionName nvarchar(150) NOT NULL,
        ModuleCode nvarchar(50) NULL,
        ActionCode nvarchar(50) NULL,
        ScopeCode nvarchar(30) NULL,
        LifecycleStatus nvarchar(30) NOT NULL,
        SourceType nvarchar(30) NOT NULL,
        SourceAssembly nvarchar(250) NULL,
        SourceTypeName nvarchar(250) NULL,
        ReplacementFunctionKey nvarchar(150) NULL,
        DefinitionHash nvarchar(128) NOT NULL,
        FirstDiscoveredAt datetime2(0) NOT NULL,
        LastSeenAt datetime2(0) NOT NULL,
        ResolvedAt datetime2(0) NULL,
        IsIgnored bit NOT NULL CONSTRAINT DF_F03SecurityFunctionRegistry_IsIgnored DEFAULT 0
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.F03SecurityFunctionRegistry') AND name = N'UX_F03SecurityFunctionRegistry_FunctionKey')
    CREATE UNIQUE INDEX UX_F03SecurityFunctionRegistry_FunctionKey ON dbo.F03SecurityFunctionRegistry(FunctionKey);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.F03SecurityFunctionRegistry') AND name = N'IX_F03SecurityFunctionRegistry_Status')
    CREATE INDEX IX_F03SecurityFunctionRegistry_Status ON dbo.F03SecurityFunctionRegistry(LifecycleStatus, IsIgnored);

/* Seed the registry from the existing function catalog without granting anything. */
INSERT INTO dbo.F03SecurityFunctionRegistry
(
    FunctionKey, FunctionCode, DefinitionName, ModuleCode, ActionCode, ScopeCode,
    LifecycleStatus, SourceType, DefinitionHash, FirstDiscoveredAt, LastSeenAt, IsIgnored
)
SELECT f.FunctionKey, f.FunctionCode, f.FunctionName, f.ModuleCode, f.ActionCode, f.ScopeCode,
       CASE WHEN f.LifecycleStatus IN (N'Retired',N'Replaced') THEN f.LifecycleStatus ELSE N'Active' END,
       f.SourceType, CONVERT(varchar(128), HASHBYTES('SHA2_256', CONCAT(f.FunctionKey, N'|', f.FunctionCode, N'|', f.FunctionName)), 2),
       f.CreatedAt, ISNULL(f.LastSeenAt,f.CreatedAt), 0
FROM dbo.F03Functions f
WHERE NOT EXISTS (SELECT 1 FROM dbo.F03SecurityFunctionRegistry r WHERE r.FunctionKey = f.FunctionKey);
GO
