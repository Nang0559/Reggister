CREATE OR ALTER PROCEDURE dbo.usp_CalculateHrmAttendance
 @DeptCode nvarchar(20)=NULL,@FromDate date,@ToDate date,@TriggeredBy nvarchar(100)=NULL,@CalculationVersion nvarchar(50)=N'HRM-PORT-1.0'
AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON;
 IF @FromDate IS NULL OR @ToDate IS NULL OR @FromDate>@ToDate THROW 51320,N'Khoảng ngày không hợp lệ.',1;
 SET @TriggeredBy = COALESCE(NULLIF(LTRIM(RTRIM(@TriggeredBy)),N''),N'SYSTEM');
 -- Single orchestration entry point for BOTH execution modes:
 --   1) background: @DeptCode = NULL => whole company
 --   2) manual:     @DeptCode + date range => selected department/date
 -- The per-staff calculation remains dbo.usp_HrmCompatibleTimeKeepingForStaff.
 DECLARE @BatchId uniqueidentifier=NEWID(),@HrmDeptId int=TRY_CONVERT(int,NULLIF(@DeptCode,N''));
 CREATE TABLE #Result(
  BCNgay datetime NOT NULL,BCMaNV int NOT NULL,BCMaBP int NOT NULL,BCMaCV int NOT NULL,BCMaCa int NOT NULL,
  BCCuaDen int NULL,BCTGDen datetime NULL,BCCuaVe int NULL,BCTGVe datetime NULL,BCCuaRa int NULL,BCTGRa datetime NULL,BCCuaVao int NULL,BCTGVao datetime NULL,
  BCTGLamNgay int NULL,BCTGLamToi int NULL,BCTGQuaGioNgay int NULL,BCTGQuaGioToi int NULL,BCTGQuaGioNgayTC int NULL,BCTGQuaGioToiTC int NULL,
  BCTGThemNgay int NULL,BCTGThemToi int NULL,BCTGRaNgoaiNgay int NULL,BCTGRaNgoaiToi int NULL,BCTGDiMuonNgay int NULL,BCTGDiMuonToi int NULL,
  BCTGVeSomNgay int NULL,BCTGVeSomToi int NULL,BCTGQuyDinh int NULL,BCGhiChu nvarchar(50) NULL,BCLoai bit NULL,BCLoaiLamThem bit NULL,BCTinhLamThem bit NULL,
  BCNghiBuChoNgay float NULL,BCNghiPhep float NULL,BCNghiH100 float NULL,BCNghiH70 float NULL,BCNghiKL float NULL,BCNghiBH100 float NULL,BCNghiBH70 float NULL,
  BCNghiCongTac float NULL,BCNghiBu float NULL,BCNghiKhac float NULL,BCLoaiNgayNghi smallint NULL,BCLydonghi nvarchar(20) NULL,BCTGNghi float NULL,
  BCTGDKNTheogio float NULL,BCLoaiDKN varchar(10) NULL,BCDangKyLTN float NULL,BCDangKyLTD float NULL,BCTGUuDaiN int NULL,BCTGUuDaiD int NULL,
  BCNgayLe int NULL,BCNgayLeNV int NULL,BCDaXacNhanLamThem bit NULL,DLocked bit NULL
 );
 DECLARE @D date=@FromDate;
 WHILE @D<=@ToDate
 BEGIN
  DECLARE @StaffID int;
  DECLARE staff_cur CURSOR LOCAL FAST_FORWARD FOR
   SELECT NVMa FROM HRM.dbo.tblNhanVien WHERE ISNULL(DLocked,0)=0 AND (@HrmDeptId IS NULL OR NVMaBP=@HrmDeptId);
  OPEN staff_cur; FETCH NEXT FROM staff_cur INTO @StaffID;
  WHILE @@FETCH_STATUS=0
  BEGIN
   DELETE FROM #Result;
   INSERT #Result EXEC dbo.usp_HrmCompatibleTimeKeepingForStaff @StaffID=@StaffID,@D=@D;
   INSERT dbo.F03HrmAttendanceCalculated(
    CalculationBatchId,CalculationVersion,WorkDate,HrmEmployeeId,EmployeeCode,FullName,HrmDeptId,DeptCode,HrmPositionId,ShiftId,ShiftAbbr,
    CheckInGate,CheckInTime,CheckOutGate,CheckOutTime,ExitGate,ExitTime,EntryGate,EntryTime,WorkMinutesDay,WorkMinutesNight,
    OTMinutesDay,OTMinutesNight,OTMinutesDayTC,OTMinutesNightTC,OTRecognizedMinutesDay,OTRecognizedMinutesNight,LateMinutesDay,LateMinutesNight,
    EarlyLeaveMinutesDay,EarlyLeaveMinutesNight,RequiredMinutes,LeaveTotal,LeaveAnnual,Leave100,Leave70,LeaveUnpaid,LeaveBH100,LeaveBH70,LeaveBusinessTrip,
    LeaveCompensatory,LeaveOther,LeaveTypeCode,LeaveReason,Note,HrmType,HrmHoliday,HrmEmployeeHoliday,IsLocked,
    HrmBCGhiChu,HrmBCLyDoNghi,HrmBCNghiTotal,HrmBCNghiPhep,HrmBCNghiH100,HrmBCNghiH70,HrmBCNghiKL,HrmBCNghiBH100,HrmBCNghiBH70,HrmBCNghiCongTac,HrmBCNghiBu,HrmBCNghiKhac,
    HrmBCDaXacNhanLamThem,HrmBCLoaiLamThem,HrmBCTinhLamThem,HrmBCNgayLe,HrmBCNgayLeNV,HrmShiftDayType,AttendanceDisplayValue,OtDisplayValue,CalculatedAt,CalculatedBy)
   SELECT @BatchId,@CalculationVersion,CAST(r.BCNgay AS date),r.BCMaNV,RTRIM(nv.NVMaNV),RTRIM(nv.NVHoTen),r.BCMaBP,CONVERT(nvarchar(20),r.BCMaBP),r.BCMaCV,r.BCMaCa,ca.CVietTat,
    r.BCCuaDen,CASE WHEN r.BCTGDen <= '19000101' THEN NULL ELSE r.BCTGDen END,r.BCCuaVe,CASE WHEN r.BCTGVe <= '19000101' THEN NULL ELSE r.BCTGVe END,r.BCCuaRa,CASE WHEN r.BCTGRa <= '19000101' THEN NULL ELSE r.BCTGRa END,r.BCCuaVao,CASE WHEN r.BCTGVao <= '19000101' THEN NULL ELSE r.BCTGVao END,ISNULL(r.BCTGLamNgay,0),ISNULL(r.BCTGLamToi,0),
    ISNULL(r.BCTGQuaGioNgay,0),ISNULL(r.BCTGQuaGioToi,0),ISNULL(r.BCTGQuaGioNgayTC,0),ISNULL(r.BCTGQuaGioToiTC,0),ISNULL(r.BCTGThemNgay,0),ISNULL(r.BCTGThemToi,0),
    ISNULL(r.BCTGDiMuonNgay,0),ISNULL(r.BCTGDiMuonToi,0),ISNULL(r.BCTGVeSomNgay,0),ISNULL(r.BCTGVeSomToi,0),ISNULL(r.BCTGQuyDinh,0),
    r.BCNghiPhep+r.BCNghiH100+r.BCNghiH70+r.BCNghiKL+r.BCNghiBH100+r.BCNghiBH70+r.BCNghiCongTac+r.BCNghiBu+r.BCNghiKhac,
    r.BCNghiPhep,r.BCNghiH100,r.BCNghiH70,r.BCNghiKL,r.BCNghiBH100,r.BCNghiBH70,r.BCNghiCongTac,r.BCNghiBu,r.BCNghiKhac,
    CONVERT(nvarchar(20),r.BCLoaiNgayNghi),r.BCLydonghi,r.BCGhiChu,r.BCLoai,CASE WHEN ISNULL(r.BCNgayLe,0)<>0 THEN 1 ELSE 0 END,CASE WHEN ISNULL(r.BCNgayLeNV,0)<>0 THEN 1 ELSE 0 END,r.DLocked,
    r.BCGhiChu,r.BCLydonghi,ISNULL(r.BCTGNghi,0),r.BCNghiPhep,r.BCNghiH100,r.BCNghiH70,r.BCNghiKL,r.BCNghiBH100,r.BCNghiBH70,r.BCNghiCongTac,r.BCNghiBu,r.BCNghiKhac,
    r.BCDaXacNhanLamThem,r.BCLoaiLamThem,r.BCTinhLamThem,r.BCNgayLe,r.BCNgayLeNV,ca.CNgaynghi,NULL,NULL,GETDATE(),@TriggeredBy
   FROM #Result r INNER JOIN HRM.dbo.tblNhanVien nv ON nv.NVMa=r.BCMaNV
   LEFT JOIN HRM.dbo.tblCa ca ON CONVERT(nvarchar(20),ca.CMa)=CONVERT(nvarchar(20),r.BCMaCa)
   WHERE r.BCMaNV=@StaffID AND CAST(r.BCNgay AS date)=@D;
   UPDATE a SET AttendanceDisplayValue=CASE
       WHEN ISNULL(a.LeaveTotal,0)>0 THEN COALESCE(NULLIF(LTRIM(RTRIM(a.HrmBCLyDoNghi)),N''),NULLIF(LTRIM(RTRIM(a.HrmBCGhiChu)),N''),N'K')
       WHEN a.CheckInTime IS NULL AND a.CheckOutTime IS NULL THEN N''
       WHEN a.CheckInTime IS NULL OR a.CheckOutTime IS NULL THEN N'?'
       WHEN ISNULL(a.WorkMinutesDay,0)+ISNULL(a.WorkMinutesNight,0)<=0 THEN N''
       WHEN ISNULL(a.RequiredMinutes,0)>0 AND ISNULL(a.WorkMinutesDay,0)+ISNULL(a.WorkMinutesNight,0)=a.RequiredMinutes THEN NULLIF(LTRIM(RTRIM(a.ShiftAbbr)),N'')
       ELSE CONVERT(nvarchar(50),CONVERT(float,(ISNULL(a.WorkMinutesDay,0)+ISNULL(a.WorkMinutesNight,0))/60.0)) END,
       OtDisplayValue=CASE
       /*
          L/holiday classification must come from the HRM attendance result
          (BCNgayLe / BCNgayLeNV), not from tblCa.CNgaynghi (HrmHoliday).
          CNgaynghi describes the shift calendar and can mark a regular
          Saturday/day-off for everyone on that shift, which caused false
          "L" values in OT exports for employees whose HRM report did not
          classify that date as a holiday.
       */
       WHEN ISNULL(a.OTRecognizedMinutesDay,0)+ISNULL(a.OTRecognizedMinutesNight,0)<=0
            THEN CASE WHEN ISNULL(a.HrmBCNgayLe,0)<>0 OR ISNULL(a.HrmBCNgayLeNV,0)<>0 THEN N'L' ELSE N'' END
       WHEN ISNULL(a.HrmBCNgayLe,0)<>0 OR ISNULL(a.HrmBCNgayLeNV,0)<>0 THEN
            CASE WHEN ISNULL(a.OTRecognizedMinutesDay,0)+ISNULL(a.OTRecognizedMinutesNight,0)>=480
                 THEN N'NL'+COALESCE(NULLIF(LTRIM(RTRIM(a.ShiftAbbr)),N''),N'')+CONVERT(nvarchar(20),CONVERT(float,(ISNULL(a.OTRecognizedMinutesDay,0)+ISNULL(a.OTRecognizedMinutesNight,0))/60.0))
                 ELSE N'NL'+LEFT(COALESCE(NULLIF(LTRIM(RTRIM(a.ShiftAbbr)),N''),N'C'),1)+CONVERT(nvarchar(20),ISNULL(a.OTRecognizedMinutesDay,0)+ISNULL(a.OTRecognizedMinutesNight,0)) END
       WHEN ISNULL(a.HrmShiftDayType,0) IN (2,3) THEN
            N'CN'+CONVERT(nvarchar(20),CONVERT(float,(ISNULL(a.OTRecognizedMinutesDay,0)+ISNULL(a.OTRecognizedMinutesNight,0))/60.0))
       ELSE CONVERT(nvarchar(20),CONVERT(float,(ISNULL(a.OTRecognizedMinutesDay,0)+ISNULL(a.OTRecognizedMinutesNight,0))/60.0)) END
   FROM dbo.F03HrmAttendanceCalculated a WHERE a.CalculationBatchId=@BatchId AND a.WorkDate=@D AND a.HrmEmployeeId=@StaffID;
   FETCH NEXT FROM staff_cur INTO @StaffID;
  END
  CLOSE staff_cur; DEALLOCATE staff_cur; SET @D=DATEADD(day,1,@D);
 END
 INSERT dbo.F03HrmOTActual(CalculationBatchId,WorkDate,HrmEmployeeId,EmployeeCode,DeptCode,ActualStartTime,ActualEndTime,ActualMinutes,ActualOTDayMinutes,ActualOTNightMinutes,RecognizedOTMinutes,SourceAttendanceId)
 SELECT CalculationBatchId,WorkDate,HrmEmployeeId,EmployeeCode,DeptCode,CheckInTime,CheckOutTime,CASE WHEN CheckInTime IS NOT NULL AND CheckOutTime IS NOT NULL THEN DATEDIFF(minute,CheckInTime,CheckOutTime) ELSE 0 END,OTMinutesDay+OTMinutesDayTC,OTMinutesNight+OTMinutesNightTC,OTRecognizedMinutesDay+OTRecognizedMinutesNight,Id
 FROM dbo.F03HrmAttendanceCalculated WHERE CalculationBatchId=@BatchId AND (CheckInTime IS NOT NULL OR CheckOutTime IS NOT NULL);

 /* Centralized OT actual synchronization: HRM calculation owns this write. */
 ;WITH LatestActual AS
 (
  SELECT h.*, ROW_NUMBER() OVER(PARTITION BY h.HrmEmployeeId,h.WorkDate ORDER BY h.CalculatedAt DESC,h.Id DESC) rn
  FROM dbo.F03HrmOTActual h
  WHERE h.CalculationBatchId=@BatchId
 )
 UPDATE emp
 SET emp.ActualStartTime = a.ActualStartTime,
     emp.ActualEndTime = a.ActualEndTime,
     emp.ActualHours = CAST((ISNULL(a.ActualOTDayMinutes,0) + ISNULL(a.ActualOTNightMinutes,0)) / 60.0 AS decimal(5,2)),
     emp.ValidationStatus = CASE WHEN a.ActualStartTime IS NOT NULL AND a.ActualEndTime IS NOT NULL THEN 1 ELSE 0 END,
     emp.ValidationMessage = CASE WHEN a.ActualStartTime IS NOT NULL AND a.ActualEndTime IS NOT NULL THEN NULL ELSE N'Chưa đủ dữ liệu CheckIn/CheckOut từ HRM-compatible calculation.' END,
     emp.ModifiedAt=GETDATE(),
     emp.ModifiedBy=0
 FROM dbo.F03OTEmployees emp
 INNER JOIN dbo.F03OTRequests ot ON ot.Id=emp.OTRequestId AND ot.IsActive=1 AND ot.RequestStatus=3
 INNER JOIN HRM.dbo.tblNhanVien nv ON RTRIM(nv.NVMaNV)=emp.EmployeeCode
 INNER JOIN LatestActual a ON a.HrmEmployeeId=nv.NVMa AND a.WorkDate=CAST(ot.OTDate AS date) AND a.rn=1
 WHERE emp.IsActive=1 AND a.CalculationBatchId=@BatchId;
 SELECT @BatchId AS CalculationBatchId,@DeptCode AS DeptCode,@FromDate AS FromDate,@ToDate AS ToDate,COUNT(DISTINCT HrmEmployeeId) AS EmployeeCount,COUNT(*) AS CalculatedRows,MIN(CalculatedAt) AS StartedAt,MAX(CalculatedAt) AS FinishedAt,@CalculationVersion AS CalculationVersion
 FROM dbo.F03HrmAttendanceCalculated WHERE CalculationBatchId=@BatchId;
END;
GO
