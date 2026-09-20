USE [FVN_REGISTER];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/*
===============================================================================
HRM ATTENDANCE HISTORY / ARCHIVE
===============================================================================
CURRENT:
  F03HrmAttendanceCalculated / F03HrmOTActual = mutable current-state only.

HISTORY:
  F03HrmAttendanceHistory / F03HrmOTActualHistory = retained business result
  history. Recalculation after archive creates a new RevisionNo instead of
  silently overwriting the historical result.

AUDIT:
  F03HrmAttendanceCalculationRun = one lightweight row per calculation run.

History is monthly-partitioned by WorkDate and PAGE-compressed. All partitions
use PRIMARY for deployment compatibility; partitioning is used for pruning and
maintenance rather than requiring new physical filegroups.
===============================================================================
*/

IF NOT EXISTS (SELECT 1 FROM sys.partition_functions WHERE name=N'PF_F03HrmAttendanceHistory_Month')
BEGIN
    CREATE PARTITION FUNCTION PF_F03HrmAttendanceHistory_Month (date)
    AS RANGE RIGHT FOR VALUES (
        '2024-01-01',
        '2024-02-01',
        '2024-03-01',
        '2024-04-01',
        '2024-05-01',
        '2024-06-01',
        '2024-07-01',
        '2024-08-01',
        '2024-09-01',
        '2024-10-01',
        '2024-11-01',
        '2024-12-01',
        '2025-01-01',
        '2025-02-01',
        '2025-03-01',
        '2025-04-01',
        '2025-05-01',
        '2025-06-01',
        '2025-07-01',
        '2025-08-01',
        '2025-09-01',
        '2025-10-01',
        '2025-11-01',
        '2025-12-01',
        '2026-01-01',
        '2026-02-01',
        '2026-03-01',
        '2026-04-01',
        '2026-05-01',
        '2026-06-01',
        '2026-07-01',
        '2026-08-01',
        '2026-09-01',
        '2026-10-01',
        '2026-11-01',
        '2026-12-01',
        '2027-01-01',
        '2027-02-01',
        '2027-03-01',
        '2027-04-01',
        '2027-05-01',
        '2027-06-01',
        '2027-07-01',
        '2027-08-01',
        '2027-09-01',
        '2027-10-01',
        '2027-11-01',
        '2027-12-01',
        '2028-01-01',
        '2028-02-01',
        '2028-03-01',
        '2028-04-01',
        '2028-05-01',
        '2028-06-01',
        '2028-07-01',
        '2028-08-01',
        '2028-09-01',
        '2028-10-01',
        '2028-11-01',
        '2028-12-01',
        '2029-01-01',
        '2029-02-01',
        '2029-03-01',
        '2029-04-01',
        '2029-05-01',
        '2029-06-01',
        '2029-07-01',
        '2029-08-01',
        '2029-09-01',
        '2029-10-01',
        '2029-11-01',
        '2029-12-01',
        '2030-01-01',
        '2030-02-01',
        '2030-03-01',
        '2030-04-01',
        '2030-05-01',
        '2030-06-01',
        '2030-07-01',
        '2030-08-01',
        '2030-09-01',
        '2030-10-01',
        '2030-11-01',
        '2030-12-01',
        '2031-01-01',
        '2031-02-01',
        '2031-03-01',
        '2031-04-01',
        '2031-05-01',
        '2031-06-01',
        '2031-07-01',
        '2031-08-01',
        '2031-09-01',
        '2031-10-01',
        '2031-11-01',
        '2031-12-01',
        '2032-01-01',
        '2032-02-01',
        '2032-03-01',
        '2032-04-01',
        '2032-05-01',
        '2032-06-01',
        '2032-07-01',
        '2032-08-01',
        '2032-09-01',
        '2032-10-01',
        '2032-11-01',
        '2032-12-01',
        '2033-01-01',
        '2033-02-01',
        '2033-03-01',
        '2033-04-01',
        '2033-05-01',
        '2033-06-01',
        '2033-07-01',
        '2033-08-01',
        '2033-09-01',
        '2033-10-01',
        '2033-11-01',
        '2033-12-01',
        '2034-01-01',
        '2034-02-01',
        '2034-03-01',
        '2034-04-01',
        '2034-05-01',
        '2034-06-01',
        '2034-07-01',
        '2034-08-01',
        '2034-09-01',
        '2034-10-01',
        '2034-11-01',
        '2034-12-01',
        '2035-01-01',
        '2035-02-01',
        '2035-03-01',
        '2035-04-01',
        '2035-05-01',
        '2035-06-01',
        '2035-07-01',
        '2035-08-01',
        '2035-09-01',
        '2035-10-01',
        '2035-11-01',
        '2035-12-01',
        '2036-01-01',
        '2036-02-01',
        '2036-03-01',
        '2036-04-01',
        '2036-05-01',
        '2036-06-01',
        '2036-07-01',
        '2036-08-01',
        '2036-09-01',
        '2036-10-01',
        '2036-11-01',
        '2036-12-01',
        '2037-01-01'
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.partition_schemes WHERE name=N'PS_F03HrmAttendanceHistory_Month')
BEGIN
    CREATE PARTITION SCHEME PS_F03HrmAttendanceHistory_Month
    AS PARTITION PF_F03HrmAttendanceHistory_Month ALL TO ([PRIMARY]);
