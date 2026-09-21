USE [FVN_REGISTER];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.F03PasswordResetRequests', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03PasswordResetRequests
    (
        Id int IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_F03PasswordResetRequests PRIMARY KEY,
        EmployeeCode nvarchar(50) NOT NULL,
        FullName nvarchar(255) NULL,
        DeptCode nvarchar(50) NULL,
        RequestNote nvarchar(1000) NULL,
        Status nvarchar(20) NOT NULL
            CONSTRAINT DF_F03PasswordResetRequests_Status DEFAULT(N'Pending'),
        RequestedAt datetime2(0) NOT NULL
            CONSTRAINT DF_F03PasswordResetRequests_RequestedAt DEFAULT(GETDATE()),
        ProcessedAt datetime2(0) NULL,
        ProcessedBy int NULL,
        ProcessorName nvarchar(255) NULL,
        ResultNote nvarchar(1000) NULL,
        IpAddress nvarchar(50) NULL,
        UserAgent nvarchar(255) NULL,
        CreatedAt datetime2(0) NULL,
        CreatedBy int NULL,
        ModifiedAt datetime2(0) NULL,
        ModifiedBy int NULL,
        LastModifiedSource nvarchar(50) NULL
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name=N'UX_F03PasswordResetRequests_Pending'
      AND object_id=OBJECT_ID(N'dbo.F03PasswordResetRequests')
)
BEGIN
    CREATE UNIQUE INDEX UX_F03PasswordResetRequests_Pending
        ON dbo.F03PasswordResetRequests(EmployeeCode)
        WHERE Status=N'Pending';
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name=N'IX_F03PasswordResetRequests_Status_RequestedAt'
      AND object_id=OBJECT_ID(N'dbo.F03PasswordResetRequests')
)
BEGIN
    CREATE INDEX IX_F03PasswordResetRequests_Status_RequestedAt
        ON dbo.F03PasswordResetRequests(Status, RequestedAt);
END;
GO

PRINT N'F03PasswordResetRequests is ready.';
GO
