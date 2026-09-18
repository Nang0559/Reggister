/*
FVN_REGISTER - SQL 04 Constraints
Idempotent integrity rules. No cross-database dependency is created here.
*/
USE [FVN_REGISTER];
GO
SET NOCOUNT ON;
GO

/* Canonical business keys. If legacy duplicates exist, stop instead of silently changing data. */
IF EXISTS (SELECT EmployeeCode FROM dbo.F03Employees GROUP BY EmployeeCode HAVING COUNT(*) > 1)
    THROW 52001, 'Duplicate F03Employees.EmployeeCode exists; resolve before creating unique key.', 1;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.F03Employees') AND name=N'UX_F03Employees_EmployeeCode')
    CREATE UNIQUE INDEX UX_F03Employees_EmployeeCode ON dbo.F03Employees(EmployeeCode);
GO

IF EXISTS (SELECT DeptCode FROM dbo.F03Departments GROUP BY DeptCode HAVING COUNT(*) > 1)
    THROW 52002, 'Duplicate F03Departments.DeptCode exists.', 1;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.F03Departments') AND name=N'UX_F03Departments_DeptCode')
    CREATE UNIQUE INDEX UX_F03Departments_DeptCode ON dbo.F03Departments(DeptCode);
GO

IF EXISTS (SELECT PositionCode FROM dbo.F03Positions GROUP BY PositionCode HAVING COUNT(*) > 1)
    THROW 52003, 'Duplicate F03Positions.PositionCode exists.', 1;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.F03Positions') AND name=N'UX_F03Positions_PositionCode')
    CREATE UNIQUE INDEX UX_F03Positions_PositionCode ON dbo.F03Positions(PositionCode);
GO

IF EXISTS (SELECT LeaveTypeCode FROM dbo.F03LeaveType GROUP BY LeaveTypeCode HAVING COUNT(*) > 1)
    THROW 52004, 'Duplicate F03LeaveType.LeaveTypeCode exists.', 1;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.F03LeaveType') AND name=N'UX_F03LeaveType_Code')
    CREATE UNIQUE INDEX UX_F03LeaveType_Code ON dbo.F03LeaveType(LeaveTypeCode);
GO

IF EXISTS (SELECT PermissionCode FROM dbo.F03Permissions GROUP BY PermissionCode HAVING COUNT(*) > 1)
    THROW 52005, 'Duplicate F03Permissions.PermissionCode exists.', 1;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.F03Permissions') AND name=N'UX_F03Permissions_Code')
    CREATE UNIQUE INDEX UX_F03Permissions_Code ON dbo.F03Permissions(PermissionCode);
GO

IF EXISTS (SELECT EmployeeCode FROM dbo.F03Users GROUP BY EmployeeCode HAVING COUNT(*) > 1)
    THROW 52006, 'Duplicate F03Users.EmployeeCode exists.', 1;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.F03Users') AND name=N'UX_F03Users_EmployeeCode')
    CREATE UNIQUE INDEX UX_F03Users_EmployeeCode ON dbo.F03Users(EmployeeCode);
GO

/* Role resolution: exact mapping must be deterministic. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.F03HrmUserRoleRules') AND name=N'IX_F03HrmUserRoleRules_Resolve')
    CREATE INDEX IX_F03HrmUserRoleRules_Resolve
        ON dbo.F03HrmUserRoleRules(IsActive, DeptCode, PositionCode, Priority, Id)
        INCLUDE(PermissionCode);
GO

/* Email registration idempotency lookup. */
IF OBJECT_ID(N'dbo.F03EmailQueues',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.F03EmailQueues') AND name=N'IX_F03EmailQueues_HrmRegistration')
    CREATE INDEX IX_F03EmailQueues_HrmRegistration
        ON dbo.F03EmailQueues(ToEmail,TemplateCode,Status,CreatedAt);
GO

PRINT N'04_Constraints: OK';
GO


/* HRM shift master business keys. */
IF OBJECT_ID(N'dbo.F03Shifts',N'U') IS NOT NULL
AND EXISTS (SELECT 1 FROM dbo.F03Shifts GROUP BY ShiftCode HAVING COUNT(*)>1)
    THROW 52007, 'Duplicate F03Shifts.ShiftCode exists.', 1;
IF OBJECT_ID(N'dbo.F03Shifts',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.F03Shifts') AND name=N'UX_F03Shifts_ShiftCode')
    CREATE UNIQUE INDEX UX_F03Shifts_ShiftCode ON dbo.F03Shifts(ShiftCode);

IF OBJECT_ID(N'dbo.F03ShiftSchedules',N'U') IS NOT NULL
AND EXISTS (SELECT 1 FROM dbo.F03ShiftSchedules GROUP BY ScheduleCode HAVING COUNT(*)>1)
    THROW 52008, 'Duplicate F03ShiftSchedules.ScheduleCode exists.', 1;
IF OBJECT_ID(N'dbo.F03ShiftSchedules',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.F03ShiftSchedules') AND name=N'UX_F03ShiftSchedules_Code')
    CREATE UNIQUE INDEX UX_F03ShiftSchedules_Code ON dbo.F03ShiftSchedules(ScheduleCode);

IF OBJECT_ID(N'dbo.F03ShiftScheduleDays',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.F03ShiftScheduleDays') AND name=N'UX_F03ShiftScheduleDays')
    CREATE UNIQUE INDEX UX_F03ShiftScheduleDays ON dbo.F03ShiftScheduleDays(ScheduleCode,DayNo,ShiftCode);

IF OBJECT_ID(N'dbo.F03EmployeeShiftSchedules',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.F03EmployeeShiftSchedules') AND name=N'UX_F03EmployeeShiftSchedules_Employee')
    CREATE UNIQUE INDEX UX_F03EmployeeShiftSchedules_Employee ON dbo.F03EmployeeShiftSchedules(EmployeeCode);
GO
