USE [FVN_REGISTER];
GO
SET NOCOUNT ON;
SELECT DB_NAME() AS DatabaseName;
SELECT name,type_desc FROM sys.tables WHERE name LIKE N'F03%' OR name LIKE N'Approval%' OR name LIKE N'Hrm%' ORDER BY name;
SELECT TABLE_SCHEMA,TABLE_NAME,COLUMN_NAME,DATA_TYPE,CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA=N'dbo' AND (TABLE_NAME LIKE N'F03%' OR TABLE_NAME LIKE N'Approval%' OR TABLE_NAME LIKE N'Hrm%') ORDER BY TABLE_NAME,ORDINAL_POSITION;
SELECT name,type_desc FROM sys.views WHERE name IN(N'vEmployeeApprover',N'vF03Employee',N'vF03leaveType',N'vF03LeaveRequest',N'vF03LeaveRequestDetail',N'vF03LeaveBalance',N'vF03OTRequest',N'vF03OTRequestDetail',N'vF03OTSummary',N'vF03Users',N'vw_CurrentlyPresentEmployees',N'vF03EmployeeAttendance',N'VwShiftCheckInOut');
SELECT name,type_desc FROM sys.procedures WHERE name IN(N'usp_GetPendingApproval',N'usp_DecideApproval',N'usp_SyncAttendanceStaging',N'usp_SyncOTActualHours',N'usp_DequeueEmail',N'usp_ProcessApprovalEscalation',N'usp_WriteAuditLog',N'usp_WriteUserLog');
SELECT fk.name,OBJECT_NAME(fk.parent_object_id) AS ParentTable,OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable FROM sys.foreign_keys fk ORDER BY ParentTable,fk.name;
GO

/* Architecture/PK verification: BaseAuditEntity-backed security entities use canonical Id. */
SELECT t.name AS TableName,c.name AS ColumnName,ty.name AS DataType,c.is_identity,c.is_nullable
FROM sys.tables t JOIN sys.columns c ON c.object_id=t.object_id JOIN sys.types ty ON ty.user_type_id=c.user_type_id
WHERE t.name IN(N'F03Users',N'F03Permissions',N'F03Functions') AND c.name IN(N'Id',N'IdUser',N'IdPermission',N'IdFunction')
ORDER BY t.name,c.column_id;

/* Canonical Id must exist; legacy per-entity PK names must not remain. */
IF COL_LENGTH('dbo.F03Users','Id') IS NULL OR COL_LENGTH('dbo.F03Permissions','Id') IS NULL OR COL_LENGTH('dbo.F03Functions','Id') IS NULL
    THROW 50010,'BaseAuditEntity.Id is missing from a canonical security table.',1;
IF COL_LENGTH('dbo.F03Users','IdUser') IS NOT NULL OR COL_LENGTH('dbo.F03Permissions','IdPermission') IS NOT NULL OR COL_LENGTH('dbo.F03Functions','IdFunction') IS NOT NULL
    THROW 50011,'Legacy per-entity security PK column remains.',1;
IF COL_LENGTH('dbo.F03Roles','Id') IS NULL OR COL_LENGTH('dbo.F03RoleFunctions','Id') IS NULL OR COL_LENGTH('dbo.F03UserRoles','Id') IS NULL
    THROW 50012,'RBAC BaseAuditEntity.Id is missing.',1;
GO

/* OT/HRM canonical verification. */
IF OBJECT_ID(N'dbo.F03Department',N'U') IS NOT NULL
    THROW 50020,'Legacy singular table F03Department must not be used; use F03Departments.',1;
IF OBJECT_ID(N'dbo.F03Employee',N'U') IS NOT NULL
    THROW 50021,'Legacy singular table F03Employee must not be used; use F03Employees.',1;
IF OBJECT_ID(N'dbo.F03OTRequest',N'U') IS NOT NULL
    THROW 50022,'Legacy singular table F03OTRequest must not be used; use F03OTRequests.',1;
IF OBJECT_ID(N'dbo.F03OTEmployee',N'U') IS NOT NULL
    THROW 50023,'Legacy singular table F03OTEmployee must not be used; use F03OTEmployees.',1;
