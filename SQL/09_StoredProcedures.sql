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
  Pipeline B — HRM attendance -> local staging -> OT reconciliation.

  NOTE:
  HRM.dbo.fn_OTActualCheckInOut is an external dependency. The returned
  column names must remain compatible with this SELECT.
*/
CREATE OR ALTER PROCEDURE dbo.usp_SyncAttendanceStaging
    @WorkDate date
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DELETE FROM dbo.F03AttendanceStaging
    WHERE WorkDate >= @WorkDate
      AND WorkDate < DATEADD(day,1,@WorkDate);

    INSERT dbo.F03AttendanceStaging
    (WorkDate,EmployeeCode,DeptCode,DeptName,FullName,CheckInText,CheckOutText,
     CheckInDateTime,CheckOutDateTime,ShiftCode,ShiftName,ShiftAbbr,ShiftCategory,
     OtHours,TotalHours,IsHoliday,HolidayType,ShiftType,SyncedAt)
    SELECT
        CAST(cc.WorkDate AS datetime2(0)),
        cc.EmployeeCode,
        cc.DeptCode,
        d.DeptName,
        cc.FullName,
        cc.CheckInText,
        cc.CheckOutText,
        cc.CheckInDateTime,
        cc.CheckOutDateTime,
        cc.ShiftCode,
        cc.ShiftName,
        cc.ShiftAbbr,
        TRY_CONVERT(int,cc.CLoaiCa),
        CAST(ISNULL(cc.OTHours,0) AS decimal(5,2)),
        CAST(ISNULL(cc.TotalHours,0) AS decimal(5,2)),
        CAST(ISNULL(cc.IsHoliday,0) AS bit),
        cc.HolidayType,
        cc.ShiftType,
        GETDATE()
    FROM HRM.dbo.fn_OTActualCheckInOut(@WorkDate) cc
    LEFT JOIN dbo.F03Departments d
      ON d.DeptCode=cc.DeptCode AND d.IsActive=1
    WHERE cc.CheckOutDateTime IS NOT NULL;

    DECLARE @Count int=@@ROWCOUNT;

    SELECT @Count AS SyncedCount, CAST(@WorkDate AS date) AS WorkDate;
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
    INNER JOIN dbo.F03AttendanceStaging stg
      ON stg.EmployeeCode=emp.EmployeeCode
     AND CAST(stg.WorkDate AS date)=@OTDate
    WHERE emp.IsActive=1
      AND ISNULL(stg.OTHours,0)>0;

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