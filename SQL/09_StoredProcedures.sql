USE [FVN_REGISTER];
GO
CREATE OR ALTER PROCEDURE dbo.usp_GetPendingApproval
    @ApproverCode nvarchar(50)=NULL,
    @ApproverEmail nvarchar(100)=NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RequestType,RequestId,Level,RoleName,ApproverCode,ApproverName,ApproverEmail,
           Required,Approved,ApprovedAt,Comment,ReminderSent
    FROM dbo.F03ApprovalSteps
    WHERE Required=1 AND Approved IS NULL
      AND (@ApproverCode IS NULL OR ApproverCode=@ApproverCode)
      AND (@ApproverEmail IS NULL OR ApproverEmail=@ApproverEmail)
    ORDER BY Level,CreatedAt;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_DecideApproval
    @StepId int,
    @Decision int,
    @Comment nvarchar(500)=NULL,
    @ApproverCode nvarchar(50)=NULL,
    @OverriddenByCode nvarchar(50)=NULL,
    @OverriddenByName nvarchar(100)=NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRAN;

    DECLARE @RequestType nvarchar(20), @RequestId int, @ApproverName nvarchar(100);
    SELECT @RequestType=RequestType,@RequestId=RequestId,@ApproverName=ApproverName
    FROM dbo.F03ApprovalSteps WITH(UPDLOCK,HOLDLOCK)
    WHERE Id=@StepId;

    IF @RequestType IS NULL
    BEGIN
        ROLLBACK;
        THROW 50001,'Approval step not found',1;
    END;

    UPDATE dbo.F03ApprovalSteps
    SET Approved=CASE WHEN @Decision=1 THEN 1 WHEN @Decision=2 THEN 0 ELSE Approved END,
        ApprovedAt=CASE WHEN @Decision IN(1,2) THEN GETDATE() ELSE ApprovedAt END,
        Comment=@Comment,
        IsOverriddenByAdmin=CASE WHEN @OverriddenByCode IS NULL THEN IsOverriddenByAdmin ELSE 1 END,
        OverriddenByCode=COALESCE(@OverriddenByCode,OverriddenByCode),
        OverriddenByName=COALESCE(@OverriddenByName,OverriddenByName),
        OverriddenAt=CASE WHEN @OverriddenByCode IS NULL THEN OverriddenAt ELSE GETDATE() END
    WHERE Id=@StepId;

    INSERT dbo.ApprovalHistories
    (RequestType,RequestId,StepId,IsOverriddenByAdmin,ApproverCode,ApproverName,
     OverriddenByCode,OverriddenByName,OverriddenAt,Decision,Comment,ActionAt,CreatedBy)
    VALUES
    (CASE @RequestType WHEN N'Leave' THEN 0 WHEN N'Overtime' THEN 1 WHEN N'Trip' THEN 2 WHEN N'Equipment' THEN 3 END,
     @RequestId,@StepId,CASE WHEN @OverriddenByCode IS NULL THEN 0 ELSE 1 END,
     COALESCE(@ApproverCode,''),COALESCE(@ApproverName,''),@OverriddenByCode,@OverriddenByName,
     CASE WHEN @OverriddenByCode IS NULL THEN NULL ELSE GETDATE() END,
     @Decision,@Comment,GETDATE(),0);
    COMMIT;
END;
GO