IF OBJECT_ID(N'dbo.F03CV',N'U') IS NOT NULL
    THROW 50024,'Legacy F03CV table must not exist in the canonical model.',1;
IF OBJECT_ID(N'dbo.F03OTApprover',N'U') IS NOT NULL
    THROW 50025,'Legacy F03OTApprover table must not exist; use F03Approvers + approval workflow.',1;

IF OBJECT_ID(N'dbo.F03AttendanceStaging',N'U') IS NULL
    THROW 50026,'F03AttendanceStaging is required by the OT attendance pipeline.',1;

IF COL_LENGTH(N'dbo.F03AttendanceStaging',N'ShiftCategory') IS NULL
    THROW 50027,'F03AttendanceStaging must expose ShiftCategory.',1;
IF COL_LENGTH(N'dbo.F03AttendanceStaging',N'OtHours') IS NULL
    THROW 50028,'F03AttendanceStaging must expose OtHours.',1;

IF OBJECT_ID(N'dbo.usp_SyncAttendanceStaging',N'P') IS NULL
    THROW 50029,'usp_SyncAttendanceStaging is missing.',1;
IF OBJECT_ID(N'dbo.usp_SyncOTActualHours',N'P') IS NULL
    THROW 50030,'usp_SyncOTActualHours is missing.',1;

/* Raw result contract checks used by C# SqlQueryRaw<T>. */
SELECT
    p.name AS ProcedureName,
    prm.name AS ParameterName,
    TYPE_NAME(prm.user_type_id) AS ParameterType,
    prm.max_length AS MaxLength
FROM sys.parameters prm
JOIN sys.procedures p ON p.object_id=prm.object_id
WHERE p.name IN(N'usp_SyncAttendanceStaging',N'usp_SyncOTActualHours')
ORDER BY p.name,prm.parameter_id;
GO

/* Schema precision checks for EF Core decimal properties. */
IF EXISTS(
    SELECT 1 FROM sys.columns c
    JOIN sys.types t ON t.user_type_id=c.user_type_id
    WHERE c.object_id=OBJECT_ID(N'dbo.F03StagingEmployee')
      AND c.name=N'TotalLeaveDays'
      AND (t.name<>N'decimal' OR c.precision<10 OR c.scale<2)
)
    THROW 50108,'F03StagingEmployee.TotalLeaveDays must be decimal(10,2) or wider.',1;

IF EXISTS(
    SELECT 1 FROM sys.columns c
    JOIN sys.types t ON t.user_type_id=c.user_type_id
    WHERE c.object_id=OBJECT_ID(N'dbo.F03StagingOTType')
      AND c.name=N'RateMultiplier'
      AND (t.name<>N'decimal' OR c.precision<5 OR c.scale<2)
)
    THROW 50109,'F03StagingOTType.RateMultiplier must be decimal(5,2) or wider.',1;

IF COL_LENGTH(N'dbo.F03Departments',N'ParentDeptCode') IS NULL
    THROW 50110,'F03Departments.ParentDeptCode is required.',1;
IF COL_LENGTH(N'dbo.F03Departments',N'DisplayPriority') IS NULL
    THROW 50111,'F03Departments.DisplayPriority is required.',1;
IF COL_LENGTH(N'dbo.F03Departments',N'ShowInReport') IS NULL
    THROW 50112,'F03Departments.ShowInReport is required.',1;

/* Pipeline A — HRM master source contracts. */
IF OBJECT_ID(N'dbo.usp_SyncHrmLeaveTypeSource',N'P') IS NULL THROW 50100,'Missing usp_SyncHrmLeaveTypeSource',1;
IF OBJECT_ID(N'dbo.usp_SyncHrmDepartmentSource',N'P') IS NULL THROW 50101,'Missing usp_SyncHrmDepartmentSource',1;
IF OBJECT_ID(N'dbo.usp_SyncHrmPositionSource',N'P') IS NULL THROW 50102,'Missing usp_SyncHrmPositionSource',1;
IF OBJECT_ID(N'dbo.usp_SyncHrmEmployeeSource',N'P') IS NULL THROW 50103,'Missing usp_SyncHrmEmployeeSource',1;

