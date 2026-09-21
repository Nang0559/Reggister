USE [FVN_REGISTER];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

/*
  FVN_REGISTER - Application Authorization
  ---------------------------------------------------------------------------
  HRM remains source-of-truth for HR masters. Security is FVN-owned.
  F03HrmUserRoleRules may derive a DEFAULT role from HRM Dept/Position only;
  it must never overwrite FVN role/function assignments.

  Compatibility:
    - F03Permissions.PermissionCode remains the legacy role code.
    - F03Users.PermissionCode remains the primary/default role.
    - F03UserFunctions remains a legacy direct user-function grant.
  New RBAC:
    - F03Roles
    - F03RoleFunctions
    - F03UserRoles
    - F03Functions.ActionCode / ScopeCode / ModuleCode
*/

IF OBJECT_ID(N'dbo.F03Roles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03Roles
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03Roles PRIMARY KEY,
        IsActive bit NULL CONSTRAINT DF_F03Roles_IsActive DEFAULT(1),
        CreatedBy int NOT NULL CONSTRAINT DF_F03Roles_CreatedBy DEFAULT(0),
        LastModifiedSource nvarchar(50) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03Roles_CreatedAt DEFAULT(GETDATE()),
        ModifiedBy int NULL,
        ModifiedAt datetime2(0) NULL,
        RoleCode int NOT NULL,
        RoleName nvarchar(100) NOT NULL,
        Detail nvarchar(500) NULL,
        IsSystem bit NOT NULL CONSTRAINT DF_F03Roles_IsSystem DEFAULT(0)
    );
END;
GO