/*
  HRM SHIFT MASTER SYNC
  Reads HRM configuration and normalizes it into FVN_REGISTER.
  No write is performed against HRM.
*/
CREATE OR ALTER PROCEDURE dbo.usp_SyncHrmShiftMaster
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Now datetime2(0)=GETDATE();

    /* 1. Shift master: HRM.tblca -> F03Shifts */
    UPDATE tgt
       SET tgt.IsActive=1,
           tgt.ShiftName=src.CTen,
           tgt.ShiftAbbr=src.CVietTat,
           tgt.StartTime=CAST(src.CTGBatDau AS time(0)),
           tgt.Break1Start=CASE WHEN src.CTGBDNghi1='19000101' THEN NULL ELSE CAST(src.CTGBDNghi1 AS time(0)) END,
           tgt.Break1End=CASE WHEN src.CTGKTNghi1='19000101' THEN NULL ELSE CAST(src.CTGKTNghi1 AS time(0)) END,
           tgt.Break2Start=CASE WHEN src.CTGBDNghi2='19000101' THEN NULL ELSE CAST(src.CTGBDNghi2 AS time(0)) END,
           tgt.Break2End=CASE WHEN src.CTGKTNghi2='19000101' THEN NULL ELSE CAST(src.CTGKTNghi2 AS time(0)) END,
           tgt.Break3Start=CASE WHEN src.CTGBDNghi3='19000101' THEN NULL ELSE CAST(src.CTGBDNghi3 AS time(0)) END,
           tgt.Break3End=CASE WHEN src.CTGKTNghi3='19000101' THEN NULL ELSE CAST(src.CTGKTNghi3 AS time(0)) END,
           tgt.EndTime=CAST(src.CTGKetThuc AS time(0)),
           tgt.LateCalcTime=CASE WHEN src.CTGTinhDimuon='19000101' THEN NULL ELSE CAST(src.CTGTinhDimuon AS time(0)) END,
           tgt.OTRateBase=src.CBDTinhLT,
           tgt.OTRateTC=src.CBDTinhLTTC,
           tgt.OTUnit=src.CDonViLamThem,
           tgt.MidBreakMinutes=src.CTGNghiGiuaGio,
           tgt.RegularMinutes=src.CTGQDD,
           tgt.DailyOTThresholdMinutes=src.CNguongLamThem,
           tgt.DailyOTTCThresholdMinutes=src.CNguongLamThemTC,
           tgt.LateThresholdMinutes=src.CNguongDiMuon,
           tgt.EarlyLeaveThresholdMinutes=src.CNguongVeSom,
           tgt.ScanBeforeMinutes=src.CQuetTruocCa,
           tgt.ScanAfterMinutes=src.CQuetSauCa,
           tgt.AttendanceUnit=src.CDonViChamCong,
           tgt.AllowSundayOT=src.CChuNhatLambt,
           tgt.AllowHolidayOT=src.CNgayLeLambt,
           tgt.ShiftType=src.CLoaiCa,
           tgt.DepartmentScope=src.CDSBoPhan,
           tgt.RestDayType=src.CNgaynghi,
           tgt.IgnoreAbsence=src.CKhongtinhVangMat,
           tgt.ScheduleInOutType=src.CLichtrinhVaora,
           tgt.SplitOTAfterShift=src.CChiaLTSauca,
           tgt.CountBreakAsWork=src.CCongNghiGiuaCa,
           tgt.CountToTotalWork=src.CCongvaotongcong,
           tgt.AllowOutside=src.CDuocRaNgoai,
           tgt.AllowEarlyCheckIn=src.CTinhVaoSom,
           tgt.ShiftGroup=src.CNhomCa,
           tgt.LastModifiedSource=N'HRM',
           tgt.ModifiedBy=0,
           tgt.ModifiedAt=@Now
    FROM dbo.F03Shifts tgt
    INNER JOIN HRM.dbo.tblca src ON tgt.ShiftCode=CONVERT(nvarchar(20),src.CMa);

    INSERT dbo.F03Shifts
    (IsActive,CreatedBy,LastModifiedSource,CreatedAt,ModifiedAt,HrmCode,ShiftCode,ShiftName,ShiftAbbr,
     StartTime,Break1Start,Break1End,Break2Start,Break2End,Break3Start,Break3End,EndTime,LateCalcTime,
     OTRateBase,OTRateTC,OTUnit,MidBreakMinutes,RegularMinutes,DailyOTThresholdMinutes,DailyOTTCThresholdMinutes,
     LateThresholdMinutes,EarlyLeaveThresholdMinutes,ScanBeforeMinutes,ScanAfterMinutes,AttendanceUnit,
     AllowSundayOT,AllowHolidayOT,ShiftType,DepartmentScope,RestDayType,IgnoreAbsence,ScheduleInOutType,
     SplitOTAfterShift,CountBreakAsWork,CountToTotalWork,AllowOutside,AllowEarlyCheckIn,ShiftGroup)
    SELECT 1,0,N'HRM',@Now,@Now,CONVERT(nvarchar(20),s.CMa),CONVERT(nvarchar(20),s.CMa),s.CTen,s.CVietTat,
           CAST(s.CTGBatDau AS time(0)),
           CASE WHEN s.CTGBDNghi1='19000101' THEN NULL ELSE CAST(s.CTGBDNghi1 AS time(0)) END,
           CASE WHEN s.CTGKTNghi1='19000101' THEN NULL ELSE CAST(s.CTGKTNghi1 AS time(0)) END,
           CASE WHEN s.CTGBDNghi2='19000101' THEN NULL ELSE CAST(s.CTGBDNghi2 AS time(0)) END,
           CASE WHEN s.CTGKTNghi2='19000101' THEN NULL ELSE CAST(s.CTGKTNghi2 AS time(0)) END,
           CASE WHEN s.CTGBDNghi3='19000101' THEN NULL ELSE CAST(s.CTGBDNghi3 AS time(0)) END,
           CASE WHEN s.CTGKTNghi3='19000101' THEN NULL ELSE CAST(s.CTGKTNghi3 AS time(0)) END,
           CAST(s.CTGKetThuc AS time(0)),
           CASE WHEN s.CTGTinhDimuon='19000101' THEN NULL ELSE CAST(s.CTGTinhDimuon AS time(0)) END,
           s.CBDTinhLT,s.CBDTinhLTTC,s.CDonViLamThem,s.CTGNghiGiuaGio,s.CTGQDD,s.CNguongLamThem,s.CNguongLamThemTC,
           s.CNguongDiMuon,s.CNguongVeSom,s.CQuetTruocCa,s.CQuetSauCa,s.CDonViChamCong,
           s.CChuNhatLambt,s.CNgayLeLambt,s.CLoaiCa,s.CDSBoPhan,s.CNgaynghi,s.CKhongtinhVangMat,
           s.CLichtrinhVaora,s.CChiaLTSauca,s.CCongNghiGiuaCa,s.CCongvaotongcong,s.CDuocRaNgoai,s.CTinhVaoSom,s.CNhomCa
    FROM HRM.dbo.tblca s
    WHERE NOT EXISTS (SELECT 1 FROM dbo.F03Shifts t WHERE t.ShiftCode=CONVERT(nvarchar(20),s.CMa));

    UPDATE t SET IsActive=0,ModifiedAt=@Now,ModifiedBy=0
    FROM dbo.F03Shifts t
    WHERE t.LastModifiedSource=N'HRM'
      AND NOT EXISTS (SELECT 1 FROM HRM.dbo.tblca s WHERE t.ShiftCode=CONVERT(nvarchar(20),s.CMa));

    /* 2. Schedule master. HRM's FindShift_New resolves only weekday columns 1..7,
          so those are the canonical active-day definitions for attendance. */
    UPDATE t
       SET IsActive=1,ScheduleName=s.Ten,IsMonthly=s.PhanTheoThang,
           HrmCode=s.Ma,LastModifiedSource=N'HRM',ModifiedBy=0,ModifiedAt=@Now
    FROM dbo.F03ShiftSchedules t
    INNER JOIN HRM.dbo.CC_LichTrinhCa s ON t.ScheduleCode=s.Ma;

    INSERT dbo.F03ShiftSchedules
    (IsActive,CreatedBy,LastModifiedSource,CreatedAt,ModifiedAt,ScheduleCode,ScheduleName,IsMonthly,HrmCode)
    SELECT 1,0,N'HRM',@Now,@Now,s.Ma,s.Ten,s.PhanTheoThang,s.Ma
    FROM HRM.dbo.CC_LichTrinhCa s
    WHERE NOT EXISTS (SELECT 1 FROM dbo.F03ShiftSchedules t WHERE t.ScheduleCode=s.Ma);

    DELETE d
    FROM dbo.F03ShiftScheduleDays d
    WHERE d.LastModifiedSource=N'HRM';

    INSERT dbo.F03ShiftScheduleDays
    (IsActive,CreatedBy,LastModifiedSource,CreatedAt,ModifiedAt,ScheduleCode,DayNo,ShiftCode)
    SELECT 1,0,N'HRM',@Now,@Now,s.Ma,v.DayNo,LTRIM(RTRIM(x.value))
    FROM HRM.dbo.CC_LichTrinhCa s
    CROSS APPLY (VALUES
       (1,s.Ngay01),(2,s.Ngay02),(3,s.Ngay03),(4,s.Ngay04),(5,s.Ngay05),(6,s.Ngay06),(7,s.Ngay07),
       (8,s.Ngay08),(9,s.Ngay09),(10,s.Ngay10),(11,s.Ngay11),(12,s.Ngay12),(13,s.Ngay13),(14,s.Ngay14),
       (15,s.Ngay15),(16,s.Ngay16),(17,s.Ngay17),(18,s.Ngay18),(19,s.Ngay19),(20,s.Ngay20),(21,s.Ngay21),
       (22,s.Ngay22),(23,s.Ngay23),(24,s.Ngay24),(25,s.Ngay25),(26,s.Ngay26),(27,s.Ngay27),(28,s.Ngay28),
       (29,s.Ngay29),(30,s.Ngay30),(31,s.Ngay31)
    ) v(DayNo,ShiftList)
    CROSS APPLY STRING_SPLIT(COALESCE(v.ShiftList,N''),',') x
    WHERE LTRIM(RTRIM(x.value))<>N''
      AND EXISTS (SELECT 1 FROM dbo.F03Shifts sh WHERE sh.ShiftCode=LTRIM(RTRIM(x.value)));

    /* 3. Current employee schedule assignment. */
    UPDATE t
       SET IsActive=CASE WHEN e.IsActive=1 AND NULLIF(nv.NVLichTrinhCa,N'') IS NOT NULL THEN 1 ELSE 0 END,
           ScheduleCode=NULLIF(nv.NVLichTrinhCa,N''),
           ScheduleType=vr.Loai,
           HrmEmployeeNo=nv.NVMa,
           ValidFrom=TRY_CONVERT(date,nv.NVNgayVao),
           ValidTo=CASE WHEN nv.NVNgayRa >= '9990-01-01' THEN NULL ELSE TRY_CONVERT(date,nv.NVNgayRa) END,
           LastModifiedSource=N'HRM',ModifiedBy=0,ModifiedAt=@Now
    FROM dbo.F03EmployeeShiftSchedules t
    INNER JOIN HRM.dbo.tblNhanVien nv ON t.EmployeeCode=RTRIM(nv.NVMaNV)
    LEFT JOIN HRM.dbo.CC_LichTrinhVaoRa vr ON vr.Ma=nv.NVLichTrinhVaoRa
    LEFT JOIN dbo.F03Employees e ON e.EmployeeCode=RTRIM(nv.NVMaNV);

    INSERT dbo.F03EmployeeShiftSchedules
    (IsActive,CreatedBy,LastModifiedSource,CreatedAt,ModifiedAt,EmployeeCode,ScheduleCode,ScheduleType,HrmEmployeeNo,ValidFrom,ValidTo)
    SELECT CASE WHEN ISNULL(e.IsActive,CASE WHEN ISNULL(nv.DLocked,0)=0 THEN 1 ELSE 0 END)=1
                  AND NULLIF(nv.NVLichTrinhCa,N'') IS NOT NULL THEN 1 ELSE 0 END,
           0,N'HRM',@Now,@Now,RTRIM(nv.NVMaNV),NULLIF(nv.NVLichTrinhCa,N''),vr.Loai,nv.NVMa,
           TRY_CONVERT(date,nv.NVNgayVao),
           CASE WHEN nv.NVNgayRa >= '9990-01-01' THEN NULL ELSE TRY_CONVERT(date,nv.NVNgayRa) END
    FROM HRM.dbo.tblNhanVien nv
    LEFT JOIN dbo.F03Employees e ON e.EmployeeCode=RTRIM(nv.NVMaNV)
    LEFT JOIN HRM.dbo.CC_LichTrinhVaoRa vr ON vr.Ma=nv.NVLichTrinhVaoRa
    WHERE NOT EXISTS (SELECT 1 FROM dbo.F03EmployeeShiftSchedules t WHERE t.EmployeeCode=RTRIM(nv.NVMaNV));

    UPDATE t SET IsActive=0,ModifiedAt=@Now,ModifiedBy=0
    FROM dbo.F03EmployeeShiftSchedules t
    WHERE t.LastModifiedSource=N'HRM'
      AND NOT EXISTS (SELECT 1 FROM HRM.dbo.tblNhanVien nv WHERE RTRIM(nv.NVMaNV)=t.EmployeeCode);

    SELECT
        (SELECT COUNT(*) FROM dbo.F03Shifts WHERE LastModifiedSource=N'HRM' AND IsActive=1) AS ShiftCount,
        (SELECT COUNT(*) FROM dbo.F03ShiftSchedules WHERE LastModifiedSource=N'HRM' AND IsActive=1) AS ScheduleCount,
        (SELECT COUNT(*) FROM dbo.F03ShiftScheduleDays WHERE LastModifiedSource=N'HRM' AND IsActive=1) AS ScheduleDayCount,
        (SELECT COUNT(*) FROM dbo.F03EmployeeShiftSchedules WHERE LastModifiedSource=N'HRM' AND IsActive=1) AS EmployeeScheduleCount;
