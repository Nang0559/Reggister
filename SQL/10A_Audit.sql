USE [FVN_REGISTER];
GO
CREATE OR ALTER PROCEDURE dbo.usp_WriteAuditLog
 @UserId int=NULL,@UserName nvarchar(100)=NULL,@Action nvarchar(100),@Description nvarchar(2000)=NULL,@IpAddress nvarchar(50)=NULL,@UserAgent nvarchar(255)=NULL
AS
BEGIN
 SET NOCOUNT ON;
 INSERT dbo.F03AuditLogs(UserId,UserName,Action,Description,IpAddress,UserAgent,CreatedBy)
 VALUES(@UserId,@UserName,@Action,@Description,@IpAddress,@UserAgent,COALESCE(@UserId,0));
END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_WriteUserLog
 @UserId int,@LastSeen nvarchar(255),@LastSeenUrl nvarchar(500),@ApplicationName nvarchar(100),@ApplicationVersion nvarchar(20),@WorkstationName nvarchar(100),@WorkstationUser nvarchar(100)
AS
BEGIN
 SET NOCOUNT ON;
 INSERT dbo.F03UserLogs(UserId,LastSeen,LastSeenUrl,ApplicationName,ApplicationVersion,WorkstationName,WorkstationUser)
 VALUES(@UserId,@LastSeen,@LastSeenUrl,@ApplicationName,@ApplicationVersion,@WorkstationName,@WorkstationUser);
END;
GO