/* Upgrade legacy RBAC tables to the canonical BaseAuditEntity Id model. */
IF OBJECT_ID(N'dbo.F03Roles',N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.F03Roles',N'Id') IS NULL
   AND COL_LENGTH(N'dbo.F03Roles',N'IdRole') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.F03Roles.IdRole', N'Id', N'COLUMN';
END;
IF OBJECT_ID(N'dbo.F03Roles',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03Roles',N'IsActive') IS NULL ALTER TABLE dbo.F03Roles ADD IsActive bit NULL CONSTRAINT DF_F03Roles_IsActive DEFAULT 1;
    IF COL_LENGTH(N'dbo.F03Roles',N'CreatedBy') IS NULL ALTER TABLE dbo.F03Roles ADD CreatedBy int NOT NULL CONSTRAINT DF_F03Roles_CreatedBy DEFAULT 0;
    IF COL_LENGTH(N'dbo.F03Roles',N'LastModifiedSource') IS NULL ALTER TABLE dbo.F03Roles ADD LastModifiedSource nvarchar(50) NULL;
    IF COL_LENGTH(N'dbo.F03Roles',N'ModifiedAt') IS NULL ALTER TABLE dbo.F03Roles ADD ModifiedAt datetime2(0) NULL;
    IF COL_LENGTH(N'dbo.F03Roles',N'ModifiedBy') IS NULL ALTER TABLE dbo.F03Roles ADD ModifiedBy int NULL;
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03Roles_RoleCode' AND object_id=OBJECT_ID(N'dbo.F03Roles'))
    CREATE UNIQUE INDEX UX_F03Roles_RoleCode ON dbo.F03Roles(RoleCode);
GO

IF COL_LENGTH(N'dbo.F03Functions',N'ModuleCode') IS NULL
    ALTER TABLE dbo.F03Functions ADD ModuleCode nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.F03Functions',N'ActionCode') IS NULL
    ALTER TABLE dbo.F03Functions ADD ActionCode nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.F03Functions',N'ScopeCode') IS NULL
    ALTER TABLE dbo.F03Functions ADD ScopeCode nvarchar(30) NULL;
IF COL_LENGTH(N'dbo.F03Functions',N'DisplayOrder') IS NULL
    ALTER TABLE dbo.F03Functions ADD DisplayOrder int NOT NULL CONSTRAINT DF_F03Functions_DisplayOrder DEFAULT(0);
GO

IF OBJECT_ID(N'dbo.F03RoleFunctions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03RoleFunctions
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03RoleFunctions PRIMARY KEY,
        IsActive bit NULL CONSTRAINT DF_F03RoleFunctions_IsActive DEFAULT(1),
        CreatedBy int NOT NULL CONSTRAINT DF_F03RoleFunctions_CreatedBy DEFAULT(0),
        LastModifiedSource nvarchar(50) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03RoleFunctions_CreatedAt DEFAULT(GETDATE()),
        ModifiedBy int NULL,
        ModifiedAt datetime2(0) NULL,
        IdRole int NOT NULL,
        IdFunction int NOT NULL,
        CONSTRAINT FK_F03RoleFunctions_Role FOREIGN KEY(IdRole) REFERENCES dbo.F03Roles(Id),
        CONSTRAINT FK_F03RoleFunctions_Function FOREIGN KEY(IdFunction) REFERENCES dbo.F03Functions(Id)
    );
END;
GO

IF OBJECT_ID(N'dbo.F03UserRoles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03UserRoles
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03UserRoles PRIMARY KEY,
        IsActive bit NULL CONSTRAINT DF_F03UserRoles_IsActive DEFAULT(1),
        CreatedBy int NOT NULL CONSTRAINT DF_F03UserRoles_CreatedBy DEFAULT(0),
        LastModifiedSource nvarchar(50) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03UserRoles_CreatedAt DEFAULT(GETDATE()),
        ModifiedBy int NULL,
        ModifiedAt datetime2(0) NULL,
        IdUser int NOT NULL,
        IdRole int NOT NULL,
        IsPrimary bit NOT NULL CONSTRAINT DF_F03UserRoles_IsPrimary DEFAULT(0),
        CONSTRAINT FK_F03UserRoles_User FOREIGN KEY(IdUser) REFERENCES dbo.F03Users(Id),
        CONSTRAINT FK_F03UserRoles_Role FOREIGN KEY(IdRole) REFERENCES dbo.F03Roles(Id)
    );
END;
GO

/* Upgrade legacy UserRole composite-key table to BaseAuditEntity.Id. */
IF OBJECT_ID(N'dbo.F03UserRoles',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03UserRoles',N'Id') IS NULL
    BEGIN
        ALTER TABLE dbo.F03UserRoles ADD Id int IDENTITY(1,1) NOT NULL;
        DECLARE @pkUserRole sysname;
        SELECT @pkUserRole = kc.name
        FROM sys.key_constraints kc
        WHERE kc.parent_object_id = OBJECT_ID(N'dbo.F03UserRoles') AND kc.type = 'PK';
        IF @pkUserRole IS NOT NULL EXEC(N'ALTER TABLE dbo.F03UserRoles DROP CONSTRAINT [' + @pkUserRole + N']');
        ALTER TABLE dbo.F03UserRoles ADD CONSTRAINT PK_F03UserRoles PRIMARY KEY(Id);
    END;
    IF COL_LENGTH(N'dbo.F03UserRoles',N'IsActive') IS NULL ALTER TABLE dbo.F03UserRoles ADD IsActive bit NULL CONSTRAINT DF_F03UserRoles_IsActive DEFAULT 1;
    IF COL_LENGTH(N'dbo.F03UserRoles',N'CreatedBy') IS NULL ALTER TABLE dbo.F03UserRoles ADD CreatedBy int NOT NULL CONSTRAINT DF_F03UserRoles_CreatedBy DEFAULT 0;
    IF COL_LENGTH(N'dbo.F03UserRoles',N'LastModifiedSource') IS NULL ALTER TABLE dbo.F03UserRoles ADD LastModifiedSource nvarchar(50) NULL;
    IF COL_LENGTH(N'dbo.F03UserRoles',N'ModifiedBy') IS NULL ALTER TABLE dbo.F03UserRoles ADD ModifiedBy int NULL;
    IF COL_LENGTH(N'dbo.F03UserRoles',N'ModifiedAt') IS NULL ALTER TABLE dbo.F03UserRoles ADD ModifiedAt datetime2(0) NULL;
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03UserRoles_User_Role' AND object_id=OBJECT_ID(N'dbo.F03UserRoles'))
    CREATE UNIQUE INDEX UX_F03UserRoles_User_Role ON dbo.F03UserRoles(IdUser,IdRole);

/* Upgrade legacy RoleFunction composite-key table to BaseAuditEntity.Id. */
IF OBJECT_ID(N'dbo.F03RoleFunctions',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03RoleFunctions',N'Id') IS NULL
    BEGIN
        ALTER TABLE dbo.F03RoleFunctions ADD Id int IDENTITY(1,1) NOT NULL;
        DECLARE @pk sysname;
        SELECT @pk = kc.name
        FROM sys.key_constraints kc
        WHERE kc.parent_object_id = OBJECT_ID(N'dbo.F03RoleFunctions') AND kc.type = 'PK';
        IF @pk IS NOT NULL EXEC(N'ALTER TABLE dbo.F03RoleFunctions DROP CONSTRAINT [' + @pk + N']');
        ALTER TABLE dbo.F03RoleFunctions ADD CONSTRAINT PK_F03RoleFunctions PRIMARY KEY(Id);
    END;
    IF COL_LENGTH(N'dbo.F03RoleFunctions',N'IsActive') IS NULL ALTER TABLE dbo.F03RoleFunctions ADD IsActive bit NULL CONSTRAINT DF_F03RoleFunctions_IsActive DEFAULT 1;
    IF COL_LENGTH(N'dbo.F03RoleFunctions',N'CreatedBy') IS NULL ALTER TABLE dbo.F03RoleFunctions ADD CreatedBy int NOT NULL CONSTRAINT DF_F03RoleFunctions_CreatedBy DEFAULT 0;
    IF COL_LENGTH(N'dbo.F03RoleFunctions',N'LastModifiedSource') IS NULL ALTER TABLE dbo.F03RoleFunctions ADD LastModifiedSource nvarchar(50) NULL;
    IF COL_LENGTH(N'dbo.F03RoleFunctions',N'ModifiedBy') IS NULL ALTER TABLE dbo.F03RoleFunctions ADD ModifiedBy int NULL;
    IF COL_LENGTH(N'dbo.F03RoleFunctions',N'ModifiedAt') IS NULL ALTER TABLE dbo.F03RoleFunctions ADD ModifiedAt datetime2(0) NULL;
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03RoleFunctions_Role_Function' AND object_id=OBJECT_ID(N'dbo.F03RoleFunctions'))
    CREATE UNIQUE INDEX UX_F03RoleFunctions_Role_Function ON dbo.F03RoleFunctions(IdRole,IdFunction);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03UserRoles_UserPrimary' AND object_id=OBJECT_ID(N'dbo.F03UserRoles'))
    CREATE INDEX IX_F03UserRoles_UserPrimary ON dbo.F03UserRoles(IdUser,IsPrimary);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03RoleFunctions_Function' AND object_id=OBJECT_ID(N'dbo.F03RoleFunctions'))
    CREATE INDEX IX_F03RoleFunctions_Function ON dbo.F03RoleFunctions(IdFunction,IdRole);
GO

/* Synchronize role catalog from the existing F03Permissions role catalog. */
INSERT dbo.F03Roles(RoleCode,RoleName,Detail,IsSystem,IsActive)
SELECT p.PermissionCode,p.PermissionName,p.Detail,1,ISNULL(p.IsActive,1)
FROM dbo.F03Permissions p
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.F03Roles r WHERE r.RoleCode=p.PermissionCode
);
GO

/* Ensure the canonical RBAC role catalog exists even when 06_Seed.sql is intentionally excluded
   from the master deployment. F03Permissions is legacy/test data and may be empty in production. */
INSERT dbo.F03Roles(RoleCode,RoleName,Detail,IsSystem,IsActive,CreatedBy)
SELECT v.RoleCode,v.RoleName,v.Detail,1,1,0
FROM (VALUES
    (1,N'SuperAdmin',N'Full system administration access'),
    (2,N'Admin',N'Administrative data access'),
    (3,N'Editor',N'Editable business data access'),
    (4,N'Approver',N'Approval workflow access'),
    (5,N'User',N'Normal user access'),
    (6,N'Guest',N'Guest access')
) AS v(RoleCode,RoleName,Detail)
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.F03Roles r WHERE r.RoleCode=v.RoleCode
);
GO

/*
  Canonical capability catalog.
  One row = one server-enforced action. Scope is a data-scope hint consumed
  by application services (Own / Department / All).
*/
INSERT dbo.F03Functions(IsActive,CreatedBy,FunctionCode,FunctionName,Detail,ModuleCode,ActionCode,ScopeCode,DisplayOrder)
SELECT 1,0,v.FunctionCode,v.Name,v.Detail,v.ModuleCode,v.ActionCode,v.ScopeCode,v.SortNo
FROM (VALUES
(2001,N'Leave.View',N'Xem đơn nghỉ phép',N'Leave',N'View',N'Own',10),
(2002,N'Leave.Create',N'Tạo đơn nghỉ phép',N'Leave',N'Create',N'Own',20),
(2003,N'Leave.Edit',N'Sửa đơn nghỉ phép',N'Leave',N'Edit',N'Own',30),
(2004,N'Leave.Cancel',N'Hủy đơn nghỉ phép',N'Leave',N'Cancel',N'Own',40),
(2005,N'Leave.Approve',N'Duyệt đơn nghỉ phép',N'Leave',N'Approve',N'Department',50),
(2006,N'Leave.Export',N'Xuất báo cáo nghỉ phép',N'Leave',N'Export',N'Department',60),
(2101,N'OT.View',N'Xem OT',N'OT',N'View',N'Own',110),
(2102,N'OT.Create',N'Tạo đăng ký OT',N'OT',N'Create',N'Own',120),
(2103,N'OT.Edit',N'Sửa đăng ký OT',N'OT',N'Edit',N'Own',130),
(2104,N'OT.Cancel',N'Hủy đăng ký OT',N'OT',N'Cancel',N'Own',140),
(2105,N'OT.Approve',N'Duyệt OT',N'OT',N'Approve',N'Department',150),
(2106,N'OT.Reconcile',N'Đối soát OT thực tế',N'OT',N'Reconcile',N'All',160),
(2107,N'OT.Export',N'Xuất báo cáo OT',N'OT',N'Export',N'Department',170),
(2201,N'Trip.View',N'Xem công tác',N'Trip',N'View',N'Own',210),
(2202,N'Trip.Create',N'Tạo đăng ký công tác',N'Trip',N'Create',N'Own',220),
(2203,N'Trip.Edit',N'Sửa đăng ký công tác',N'Trip',N'Edit',N'Own',230),
(2204,N'Trip.Cancel',N'Hủy đăng ký công tác',N'Trip',N'Cancel',N'Own',240),
(2205,N'Trip.Approve',N'Duyệt công tác',N'Trip',N'Approve',N'Department',250),
(2206,N'Trip.Export',N'Xuất báo cáo công tác',N'Trip',N'Export',N'Department',260),
(2301,N'Equipment.View',N'Xem thiết bị',N'Equipment',N'View',N'Department',310),
(2302,N'Equipment.Create',N'Tạo yêu cầu thiết bị',N'Equipment',N'Create',N'Own',320),
(2303,N'Equipment.Edit',N'Sửa yêu cầu thiết bị',N'Equipment',N'Edit',N'Own',330),
(2304,N'Equipment.Repair',N'Xử lý sửa chữa',N'Equipment',N'Repair',N'Department',340),
(2305,N'Equipment.Approve',N'Duyệt thiết bị',N'Equipment',N'Approve',N'Department',350),
(2401,N'UserManagement.View',N'Xem tài khoản',N'UserManagement',N'View',N'All',410),
(2402,N'UserManagement.Create',N'Tạo tài khoản',N'UserManagement',N'Create',N'All',420),
(2403,N'UserManagement.Edit',N'Sửa tài khoản',N'UserManagement',N'Edit',N'All',430),
(2404,N'UserManagement.Lock',N'Khóa/mở khóa tài khoản',N'UserManagement',N'Lock',N'All',440),
(2405,N'UserManagement.ResetPassword',N'Đặt lại mật khẩu',N'UserManagement',N'ResetPassword',N'All',450),
(2406,N'UserManagement.AssignPermission',N'Gán quyền',N'UserManagement',N'AssignPermission',N'All',460),
(2501,N'HrmSync.ViewStatus',N'Xem trạng thái đồng bộ HRM',N'HrmSync',N'ViewStatus',N'All',510),
(2502,N'HrmSync.Sync',N'Chạy đồng bộ HRM',N'HrmSync',N'Sync',N'All',520),
(2503,N'HrmSync.Review',N'Xem/review dữ liệu HRM',N'HrmSync',N'Review',N'All',530),
(2504,N'HrmSync.Retry',N'Chạy lại đồng bộ lỗi',N'HrmSync',N'Retry',N'All',540),
(2601,N'Security.View',N'Xem Security Center',N'Security',N'View',N'All',610),
(2602,N'Security.ManageRoles',N'Quản lý role',N'Security',N'ManageRoles',N'All',620),
(2603,N'Security.ManageFunctions',N'Quản lý function/action',N'Security',N'ManageFunctions',N'All',630),
(2604,N'Security.Audit',N'Xem audit security',N'Security',N'Audit',N'All',640),
(2701,N'Dashboard.View',N'Xem dashboard',N'Dashboard',N'View',N'Own',710),
(2307,N'Equipment.Export',N'Xuất báo cáo thiết bị',N'Equipment',N'Export',N'Department',360),
(2901,N'Attendance.View',N'Xem báo cáo chấm công',N'Attendance',N'View',N'All',810),
(2902,N'Attendance.Export',N'Xuất báo cáo chấm công',N'Attendance',N'Export',N'All',820)
) AS v(FunctionCode,Name,Detail,ModuleCode,ActionCode,ScopeCode,SortNo)
WHERE NOT EXISTS(SELECT 1 FROM dbo.F03Functions f WHERE f.FunctionCode=v.FunctionCode);
GO

/* Update metadata for already-created canonical functions without overwriting business data. */
UPDATE f
SET ModuleCode=LEFT(f.FunctionName,CHARINDEX(N'.',f.FunctionName+N'.')-1),
    ActionCode=SUBSTRING(f.FunctionName,CHARINDEX(N'.',f.FunctionName+N'.')+1,50),
    ScopeCode=CASE
        WHEN f.FunctionCode IN (2005,2006,2105,2107,2205,2301,2304,2305) THEN N'Department'
        WHEN f.FunctionCode IN (2106,2401,2402,2403,2404,2405,2406,2501,2502,2503,2504,2601,2602,2603,2604) THEN N'All'
        WHEN f.FunctionCode BETWEEN 2001 AND 2999 THEN N'Own'
        ELSE f.ScopeCode
    END
FROM dbo.F03Functions AS f
WHERE f.FunctionCode BETWEEN 2000 AND 2999;
GO

/* Seed role -> function matrix. Role codes retain existing F03Permissions values. */
DECLARE @SuperAdmin int=(SELECT Id FROM dbo.F03Roles WHERE RoleCode=1);
DECLARE @Admin int=(SELECT Id FROM dbo.F03Roles WHERE RoleCode=2);
DECLARE @Editor int=(SELECT Id FROM dbo.F03Roles WHERE RoleCode=3);
DECLARE @Approver int=(SELECT Id FROM dbo.F03Roles WHERE RoleCode=4);
DECLARE @User int=(SELECT Id FROM dbo.F03Roles WHERE RoleCode=5);
DECLARE @Guest int=(SELECT Id FROM dbo.F03Roles WHERE RoleCode=6);

INSERT dbo.F03RoleFunctions(IdRole,IdFunction)
SELECT r.Id,f.Id
FROM
(
    SELECT @SuperAdmin IdRole, f.FunctionCode FROM dbo.F03Functions f WHERE f.FunctionCode BETWEEN 2001 AND 2999
    UNION ALL
    SELECT @Admin, f.FunctionCode FROM dbo.F03Functions f
      WHERE f.FunctionCode BETWEEN 2001 AND 2999
        AND f.FunctionCode NOT IN(2602,2603)
    UNION ALL
    SELECT @Editor, f.FunctionCode FROM dbo.F03Functions f
      WHERE f.ActionCode IN(N'View',N'Create',N'Edit',N'Cancel')
        AND f.ModuleCode IN(N'Leave',N'OT',N'Trip',N'Equipment')
    UNION ALL
    SELECT @Approver, f.FunctionCode FROM dbo.F03Functions f
      WHERE f.ActionCode IN(N'View',N'Create',N'Edit',N'Cancel',N'Approve')
        AND f.ModuleCode IN(N'Leave',N'OT',N'Trip',N'Equipment')
    UNION ALL
    SELECT @User, f.FunctionCode FROM dbo.F03Functions f
      WHERE f.ActionCode IN(N'View',N'Create',N'Edit',N'Cancel')
        AND f.ModuleCode IN(N'Leave',N'OT',N'Trip',N'Equipment')
    UNION ALL
    SELECT @User, f.FunctionCode FROM dbo.F03Functions f
      WHERE f.FunctionCode=2901
    UNION ALL
    SELECT @Guest, f.FunctionCode FROM dbo.F03Functions f
      WHERE f.ActionCode=N'View' AND f.ModuleCode IN(N'Leave',N'OT',N'Trip',N'Equipment')
) x
JOIN dbo.F03Roles r ON r.Id=x.IdRole
JOIN dbo.F03Functions f ON f.FunctionCode=x.FunctionCode
WHERE x.IdRole IS NOT NULL
  AND NOT EXISTS(SELECT 1 FROM dbo.F03RoleFunctions rf WHERE rf.IdRole=r.Id AND rf.IdFunction=f.Id);
GO

/* Every authenticated role gets the dashboard shell; module providers still require their own capability. */
INSERT dbo.F03RoleFunctions(IdRole,IdFunction)
SELECT r.Id,f.Id
FROM dbo.F03Roles r
JOIN dbo.F03Functions f ON f.FunctionCode=2701
WHERE r.IsActive=1
  AND NOT EXISTS(
      SELECT 1 FROM dbo.F03RoleFunctions rf
      WHERE rf.IdRole=r.Id AND rf.IdFunction=f.Id
  );
GO

/* Migrate the legacy primary role into the normalized multi-role table. */
INSERT dbo.F03UserRoles(IdUser,IdRole,IsPrimary,CreatedBy)
SELECT u.Id,r.Id,1,0
FROM dbo.F03Users u
JOIN dbo.F03Roles r ON r.RoleCode=u.PermissionCode
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.F03UserRoles ur WHERE ur.IdUser=u.Id AND ur.IdRole=r.Id
);
GO

PRINT N'FVN_REGISTER application authorization schema/seed completed.';
GO

/* P0/P1 RBAC hardening: dedicated administration capabilities. */
INSERT dbo.F03Functions(IsActive,CreatedBy,FunctionCode,FunctionName,Detail,ModuleCode,ActionCode,ScopeCode,DisplayOrder)
SELECT 1,0,v.FunctionCode,v.Name,v.Detail,v.ModuleCode,v.ActionCode,v.ScopeCode,v.SortNo
FROM (VALUES
(3001,N'Department.View',N'Xem bộ phận',N'Department',N'View',N'All',900),
(3002,N'Department.Manage',N'Thêm/sửa/xóa bộ phận',N'Department',N'Manage',N'All',910),
(3011,N'Employee.View',N'Xem nhân viên',N'Employee',N'View',N'Department',920),
(3012,N'Employee.Manage',N'Thêm/sửa/xóa nhân viên',N'Employee',N'Manage',N'All',930),
(3021,N'LeaveType.View',N'Xem loại nghỉ',N'LeaveType',N'View',N'All',940),
(3022,N'LeaveType.Manage',N'Quản lý loại nghỉ',N'LeaveType',N'Manage',N'All',950),
(3031,N'Approver.View',N'Xem cấu hình người duyệt',N'Approver',N'View',N'All',960),
(3032,N'Approver.Manage',N'Quản lý người duyệt',N'Approver',N'Manage',N'All',970),
(3041,N'WorkCalendar.View',N'Xem ngày lễ/năm làm việc',N'WorkCalendar',N'View',N'All',980),
(3042,N'WorkCalendar.Manage',N'Quản lý ngày lễ/năm làm việc',N'WorkCalendar',N'Manage',N'All',990),
(3051,N'DepartmentStatus.View',N'Xem trạng thái bộ phận',N'DepartmentStatus',N'View',N'Department',1000),
(3061,N'OTLimit.Manage',N'Quản lý hạn mức OT',N'OTLimit',N'Manage',N'All',1010),
(3071,N'ApprovalPolicy.Manage',N'Quản lý policy phê duyệt',N'ApprovalPolicy',N'Manage',N'All',1020),
(3072,N'HrmUserRoleRule.Manage',N'Quản lý rule role theo HRM',N'HrmUserRoleRule',N'Manage',N'All',1030)
) v(FunctionCode,Name,Detail,ModuleCode,ActionCode,ScopeCode,SortNo)
WHERE NOT EXISTS(SELECT 1 FROM dbo.F03Functions f WHERE f.FunctionCode=v.FunctionCode);
GO

/* New management capabilities are restricted to SuperAdmin/Admin. */
INSERT dbo.F03RoleFunctions(IdRole,IdFunction)
SELECT r.Id,f.Id
FROM dbo.F03Roles r
CROSS JOIN dbo.F03Functions f
WHERE r.RoleCode IN (1,2)
  AND f.FunctionCode IN (3001,3002,3011,3012,3021,3022,3031,3032,3041,3042,3051,3061,3071,3072)
  AND NOT EXISTS(SELECT 1 FROM dbo.F03RoleFunctions rf WHERE rf.IdRole=r.Id AND rf.IdFunction=f.Id);
GO

/* Department managers/approvers need department-scoped attendance view. */
INSERT dbo.F03RoleFunctions(IdRole,IdFunction)
SELECT r.Id,f.Id
FROM dbo.F03Roles r
CROSS JOIN dbo.F03Functions f
WHERE r.RoleCode IN (1,2,3,4)
  AND f.FunctionCode=2901
  AND NOT EXISTS(SELECT 1 FROM dbo.F03RoleFunctions rf WHERE rf.IdRole=r.Id AND rf.IdFunction=f.Id);
GO

/* OT export and attendance view/export are Department scoped. */
UPDATE dbo.F03Functions SET ScopeCode=N'Department'
WHERE FunctionCode IN (2107,2901,2902);
GO

/* Equipment cancellation is an own-request capability, available to business roles. */
INSERT dbo.F03Functions(IsActive,CreatedBy,FunctionCode,FunctionName,Detail,ModuleCode,ActionCode,ScopeCode,DisplayOrder)
SELECT 1,0,2308,N'Equipment.Cancel',N'Hủy yêu cầu thiết bị',N'Equipment',N'Cancel',N'Own',370
WHERE NOT EXISTS(SELECT 1 FROM dbo.F03Functions WHERE FunctionCode=2308);
GO
INSERT dbo.F03RoleFunctions(IdRole,IdFunction)
SELECT r.Id,f.Id
FROM dbo.F03Roles r CROSS JOIN dbo.F03Functions f
WHERE r.RoleCode IN (1,2,3,4,5) AND f.FunctionCode=2308
  AND NOT EXISTS(SELECT 1 FROM dbo.F03RoleFunctions rf WHERE rf.IdRole=r.Id AND rf.IdFunction=f.Id);
GO


INSERT dbo.F03Functions(IsActive,CreatedBy,FunctionCode,FunctionName,Detail,ModuleCode,ActionCode,ScopeCode,DisplayOrder)
SELECT 1,0,v.FunctionCode,v.Name,v.Detail,v.ModuleCode,v.ActionCode,v.ScopeCode,v.SortNo
FROM (VALUES
(3081,N'EmailQueue.Manage',N'Quản lý email queue',N'EmailQueue',N'Manage',N'All',1040),
(3082,N'EmailTemplate.Manage',N'Quản lý mẫu email',N'EmailTemplate',N'Manage',N'All',1050)
) v(FunctionCode,Name,Detail,ModuleCode,ActionCode,ScopeCode,SortNo)
WHERE NOT EXISTS(SELECT 1 FROM dbo.F03Functions f WHERE f.FunctionCode=v.FunctionCode);
GO
INSERT dbo.F03RoleFunctions(IdRole,IdFunction)
SELECT r.Id,f.Id FROM dbo.F03Roles r CROSS JOIN dbo.F03Functions f
WHERE r.RoleCode IN(1,2) AND f.FunctionCode IN(3081,3082)
AND NOT EXISTS(SELECT 1 FROM dbo.F03RoleFunctions rf WHERE rf.IdRole=r.Id AND rf.IdFunction=f.Id);
GO

/* Attendance.View is for management/approval roles; normal User must not inherit the Department scope. */
DELETE rf
FROM dbo.F03RoleFunctions rf
JOIN dbo.F03Roles r ON r.Id=rf.IdRole
JOIN dbo.F03Functions f ON f.Id=rf.IdFunction
WHERE r.RoleCode=5 AND f.FunctionCode=2901;
GO