END;
GO

/*
  Pipeline B — HRM attendance -> local staging -> OT reconciliation.

  ARCHITECTURE RULE:
  HRM is a SOURCE ONLY. FVN_REGISTER owns attendance pairing,
  OT calculation, holiday classification and reconciliation rules.

  This procedure reads HRM base tables directly. It MUST NOT call an
  HRM function that contains FVN_REGISTER business logic.

  OT start authority:
  F03OTRequests.StartTime of an APPROVED OT request is the local
  business source for the beginning of the requested OT window.
*/

/*
  Pipeline B — local shift master + HRM attendance -> F03AttendanceStaging.
  HRM is READ ONLY. Shift configuration is first synchronized by
  usp_SyncHrmShiftMaster; attendance uses only the local F03* shift tables.
*/
CREATE OR ALTER PROCEDURE dbo.usp_SyncAttendanceStaging
    @WorkDate date
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DELETE FROM dbo.F03AttendanceStaging
    WHERE WorkDate >= CAST(@WorkDate AS datetime2(0))
      AND WorkDate < DATEADD(day,1,CAST(@WorkDate AS datetime2(0)));

    IF NOT EXISTS (SELECT 1 FROM dbo.F03Shifts WHERE IsActive=1)
        THROW 51301,'F03Shifts is empty. Run dbo.usp_SyncHrmShiftMaster first.',1;

    IF NOT EXISTS (SELECT 1 FROM dbo.F03ShiftSchedules WHERE IsActive=1)
        THROW 51302,'F03ShiftSchedules is empty. Run dbo.usp_SyncHrmShiftMaster first.',1;

    CREATE TABLE #Employees(
        EmployeeCode nvarchar(50) NOT NULL PRIMARY KEY,
        FullName nvarchar(100) NULL,
        DeptCode nvarchar(20) NULL,
        ScheduleCode nvarchar(50) NULL,
        ScheduleType nvarchar(20) NULL,
        HrmScheduleCode nvarchar(50) NULL
    );

    INSERT #Employees(EmployeeCode,FullName,DeptCode,ScheduleCode,ScheduleType,HrmScheduleCode)
    SELECT
        e.EmployeeCode,e.EmployeeName,e.DeptCode,
        COALESCE(NULLIF(RTRIM(bc.BCLichTrinhCa),N''),es.ScheduleCode),
        es.ScheduleType,
        COALESCE(NULLIF(RTRIM(bc.BCLichTrinhCa),N''),es.ScheduleCode)
    FROM dbo.F03Employees e
    LEFT JOIN dbo.F03EmployeeShiftSchedules es
        ON es.EmployeeCode=e.EmployeeCode AND es.IsActive=1
    LEFT JOIN HRM.dbo.tblNhanVien nv
        ON RTRIM(nv.NVMaNV)=e.EmployeeCode
    LEFT JOIN HRM.dbo.tblBaoCao bc
        ON bc.BCNgay=@WorkDate AND bc.BCMaNV=nv.NVMa
    WHERE e.IsActive=1
      AND COALESCE(NULLIF(RTRIM(bc.BCLichTrinhCa),N''),es.ScheduleCode) IS NOT NULL;

    CREATE TABLE #Candidates(
        EmployeeCode nvarchar(50) NOT NULL,
        ShiftCode nvarchar(20) NOT NULL,
        ShiftName nvarchar(100) NOT NULL,
        ShiftAbbr nvarchar(10) NULL,
        ShiftCategory int NULL,
        ShiftGroup nvarchar(50) NULL,
        ShiftStart datetime2(0) NOT NULL,
        ShiftEnd datetime2(0) NOT NULL,
        ScanStart datetime2(0) NOT NULL,
        ScanEnd datetime2(0) NOT NULL,
        ScheduleType nvarchar(20) NULL,
        AllowEarlyCheckIn bit NOT NULL,
        CountBreakAsWork bit NULL,
        CountToTotalWork bit NULL,
        Break1Start datetime2(0) NULL, Break1End datetime2(0) NULL,
        Break2Start datetime2(0) NULL, Break2End datetime2(0) NULL,
        Break3Start datetime2(0) NULL, Break3End datetime2(0) NULL
    );

    ;WITH DayCandidates AS
    (
        SELECT e.EmployeeCode,e.ScheduleType,d.ShiftCode
        FROM #Employees e
        INNER JOIN dbo.F03ShiftScheduleDays d
          ON d.ScheduleCode=e.ScheduleCode
         AND d.DayNo=DATEPART(WEEKDAY,@WorkDate)
         AND d.IsActive=1
    )
    INSERT #Candidates
    SELECT
        dc.EmployeeCode,s.ShiftCode,s.ShiftName,s.ShiftAbbr,s.ShiftType,s.ShiftGroup,
        x.ShiftStart,x.ShiftEnd,
        DATEADD(MINUTE,-s.ScanBeforeMinutes,x.ShiftStart),
        DATEADD(MINUTE, s.ScanAfterMinutes,x.ShiftEnd),
        dc.ScheduleType,s.AllowEarlyCheckIn,s.CountBreakAsWork,s.CountToTotalWork,
        CASE WHEN s.Break1Start IS NULL THEN NULL ELSE DATEADD(MINUTE,DATEDIFF(MINUTE,CAST('00:00:00' AS time),s.Break1Start),x.ShiftStart) END,
        CASE WHEN s.Break1End IS NULL THEN NULL ELSE DATEADD(MINUTE,DATEDIFF(MINUTE,CAST('00:00:00' AS time),s.Break1End),x.ShiftStart) END,
        CASE WHEN s.Break2Start IS NULL THEN NULL ELSE DATEADD(MINUTE,DATEDIFF(MINUTE,CAST('00:00:00' AS time),s.Break2Start),x.ShiftStart) END,
        CASE WHEN s.Break2End IS NULL THEN NULL ELSE DATEADD(MINUTE,DATEDIFF(MINUTE,CAST('00:00:00' AS time),s.Break2End),x.ShiftStart) END,
        CASE WHEN s.Break3Start IS NULL THEN NULL ELSE DATEADD(MINUTE,DATEDIFF(MINUTE,CAST('00:00:00' AS time),s.Break3Start),x.ShiftStart) END,
        CASE WHEN s.Break3End IS NULL THEN NULL ELSE DATEADD(MINUTE,DATEDIFF(MINUTE,CAST('00:00:00' AS time),s.Break3End),x.ShiftStart) END
    FROM DayCandidates dc
    INNER JOIN dbo.F03Shifts s ON s.ShiftCode=dc.ShiftCode AND s.IsActive=1
    CROSS APPLY
    (
        SELECT
            ShiftStart=DATEADD(MINUTE,DATEDIFF(MINUTE,CAST('00:00:00' AS time),s.StartTime),CAST(@WorkDate AS datetime2(0))),
            ShiftEnd=DATEADD(MINUTE,
                DATEDIFF(MINUTE,CAST('00:00:00' AS time),s.EndTime)
                + CASE WHEN s.EndTime < s.StartTime THEN 1440 ELSE 0 END,
                CAST(@WorkDate AS datetime2(0)))
    ) x;

    /* Move breaks across midnight when necessary. */
    UPDATE c
       SET Break1Start=CASE WHEN Break1Start IS NULL THEN NULL WHEN Break1Start < ShiftStart THEN DATEADD(day,1,Break1Start) ELSE Break1Start END,
           Break1End=CASE WHEN Break1End IS NULL THEN NULL WHEN Break1End < ShiftStart THEN DATEADD(day,1,Break1End) ELSE Break1End END,
           Break2Start=CASE WHEN Break2Start IS NULL THEN NULL WHEN Break2Start < ShiftStart THEN DATEADD(day,1,Break2Start) ELSE Break2Start END,
           Break2End=CASE WHEN Break2End IS NULL THEN NULL WHEN Break2End < ShiftStart THEN DATEADD(day,1,Break2End) ELSE Break2End END,
           Break3Start=CASE WHEN Break3Start IS NULL THEN NULL WHEN Break3Start < ShiftStart THEN DATEADD(day,1,Break3Start) ELSE Break3Start END,
           Break3End=CASE WHEN Break3End IS NULL THEN NULL WHEN Break3End < ShiftStart THEN DATEADD(day,1,Break3End) ELSE Break3End END
    FROM #Candidates c;

    CREATE TABLE #Swipes(
        EmployeeCode nvarchar(50) NOT NULL,
        SwipeTime datetime2(0) NOT NULL,
        IsCheckIn bit NULL
    );

    INSERT #Swipes(EmployeeCode,SwipeTime,IsCheckIn)
    SELECT DISTINCT
        RTRIM(nv.NVMaNV),r.ThoiGian,CAST(dd.DDChinhVao AS bit)
    FROM HRM.dbo.RecordDataNew r
    INNER JOIN HRM.dbo.tblDauDoc dd ON dd.DDMa=r.IDM
    INNER JOIN HRM.dbo.tblCapThe ct ON ct.CTMaThe=r.IDCard
    INNER JOIN HRM.dbo.tblNhanVien nv ON nv.NVMa=ct.CTMaNV
    INNER JOIN #Employees e ON e.EmployeeCode=RTRIM(nv.NVMaNV)
    WHERE ct.CTNgayApDung <= @WorkDate
      AND ct.CTNgayKetThuc >= @WorkDate
      AND r.ThoiGian >= DATEADD(HOUR,17,CAST(DATEADD(DAY,-1,@WorkDate) AS datetime2(0)))
      AND r.ThoiGian < DATEADD(HOUR,12,CAST(DATEADD(DAY,1,@WorkDate) AS datetime2(0)));

    CREATE TABLE #Resolved(
        EmployeeCode nvarchar(50) NOT NULL PRIMARY KEY,
        ShiftCode nvarchar(20) NOT NULL,
        ShiftName nvarchar(100) NOT NULL,
        ShiftAbbr nvarchar(10) NULL,
        ShiftCategory int NULL,
        ShiftGroup nvarchar(50) NULL,
        ShiftStart datetime2(0) NOT NULL,
        ShiftEnd datetime2(0) NOT NULL,
        CheckIn datetime2(0) NULL,
        CheckOut datetime2(0) NULL,
        ScheduleType nvarchar(20) NULL,
        CountBreakAsWork bit NULL,
        CountToTotalWork bit NULL,
        BreakMinutes int NOT NULL DEFAULT 0
    );

    ;WITH CandidateSwipes AS
    (
        SELECT c.*,sw.SwipeTime,sw.IsCheckIn
        FROM #Candidates c
        INNER JOIN #Swipes sw
          ON sw.EmployeeCode=c.EmployeeCode
         AND sw.SwipeTime BETWEEN c.ScanStart AND c.ScanEnd
    ),
    Ranked AS
    (
        SELECT *,
          MIN(CASE WHEN IsCheckIn=1 THEN SwipeTime END) OVER(PARTITION BY EmployeeCode,ShiftCode) AS FirstInTTDD,
          MAX(CASE WHEN IsCheckIn=0 THEN SwipeTime END) OVER(PARTITION BY EmployeeCode,ShiftCode) AS LastOutTTDD,
          MIN(SwipeTime) OVER(PARTITION BY EmployeeCode,ShiftCode) AS FirstAny,
          MAX(SwipeTime) OVER(PARTITION BY EmployeeCode,ShiftCode) AS LastAny
        FROM CandidateSwipes
    ),
    Match AS
    (
        SELECT DISTINCT EmployeeCode,ShiftCode,ShiftName,ShiftAbbr,ShiftCategory,ShiftGroup,
               ShiftStart,ShiftEnd,ScheduleType,CountBreakAsWork,CountToTotalWork,
               CASE WHEN ScheduleType=N'TTXK'
                    THEN FirstAny ELSE FirstInTTDD END AS CheckIn,
               CASE WHEN ScheduleType=N'TTXK'
                    THEN CASE WHEN LastAny>FirstAny THEN LastAny END
                    ELSE LastOutTTDD END AS CheckOut,
               CASE WHEN ScheduleType=N'TTXK' THEN 2
                    WHEN FirstInTTDD IS NOT NULL AND LastOutTTDD IS NOT NULL THEN 2
                    WHEN FirstInTTDD IS NOT NULL OR LastOutTTDD IS NOT NULL THEN 1
                    ELSE 0 END AS MatchScore
        FROM Ranked
    ),
    Best AS
    (
        SELECT *,ROW_NUMBER() OVER(
            PARTITION BY EmployeeCode
            ORDER BY MatchScore DESC,
                     CASE WHEN CheckIn IS NOT NULL AND CheckOut IS NOT NULL THEN 1 ELSE 0 END DESC,
                     ABS(DATEDIFF(MINUTE,ShiftStart,ISNULL(CheckIn,ShiftStart))) ASC,
                     ShiftStart
        ) rn
        FROM Match
        WHERE MatchScore>0
    )
    INSERT #Resolved(EmployeeCode,ShiftCode,ShiftName,ShiftAbbr,ShiftCategory,ShiftGroup,ShiftStart,ShiftEnd,CheckIn,CheckOut,ScheduleType,CountBreakAsWork,CountToTotalWork)
    SELECT EmployeeCode,ShiftCode,ShiftName,ShiftAbbr,ShiftCategory,ShiftGroup,ShiftStart,ShiftEnd,CheckIn,CheckOut,ScheduleType,CountBreakAsWork,CountToTotalWork
    FROM Best WHERE rn=1;

    UPDATE r
       SET BreakMinutes =
           CASE WHEN ISNULL(r.CountBreakAsWork,0)=1 THEN 0 ELSE
             ISNULL(CASE WHEN c.Break1Start IS NOT NULL AND c.Break1End IS NOT NULL THEN
                CASE WHEN DATEDIFF(MINUTE,CASE WHEN r.CheckIn>c.Break1Start THEN r.CheckIn ELSE c.Break1Start END,
                                      CASE WHEN r.CheckOut<c.Break1End THEN r.CheckOut ELSE c.Break1End END)>0
                     THEN DATEDIFF(MINUTE,CASE WHEN r.CheckIn>c.Break1Start THEN r.CheckIn ELSE c.Break1Start END,
                                      CASE WHEN r.CheckOut<c.Break1End THEN r.CheckOut ELSE c.Break1End END) ELSE 0 END
             ELSE 0 END,0)
             + ISNULL(CASE WHEN c.Break2Start IS NOT NULL AND c.Break2End IS NOT NULL THEN
                CASE WHEN DATEDIFF(MINUTE,CASE WHEN r.CheckIn>c.Break2Start THEN r.CheckIn ELSE c.Break2Start END,
                                      CASE WHEN r.CheckOut<c.Break2End THEN r.CheckOut ELSE c.Break2End END)>0
                     THEN DATEDIFF(MINUTE,CASE WHEN r.CheckIn>c.Break2Start THEN r.CheckIn ELSE c.Break2Start END,
                                      CASE WHEN r.CheckOut<c.Break2End THEN r.CheckOut ELSE c.Break2End END) ELSE 0 END
             ELSE 0 END,0)
             + ISNULL(CASE WHEN c.Break3Start IS NOT NULL AND c.Break3End IS NOT NULL THEN
                CASE WHEN DATEDIFF(MINUTE,CASE WHEN r.CheckIn>c.Break3Start THEN r.CheckIn ELSE c.Break3Start END,
                                      CASE WHEN r.CheckOut<c.Break3End THEN r.CheckOut ELSE c.Break3End END)>0
                     THEN DATEDIFF(MINUTE,CASE WHEN r.CheckIn>c.Break3Start THEN r.CheckIn ELSE c.Break3Start END,
                                      CASE WHEN r.CheckOut<c.Break3End THEN r.CheckOut ELSE c.Break3End END) ELSE 0 END
             ELSE 0 END,0)
           END
    FROM #Resolved r
    INNER JOIN #Candidates c ON c.EmployeeCode=r.EmployeeCode AND c.ShiftCode=r.ShiftCode;

    ;WITH ApprovedOT AS
    (
        SELECT oe.EmployeeCode,ot.StartTime,ot.EndTime,
               ROW_NUMBER() OVER(PARTITION BY oe.EmployeeCode ORDER BY ot.StartTime,ot.EndTime,ot.Id) rn
        FROM dbo.F03OTRequests ot
        INNER JOIN dbo.F03OTEmployees oe ON oe.OTRequestId=ot.Id AND oe.IsActive=1
        WHERE ot.IsActive=1 AND CAST(ot.OTDate AS date)=@WorkDate AND ot.RequestStatus=3
    ),
    OTWindow AS
    (
        SELECT EmployeeCode,MIN(StartTime) StartTime,MAX(EndTime) EndTime
        FROM ApprovedOT GROUP BY EmployeeCode
    )
    INSERT dbo.F03AttendanceStaging
    (WorkDate,EmployeeCode,DeptCode,DeptName,FullName,CheckInText,CheckOutText,CheckInDateTime,CheckOutDateTime,
     ShiftCode,ShiftName,ShiftAbbr,ShiftCategory,OtHours,TotalHours,IsHoliday,HolidayType,ShiftType,SyncedAt)
    SELECT CAST(@WorkDate AS datetime2(0)),r.EmployeeCode,e.DeptCode,d.DeptName,e.FullName,
           CASE WHEN r.CheckIn IS NULL THEN NULL ELSE CONVERT(varchar(5),CAST(r.CheckIn AS time(0)),108) END,
           CASE WHEN r.CheckOut IS NULL THEN NULL ELSE CONVERT(varchar(5),CAST(r.CheckOut AS time(0)),108) END,
           r.CheckIn,r.CheckOut,r.ShiftCode,r.ShiftName,r.ShiftAbbr,r.ShiftCategory,
           CAST(CASE WHEN ot.StartTime IS NULL OR r.CheckIn IS NULL OR r.CheckOut IS NULL THEN 0
                    WHEN r.CheckOut <= ot.StartTime OR r.CheckIn >= ot.EndTime THEN 0
                    ELSE DATEDIFF(MINUTE,
                           CASE WHEN r.CheckIn>ot.StartTime THEN r.CheckIn ELSE ot.StartTime END,
                           CASE WHEN r.CheckOut<ot.EndTime THEN r.CheckOut ELSE ot.EndTime END)/60.0 END AS decimal(5,2)),
           CAST(CASE WHEN r.CheckIn IS NULL OR r.CheckOut IS NULL THEN 0
                    ELSE CASE WHEN DATEDIFF(MINUTE,r.CheckIn,r.CheckOut)-r.BreakMinutes<0 THEN 0
                              ELSE (DATEDIFF(MINUTE,r.CheckIn,r.CheckOut)-r.BreakMinutes)/60.0 END END AS decimal(5,2)),
           CAST(CASE WHEN h.HolidayDate IS NULL THEN 0 ELSE 1 END AS bit),h.HolidayType,
           CASE WHEN r.CheckIn IS NULL THEN N'NO_CHECKIN'
                WHEN r.CheckOut IS NULL THEN N'NO_CHECKOUT'
                WHEN h.HolidayType=N'PUBLIC_HOLIDAY' THEN N'OT_HOLIDAY'
                WHEN h.HolidayType=N'SUNDAY' THEN N'OT_SUNDAY'
                WHEN h.HolidayType=N'SATURDAY' THEN N'OT_SATURDAY'
                ELSE N'ATTENDANCE' END,
           @Now
    FROM #Resolved r
    INNER JOIN #Employees e ON e.EmployeeCode=r.EmployeeCode
    LEFT JOIN dbo.F03Departments d ON d.DeptCode=e.DeptCode AND d.IsActive=1
    LEFT JOIN dbo.F03CompanyHolidays h ON h.HolidayDate=@WorkDate AND h.IsActive=1
    LEFT JOIN OTWindow ot ON ot.EmployeeCode=r.EmployeeCode;

    DECLARE @Count int=@@ROWCOUNT;

    /* Keep HRM.tblBaoCao as a read-only reference for regression comparison.
       FVN_REGISTER never writes to HRM.tblBaoCao. */
    DELETE FROM dbo.F03HrmShiftReference
    WHERE WorkDate=@WorkDate;

    INSERT dbo.F03HrmShiftReference
    (EmployeeCode,WorkDate,HrmScheduleCode,HrmShiftCode,HrmShiftAbbr,HrmCheckIn,HrmCheckOut,SourceUpdatedAt)
    SELECT
        RTRIM(nv.NVMaNV),
        @WorkDate,
        NULLIF(RTRIM(bc.BCLichTrinhCa),N''),
        CONVERT(nvarchar(20),bc.BCMaCa),
        sh.ShiftAbbr,
        bc.BCTGVao,
        bc.BCTGVe,
        NULL
    FROM HRM.dbo.tblBaoCao bc
    INNER JOIN HRM.dbo.tblNhanVien nv ON nv.NVMa=bc.BCMaNV
    LEFT JOIN dbo.F03Shifts sh ON sh.ShiftCode=CONVERT(nvarchar(20),bc.BCMaCa)
    WHERE bc.BCNgay=@WorkDate;

    SELECT @Count AS SyncedCount,CAST(@WorkDate AS date) AS WorkDate;
