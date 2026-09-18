USE [FVN_REGISTER];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/*
===============================================================================
HRM SHIFT MASTER / ATTENDANCE RESOLUTION
===============================================================================
HRM remains SOURCE ONLY.

Master source:
  HRM.dbo.tblca
  HRM.dbo.CC_LichTrinhCa
  HRM.dbo.tblNhanVien.NVLichTrinhCa / NVLichTrinhVaoRa
  HRM.dbo.CC_LichTrinhVaoRa.Loai

Reference only:
  HRM.dbo.tblBaoCao

FVN owns:
  F03Shifts
  F03ShiftSchedules
  F03ShiftScheduleDays
  F03EmployeeShiftSchedules
  F03AttendanceStaging

The procedure intentionally does NOT call HRM.dbo.sphrmvn_FindShift_New.
===============================================================================
*/

IF OBJECT_ID(N'dbo.F03Shifts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03Shifts
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03Shifts PRIMARY KEY,
        ShiftCode nvarchar(7) NOT NULL,
        ShiftName nvarchar(100) NOT NULL,
        ShiftAbbr nvarchar(10) NOT NULL,
        StartTime time(0) NOT NULL,
        Break1Start time(0) NULL,
        Break1End time(0) NULL,
        Break2Start time(0) NULL,
        Break2End time(0) NULL,
        Break3Start time(0) NULL,
        Break3End time(0) NULL,
        EndTime time(0) NOT NULL,
        LateCalcTime time(0) NULL,
        CountOTTC smallint NOT NULL CONSTRAINT DF_F03Shifts_CountOTTC DEFAULT 0,
        CountOT smallint NOT NULL CONSTRAINT DF_F03Shifts_CountOT DEFAULT 0,
        BreakMinutes smallint NOT NULL CONSTRAINT DF_F03Shifts_BreakMinutes DEFAULT 0,
        ThresholdDayMinutes smallint NOT NULL CONSTRAINT DF_F03Shifts_ThresholdDayMinutes DEFAULT 0,
        ThresholdNightMinutes smallint NOT NULL CONSTRAINT DF_F03Shifts_ThresholdNightMinutes DEFAULT 0,
        AllowOutside bit NOT NULL CONSTRAINT DF_F03Shifts_AllowOutside DEFAULT 0,
        AllowEarlyCheckIn bit NOT NULL CONSTRAINT DF_F03Shifts_AllowEarlyCheckIn DEFAULT 0,
        CompensateWorkingTime bit NOT NULL CONSTRAINT DF_F03Shifts_CompensateWorkingTime DEFAULT 0,
        CountBreakAsWork bit NOT NULL CONSTRAINT DF_F03Shifts_CountBreakAsWork DEFAULT 0,
        OTThresholdMinutes int NOT NULL CONSTRAINT DF_F03Shifts_OTThresholdMinutes DEFAULT 0,
        OTThresholdTCMinutes tinyint NOT NULL CONSTRAINT DF_F03Shifts_OTThresholdTCMinutes DEFAULT 0,
        OTUnit tinyint NOT NULL CONSTRAINT DF_F03Shifts_OTUnit DEFAULT 1,
        LateThresholdMinutes tinyint NOT NULL CONSTRAINT DF_F03Shifts_LateThresholdMinutes DEFAULT 0,
        EarlyLeaveThresholdMinutes tinyint NOT NULL CONSTRAINT DF_F03Shifts_EarlyLeaveThresholdMinutes DEFAULT 0,
        ScanBeforeMinutes smallint NOT NULL CONSTRAINT DF_F03Shifts_ScanBeforeMinutes DEFAULT 240,
        ScanAfterMinutes smallint NOT NULL CONSTRAINT DF_F03Shifts_ScanAfterMinutes DEFAULT 240,
        AttendanceUnit int NOT NULL CONSTRAINT DF_F03Shifts_AttendanceUnit DEFAULT 1,
        AllowSundayOT bit NOT NULL CONSTRAINT DF_F03Shifts_AllowSundayOT DEFAULT 0,
        AllowHolidayOT bit NOT NULL CONSTRAINT DF_F03Shifts_AllowHolidayOT DEFAULT 0,
        ShiftType tinyint NOT NULL CONSTRAINT DF_F03Shifts_ShiftType DEFAULT 0,
        AllowedDepartments nvarchar(1000) NOT NULL CONSTRAINT DF_F03Shifts_AllowedDepartments DEFAULT N'',
        RestDay tinyint NOT NULL CONSTRAINT DF_F03Shifts_RestDay DEFAULT 0,
        IgnoreAbsence bit NOT NULL CONSTRAINT DF_F03Shifts_IgnoreAbsence DEFAULT 1,
        ScheduleInOutType tinyint NOT NULL CONSTRAINT DF_F03Shifts_ScheduleInOutType DEFAULT 0,
        SplitOTAfterShift bit NOT NULL CONSTRAINT DF_F03Shifts_SplitOTAfterShift DEFAULT 0,
        CountToTotalWork bit NULL,
        EarlyLeaveCalcTime time(0) NULL,
        ShiftGroup nvarchar(50) NULL,
        IsActive bit NOT NULL CONSTRAINT DF_F03Shifts_IsActive DEFAULT 1,
        LastModifiedSource nvarchar(50) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03Shifts_CreatedAt DEFAULT GETDATE(),
        ModifiedAt datetime2(0) NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.F03ShiftSchedules', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03ShiftSchedules
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03ShiftSchedules PRIMARY KEY,
        ScheduleCode nvarchar(50) NOT NULL,
        ScheduleName nvarchar(200) NOT NULL,
        SplitByMonth bit NOT NULL CONSTRAINT DF_F03ShiftSchedules_SplitByMonth DEFAULT 0,
        IsActive bit NOT NULL CONSTRAINT DF_F03ShiftSchedules_IsActive DEFAULT 1,
        LastModifiedSource nvarchar(50) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03ShiftSchedules_CreatedAt DEFAULT GETDATE(),
        ModifiedAt datetime2(0) NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.F03ShiftScheduleDays', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03ShiftScheduleDays
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03ShiftScheduleDays PRIMARY KEY,
        ScheduleCode nvarchar(50) NOT NULL,
        DayNumber tinyint NOT NULL,
        ShiftCode nvarchar(7) NOT NULL,
        ShiftOrder int NOT NULL CONSTRAINT DF_F03ShiftScheduleDays_ShiftOrder DEFAULT 0,
        IsActive bit NOT NULL CONSTRAINT DF_F03ShiftScheduleDays_IsActive DEFAULT 1,
        LastModifiedSource nvarchar(50) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03ShiftScheduleDays_CreatedAt DEFAULT GETDATE(),
        ModifiedAt datetime2(0) NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.F03EmployeeShiftSchedules', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03EmployeeShiftSchedules
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03EmployeeShiftSchedules PRIMARY KEY,
        EmployeeCode nvarchar(50) NOT NULL,
        ScheduleCode nvarchar(50) NULL,
        SearchType nvarchar(20) NULL,
        IsActive bit NOT NULL CONSTRAINT DF_F03EmployeeShiftSchedules_IsActive DEFAULT 1,
        LastModifiedSource nvarchar(50) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03EmployeeShiftSchedules_CreatedAt DEFAULT GETDATE(),
        ModifiedAt datetime2(0) NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.F03HrmShiftReference', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03HrmShiftReference
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03HrmShiftReference PRIMARY KEY,
        WorkDate date NOT NULL,
        EmployeeCode nvarchar(50) NOT NULL,
        HrmShiftCode nvarchar(7) NULL,
        HrmScheduleCode nvarchar(50) NULL,
        HrmSearchType nvarchar(20) NULL,
        CheckInDateTime datetime2(0) NULL,
        CheckOutDateTime datetime2(0) NULL,
        SyncedAt datetime2(0) NOT NULL CONSTRAINT DF_F03HrmShiftReference_SyncedAt DEFAULT GETDATE()
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03Shifts_ShiftCode' AND object_id=OBJECT_ID(N'dbo.F03Shifts'))
    CREATE UNIQUE INDEX UX_F03Shifts_ShiftCode ON dbo.F03Shifts(ShiftCode);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03ShiftSchedules_ScheduleCode' AND object_id=OBJECT_ID(N'dbo.F03ShiftSchedules'))
    CREATE UNIQUE INDEX UX_F03ShiftSchedules_ScheduleCode ON dbo.F03ShiftSchedules(ScheduleCode);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03ShiftScheduleDays_Key' AND object_id=OBJECT_ID(N'dbo.F03ShiftScheduleDays'))
    CREATE UNIQUE INDEX UX_F03ShiftScheduleDays_Key ON dbo.F03ShiftScheduleDays(ScheduleCode,DayNumber,ShiftCode);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03EmployeeShiftSchedules_Employee' AND object_id=OBJECT_ID(N'dbo.F03EmployeeShiftSchedules'))
    CREATE UNIQUE INDEX UX_F03EmployeeShiftSchedules_Employee ON dbo.F03EmployeeShiftSchedules(EmployeeCode);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03HrmShiftReference_DateEmployee' AND object_id=OBJECT_ID(N'dbo.F03HrmShiftReference'))
    CREATE UNIQUE INDEX UX_F03HrmShiftReference_DateEmployee ON dbo.F03HrmShiftReference(WorkDate,EmployeeCode);
GO

CREATE OR ALTER PROCEDURE dbo.usp_SyncHrmShiftMaster
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRAN;

    /* 1. Shift master: HRM.tblca -> F03Shifts */
    ;WITH S AS
    (
        SELECT
            ShiftCode = LTRIM(RTRIM(CMa)),
            ShiftName = CTen,
            ShiftAbbr = CVietTat,
            StartTime = CONVERT(time(0),CTGBatDau),
            Break1Start = CONVERT(time(0),CTGBDNghi1),
            Break1End = CONVERT(time(0),CTGKTNghi1),
            Break2Start = CONVERT(time(0),CTGBDNghi2),
            Break2End = CONVERT(time(0),CTGKTNghi2),
            Break3Start = CONVERT(time(0),CTGBDNghi3),
            Break3End = CONVERT(time(0),CTGKTNghi3),
            EndTime = CONVERT(time(0),CTGKetThuc),
            LateCalcTime = CONVERT(time(0),CTGTinhDimuon),
            CountOTTC = CBDTinhLTTC,
            CountOT = CBDTinhLT,
            BreakMinutes = CTGNghiGiuaGio,
            ThresholdDayMinutes = CTGQDD,
            ThresholdNightMinutes = CTGQDC,
            AllowOutside = CDuocRaNgoai,
            AllowEarlyCheckIn = CTinhVaoSom,
            CompensateWorkingTime = CBuTGLam,
            CountBreakAsWork = CCongNghiGiuaCa,
            OTThresholdMinutes = CNgUongLamThem,
            OTThresholdTCMinutes = CNguongLamThemTC,
            OTUnit = CDonViLamThem,
            LateThresholdMinutes = CNgUongDiMuon,
            EarlyLeaveThresholdMinutes = CNgUongVeSom,
            ScanBeforeMinutes = CQuetTruocCa,
            ScanAfterMinutes = CQuetSauCa,
            AttendanceUnit = CDonViChamCong,
            AllowSundayOT = CChuNhatLambt,
            AllowHolidayOT = CNgayLeLambt,
            ShiftType = CLoaiCa,
            AllowedDepartments = CDSBoPhan,
            RestDay = CNgaynghi,
            IgnoreAbsence = CKhongtinhVangMat,
            ScheduleInOutType = CLichtrinhVaora,
            SplitOTAfterShift = CChiaLTSauca,
            CountToTotalWork = CCongvaotongcong,
            EarlyLeaveCalcTime = CONVERT(time(0),CTGTinhVeSom),
            ShiftGroup = CNhomCa
        FROM HRM.dbo.tblca
        WHERE NULLIF(LTRIM(RTRIM(CMa)),N'') IS NOT NULL
    )
    UPDATE T
       SET T.ShiftName=S.ShiftName,T.ShiftAbbr=S.ShiftAbbr,T.StartTime=S.StartTime,
           T.Break1Start=S.Break1Start,T.Break1End=S.Break1End,T.Break2Start=S.Break2Start,
           T.Break2End=S.Break2End,T.Break3Start=S.Break3Start,T.Break3End=S.Break3End,
           T.EndTime=S.EndTime,T.LateCalcTime=S.LateCalcTime,T.CountOTTC=S.CountOTTC,
           T.CountOT=S.CountOT,T.BreakMinutes=S.BreakMinutes,T.ThresholdDayMinutes=S.ThresholdDayMinutes,
           T.ThresholdNightMinutes=S.ThresholdNightMinutes,T.AllowOutside=S.AllowOutside,
           T.AllowEarlyCheckIn=S.AllowEarlyCheckIn,T.CompensateWorkingTime=S.CompensateWorkingTime,
           T.CountBreakAsWork=S.CountBreakAsWork,T.OTThresholdMinutes=S.OTThresholdMinutes,
           T.OTThresholdTCMinutes=S.OTThresholdTCMinutes,T.OTUnit=S.OTUnit,
           T.LateThresholdMinutes=S.LateThresholdMinutes,T.EarlyLeaveThresholdMinutes=S.EarlyLeaveThresholdMinutes,
           T.ScanBeforeMinutes=S.ScanBeforeMinutes,T.ScanAfterMinutes=S.ScanAfterMinutes,
           T.AttendanceUnit=S.AttendanceUnit,T.AllowSundayOT=S.AllowSundayOT,T.AllowHolidayOT=S.AllowHolidayOT,
           T.ShiftType=S.ShiftType,T.AllowedDepartments=S.AllowedDepartments,T.RestDay=S.RestDay,
           T.IgnoreAbsence=S.IgnoreAbsence,T.ScheduleInOutType=S.ScheduleInOutType,
           T.SplitOTAfterShift=S.SplitOTAfterShift,T.CountToTotalWork=S.CountToTotalWork,
           T.EarlyLeaveCalcTime=S.EarlyLeaveCalcTime,T.ShiftGroup=S.ShiftGroup,
           T.IsActive=1,T.LastModifiedSource=N'HRM',T.ModifiedAt=GETDATE()
    FROM dbo.F03Shifts T
    JOIN S ON S.ShiftCode=T.ShiftCode;

    INSERT dbo.F03Shifts
    (ShiftCode,ShiftName,ShiftAbbr,StartTime,Break1Start,Break1End,Break2Start,Break2End,
     Break3Start,Break3End,EndTime,LateCalcTime,CountOTTC,CountOT,BreakMinutes,ThresholdDayMinutes,
     ThresholdNightMinutes,AllowOutside,AllowEarlyCheckIn,CompensateWorkingTime,CountBreakAsWork,
     OTThresholdMinutes,OTThresholdTCMinutes,OTUnit,LateThresholdMinutes,EarlyLeaveThresholdMinutes,
     ScanBeforeMinutes,ScanAfterMinutes,AttendanceUnit,AllowSundayOT,AllowHolidayOT,ShiftType,
     AllowedDepartments,RestDay,IgnoreAbsence,ScheduleInOutType,SplitOTAfterShift,CountToTotalWork,
     EarlyLeaveCalcTime,ShiftGroup,IsActive,LastModifiedSource)
    SELECT S.ShiftCode,S.ShiftName,S.ShiftAbbr,S.StartTime,S.Break1Start,S.Break1End,S.Break2Start,S.Break2End,
           S.Break3Start,S.Break3End,S.EndTime,S.LateCalcTime,S.CountOTTC,S.CountOT,S.BreakMinutes,S.ThresholdDayMinutes,
           S.ThresholdNightMinutes,S.AllowOutside,S.AllowEarlyCheckIn,S.CompensateWorkingTime,S.CountBreakAsWork,
           S.OTThresholdMinutes,S.OTThresholdTCMinutes,S.OTUnit,S.LateThresholdMinutes,S.EarlyLeaveThresholdMinutes,
           S.ScanBeforeMinutes,S.ScanAfterMinutes,S.AttendanceUnit,S.AllowSundayOT,S.AllowHolidayOT,S.ShiftType,
           S.AllowedDepartments,S.RestDay,S.IgnoreAbsence,S.ScheduleInOutType,S.SplitOTAfterShift,S.CountToTotalWork,
           S.EarlyLeaveCalcTime,S.ShiftGroup,1,N'HRM'
    FROM
    (
        SELECT LTRIM(RTRIM(CMa)) ShiftCode,CTen ShiftName,CVietTat ShiftAbbr,
               CONVERT(time(0),CTGBatDau) StartTime,CONVERT(time(0),CTGBDNghi1) Break1Start,
               CONVERT(time(0),CTGKTNghi1) Break1End,CONVERT(time(0),CTGBDNghi2) Break2Start,
               CONVERT(time(0),CTGKTNghi2) Break2End,CONVERT(time(0),CTGBDNghi3) Break3Start,
               CONVERT(time(0),CTGKTNghi3) Break3End,CONVERT(time(0),CTGKetThuc) EndTime,
               CONVERT(time(0),CTGTinhDimuon) LateCalcTime,CBDTinhLTTC CountOTTC,CBDTinhLT CountOT,
               CTGNghiGiuaGio BreakMinutes,CTGQDD ThresholdDayMinutes,CTGQDC ThresholdNightMinutes,
               CDuocRaNgoai AllowOutside,CTinhVaoSom AllowEarlyCheckIn,CBuTGLam CompensateWorkingTime,
               CCongNghiGiuaCa CountBreakAsWork,CNguongLamThem OTThresholdMinutes,CNguongLamThemTC OTThresholdTCMinutes,
               CDonViLamThem OTUnit,CNguongDiMuon LateThresholdMinutes,CNguongVeSom EarlyLeaveThresholdMinutes,
               CQuetTruocCa ScanBeforeMinutes,CQuetSauCa ScanAfterMinutes,CDonViChamCong AttendanceUnit,
               CChuNhatLambt AllowSundayOT,CNgayLeLambt AllowHolidayOT,CLoaiCa ShiftType,CDSBoPhan AllowedDepartments,
               CNgaynghi RestDay,CKhongtinhVangMat IgnoreAbsence,CLichtrinhVaora ScheduleInOutType,
               CChiaLTSauca SplitOTAfterShift,CCongvaotongcong CountToTotalWork,CONVERT(time(0),CTGTinhVeSom) EarlyLeaveCalcTime,
               CNhomCa ShiftGroup
        FROM HRM.dbo.tblca
        WHERE NULLIF(LTRIM(RTRIM(CMa)),N'') IS NOT NULL
    ) S
    WHERE NOT EXISTS (SELECT 1 FROM dbo.F03Shifts T WHERE T.ShiftCode=S.ShiftCode);

    /* 2. Schedule master */
    UPDATE T
       SET T.ScheduleName=S.ScheduleName,T.SplitByMonth=S.SplitByMonth,T.IsActive=1,
           T.LastModifiedSource=N'HRM',T.ModifiedAt=GETDATE()
    FROM dbo.F03ShiftSchedules T
    JOIN
    (
        SELECT LTRIM(RTRIM(CONVERT(nvarchar(50),Ma))) ScheduleCode,
               Ten ScheduleName,CAST(ISNULL(PhanTheoThang,0) AS bit) SplitByMonth
        FROM HRM.dbo.CC_LichTrinhCa
        WHERE NULLIF(LTRIM(RTRIM(CONVERT(nvarchar(50),Ma))),N'') IS NOT NULL
          AND ISNULL(DLocked,0)=0
    ) S ON S.ScheduleCode=T.ScheduleCode;

    INSERT dbo.F03ShiftSchedules(ScheduleCode,ScheduleName,SplitByMonth,IsActive,LastModifiedSource)
    SELECT S.ScheduleCode,S.ScheduleName,S.SplitByMonth,1,N'HRM'
    FROM
    (
        SELECT LTRIM(RTRIM(CONVERT(nvarchar(50),Ma))) ScheduleCode,
               Ten ScheduleName,CAST(ISNULL(PhanTheoThang,0) AS bit) SplitByMonth
        FROM HRM.dbo.CC_LichTrinhCa
        WHERE NULLIF(LTRIM(RTRIM(CONVERT(nvarchar(50),Ma))),N'') IS NOT NULL
          AND ISNULL(DLocked,0)=0
    ) S
    WHERE NOT EXISTS (SELECT 1 FROM dbo.F03ShiftSchedules T WHERE T.ScheduleCode=S.ScheduleCode);

    UPDATE T SET IsActive=0,LastModifiedSource=N'HRM',ModifiedAt=GETDATE()
    FROM dbo.F03ShiftSchedules T
    WHERE NOT EXISTS
    (
        SELECT 1 FROM HRM.dbo.CC_LichTrinhCa S
        WHERE LTRIM(RTRIM(CONVERT(nvarchar(50),S.Ma)))=T.ScheduleCode
          AND ISNULL(S.DLocked,0)=0
    );

    /* 3. Normalize Ngay01..Ngay31 into schedule/day/shift rows. */
    DELETE FROM dbo.F03ShiftScheduleDays;

    ;WITH D AS
    (
        SELECT
            ScheduleCode=LTRIM(RTRIM(CONVERT(nvarchar(50),L.Ma))),
            DayNumber=V.DayNumber,
            RawShifts=V.RawShifts
        FROM HRM.dbo.CC_LichTrinhCa L
        CROSS APPLY
        (
            VALUES
            (1,L.Ngay01),(2,L.Ngay02),(3,L.Ngay03),(4,L.Ngay04),(5,L.Ngay05),
            (6,L.Ngay06),(7,L.Ngay07),(8,L.Ngay08),(9,L.Ngay09),(10,L.Ngay10),
            (11,L.Ngay11),(12,L.Ngay12),(13,L.Ngay13),(14,L.Ngay14),(15,L.Ngay15),
            (16,L.Ngay16),(17,L.Ngay17),(18,L.Ngay18),(19,L.Ngay19),(20,L.Ngay20),
            (21,L.Ngay21),(22,L.Ngay22),(23,L.Ngay23),(24,L.Ngay24),(25,L.Ngay25),
            (26,L.Ngay26),(27,L.Ngay27),(28,L.Ngay28),(29,L.Ngay29),(30,L.Ngay30),
            (31,L.Ngay31)
        ) V(DayNumber,RawShifts)
        WHERE ISNULL(L.DLocked,0)=0
    )
    INSERT dbo.F03ShiftScheduleDays
    (ScheduleCode,DayNumber,ShiftCode,ShiftOrder,IsActive,LastModifiedSource)
    SELECT D.ScheduleCode,D.DayNumber,LTRIM(RTRIM(X.value)),ROW_NUMBER() OVER
           (PARTITION BY D.ScheduleCode,D.DayNumber ORDER BY (SELECT 1)),
           1,N'HRM'
    FROM D
    CROSS APPLY STRING_SPLIT(REPLACE(REPLACE(ISNULL(D.RawShifts,N''),N'{',N''),N'}',N''),N',') X
    WHERE NULLIF(LTRIM(RTRIM(X.value)),N'') IS NOT NULL;

    /* 4. Employee -> schedule mapping. NVLichTrinhCa is the actual schedule code
          consumed by sphrmvn_FindShift_New; Loai controls TTDD/TTXK. */
    DELETE FROM dbo.F03EmployeeShiftSchedules;

    INSERT dbo.F03EmployeeShiftSchedules
    (EmployeeCode,ScheduleCode,SearchType,IsActive,LastModifiedSource)
    SELECT
        LTRIM(RTRIM(NV.NVMaNV)),
        NULLIF(LTRIM(RTRIM(CONVERT(nvarchar(50),NV.NVLichTrinhCa))),N''),
        NULLIF(LTRIM(RTRIM(CONVERT(nvarchar(20),LT.Loai))),N''),
        1,N'HRM'
    FROM HRM.dbo.tblNhanVien NV
    LEFT JOIN HRM.dbo.CC_LichTrinhVaoRa LT
      ON NV.NVLichTrinhVaoRa=LT.Ma
    WHERE ISNULL(NV.DLocked,0)=0
      AND NULLIF(LTRIM(RTRIM(NV.NVMaNV)),N'') IS NOT NULL;

    COMMIT;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_SyncAttendanceStaging
    @WorkDate date
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    /* Always refresh the local shift master before resolving attendance.
       This keeps manual/worker attendance runs safe even when HRM master changed
       between normal HRM sync cycles. */
    EXEC dbo.usp_SyncHrmShiftMaster;

    DELETE FROM dbo.F03AttendanceStaging
    WHERE WorkDate >= @WorkDate AND WorkDate < DATEADD(day,1,@WorkDate);

    /* Keep HRM.tblBaoCao as an audit/reference snapshot only. */
    DELETE FROM dbo.F03HrmShiftReference WHERE WorkDate=@WorkDate;

    INSERT dbo.F03HrmShiftReference
    (WorkDate,EmployeeCode,HrmShiftCode,HrmScheduleCode,HrmSearchType,
     CheckInDateTime,CheckOutDateTime)
    SELECT
        @WorkDate,
        LTRIM(RTRIM(NV.NVMaNV)),
        CONVERT(nvarchar(7),BC.BCMaCa),
        NULLIF(LTRIM(RTRIM(BC.BCLichTrinhCa)),N''),
        ES.SearchType,
        BC.BCTGVao,
        BC.BCTGVe
    FROM HRM.dbo.tblBaoCao BC
    INNER JOIN HRM.dbo.tblNhanVien NV ON NV.NVMa=BC.BCMaNV
    LEFT JOIN dbo.F03EmployeeShiftSchedules ES ON ES.EmployeeCode=LTRIM(RTRIM(NV.NVMaNV)) AND ES.IsActive=1
    WHERE BC.BCNgay >= @WorkDate
      AND BC.BCNgay < DATEADD(day,1,@WorkDate);

    ;WITH EmployeeContext AS
    (
        SELECT
            E.EmployeeCode,E.FullName,E.DeptCode,
            ES.ScheduleCode,ES.SearchType
        FROM dbo.F03Employees E
        LEFT JOIN dbo.F03EmployeeShiftSchedules ES
          ON ES.EmployeeCode=E.EmployeeCode AND ES.IsActive=1
        WHERE E.IsActive=1
          AND (E.FirstWorkingDate IS NULL OR CAST(E.FirstWorkingDate AS date)<=@WorkDate)
          AND (E.EndWorkingDate IS NULL OR CAST(E.EndWorkingDate AS date)>=@WorkDate)
    ),
    CandidateShifts AS
    (
        SELECT
            EC.EmployeeCode,EC.FullName,EC.DeptCode,EC.ScheduleCode,EC.SearchType,
            S.ShiftCode,S.ShiftName,S.ShiftAbbr,S.ShiftType,
            S.StartTime,S.EndTime,S.ScanBeforeMinutes,S.ScanAfterMinutes,
            S.Break1Start,S.Break1End,S.Break2Start,S.Break2End,S.Break3Start,S.Break3End,
            S.CountBreakAsWork
        FROM EmployeeContext EC
        INNER JOIN dbo.F03ShiftScheduleDays SD
          ON SD.ScheduleCode=EC.ScheduleCode
         AND SD.DayNumber=DATEPART(WEEKDAY,@WorkDate)
         AND SD.IsActive=1
        INNER JOIN dbo.F03Shifts S
          ON S.ShiftCode=SD.ShiftCode
         AND S.IsActive=1
    ),
    CandidateWindows AS
    (
        SELECT C.*,
               DATEADD(MINUTE,-C.ScanBeforeMinutes,
                   DATEADD(MINUTE,DATEDIFF(MINUTE,CAST('00:00' AS time),C.StartTime),CAST(@WorkDate AS datetime2))) AS WindowStart,
               DATEADD(MINUTE,C.ScanAfterMinutes,
                   DATEADD(DAY,CASE WHEN C.EndTime<C.StartTime THEN 1 ELSE 0 END,
                       DATEADD(MINUTE,DATEDIFF(MINUTE,CAST('00:00' AS time),C.EndTime),CAST(@WorkDate AS datetime2)))) AS WindowEnd,
               DATEADD(MINUTE,DATEDIFF(MINUTE,CAST('00:00' AS time),C.StartTime),CAST(@WorkDate AS datetime2)) AS ShiftStart,
               DATEADD(DAY,CASE WHEN C.EndTime<C.StartTime THEN 1 ELSE 0 END,
                   DATEADD(MINUTE,DATEDIFF(MINUTE,CAST('00:00' AS time),C.EndTime),CAST(@WorkDate AS datetime2))) AS ShiftEnd
        FROM CandidateShifts C
    ),
    Cards AS
    (
        SELECT DISTINCT CTMaNV,CTMaThe
        FROM HRM.dbo.tblCapThe
        WHERE CTMaNV IS NOT NULL AND CTMaThe IS NOT NULL
          AND @WorkDate BETWEEN CAST(CTNgayApDung AS date) AND CAST(CTNgayKetThuc AS date)
    ),
    Swipes AS
    (
        SELECT
            LTRIM(RTRIM(NV.NVMaNV)) EmployeeCode,
            R.ThoiGian,
            CAST(ISNULL(DD.DDChinhVao,0) AS int) IsCheckIn
        FROM HRM.dbo.RecordDataNew R
        INNER JOIN HRM.dbo.tblDauDoc DD ON DD.DDMa=R.IDM
        INNER JOIN Cards C ON C.CTMaThe=R.IDCard
        INNER JOIN HRM.dbo.tblNhanVien NV ON NV.NVMa=C.CTMaNV
        WHERE R.ThoiGian >= DATEADD(HOUR,17,CAST(DATEADD(DAY,-1,@WorkDate) AS datetime))
          AND R.ThoiGian <  DATEADD(HOUR,7,CAST(DATEADD(DAY,1,@WorkDate) AS datetime))
          AND ISNULL(DD.DDLoaiChamCong,N'')<>N'NA'
    ),
    Matched AS
    (
        SELECT
            W.EmployeeCode,W.FullName,W.DeptCode,W.ScheduleCode,W.SearchType,
            W.ShiftCode,W.ShiftName,W.ShiftAbbr,W.ShiftType,
            W.ShiftStart,W.ShiftEnd,W.Break1Start,W.Break1End,
            W.Break2Start,W.Break2End,W.Break3Start,W.Break3End,
            W.CountBreakAsWork,
            I.CheckInDateTime,O.CheckOutDateTime,
            MatchScore =
                CASE WHEN I.CheckInDateTime IS NOT NULL
                       AND O.CheckOutDateTime IS NOT NULL THEN 2
                     WHEN I.CheckInDateTime IS NOT NULL
                       OR O.CheckOutDateTime IS NOT NULL THEN 1
                     ELSE 0 END
        FROM CandidateWindows W
        OUTER APPLY
        (
            SELECT TOP(1) S.ThoiGian CheckInDateTime
            FROM Swipes S
            WHERE S.EmployeeCode=W.EmployeeCode
              AND S.ThoiGian BETWEEN W.WindowStart AND W.WindowEnd
              AND (W.SearchType=N'TTXK' OR S.IsCheckIn=1)
            ORDER BY S.ThoiGian
        ) I
        OUTER APPLY
        (
            SELECT TOP(1) S.ThoiGian CheckOutDateTime
            FROM Swipes S
            WHERE S.EmployeeCode=W.EmployeeCode
              AND S.ThoiGian BETWEEN W.WindowStart AND W.WindowEnd
              AND S.ThoiGian>I.CheckInDateTime
              AND (W.SearchType=N'TTXK' OR S.IsCheckIn=0)
            ORDER BY S.ThoiGian DESC
        ) O
    ),
    Resolved AS
    (
        SELECT *
        FROM
        (
            SELECT M.*,
                   ROW_NUMBER() OVER
                   (
                       PARTITION BY M.EmployeeCode
                       ORDER BY M.MatchScore DESC,
                                CASE WHEN M.CheckInDateTime IS NOT NULL THEN 0 ELSE 1 END,
                                M.ShiftStart
                   ) rn
            FROM Matched M
            WHERE M.MatchScore>0
        ) X
        WHERE X.rn=1
    ),
    Holidays AS
    (
        SELECT TOP(1)
            CAST(1 AS bit) IsHoliday,
            Description,
            CASE WHEN DATEPART(WEEKDAY,HolidayDate)=1 THEN N'SUNDAY'
                 WHEN DATEPART(WEEKDAY,HolidayDate)=7 THEN N'SATURDAY'
                 ELSE N'PUBLIC_HOLIDAY' END HolidayType
        FROM dbo.F03CompanyHolidays
        WHERE IsActive=1 AND HolidayDate=@WorkDate
    )
    INSERT dbo.F03AttendanceStaging
    (
        WorkDate,EmployeeCode,DeptCode,DeptName,FullName,
        CheckInText,CheckOutText,CheckInDateTime,CheckOutDateTime,
        ShiftCode,ShiftName,ShiftAbbr,ShiftCategory,
        OtHours,TotalHours,IsHoliday,HolidayType,ShiftType,SyncedAt
    )
    SELECT
        CAST(@WorkDate AS datetime2(0)),
        R.EmployeeCode,R.DeptCode,D.DeptName,R.FullName,
        CASE WHEN R.CheckInDateTime IS NULL THEN NULL
             ELSE CONVERT(nvarchar(5),CAST(R.CheckInDateTime AS time(0)),108) END,
        CASE WHEN R.CheckOutDateTime IS NULL THEN NULL
             ELSE CONVERT(nvarchar(5),CAST(R.CheckOutDateTime AS time(0)),108) END,
        R.CheckInDateTime,R.CheckOutDateTime,
        R.ShiftCode,R.ShiftName,R.ShiftAbbr,R.ShiftType,
        CAST(
            CASE
                WHEN R.CheckOutDateTime IS NULL THEN 0
                WHEN OT.OTStartTime IS NULL THEN 0
                WHEN R.CheckOutDateTime<=OT.OTStartTime THEN 0
                ELSE DATEDIFF(MINUTE,
                    CASE WHEN R.ShiftEnd>OT.OTStartTime THEN R.ShiftEnd ELSE OT.OTStartTime END,
                    R.CheckOutDateTime)/60.0
            END AS decimal(5,2)),
        CAST(
            CASE WHEN R.CheckInDateTime IS NULL OR R.CheckOutDateTime IS NULL THEN 0
                 ELSE DATEDIFF(MINUTE,R.CheckInDateTime,R.CheckOutDateTime)
                      -
                      CASE WHEN R.CountBreakAsWork=1 THEN 0 ELSE
                           CASE WHEN R.Break1Start IS NULL OR R.Break1End IS NULL THEN 0
                                ELSE
                                  CASE WHEN R.Break1End>R.Break1Start
                                       THEN DATEDIFF(MINUTE,R.Break1Start,R.Break1End) ELSE 0 END
                           END
                           -
                           CASE WHEN R.Break2Start IS NULL OR R.Break2End IS NULL THEN 0
                                ELSE
                                  CASE WHEN R.Break2End>R.Break2Start
                                       THEN DATEDIFF(MINUTE,R.Break2Start,R.Break2End) ELSE 0 END
                           END
                           -
                           CASE WHEN R.Break3Start IS NULL OR R.Break3End IS NULL THEN 0
                                ELSE
                                  CASE WHEN R.Break3End>R.Break3Start
                                       THEN DATEDIFF(MINUTE,R.Break3Start,R.Break3End) ELSE 0 END
                           END
                      END
                 END/60.0
            END AS decimal(5,2)),
        CAST(ISNULL(H.IsHoliday,0) AS bit),
        H.HolidayType,
        CASE WHEN R.CheckOutDateTime IS NULL THEN N'NO_CHECKOUT'
             WHEN H.HolidayType=N'PUBLIC_HOLIDAY' THEN N'OT_HOLIDAY'
             WHEN H.HolidayType=N'SUNDAY' THEN N'OT_SUNDAY'
             WHEN H.HolidayType=N'SATURDAY' THEN N'OT_SATURDAY'
             ELSE N'WORKDAY' END,
        GETDATE()
    FROM Resolved R
    LEFT JOIN dbo.F03Departments D ON D.DeptCode=R.DeptCode AND D.IsActive=1
    LEFT JOIN Holidays H ON 1=1
    OUTER APPLY
    (
        SELECT TOP(1)
            O.StartTime OTStartTime
        FROM dbo.F03OTRequests O
        INNER JOIN dbo.F03OTEmployees OE
          ON OE.OTRequestId=O.Id AND OE.EmployeeCode=R.EmployeeCode AND OE.IsActive=1
        WHERE O.IsActive=1 AND CAST(O.OTDate AS date)=@WorkDate AND O.RequestStatus=3
        ORDER BY O.StartTime,O.Id
    ) OT;

    /* CROSS JOIN with an empty holiday CTE would suppress all rows.
       Reinsert non-holiday rows through a safe fallback if no holiday exists. */
    IF NOT EXISTS (SELECT 1 FROM dbo.F03AttendanceStaging WHERE WorkDate>=@WorkDate AND WorkDate<DATEADD(day,1,@WorkDate))
       AND NOT EXISTS (SELECT 1 FROM dbo.F03CompanyHolidays WHERE IsActive=1 AND HolidayDate=@WorkDate)
    BEGIN
        /* No-op: the INSERT above uses CROSS JOIN only for holiday days.
           Re-run the same result without holiday dependency via the reference
           resolver below. */
        INSERT dbo.F03AttendanceStaging
        (
            WorkDate,EmployeeCode,DeptCode,DeptName,FullName,
            CheckInText,CheckOutText,CheckInDateTime,CheckOutDateTime,
            ShiftCode,ShiftName,ShiftAbbr,ShiftCategory,
            OtHours,TotalHours,IsHoliday,HolidayType,ShiftType,SyncedAt
        )
        SELECT
            CAST(@WorkDate AS datetime2(0)),R.EmployeeCode,R.DeptCode,D.DeptName,R.FullName,
            CASE WHEN R.CheckInDateTime IS NULL THEN NULL ELSE CONVERT(nvarchar(5),CAST(R.CheckInDateTime AS time(0)),108) END,
            CASE WHEN R.CheckOutDateTime IS NULL THEN NULL ELSE CONVERT(nvarchar(5),CAST(R.CheckOutDateTime AS time(0)),108) END,
            R.CheckInDateTime,R.CheckOutDateTime,R.ShiftCode,R.ShiftName,R.ShiftAbbr,R.ShiftType,
            CAST(CASE WHEN R.CheckOutDateTime IS NULL OR OT.OTStartTime IS NULL OR R.CheckOutDateTime<=OT.OTStartTime THEN 0
                      ELSE DATEDIFF(MINUTE,CASE WHEN R.ShiftEnd>OT.OTStartTime THEN R.ShiftEnd ELSE OT.OTStartTime END,R.CheckOutDateTime)/60.0 END AS decimal(5,2)),
            CAST(CASE WHEN R.CheckInDateTime IS NULL OR R.CheckOutDateTime IS NULL THEN 0
                      ELSE DATEDIFF(MINUTE,R.CheckInDateTime,R.CheckOutDateTime)/60.0 END AS decimal(5,2)),
            CAST(0 AS bit),NULL,
            CASE WHEN R.CheckOutDateTime IS NULL THEN N'NO_CHECKOUT' ELSE N'WORKDAY' END,
            GETDATE()
        FROM Resolved R
        LEFT JOIN dbo.F03Departments D ON D.DeptCode=R.DeptCode AND D.IsActive=1
        OUTER APPLY
        (
            SELECT TOP(1) O.StartTime OTStartTime
            FROM dbo.F03OTRequests O
            INNER JOIN dbo.F03OTEmployees OE ON OE.OTRequestId=O.Id AND OE.EmployeeCode=R.EmployeeCode AND OE.IsActive=1
            WHERE O.IsActive=1 AND CAST(O.OTDate AS date)=@WorkDate AND O.RequestStatus=3
            ORDER BY O.StartTime,O.Id
        ) OT;
    END;

    DECLARE @Count int = @@ROWCOUNT;
    SELECT @Count AS SyncedCount,CAST(@WorkDate AS date) AS WorkDate;
END;
GO
