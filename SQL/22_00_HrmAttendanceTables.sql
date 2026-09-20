USE [FVN_REGISTER];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/*
===============================================================================
FVN HRM-COMPATIBLE ATTENDANCE - LOCAL RESULT TABLES

Architecture:
  - HRM database is READ ONLY from FVN.
  - Calculation reads HRM.dbo.* source/master/attendance tables.
  - No HRM.dbo.tblBaoCao / tblBaoCaoK row is created or modified by FVN.
  - These tables are the FVN-owned calculation/result store.

The calculation engine is deployed separately:
  22_01  InterSectionTime3
  22_02  HRM-compatible per-staff calculation
  22_03  range orchestration + FVN result persistence
===============================================================================
*/

IF OBJECT_ID(N'dbo.F03HrmAttendanceCalculated',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03HrmAttendanceCalculated
    (
        Id bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03HrmAttendanceCalculated PRIMARY KEY,
        CalculationBatchId uniqueidentifier NOT NULL,
        CalculationVersion nvarchar(50) NOT NULL,
        WorkDate date NOT NULL,

        HrmEmployeeId int NOT NULL,
        EmployeeCode nvarchar(50) NULL,
        FullName nvarchar(200) NULL,
        HrmDeptId int NULL,
        DeptCode nvarchar(20) NULL,
        HrmPositionId int NULL,
        ShiftId int NULL,
        ShiftAbbr nvarchar(10) NULL,

        CheckInGate int NULL,
        CheckInTime datetime2(0) NULL,
        CheckOutGate int NULL,
        CheckOutTime datetime2(0) NULL,
        ExitGate int NULL,
        ExitTime datetime2(0) NULL,
        EntryGate int NULL,
        EntryTime datetime2(0) NULL,

        WorkMinutesDay int NOT NULL DEFAULT 0,
        WorkMinutesNight int NOT NULL DEFAULT 0,
        OTMinutesDay int NOT NULL DEFAULT 0,
        OTMinutesNight int NOT NULL DEFAULT 0,
        OTMinutesDayTC int NOT NULL DEFAULT 0,
        OTMinutesNightTC int NOT NULL DEFAULT 0,
        OTRecognizedMinutesDay int NOT NULL DEFAULT 0,
        OTRecognizedMinutesNight int NOT NULL DEFAULT 0,
        LateMinutesDay int NOT NULL DEFAULT 0,
        LateMinutesNight int NOT NULL DEFAULT 0,
        EarlyLeaveMinutesDay int NOT NULL DEFAULT 0,
        EarlyLeaveMinutesNight int NOT NULL DEFAULT 0,
        RequiredMinutes int NOT NULL DEFAULT 0,

        LeaveTotal decimal(9,2) NULL,
        LeaveAnnual decimal(9,2) NULL,
        Leave100 decimal(9,2) NULL,
        Leave70 decimal(9,2) NULL,
        LeaveUnpaid decimal(9,2) NULL,
        LeaveBH100 decimal(9,2) NULL,
        LeaveBH70 decimal(9,2) NULL,
        LeaveBusinessTrip decimal(9,2) NULL,
        LeaveCompensatory decimal(9,2) NULL,
        LeaveOther decimal(9,2) NULL,
        LeaveTypeCode nvarchar(20) NULL,
        LeaveReason nvarchar(50) NULL,
        Note nvarchar(500) NULL,

        HrmType bit NULL,
        HrmHoliday bit NULL,
        HrmEmployeeHoliday bit NULL,
        IsLocked bit NULL,
        HrmBCGhiChu nvarchar(50) NULL,
        HrmBCLyDoNghi nvarchar(20) NULL,
        HrmBCNghiTotal decimal(9,2) NULL,
        HrmBCNghiPhep decimal(9,2) NULL,
        HrmBCNghiH100 decimal(9,2) NULL,
        HrmBCNghiH70 decimal(9,2) NULL,
        HrmBCNghiKL decimal(9,2) NULL,
        HrmBCNghiBH100 decimal(9,2) NULL,
        HrmBCNghiBH70 decimal(9,2) NULL,
        HrmBCNghiCongTac decimal(9,2) NULL,
        HrmBCNghiBu decimal(9,2) NULL,
        HrmBCNghiKhac decimal(9,2) NULL,
        HrmBCDaXacNhanLamThem bit NULL,
        HrmBCLoaiLamThem bit NULL,
        HrmBCTinhLamThem bit NULL,
        HrmBCNgayLe int NULL,
        HrmBCNgayLeNV int NULL,
        HrmShiftDayType smallint NULL,

        AttendanceDisplayValue nvarchar(50) NULL,
        OtDisplayValue nvarchar(50) NULL,
        CalculatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03HrmAttendanceCalculated_CalculatedAt DEFAULT GETDATE(),
        CalculatedBy nvarchar(100) NULL,
        SourceSystem nvarchar(20) NOT NULL CONSTRAINT DF_F03HrmAttendanceCalculated_SourceSystem DEFAULT N'HRM',

        CONSTRAINT UQ_F03HrmAttendanceCalculated_Batch
            UNIQUE(CalculationBatchId,HrmEmployeeId,WorkDate)
    );
END;
GO

IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCGhiChu') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCGhiChu nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCLyDoNghi') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCLyDoNghi nvarchar(20) NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCNghiTotal') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCNghiTotal decimal(9,2) NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCNghiPhep') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCNghiPhep decimal(9,2) NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCNghiH100') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCNghiH100 decimal(9,2) NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCNghiH70') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCNghiH70 decimal(9,2) NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCNghiKL') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCNghiKL decimal(9,2) NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCNghiBH100') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCNghiBH100 decimal(9,2) NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCNghiBH70') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCNghiBH70 decimal(9,2) NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCNghiCongTac') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCNghiCongTac decimal(9,2) NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCNghiBu') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCNghiBu decimal(9,2) NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCNghiKhac') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCNghiKhac decimal(9,2) NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCDaXacNhanLamThem') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCDaXacNhanLamThem bit NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCLoaiLamThem') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCLoaiLamThem bit NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCTinhLamThem') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCTinhLamThem bit NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCNgayLe') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCNgayLe int NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmBCNgayLeNV') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmBCNgayLeNV int NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'HrmShiftDayType') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD HrmShiftDayType smallint NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'AttendanceDisplayValue') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD AttendanceDisplayValue nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.F03HrmAttendanceCalculated',N'OtDisplayValue') IS NULL ALTER TABLE dbo.F03HrmAttendanceCalculated ADD OtDisplayValue nvarchar(50) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.F03HrmAttendanceCalculated') AND name=N'IX_F03HrmAttendanceCalculated_DateDept')
    CREATE INDEX IX_F03HrmAttendanceCalculated_DateDept
        ON dbo.F03HrmAttendanceCalculated(WorkDate,DeptCode,HrmEmployeeId,Id);
