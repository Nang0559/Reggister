/*
FVN_REGISTER - SQL 12 Final Verification
This is the canonical final verification script for deployment 01..12.
*/
USE [FVN_REGISTER];
GO
SET NOCOUNT ON;

DECLARE @RequiredTables TABLE(Name sysname);
INSERT @RequiredTables VALUES
(N'F03Employees'),(N'F03Users'),(N'F03Departments'),(N'F03Positions'),
(N'F03LeaveType'),(N'F03Permissions'),(N'F03HrmUserRoleRules'),
(N'F03EmailQueues'),(N'F03SyncReviewFlag'),(N'F03StagingEmployee'),
(N'F03StagingDepartment'),(N'F03StagingPosition'),(N'F03StagingLeaveType'),
(N'F03Shifts'),(N'F03ShiftSchedules'),(N'F03ShiftScheduleDays'),
(N'F03EmployeeShiftSchedules'),(N'F03HrmShiftReference');

IF EXISTS (
    SELECT 1 FROM @RequiredTables r
    WHERE OBJECT_ID(N'dbo.'+r.Name,N'U') IS NULL
)
BEGIN
    SELECT r.Name AS MissingTable FROM @RequiredTables r
    WHERE OBJECT_ID(N'dbo.'+r.Name,N'U') IS NULL;
    THROW 51200, 'One or more required FVN_REGISTER tables are missing.', 1;
END;

IF COL_LENGTH(N'dbo.F03Employees',N'EndWorkingDate') IS NULL
    THROW 51201, 'F03Employees.EndWorkingDate is required.', 1;
IF COL_LENGTH(N'dbo.F03Employees',N'EmployeeNo') IS NULL
    THROW 51202, 'F03Employees.EmployeeNo is required.', 1;
IF COL_LENGTH(N'dbo.F03Employees',N'LevelApprove') IS NULL
    THROW 51203, 'F03Employees.LevelApprove is required.', 1;

DECLARE @p int,@s int;
SELECT @p=c.precision,@s=c.scale
FROM sys.columns c
JOIN sys.types t ON t.user_type_id=c.user_type_id
WHERE c.object_id=OBJECT_ID(N'dbo.F03Employees') AND c.name=N'TotalLeaveDays';
IF @p < 10 OR @s < 2
    THROW 51204, 'F03Employees.TotalLeaveDays must be decimal(10,2) or wider.', 1;

SELECT @p=c.precision,@s=c.scale
FROM sys.columns c
JOIN sys.types t ON t.user_type_id=c.user_type_id
WHERE c.object_id=OBJECT_ID(N'dbo.F03OTTypes') AND c.name=N'RateMultiplier';
IF @p < 5 OR @s < 2
    THROW 51205, 'F03OTTypes.RateMultiplier must be decimal(5,2) or wider.', 1;

IF OBJECT_ID(N'dbo.usp_SyncHrmLeaveTypeSource',N'P') IS NULL THROW 51210,'Missing HRM LeaveType source procedure.',1;
IF OBJECT_ID(N'dbo.usp_SyncHrmDepartmentSource',N'P') IS NULL THROW 51211,'Missing HRM Department source procedure.',1;
IF OBJECT_ID(N'dbo.usp_SyncHrmPositionSource',N'P') IS NULL THROW 51212,'Missing HRM Position source procedure.',1;
IF OBJECT_ID(N'dbo.usp_SyncHrmEmployeeSource',N'P') IS NULL THROW 51213,'Missing HRM Employee source procedure.',1;
IF OBJECT_ID(N'dbo.usp_SyncHrmShiftMaster',N'P') IS NULL THROW 51214,'Missing HRM shift master sync procedure.',1;
IF OBJECT_ID(N'dbo.usp_SyncAttendanceStaging',N'P') IS NULL THROW 51217,'Missing attendance staging procedure.',1;
IF OBJECT_ID(N'dbo.usp_SyncOTActualHours',N'P') IS NULL THROW 51215,'Missing OT reconciliation procedure.',1;
IF OBJECT_ID(N'dbo.usp_DequeueEmail',N'P') IS NULL THROW 51216,'Missing email dequeue procedure.',1;

IF COL_LENGTH(N'dbo.F03HrmUserRoleRules',N'DeptCode') IS NULL
    OR COL_LENGTH(N'dbo.F03HrmUserRoleRules',N'PositionCode') IS NULL
    OR COL_LENGTH(N'dbo.F03HrmUserRoleRules',N'PermissionCode') IS NULL
    THROW 51220,'F03HrmUserRoleRules schema is incomplete.',1;

SELECT
    DatabaseName = DB_NAME(),
    EmployeeCount = (SELECT COUNT(*) FROM dbo.F03Employees),
    UserCount = (SELECT COUNT(*) FROM dbo.F03Users),
    ActiveUserCount = (SELECT COUNT(*) FROM dbo.F03Users WHERE IsActive=1),
    RoleRuleCount = (SELECT COUNT(*) FROM dbo.F03HrmUserRoleRules),
    PendingEmailCount = (SELECT COUNT(*) FROM dbo.F03EmailQueues WHERE Status IN(N'Pending',N'Retry')),
    OpenReviewFlagCount = (SELECT COUNT(*) FROM dbo.F03SyncReviewFlag WHERE IsResolved=0),
    ActiveShiftCount = (SELECT COUNT(*) FROM dbo.F03Shifts WHERE IsActive=1),
    ActiveShiftScheduleCount = (SELECT COUNT(*) FROM dbo.F03ShiftSchedules WHERE IsActive=1),
    ActiveEmployeeShiftScheduleCount = (SELECT COUNT(*) FROM dbo.F03EmployeeShiftSchedules WHERE IsActive=1);

PRINT N'12_Verify: PASS';
GO


/* Approval Route v2 verification (run after SQL/17_ApprovalRouteSelection.sql) */
IF OBJECT_ID(N'dbo.F03ApprovalPolicies',N'U') IS NULL
    THROW 51230, 'Missing F03ApprovalPolicies. Run SQL/17_ApprovalRouteSelection.sql.', 1;
IF OBJECT_ID(N'dbo.F03ApprovalSelections',N'U') IS NULL
    THROW 51231, 'Missing F03ApprovalSelections. Run SQL/17_ApprovalRouteSelection.sql.', 1;
GO