END;
GO

IF OBJECT_ID(N'dbo.F03HrmAttendanceHistory',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03HrmAttendanceHistory
    (
        HistoryId bigint IDENTITY(1,1) NOT NULL,
        RevisionNo int NOT NULL,
        ArchivedAt datetime2(0) NOT NULL CONSTRAINT DF_F03HrmAttendanceHistory_ArchivedAt DEFAULT GETDATE(),
        ArchiveReason nvarchar(50) NOT NULL CONSTRAINT DF_F03HrmAttendanceHistory_ArchiveReason DEFAULT N'PAYROLL_CLOSE',
        CalculationBatchId uniqueidentifier NOT NULL, CalculationVersion nvarchar(50) NOT NULL, WorkDate date NOT NULL, HrmEmployeeId int NOT NULL, EmployeeCode nvarchar(50) NULL, FullName nvarchar(200) NULL, HrmDeptId int NULL, DeptCode nvarchar(20) NULL, HrmPositionId int NULL, ShiftId int NULL, ShiftAbbr nvarchar(10) NULL, CheckInGate int NULL, CheckInTime datetime2(0) NULL, CheckOutGate int NULL, CheckOutTime datetime2(0) NULL, ExitGate int NULL, ExitTime datetime2(0) NULL, EntryGate int NULL, EntryTime datetime2(0) NULL, WorkMinutesDay int NOT NULL, WorkMinutesNight int NOT NULL, OTMinutesDay int NOT NULL, OTMinutesNight int NOT NULL, OTMinutesDayTC int NOT NULL, OTMinutesNightTC int NOT NULL, OTRecognizedMinutesDay int NOT NULL, OTRecognizedMinutesNight int NOT NULL, LateMinutesDay int NOT NULL, LateMinutesNight int NOT NULL, EarlyLeaveMinutesDay int NOT NULL, EarlyLeaveMinutesNight int NOT NULL, RequiredMinutes int NOT NULL, LeaveTotal decimal(9,2) NULL, LeaveAnnual decimal(9,2) NULL, Leave100 decimal(9,2) NULL, Leave70 decimal(9,2) NULL, LeaveUnpaid decimal(9,2) NULL, LeaveBH100 decimal(9,2) NULL, LeaveBH70 decimal(9,2) NULL, LeaveBusinessTrip decimal(9,2) NULL, LeaveCompensatory decimal(9,2) NULL, LeaveOther decimal(9,2) NULL, LeaveTypeCode nvarchar(20) NULL, LeaveReason nvarchar(50) NULL, Note nvarchar(500) NULL, HrmType bit NULL, HrmHoliday bit NULL, HrmEmployeeHoliday bit NULL, IsLocked bit NULL, HrmBCGhiChu nvarchar(50) NULL, HrmBCLyDoNghi nvarchar(20) NULL, HrmBCNghiTotal decimal(9,2) NULL, HrmBCNghiPhep decimal(9,2) NULL, HrmBCNghiH100 decimal(9,2) NULL, HrmBCNghiH70 decimal(9,2) NULL, HrmBCNghiKL decimal(9,2) NULL, HrmBCNghiBH100 decimal(9,2) NULL, HrmBCNghiBH70 decimal(9,2) NULL, HrmBCNghiCongTac decimal(9,2) NULL, HrmBCNghiBu decimal(9,2) NULL, HrmBCNghiKhac decimal(9,2) NULL, HrmBCDaXacNhanLamThem bit NULL, HrmBCLoaiLamThem bit NULL, HrmBCTinhLamThem bit NULL, HrmBCNgayLe int NULL, HrmBCNgayLeNV int NULL, HrmShiftDayType smallint NULL, AttendanceDisplayValue nvarchar(50) NULL, OtDisplayValue nvarchar(50) NULL, CalculatedAt datetime2(0) NOT NULL, CalculatedBy nvarchar(100) NULL, SourceSystem nvarchar(20) NOT NULL,
        CONSTRAINT PK_F03HrmAttendanceHistory PRIMARY KEY CLUSTERED (WorkDate,HistoryId)
            WITH (DATA_COMPRESSION=PAGE)
            ON PS_F03HrmAttendanceHistory_Month(WorkDate)
    ) ON PS_F03HrmAttendanceHistory_Month(WorkDate);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.F03HrmAttendanceHistory') AND name=N'UX_F03HrmAttendanceHistory_EmployeeDateRevision')
BEGIN
    CREATE UNIQUE INDEX UX_F03HrmAttendanceHistory_EmployeeDateRevision
        ON dbo.F03HrmAttendanceHistory(WorkDate,HrmEmployeeId,RevisionNo)
        WITH (DATA_COMPRESSION=PAGE)
        ON PS_F03HrmAttendanceHistory_Month(WorkDate);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.F03HrmAttendanceHistory') AND name=N'IX_F03HrmAttendanceHistory_EmployeeDate')
BEGIN
    CREATE INDEX IX_F03HrmAttendanceHistory_EmployeeDate
        ON dbo.F03HrmAttendanceHistory(HrmEmployeeId,WorkDate)
        INCLUDE (EmployeeCode,DeptCode,AttendanceDisplayValue,OtDisplayValue,CalculatedAt)
        WITH (DATA_COMPRESSION=PAGE)
        ON PS_F03HrmAttendanceHistory_Month(WorkDate);
END;
GO

IF OBJECT_ID(N'dbo.F03HrmOTActualHistory',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03HrmOTActualHistory
    (
        HistoryId bigint IDENTITY(1,1) NOT NULL,
        RevisionNo int NOT NULL,
        ArchivedAt datetime2(0) NOT NULL CONSTRAINT DF_F03HrmOTActualHistory_ArchivedAt DEFAULT GETDATE(),
        ArchiveReason nvarchar(50) NOT NULL CONSTRAINT DF_F03HrmOTActualHistory_ArchiveReason DEFAULT N'PAYROLL_CLOSE',
        CalculationBatchId uniqueidentifier NOT NULL, WorkDate date NOT NULL, HrmEmployeeId int NOT NULL, EmployeeCode nvarchar(50) NULL, DeptCode nvarchar(20) NULL, ActualStartTime datetime2(0) NULL, ActualEndTime datetime2(0) NULL, ActualMinutes int NOT NULL, ActualOTDayMinutes int NOT NULL, ActualOTNightMinutes int NOT NULL, RecognizedOTMinutes int NOT NULL, SourceAttendanceId bigint NULL, CalculatedAt datetime2(0) NOT NULL,
        CONSTRAINT PK_F03HrmOTActualHistory PRIMARY KEY CLUSTERED (WorkDate,HistoryId)
            WITH (DATA_COMPRESSION=PAGE)
            ON PS_F03HrmAttendanceHistory_Month(WorkDate)
    ) ON PS_F03HrmAttendanceHistory_Month(WorkDate);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.F03HrmOTActualHistory') AND name=N'UX_F03HrmOTActualHistory_EmployeeDateRevision')
BEGIN
    CREATE UNIQUE INDEX UX_F03HrmOTActualHistory_EmployeeDateRevision
        ON dbo.F03HrmOTActualHistory(WorkDate,HrmEmployeeId,RevisionNo)
        WITH (DATA_COMPRESSION=PAGE)
        ON PS_F03HrmAttendanceHistory_Month(WorkDate);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.F03HrmOTActualHistory') AND name=N'IX_F03HrmOTActualHistory_EmployeeDate')
BEGIN
    CREATE INDEX IX_F03HrmOTActualHistory_EmployeeDate
        ON dbo.F03HrmOTActualHistory(HrmEmployeeId,WorkDate)
        INCLUDE (EmployeeCode,DeptCode,ActualMinutes,RecognizedOTMinutes,CalculatedAt)
        WITH (DATA_COMPRESSION=PAGE)
        ON PS_F03HrmAttendanceHistory_Month(WorkDate);
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_ArchiveHrmAttendancePeriod
    @FromDate date,
    @ToDate date,
    @ArchiveReason nvarchar(50)=N'PAYROLL_CLOSE'
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @FromDate IS NULL OR @ToDate IS NULL OR @FromDate>@ToDate
        THROW 51340,N'Khoảng archive không hợp lệ.',1;

    DECLARE @Today date=CAST(GETDATE() AS date);
    DECLARE @LastClosedDate date =
        CASE
            WHEN DAY(@Today) >= 21
                THEN DATEFROMPARTS(YEAR(@Today),MONTH(@Today),20)
            ELSE DATEADD(day,-1,DATEFROMPARTS(YEAR(@Today),MONTH(@Today),21))
        END;

    IF @ToDate>@LastClosedDate
        THROW 51341,N'Không được archive kỳ công hiện tại/chưa đóng.',1;

    BEGIN TRANSACTION;

    INSERT dbo.F03HrmAttendanceHistory
    (RevisionNo,ArchiveReason,CalculationBatchId,CalculationVersion,WorkDate,HrmEmployeeId,EmployeeCode,FullName,HrmDeptId,DeptCode,HrmPositionId,ShiftId,ShiftAbbr,CheckInGate,CheckInTime,CheckOutGate,CheckOutTime,ExitGate,ExitTime,EntryGate,EntryTime,WorkMinutesDay,WorkMinutesNight,OTMinutesDay,OTMinutesNight,OTMinutesDayTC,OTMinutesNightTC,OTRecognizedMinutesDay,OTRecognizedMinutesNight,LateMinutesDay,LateMinutesNight,EarlyLeaveMinutesDay,EarlyLeaveMinutesNight,RequiredMinutes,LeaveTotal,LeaveAnnual,Leave100,Leave70,LeaveUnpaid,LeaveBH100,LeaveBH70,LeaveBusinessTrip,LeaveCompensatory,LeaveOther,LeaveTypeCode,LeaveReason,Note,HrmType,HrmHoliday,HrmEmployeeHoliday,IsLocked,HrmBCGhiChu,HrmBCLyDoNghi,HrmBCNghiTotal,HrmBCNghiPhep,HrmBCNghiH100,HrmBCNghiH70,HrmBCNghiKL,HrmBCNghiBH100,HrmBCNghiBH70,HrmBCNghiCongTac,HrmBCNghiBu,HrmBCNghiKhac,HrmBCDaXacNhanLamThem,HrmBCLoaiLamThem,HrmBCTinhLamThem,HrmBCNgayLe,HrmBCNgayLeNV,HrmShiftDayType,AttendanceDisplayValue,OtDisplayValue,CalculatedAt,CalculatedBy,SourceSystem)
    SELECT ISNULL(r.MaxRevisionNo,0)+1,@ArchiveReason,a.CalculationBatchId,a.CalculationVersion,a.WorkDate,a.HrmEmployeeId,a.EmployeeCode,a.FullName,a.HrmDeptId,a.DeptCode,a.HrmPositionId,a.ShiftId,a.ShiftAbbr,a.CheckInGate,a.CheckInTime,a.CheckOutGate,a.CheckOutTime,a.ExitGate,a.ExitTime,a.EntryGate,a.EntryTime,a.WorkMinutesDay,a.WorkMinutesNight,a.OTMinutesDay,a.OTMinutesNight,a.OTMinutesDayTC,a.OTMinutesNightTC,a.OTRecognizedMinutesDay,a.OTRecognizedMinutesNight,a.LateMinutesDay,a.LateMinutesNight,a.EarlyLeaveMinutesDay,a.EarlyLeaveMinutesNight,a.RequiredMinutes,a.LeaveTotal,a.LeaveAnnual,a.Leave100,a.Leave70,a.LeaveUnpaid,a.LeaveBH100,a.LeaveBH70,a.LeaveBusinessTrip,a.LeaveCompensatory,a.LeaveOther,a.LeaveTypeCode,a.LeaveReason,a.Note,a.HrmType,a.HrmHoliday,a.HrmEmployeeHoliday,a.IsLocked,a.HrmBCGhiChu,a.HrmBCLyDoNghi,a.HrmBCNghiTotal,a.HrmBCNghiPhep,a.HrmBCNghiH100,a.HrmBCNghiH70,a.HrmBCNghiKL,a.HrmBCNghiBH100,a.HrmBCNghiBH70,a.HrmBCNghiCongTac,a.HrmBCNghiBu,a.HrmBCNghiKhac,a.HrmBCDaXacNhanLamThem,a.HrmBCLoaiLamThem,a.HrmBCTinhLamThem,a.HrmBCNgayLe,a.HrmBCNgayLeNV,a.HrmShiftDayType,a.AttendanceDisplayValue,a.OtDisplayValue,a.CalculatedAt,a.CalculatedBy,a.SourceSystem
    FROM dbo.F03HrmAttendanceCalculated a
    OUTER APPLY
    (
        SELECT MAX(h.RevisionNo) AS MaxRevisionNo
        FROM dbo.F03HrmAttendanceHistory h WITH (UPDLOCK,HOLDLOCK)
        WHERE h.HrmEmployeeId=a.HrmEmployeeId AND h.WorkDate=a.WorkDate
    ) r
    WHERE a.WorkDate BETWEEN @FromDate AND @ToDate
      AND NOT EXISTS
      (
          SELECT 1 FROM dbo.F03HrmAttendanceHistory h
          WHERE h.HrmEmployeeId=a.HrmEmployeeId
            AND h.WorkDate=a.WorkDate
            AND h.CalculationBatchId=a.CalculationBatchId
      );

    INSERT dbo.F03HrmOTActualHistory
    (RevisionNo,ArchiveReason,CalculationBatchId,WorkDate,HrmEmployeeId,EmployeeCode,DeptCode,ActualStartTime,ActualEndTime,ActualMinutes,ActualOTDayMinutes,ActualOTNightMinutes,RecognizedOTMinutes,SourceAttendanceId,CalculatedAt)
    SELECT ISNULL(r.MaxRevisionNo,0)+1,@ArchiveReason,a.CalculationBatchId,a.WorkDate,a.HrmEmployeeId,a.EmployeeCode,a.DeptCode,a.ActualStartTime,a.ActualEndTime,a.ActualMinutes,a.ActualOTDayMinutes,a.ActualOTNightMinutes,a.RecognizedOTMinutes,a.SourceAttendanceId,a.CalculatedAt
    FROM dbo.F03HrmOTActual a
    OUTER APPLY
    (
        SELECT MAX(h.RevisionNo) AS MaxRevisionNo
        FROM dbo.F03HrmOTActualHistory h WITH (UPDLOCK,HOLDLOCK)
        WHERE h.HrmEmployeeId=a.HrmEmployeeId AND h.WorkDate=a.WorkDate
    ) r
    WHERE a.WorkDate BETWEEN @FromDate AND @ToDate
      AND NOT EXISTS
      (
          SELECT 1 FROM dbo.F03HrmOTActualHistory h
          WHERE h.HrmEmployeeId=a.HrmEmployeeId
            AND h.WorkDate=a.WorkDate
            AND h.CalculationBatchId=a.CalculationBatchId
      );

    DELETE FROM dbo.F03HrmOTActual
    WHERE WorkDate BETWEEN @FromDate AND @ToDate;

    DELETE FROM dbo.F03HrmAttendanceCalculated
    WHERE WorkDate BETWEEN @FromDate AND @ToDate;

    COMMIT TRANSACTION;

    SELECT @FromDate AS FromDate,@ToDate AS ToDate,
           (SELECT COUNT(*) FROM dbo.F03HrmAttendanceHistory WHERE WorkDate BETWEEN @FromDate AND @ToDate) AS AttendanceHistoryRows,
           (SELECT COUNT(*) FROM dbo.F03HrmOTActualHistory WHERE WorkDate BETWEEN @FromDate AND @ToDate) AS OTHistoryRows;
END;
GO

PRINT N'HRM attendance current/history archive architecture deployed.';
GO