IF COL_LENGTH(N'dbo.F03StagingDepartment',N'ParentDeptCode') IS NULL THROW 50104,'F03StagingDepartment.ParentDeptCode missing',1;
IF COL_LENGTH(N'dbo.F03StagingDepartment',N'DisplayPriority') IS NULL THROW 50105,'F03StagingDepartment.DisplayPriority missing',1;
IF COL_LENGTH(N'dbo.F03StagingDepartment',N'ShowInReport') IS NULL THROW 50106,'F03StagingDepartment.ShowInReport missing',1;
IF COL_LENGTH(N'dbo.F03StagingEmployee',N'EmployeeNo') IS NULL THROW 50107,'F03StagingEmployee.EmployeeNo missing',1;
GO


PRINT '--- HRM automatic user provisioning ---';
IF OBJECT_ID(N'dbo.F03HrmUserRoleRules',N'U') IS NULL
    THROW 51001, 'Missing dbo.F03HrmUserRoleRules', 1;

SELECT
    UserRoleRuleCount = (SELECT COUNT(*) FROM dbo.F03HrmUserRoleRules);

/* BaseAudit synchronization checks for common entities normalized in this pass. */
IF OBJECT_ID(N'dbo.F03LeaveDayDetails',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03LeaveDayDetails',N'Id') IS NULL THROW 51010, 'F03LeaveDayDetails must expose canonical Id.', 1;
    IF COL_LENGTH(N'dbo.F03LeaveDayDetails',N'CreatedBy') IS NULL THROW 51011, 'F03LeaveDayDetails must expose BaseAudit CreatedBy.', 1;
    IF COL_LENGTH(N'dbo.F03LeaveDayDetails',N'CreatedAt') IS NULL THROW 51012, 'F03LeaveDayDetails must expose BaseAudit CreatedAt.', 1;
END;
IF OBJECT_ID(N'dbo.F03UserSessions',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03UserSessions',N'Id') IS NULL THROW 51013, 'F03UserSessions must expose canonical Id.', 1;
    IF COL_LENGTH(N'dbo.F03UserSessions',N'CreatedBy') IS NULL THROW 51014, 'F03UserSessions must expose BaseAudit CreatedBy.', 1;
    IF COL_LENGTH(N'dbo.F03UserSessions',N'CreatedAt') IS NULL THROW 51015, 'F03UserSessions must expose BaseAudit CreatedAt.', 1;
END;
IF OBJECT_ID(N'dbo.F03Attachment',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03Attachment',N'Id') IS NULL THROW 51016, 'F03Attachment must expose canonical Id.', 1;
    IF COL_LENGTH(N'dbo.F03Attachment',N'FileId') IS NOT NULL THROW 51017, 'Legacy F03Attachment.FileId must not remain as the database PK column.', 1;
END;
IF OBJECT_ID(N'dbo.F03EmailLogs',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03EmailLogs',N'CreatedBy') IS NULL THROW 51018, 'F03EmailLogs must expose BaseAudit CreatedBy.', 1;
END;
IF OBJECT_ID(N'dbo.F03UserLogs',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03UserLogs',N'CreatedBy') IS NULL THROW 51019, 'F03UserLogs must expose BaseAudit CreatedBy.', 1;
END;

/* Approval BaseAudit checks */
IF OBJECT_ID(N'dbo.F03ApprovalSnapshots',N'U') IS NOT NULL AND COL_LENGTH(N'dbo.F03ApprovalSnapshots',N'CreatedBy') IS NULL THROW 51020,'F03ApprovalSnapshots must expose BaseAudit fields.',1;
IF OBJECT_ID(N'dbo.F03ApprovalStepSnapshots',N'U') IS NOT NULL AND COL_LENGTH(N'dbo.F03ApprovalStepSnapshots',N'CreatedBy') IS NULL THROW 51021,'F03ApprovalStepSnapshots must expose BaseAudit fields.',1;
IF OBJECT_ID(N'dbo.F03ApprovalReminderLog',N'U') IS NOT NULL AND COL_LENGTH(N'dbo.F03ApprovalReminderLog',N'CreatedBy') IS NULL THROW 51022,'F03ApprovalReminderLog must expose BaseAudit fields.',1;