END;
GO

/*
  RequestStatus is an INT enum:
  Draft=0, Pending=1, InProgress=2, Approved=3,
  Rejected=4, Cancelled=5, Escalated=6, NeedsRevision=7.
*/
CREATE OR ALTER PROCEDURE dbo.usp_SyncOTActualHours
    @OTDate date=NULL,
    @DeptCode nvarchar(30)=NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @OTDate=COALESCE(@OTDate,CAST(GETDATE()-1 AS date));

    UPDATE emp
    SET emp.ActualStartTime=stg.CheckInDateTime,
        emp.ActualEndTime=stg.CheckOutDateTime,
        emp.ActualHours=CAST(ISNULL(stg.OTHours,0) AS decimal(5,2)),
        emp.ValidationStatus=
            CASE WHEN stg.CheckInDateTime IS NOT NULL AND stg.CheckOutDateTime IS NOT NULL THEN 1 ELSE 0 END,
        emp.ValidationMessage=
            CASE WHEN stg.CheckInDateTime IS NOT NULL AND stg.CheckOutDateTime IS NOT NULL
                 THEN NULL ELSE N'Chưa đủ dữ liệu CheckIn/CheckOut từ HRM.' END,
        emp.ModifiedAt=GETDATE(),
        emp.ModifiedBy=0
    FROM dbo.F03OTEmployees emp
    INNER JOIN dbo.F03OTRequests ot
      ON ot.Id=emp.OTRequestId
     AND ot.IsActive=1
     AND CAST(ot.OTDate AS date)=@OTDate
     AND ot.RequestStatus=3
     AND (@DeptCode IS NULL OR ot.DeptCode=@DeptCode)
    CROSS APPLY
    (
        SELECT TOP(1)
            s.Id,s.CheckInDateTime,s.CheckOutDateTime,s.OTHours
        FROM dbo.F03AttendanceStaging s
        WHERE s.EmployeeCode=emp.EmployeeCode
          AND s.WorkDate >= @OTDate
          AND s.WorkDate < DATEADD(day,1,@OTDate)
          AND ISNULL(s.OTHours,0)>0
        ORDER BY s.SyncedAt DESC,s.Id DESC
    ) stg
    WHERE emp.IsActive=1;

    SELECT
        CAST(stg.WorkDate AS datetime2(0)) AS WorkDate,
        COALESCE(stg.FullName,emp.EmployeeName,e.EmployeeName,N'') AS FullName,
        stg.EmployeeCode,
        CAST(COALESCE(ot.DeptCode,stg.DeptCode,N'') AS nvarchar(30)) AS DeptCode,
        CAST(COALESCE(d_ot.DeptName,stg.DeptName,N'') AS nvarchar(100)) AS DeptName,
        stg.CheckInText,stg.CheckOutText,
        CAST(ISNULL(stg.OTHours,0) AS decimal(5,2)) AS OTHoursActual,
        CAST(ISNULL(ot.PlannedHours,0) AS decimal(5,2)) AS OTHoursPlanned,
        CAST(ISNULL(ot.PlannedHours,0) AS decimal(5,2)) AS OTHoursRequest,
        CAST(ISNULL(stg.IsHoliday,0) AS bit) AS IsHoliday,
        CAST(ISNULL(stg.HolidayType,N'') AS nvarchar(50)) AS HolidayType,
        CAST(ISNULL(stg.ShiftType,N'') AS nvarchar(20)) AS ShiftType,
        CAST(ISNULL(stg.ShiftCode,N'') AS nvarchar(20)) AS ShiftCode,
        CAST(ISNULL(stg.ShiftName,N'') AS nvarchar(100)) AS ShiftName,
        CAST(emp.OTRequestId AS int) AS OTRequestId,
        CAST(ISNULL(ot.OTCode,N'') AS nvarchar(20)) AS OTCode,
        CAST(CASE WHEN ot.Id IS NULL THEN N'' ELSE CONVERT(nvarchar(20),ot.RequestStatus) END AS nvarchar(20)) AS RequestStatus,
        CAST(CASE
            WHEN emp.OTRequestId IS NULL THEN N'Warning'
            WHEN ot.RequestStatus<>3 THEN N'Warning'
            WHEN ABS(ISNULL(stg.OTHours,0)-ISNULL(ot.PlannedHours,0))>1.0 THEN N'Warning'
            ELSE N'Valid' END AS nvarchar(20)) AS ValidationStatus,
        CAST(CASE
            WHEN emp.OTRequestId IS NULL THEN N'Chưa có đơn OT'
            WHEN ot.RequestStatus<>3 THEN N'Đơn chưa được duyệt'
            WHEN ABS(ISNULL(stg.OTHours,0)-ISNULL(ot.PlannedHours,0))>1.0
                THEN N'Giờ OT thực tế lệch > 1h so với kế hoạch'
            ELSE N'OK' END AS nvarchar(200)) AS ValidationMessage,
        CAST(CASE
            WHEN emp.OTRequestId IS NULL THEN N'CHUA_CO_DON'
            WHEN ot.RequestStatus<>3 THEN N'DON_CHUA_DUYET'
            WHEN emp.ActualHours IS NULL THEN N'CHUA_CONFIRM'
            ELSE N'DA_XU_LY' END AS nvarchar(20)) AS TinhHuong
    FROM dbo.F03AttendanceStaging stg
    OUTER APPLY
    (
        SELECT TOP(1) r.Id,r.DeptCode,r.PlannedHours,r.OTCode,r.RequestStatus
        FROM dbo.F03OTRequests r
        INNER JOIN dbo.F03OTEmployees oe
          ON oe.OTRequestId=r.Id
         AND oe.EmployeeCode=stg.EmployeeCode
         AND oe.IsActive=1
        WHERE r.IsActive=1
          AND CAST(r.OTDate AS date)=@OTDate
          AND (@DeptCode IS NULL OR r.DeptCode=@DeptCode)
        ORDER BY CASE WHEN r.RequestStatus=3 THEN 0 ELSE 1 END,
                 r.CreatedAt DESC,r.Id DESC
    ) ot
    OUTER APPLY
    (
        SELECT TOP(1) oe.OTRequestId,oe.EmployeeName,oe.ActualHours
        FROM dbo.F03OTEmployees oe
        WHERE oe.OTRequestId=ot.Id
          AND oe.EmployeeCode=stg.EmployeeCode
          AND oe.IsActive=1
        ORDER BY oe.Id DESC
    ) emp
    LEFT JOIN dbo.F03Employees e ON e.EmployeeCode=stg.EmployeeCode
    LEFT JOIN dbo.F03Departments d_ot ON d_ot.DeptCode=ot.DeptCode AND d_ot.IsActive=1
    WHERE CAST(stg.WorkDate AS date)=@OTDate
      AND ISNULL(stg.OTHours,0)>0
    ORDER BY COALESCE(ot.DeptCode,stg.DeptCode),stg.FullName,stg.EmployeeCode;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_DequeueEmail @BatchSize int=20 AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON;
 ;WITH q AS
 (
   SELECT TOP(@BatchSize) *
   FROM dbo.F03EmailQueues WITH(UPDLOCK,READPAST,ROWLOCK)
   WHERE Status IN(N'Pending',N'Retry') AND RetryCount<MaxRetry
   ORDER BY CreatedAt,Id
 )
 UPDATE q SET Status=N'Processing',RetryCount=RetryCount+1 OUTPUT inserted.*;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_ProcessApprovalEscalation @Now datetime2(0)=NULL AS
