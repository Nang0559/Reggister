/*
FVN_REGISTER - SQL 12 Final Verification
Canonical verification for the current production architecture.

Important:
- Approval route source-of-truth is F03Employees.PositionCode -> F03Positions
  -> F03ApprovalPolicies -> F03Approvers.
- Legacy ApprovalGroup / ApprovalPositionGroup / GroupCode requester routing
  is not part of the canonical model.
- HRM attendance calculation owns actual OT synchronization.
  Legacy usp_SyncOTActualHours must not exist.
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
(N'F03EmployeeShiftSchedules'),(N'F03HrmShiftReference'),
(N'F03ApprovalPolicies'),(N'F03ApprovalSelections'),(N'F03Approvers');

IF EXISTS (
    SELECT 1 FROM @RequiredTables r
    WHERE OBJECT_ID(N'dbo.'+r.Name,N'U') IS NULL
)
BEGIN
    SELECT r.Name AS MissingTable
    FROM @RequiredTables r
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

IF OBJECT_ID(N'dbo.usp_SyncHrmLeaveTypeSource',N'P') IS NULL
    THROW 51210,'Missing HRM LeaveType source procedure.',1;
IF OBJECT_ID(N'dbo.usp_SyncHrmDepartmentSource',N'P') IS NULL
    THROW 51211,'Missing HRM Department source procedure.',1;
IF OBJECT_ID(N'dbo.usp_SyncHrmPositionSource',N'P') IS NULL
    THROW 51212,'Missing HRM Position source procedure.',1;
IF OBJECT_ID(N'dbo.usp_SyncHrmEmployeeSource',N'P') IS NULL
    THROW 51213,'Missing HRM Employee source procedure.',1;
IF OBJECT_ID(N'dbo.usp_SyncHrmShiftMaster',N'P') IS NULL
    THROW 51214,'Missing HRM shift master sync procedure.',1;
IF OBJECT_ID(N'dbo.usp_SyncAttendanceStaging',N'P') IS NULL
    THROW 51217,'Missing attendance staging procedure.',1;
IF OBJECT_ID(N'dbo.usp_DequeueEmail',N'P') IS NULL
    THROW 51216,'Missing email dequeue procedure.',1;

IF OBJECT_ID(N'dbo.usp_SyncOTActualHours',N'P') IS NOT NULL
    THROW 51215,'Legacy usp_SyncOTActualHours must not exist; HRM attendance calculation owns actual OT.',1;

IF COL_LENGTH(N'dbo.F03HrmUserRoleRules',N'DeptCode') IS NULL
    OR COL_LENGTH(N'dbo.F03HrmUserRoleRules',N'PositionCode') IS NULL
    OR COL_LENGTH(N'dbo.F03HrmUserRoleRules',N'PermissionCode') IS NULL
    THROW 51220,'F03HrmUserRoleRules schema is incomplete.',1;

/* ============================================================
   APPROVAL ROUTE V3 — HRM PositionCode is the source of truth
   ============================================================ */

IF COL_LENGTH(N'dbo.F03ApprovalPolicies',N'PositionCode') IS NULL
    THROW 51230,'F03ApprovalPolicies.PositionCode is required.',1;
IF COL_LENGTH(N'dbo.F03ApprovalPolicies',N'RequestType') IS NULL
    THROW 51231,'F03ApprovalPolicies.RequestType is required.',1;
IF COL_LENGTH(N'dbo.F03ApprovalPolicies',N'Level') IS NULL
    THROW 51232,'F03ApprovalPolicies.Level is required.',1;
IF COL_LENGTH(N'dbo.F03ApprovalPolicies',N'Sequence') IS NULL
    THROW 51233,'F03ApprovalPolicies.Sequence is required.',1;
IF COL_LENGTH(N'dbo.F03ApprovalPolicies',N'Required') IS NULL
    THROW 51234,'F03ApprovalPolicies.Required is required.',1;

IF EXISTS (
    SELECT 1
    FROM dbo.F03ApprovalPolicies
    WHERE PositionCode IS NULL
)
    THROW 51235,'F03ApprovalPolicies contains NULL PositionCode rows.',1;

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name=N'FK_F03ApprovalPolicies_F03Positions'
      AND parent_object_id=OBJECT_ID(N'dbo.F03ApprovalPolicies')
)
    THROW 51236,'F03ApprovalPolicies must have FK to F03Positions.',1;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name=N'UX_F03ApprovalPolicies_Request_Position_Level'
      AND object_id=OBJECT_ID(N'dbo.F03ApprovalPolicies')
)
    THROW 51237,'Missing unique index UX_F03ApprovalPolicies_Request_Position_Level.',1;