GO

IF OBJECT_ID(N'dbo.F03HrmOTActual',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03HrmOTActual
    (
        Id bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03HrmOTActual PRIMARY KEY,
        CalculationBatchId uniqueidentifier NOT NULL,
        WorkDate date NOT NULL,
        HrmEmployeeId int NOT NULL,
        EmployeeCode nvarchar(50) NULL,
        DeptCode nvarchar(20) NULL,
        ActualStartTime datetime2(0) NULL,
        ActualEndTime datetime2(0) NULL,
        ActualMinutes int NOT NULL DEFAULT 0,
        ActualOTDayMinutes int NOT NULL DEFAULT 0,
        ActualOTNightMinutes int NOT NULL DEFAULT 0,
        RecognizedOTMinutes int NOT NULL DEFAULT 0,
        SourceAttendanceId bigint NULL,
        CalculatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),

        CONSTRAINT UQ_F03HrmOTActual_Batch
            UNIQUE(CalculationBatchId,HrmEmployeeId,WorkDate)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.F03HrmOTActual') AND name=N'IX_F03HrmOTActual_DateDept')
    CREATE INDEX IX_F03HrmOTActual_DateDept
        ON dbo.F03HrmOTActual(WorkDate,DeptCode,HrmEmployeeId,Id);
GO


/*
===============================================================================
CURRENT-STATE STORAGE HARDENING

F03HrmAttendanceCalculated / F03HrmOTActual are result tables, not an
append-only calculation log. A calculation batch identifies the latest
calculation that produced a row, while the physical result store keeps only
one row per employee/date. This prevents startup catch-up + daily reruns +
manual recalculation from multiplying the attendance volume.

Historical/audit information is represented by CalculationBatchId on the
current row. The calculation engine replaces the affected date/scope
atomically; see 22_03.
===============================================================================
*/

IF EXISTS
(
    SELECT 1
    FROM sys.key_constraints
    WHERE parent_object_id = OBJECT_ID(N'dbo.F03HrmAttendanceCalculated')
      AND name = N'UQ_F03HrmAttendanceCalculated_Batch'
)
BEGIN
    ALTER TABLE dbo.F03HrmAttendanceCalculated
        DROP CONSTRAINT UQ_F03HrmAttendanceCalculated_Batch;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.F03HrmAttendanceCalculated')
      AND name = N'UQ_F03HrmAttendanceCalculated_Current'
)
BEGIN
    ;WITH Duplicates AS
    (
        SELECT Id,
               ROW_NUMBER() OVER
               (
                   PARTITION BY HrmEmployeeId, WorkDate
                   ORDER BY CalculatedAt DESC, Id DESC
               ) AS rn
        FROM dbo.F03HrmAttendanceCalculated
    )
    DELETE FROM Duplicates WHERE rn > 1;

    CREATE UNIQUE INDEX UQ_F03HrmAttendanceCalculated_Current
        ON dbo.F03HrmAttendanceCalculated(HrmEmployeeId, WorkDate);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.F03HrmAttendanceCalculated')
      AND name = N'IX_F03HrmAttendanceCalculated_Batch'
)
    CREATE INDEX IX_F03HrmAttendanceCalculated_Batch
        ON dbo.F03HrmAttendanceCalculated(CalculationBatchId, WorkDate, HrmEmployeeId, Id);
GO

IF EXISTS
(
    SELECT 1
    FROM sys.key_constraints
    WHERE parent_object_id = OBJECT_ID(N'dbo.F03HrmOTActual')
      AND name = N'UQ_F03HrmOTActual_Batch'
)
BEGIN
    ALTER TABLE dbo.F03HrmOTActual
        DROP CONSTRAINT UQ_F03HrmOTActual_Batch;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.F03HrmOTActual')
      AND name = N'UQ_F03HrmOTActual_Current'
)
BEGIN
    ;WITH Duplicates AS
    (
        SELECT Id,
               ROW_NUMBER() OVER
               (
                   PARTITION BY HrmEmployeeId, WorkDate
                   ORDER BY CalculatedAt DESC, Id DESC
               ) AS rn
        FROM dbo.F03HrmOTActual
    )
    DELETE FROM Duplicates WHERE rn > 1;

    CREATE UNIQUE INDEX UQ_F03HrmOTActual_Current
        ON dbo.F03HrmOTActual(HrmEmployeeId, WorkDate);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.F03HrmOTActual')
      AND name = N'IX_F03HrmOTActual_Batch'
)
    CREATE INDEX IX_F03HrmOTActual_Batch
        ON dbo.F03HrmOTActual(CalculationBatchId, WorkDate, HrmEmployeeId, Id);
GO