BEGIN
 SET NOCOUNT ON;
 SET @Now=COALESCE(@Now,GETDATE());
 SELECT s.Id,s.RequestType,s.RequestId,s.Level,s.ApproverCode,s.ApproverEmail,r.EscalateHours
 FROM dbo.F03ApprovalSteps s
 JOIN dbo.F03EscalationRules r
   ON r.RequestModule=s.RequestType AND r.Level=s.Level AND r.IsActive=1
 WHERE s.Required=1 AND s.Approved IS NULL
   AND DATEDIFF(minute,s.CreatedAt,@Now)>=r.EscalateHours*60;
END;
GO
/*
================================================================================
PIPELINE A — HRM MASTER DATA SOURCE CONTRACTS
Source: [HRM].[dbo]
Target: F03Staging* -> HrmSyncJob -> F03*
The procedures below are READ-ONLY against HRM. They expose stable aliases
consumed by Dapper source-row DTOs. Soft/deleted HRM rows (DLocked=1) are not
returned; the importer detects missing keys and creates Delete staging records.
================================================================================
*/
CREATE OR ALTER PROCEDURE dbo.usp_SyncHrmLeaveTypeSource
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        LeaveTypeCode = LTRIM(RTRIM(LN.LNMa)),
        LeaveTypeName = LN.LNTen,
        LeaveTypeName2 = NULLIF(LTRIM(RTRIM(LN.LNViettat)), N''),
        TinhPhep = CAST(ISNULL(LN.LNTinhDKNgayNghi, 0) AS bit),
        HRMCode = LTRIM(RTRIM(LN.LNMa))
    FROM HRM.dbo.tblLoaiNghi AS LN
    WHERE ISNULL(LN.DLocked, 0) = 0
      AND NULLIF(LTRIM(RTRIM(LN.LNMa)), N'') IS NOT NULL
    ORDER BY LN.LNUuTien, LN.LNMa;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_SyncHrmDepartmentSource
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        DeptCode = CONVERT(nvarchar(20), BP.BPMa),
        DeptName = BP.BPTen,
        ParentDeptCode =
            CASE WHEN ISNULL(BP.BPMaCha, 0) = 0 THEN NULL
                 ELSE CONVERT(nvarchar(50), BP.BPMaCha) END,
        DisplayPriority =
            CASE WHEN BP.BPUuTien IS NULL THEN NULL
                 WHEN BP.BPUuTien > 2147483647 OR BP.BPUuTien < -2147483648 THEN NULL
                 ELSE CONVERT(int, BP.BPUuTien) END,
        ShowInReport = CAST(ISNULL(BP.BPHienThiBC, 1) AS bit)
    FROM HRM.dbo.tblBoPhan AS BP
    WHERE ISNULL(BP.DLocked, 0) = 0
    ORDER BY BP.BPUuTien, BP.BPMa;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_SyncHrmPositionSource
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        PositionCode = LTRIM(RTRIM(CV.CVMa)),
        PositionName = CV.CVTen
    FROM HRM.dbo.tblChucVu AS CV
    WHERE ISNULL(CV.DLocked, 0) = 0
      AND NULLIF(LTRIM(RTRIM(CV.CVMa)), N'') IS NOT NULL
    ORDER BY CV.CVMa;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_SyncHrmEmployeeSource
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        EmployeeCode = LTRIM(RTRIM(NV.NVMaNV)),
        EmployeeName = COALESCE(NULLIF(LTRIM(RTRIM(NV.NVHoTen)), N''), LTRIM(RTRIM(NV.NVMaNV))),
        DeptCode =
            CASE WHEN ISNULL(NV.NVMaBP, 0) = 0 THEN NULL
                 ELSE CONVERT(nvarchar(20), NV.NVMaBP) END,
        PositionCode = NULLIF(LEFT(LTRIM(RTRIM(NV.NVMaCV)), 20), N''),
        BirthDate = NV.NVNgaySinh,
        GenderCode = CONVERT(int, NV.NVGioiTinh),
        EmailAddress = ISNULL(NV.NVEmail, N''),
        PhoneNumber = NULLIF(LTRIM(RTRIM(NV.NVDienThoai)), N''),
        FirstWorkingDate = NV.NVNgayVao,
        EndWorkingDate =
            CASE
                WHEN NV.NVNgayRa IS NULL OR NV.NVNgayRa >= '9990-01-01'
                    THEN NULL
                ELSE NV.NVNgayRa
            END,
        TotalLeaveDays = CONVERT(decimal(5,2), NV.NVSoNgayPhep),
        EmployeeNo = NV.NVMa
    FROM HRM.dbo.tblNhanVien AS NV
    WHERE ISNULL(NV.DLocked, 0) = 0
      AND NULLIF(LTRIM(RTRIM(NV.NVMaNV)), N'') IS NOT NULL
    ORDER BY NV.NVMaNV;
END;
GO