IF EXISTS (
    SELECT RequestType,PositionCode,Level
    FROM dbo.F03ApprovalPolicies
    GROUP BY RequestType,PositionCode,Level
    HAVING COUNT(*) > 1
)
    THROW 51238,'Duplicate approval policies exist for RequestType + PositionCode + Level.',1;

/* Legacy requester-position/group columns must not remain. */
IF COL_LENGTH(N'dbo.F03ApprovalPolicies',N'GroupCode') IS NOT NULL
    OR COL_LENGTH(N'dbo.F03ApprovalPolicies',N'ApprovalGroupCode') IS NOT NULL
    OR COL_LENGTH(N'dbo.F03ApprovalPolicies',N'RequesterPositionCode') IS NOT NULL
    THROW 51239,'Legacy approval requester/group columns remain in F03ApprovalPolicies.',1;

/* Legacy pre-Snapshot approval step table must not remain. */
IF OBJECT_ID(N'dbo.F03ApprovalSteps',N'U') IS NOT NULL
    THROW 51247,'Legacy F03ApprovalSteps must be removed; canonical approval uses snapshots + history.',1;

IF OBJECT_ID(N'dbo.F03ApprovalPositionGroups',N'U') IS NOT NULL
    THROW 51240,'Legacy F03ApprovalPositionGroups must not exist.',1;

/*
   F03ApprovalGroups is a legacy schema object only when it represents
   requester-position routing. The current route does not read it.
   If the deployment still contains it, SQL/17 must be followed by the
   dedicated legacy cleanup before production.
*/
IF OBJECT_ID(N'dbo.F03ApprovalGroups',N'U') IS NOT NULL
    THROW 51241,'Legacy F03ApprovalGroups must be removed before production.',1;

IF OBJECT_ID(N'dbo.F03ApprovalSelections',N'U') IS NULL
    THROW 51242,'Missing F03ApprovalSelections.',1;
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name=N'UX_F03ApprovalSelections_Request_Level'
      AND object_id=OBJECT_ID(N'dbo.F03ApprovalSelections')
)
    THROW 51243,'Missing unique approval selection index.',1;

IF OBJECT_ID(N'dbo.usp_ReconcileEmployeeUsers',N'P') IS NULL
    THROW 51244,'Missing employee -> user reconciliation procedure.',1;
IF OBJECT_ID(N'dbo.usp_ReconcileEmployeeApprovers',N'P') IS NULL
    THROW 51245,'Missing employee -> approver reconciliation procedure.',1;
IF OBJECT_ID(N'dbo.usp_ReconcileHrmSecurity',N'P') IS NULL
    THROW 51246,'Missing combined HRM security reconciliation procedure.',1;

/* ============================================================
   WORK CALENDAR / ANNUAL LEAVE
   ============================================================ */
IF OBJECT_ID(N'dbo.F03WorkYear',N'U') IS NULL
    THROW 50991,'Missing canonical table dbo.F03WorkYear.',1;
IF OBJECT_ID(N'dbo.F03CompanyHoliday',N'U') IS NULL
    THROW 50992,'Missing canonical table dbo.F03CompanyHoliday.',1;
IF COL_LENGTH(N'dbo.F03CompanyHoliday',N'TinhPhep') IS NULL
    THROW 50993,'Missing dbo.F03CompanyHoliday.TinhPhep.',1;
IF COL_LENGTH(N'dbo.F03LeaveBalances',N'BaseLeaveDays') IS NULL
    THROW 50994,'Missing dbo.F03LeaveBalances.BaseLeaveDays.',1;
IF COL_LENGTH(N'dbo.F03LeaveBalances',N'SeniorityLeaveDays') IS NULL
    THROW 50995,'Missing dbo.F03LeaveBalances.SeniorityLeaveDays.',1;
IF COL_LENGTH(N'dbo.F03LeaveBalances',N'YearsOfService') IS NULL
    THROW 50996,'Missing dbo.F03LeaveBalances.YearsOfService.',1;
IF COL_LENGTH(N'dbo.F03LeaveBalances',N'CalculatedAt') IS NULL
    THROW 50997,'Missing dbo.F03LeaveBalances.CalculatedAt.',1;

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
    ActiveEmployeeShiftScheduleCount = (SELECT COUNT(*) FROM dbo.F03EmployeeShiftSchedules WHERE IsActive=1),
    ActiveApprovalPolicyCount = (SELECT COUNT(*) FROM dbo.F03ApprovalPolicies WHERE IsActive=1),
    ActiveApproverCount = (SELECT COUNT(*) FROM dbo.F03Approvers WHERE IsActive=1);

PRINT N'12_Verify: PASS';
GO
