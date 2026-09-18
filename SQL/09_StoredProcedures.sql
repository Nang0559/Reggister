USE [FVN_REGISTER];
GO
CREATE OR ALTER PROCEDURE dbo.usp_GetPendingApproval @ApproverCode nvarchar(50)=NULL,@ApproverEmail nvarchar(100)=NULL AS
BEGIN
 SET NOCOUNT ON;
 SELECT RequestType,RequestId,Level,RoleName,ApproverCode,ApproverName,ApproverEmail,Required,Approved,ApprovedAt,Comment,ReminderSent
 FROM dbo.F03ApprovalSteps WHERE Required=1 AND Approved IS NULL
 AND (@ApproverCode IS NULL OR ApproverCode=@ApproverCode) AND (@ApproverEmail IS NULL OR ApproverEmail=@ApproverEmail)
 ORDER BY Level,CreatedAt;
END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_DecideApproval
 @StepId int,@Decision int,@Comment nvarchar(500)=NULL,@ApproverCode nvarchar(50)=NULL,@OverriddenByCode nvarchar(50)=NULL,@OverriddenByName nvarchar(100)=NULL
AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON;
 BEGIN TRAN;
 DECLARE @RequestType nvarchar(20),@RequestId int,@ApproverName nvarchar(100);
 SELECT @RequestType=RequestType,@RequestId=RequestId,@ApproverName=ApproverName FROM dbo.F03ApprovalSteps WITH(UPDLOCK,HOLDLOCK) WHERE Id=@StepId;
 IF @RequestType IS NULL BEGIN ROLLBACK; THROW 50001,'Approval step not found',1; END;
 UPDATE dbo.F03ApprovalSteps SET Approved=CASE WHEN @Decision=1 THEN 1 WHEN @Decision=2 THEN 0 ELSE Approved END,
 ApprovedAt=CASE WHEN @Decision IN(1,2) THEN GETDATE() ELSE ApprovedAt END,Comment=@Comment,
 IsOverriddenByAdmin=CASE WHEN @OverriddenByCode IS NULL THEN IsOverriddenByAdmin ELSE 1 END,
 OverriddenByCode=COALESCE(@OverriddenByCode,OverriddenByCode),OverriddenByName=COALESCE(@OverriddenByName,OverriddenByName),
 OverriddenAt=CASE WHEN @OverriddenByCode IS NULL THEN OverriddenAt ELSE GETDATE() END WHERE Id=@StepId;
 INSERT dbo.ApprovalHistories(RequestType,RequestId,StepId,IsOverriddenByAdmin,ApproverCode,ApproverName,OverriddenByCode,OverriddenByName,OverriddenAt,Decision,Comment,ActionAt,CreatedBy)
 VALUES(CASE @RequestType WHEN N'Leave' THEN 0 WHEN N'Overtime' THEN 1 WHEN N'Trip' THEN 2 WHEN N'Equipment' THEN 3 END,@RequestId,@StepId,
 CASE WHEN @OverriddenByCode IS NULL THEN 0 ELSE 1 END,COALESCE(@ApproverCode,''),COALESCE(@ApproverName,''),@OverriddenByCode,@OverriddenByName,
 CASE WHEN @OverriddenByCode IS NULL THEN NULL ELSE GETDATE() END,@Decision,@Comment,GETDATE(),0);
 COMMIT;
END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_SyncAttendanceStaging @WorkDate date=NULL AS
BEGIN
 SET NOCOUNT ON;
 SELECT COUNT(*) AS StagingRows,
 SUM(CASE WHEN @WorkDate IS NULL OR CAST(WorkDate AS date)=@WorkDate THEN 1 ELSE 0 END) AS MatchingRows
 FROM dbo.F03AttendanceStaging;
END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_SyncOTActualHours @WorkDate date=NULL AS
BEGIN
 SET NOCOUNT ON;
 UPDATE e SET ActualStartTime=a.CheckInDateTime,ActualEndTime=a.CheckOutDateTime,
 ActualHours=CASE WHEN a.CheckInDateTime IS NOT NULL AND a.CheckOutDateTime IS NOT NULL THEN CAST(DATEDIFF(minute,a.CheckInDateTime,a.CheckOutDateTime)/60.0 AS decimal(5,2)) END,
 ValidationStatus=CASE WHEN a.CheckInDateTime IS NOT NULL AND a.CheckOutDateTime IS NOT NULL THEN 1 ELSE 0 END
 FROM dbo.F03OTEmployees e JOIN dbo.F03OTRequests r ON r.Id=e.OTRequestId
 JOIN dbo.F03AttendanceStaging a ON a.EmployeeCode=e.EmployeeCode AND CAST(a.WorkDate AS date)=CAST(r.OTDate AS date)
 WHERE @WorkDate IS NULL OR CAST(r.OTDate AS date)=@WorkDate;
 SELECT @@ROWCOUNT AS UpdatedRows;
END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_DequeueEmail @BatchSize int=20 AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON;
 ;WITH q AS(SELECT TOP(@BatchSize) * FROM dbo.F03EmailQueues WITH(UPDLOCK,READPAST,ROWLOCK) WHERE Status IN(N'Pending',N'Retry') AND RetryCount<MaxRetry ORDER BY CreatedAt,Id)
 UPDATE q SET Status=N'Processing',RetryCount=RetryCount+1 OUTPUT inserted.*;
END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_ProcessApprovalEscalation @Now datetime2(0)=NULL AS
BEGIN
 SET NOCOUNT ON; SET @Now=COALESCE(@Now,GETDATE());
 SELECT s.Id,s.RequestType,s.RequestId,s.Level,s.ApproverCode,s.ApproverEmail,r.EscalateHours
 FROM dbo.F03ApprovalSteps s JOIN dbo.F03EscalationRules r ON r.RequestModule=s.RequestType AND r.Level=s.Level AND r.IsActive=1
 WHERE s.Required=1 AND s.Approved IS NULL AND DATEDIFF(minute,s.CreatedAt,@Now)>=r.EscalateHours*60;
END;
GO
