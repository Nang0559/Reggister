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

/* Architecture/PK verification: security entities use legacy DB PK names mapped to C# BaseAuditEntity.Id. */
SELECT t.name AS TableName,c.name AS ColumnName,ty.name AS DataType,c.is_identity,c.is_nullable
FROM sys.tables t JOIN sys.columns c ON c.object_id=t.object_id JOIN sys.types ty ON ty.user_type_id=c.user_type_id
WHERE t.name IN(N'F03Users',N'F03Permissions',N'F03Functions') AND c.name IN(N'Id',N'IdUser',N'IdPermission',N'IdFunction')
ORDER BY t.name,c.column_id;

/* Duplicate canonical Id columns must not remain in the three legacy security tables. */
IF COL_LENGTH('dbo.F03Users','Id') IS NOT NULL OR COL_LENGTH('dbo.F03Permissions','Id') IS NOT NULL OR COL_LENGTH('dbo.F03Functions','Id') IS NOT NULL
    THROW 50010,'Duplicate canonical Id column remains in a security table.',1;
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
