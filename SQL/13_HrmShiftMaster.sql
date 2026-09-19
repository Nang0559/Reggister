USE [FVN_REGISTER];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/*
===============================================================================
HRM SHIFT MASTER / ATTENDANCE CONTRACT
===============================================================================
13_HrmShiftMaster.sql is the schema/contract gate for the HRM shift layer.

IMPORTANT:
  - Canonical table definitions live in 03_Tables.sql.
  - Canonical stored procedures live in 09_StoredProcedures.sql.
  - This file MUST NOT redefine those procedures with a second, incompatible
    schema. The previous version did exactly that and mixed two generations:
      DayNumber vs DayNo
      CountOTTC vs OTRateBase/OTRateTC
      SplitByMonth vs IsMonthly
      SearchType vs ScheduleType
    which caused deployment-time compile errors.

HRM remains READ ONLY from FVN_REGISTER.
===============================================================================
*/

/* ---------------------------------------------------------------------------
   1. Compatibility repair for databases created by the obsolete 13 script.
   The canonical column is DayNo (defined by 03_Tables.sql).
--------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.F03ShiftScheduleDays', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03ShiftScheduleDays', N'DayNo') IS NULL
       AND COL_LENGTH(N'dbo.F03ShiftScheduleDays', N'DayNumber') IS NOT NULL
    BEGIN
        EXEC sys.sp_rename
            N'dbo.F03ShiftScheduleDays.DayNumber',
            N'DayNo',
            N'COLUMN';
    END;
END;
GO

/* ---------------------------------------------------------------------------
   2. Canonical uniqueness/performance indexes.
--------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.F03Shifts', N'U') IS NOT NULL
   AND NOT EXISTS
   (
       SELECT 1
       FROM sys.indexes
       WHERE name=N'UX_F03Shifts_ShiftCode'
         AND object_id=OBJECT_ID(N'dbo.F03Shifts')
   )
BEGIN
    CREATE UNIQUE INDEX UX_F03Shifts_ShiftCode
        ON dbo.F03Shifts(ShiftCode);
END;
GO

IF OBJECT_ID(N'dbo.F03ShiftSchedules', N'U') IS NOT NULL
   AND NOT EXISTS
   (
       SELECT 1
       FROM sys.indexes
       WHERE name=N'UX_F03ShiftSchedules_ScheduleCode'
         AND object_id=OBJECT_ID(N'dbo.F03ShiftSchedules')
   )
BEGIN
    CREATE UNIQUE INDEX UX_F03ShiftSchedules_ScheduleCode
        ON dbo.F03ShiftSchedules(ScheduleCode);
END;
GO

IF OBJECT_ID(N'dbo.F03ShiftScheduleDays', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.F03ShiftScheduleDays', N'DayNo') IS NOT NULL
   AND NOT EXISTS
   (
       SELECT 1
       FROM sys.indexes
       WHERE name=N'UX_F03ShiftScheduleDays_Key'
         AND object_id=OBJECT_ID(N'dbo.F03ShiftScheduleDays')
   )
BEGIN
    CREATE UNIQUE INDEX UX_F03ShiftScheduleDays_Key
        ON dbo.F03ShiftScheduleDays(ScheduleCode,DayNo,ShiftCode);
END;
GO

IF OBJECT_ID(N'dbo.F03EmployeeShiftSchedules', N'U') IS NOT NULL
   AND NOT EXISTS
   (
       SELECT 1
       FROM sys.indexes
       WHERE name=N'UX_F03EmployeeShiftSchedules_Employee'
         AND object_id=OBJECT_ID(N'dbo.F03EmployeeShiftSchedules')
   )
BEGIN
    CREATE UNIQUE INDEX UX_F03EmployeeShiftSchedules_Employee
        ON dbo.F03EmployeeShiftSchedules(EmployeeCode);
END;
GO

IF OBJECT_ID(N'dbo.F03HrmShiftReference', N'U') IS NOT NULL
   AND NOT EXISTS
   (
       SELECT 1
       FROM sys.indexes
       WHERE name=N'UX_F03HrmShiftReference_DateEmployee'
         AND object_id=OBJECT_ID(N'dbo.F03HrmShiftReference')
   )
BEGIN
    CREATE UNIQUE INDEX UX_F03HrmShiftReference_DateEmployee
        ON dbo.F03HrmShiftReference(WorkDate,EmployeeCode);
END;
GO

/* ---------------------------------------------------------------------------
   3. Contract verification.
--------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.F03Shifts', N'U') IS NULL
    THROW 51310, 'F03Shifts is missing. Run 03_Tables.sql first.', 1;

IF OBJECT_ID(N'dbo.F03ShiftSchedules', N'U') IS NULL
    THROW 51311, 'F03ShiftSchedules is missing. Run 03_Tables.sql first.', 1;

IF OBJECT_ID(N'dbo.F03ShiftScheduleDays', N'U') IS NULL
    THROW 51312, 'F03ShiftScheduleDays is missing. Run 03_Tables.sql first.', 1;

IF COL_LENGTH(N'dbo.F03ShiftScheduleDays', N'DayNo') IS NULL
    THROW 51313, 'F03ShiftScheduleDays.DayNo is missing. Canonical column is DayNo.', 1;

IF COL_LENGTH(N'dbo.F03Shifts', N'CountOTTC') IS NOT NULL
    PRINT N'NOTE: legacy F03Shifts.CountOTTC exists; it is not used by the canonical FVN_REGISTER model.';
GO

/* ---------------------------------------------------------------------------
   4. Procedure ownership.
   09_StoredProcedures.sql owns:
      dbo.usp_SyncHrmShiftMaster
      dbo.usp_SyncAttendanceStaging
   Do not redefine them here.
--------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.usp_SyncHrmShiftMaster', N'P') IS NULL
    THROW 51314, 'usp_SyncHrmShiftMaster is missing. Run 09_StoredProcedures.sql first.', 1;

IF OBJECT_ID(N'dbo.usp_SyncAttendanceStaging', N'P') IS NULL
    THROW 51315, 'usp_SyncAttendanceStaging is missing. Run 09_StoredProcedures.sql first.', 1;
GO

PRINT N'13_HrmShiftMaster: canonical HRM shift schema/contract verified.';
GO
