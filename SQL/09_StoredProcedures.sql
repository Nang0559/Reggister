USE [FVN_REGISTER];
GO
CREATE OR ALTER PROCEDURE leave.usp_GetPendingApproval
    @ApproverCode nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM leave.vw_PendingApproval
    WHERE ApproverCode=@ApproverCode
    ORDER BY SubmittedAt ASC,RequestId ASC;
END
GO

CREATE OR ALTER PROCEDURE leave.usp_DecideApproval
    @LeaveRequestId int,
    @LevelNo int,
    @ApproverCode nvarchar(50),
    @Decision tinyint,
    @Comment nvarchar(1000)=NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRAN;
    UPDATE leave.Approval
       SET Decision=@Decision,ApproveTime=SYSDATETIME(),Comment=@Comment
     WHERE LeaveRequestId=@LeaveRequestId AND LevelNo=@LevelNo AND ApproverCode=@ApproverCode AND Decision=0;
    IF @@ROWCOUNT=0 BEGIN ROLLBACK; THROW 50001,'Approval step is not available.',1; END;

    IF @Decision=2
        UPDATE leave.LeaveRequest SET Status=3,UpdatedAt=SYSDATETIME() WHERE Id=@LeaveRequestId;
    ELSE IF @Decision=1 AND NOT EXISTS(
        SELECT 1 FROM leave.Approval WHERE LeaveRequestId=@LeaveRequestId AND IsRequired=1 AND Decision<>1
    )
        UPDATE leave.LeaveRequest SET Status=2,UpdatedAt=SYSDATETIME() WHERE Id=@LeaveRequestId;
    ELSE IF @Decision=1
        UPDATE leave.LeaveRequest SET Status=1,UpdatedAt=SYSDATETIME() WHERE Id=@LeaveRequestId;
    COMMIT;
END
GO

CREATE OR ALTER PROCEDURE notify.usp_DequeueEmail
    @BatchSize int=50
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH q AS (
        SELECT TOP(@BatchSize) * FROM notify.EmailQueue WITH(UPDLOCK,READPAST,ROWLOCK)
        WHERE Status=0 ORDER BY Id
    )
    UPDATE q SET Status=1
    OUTPUT inserted.*;
END
GO
