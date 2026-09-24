USE [FVN_REGISTER];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.F03AccessChangeRequests', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03AccessChangeRequests
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03AccessChangeRequests PRIMARY KEY,
        IsActive bit NULL CONSTRAINT DF_F03AccessChangeRequests_IsActive DEFAULT(1),
        CreatedBy int NOT NULL CONSTRAINT DF_F03AccessChangeRequests_CreatedBy DEFAULT(0),
        LastModifiedSource nvarchar(200) NULL,
        CreatedAt datetime2 NOT NULL CONSTRAINT DF_F03AccessChangeRequests_CreatedAt DEFAULT(SYSDATETIME()),
        ModifiedBy int NULL,
        ModifiedAt datetime2 NULL,
        BusinessModule int NOT NULL,
        RequesterEmployeeCode nvarchar(50) NOT NULL,
        OldEmployeeCode nvarchar(50) NOT NULL,
        NewEmployeeCode nvarchar(50) NOT NULL,
        DeptCode nvarchar(20) NOT NULL,
        NewPositionCode nvarchar(20) NULL,
        Reason nvarchar(500) NOT NULL,
        RequestedFunctionCodesJson nvarchar(max) NOT NULL CONSTRAINT DF_F03AccessChangeRequests_Functions DEFAULT(N'[]'),
        EquipmentAssetIdsJson nvarchar(max) NOT NULL CONSTRAINT DF_F03AccessChangeRequests_Assets DEFAULT(N'[]'),
        OldEquipmentResponsibleCode nvarchar(50) NULL,
        NewEquipmentResponsibleCode nvarchar(50) NULL,
        OldEquipmentApproverCode nvarchar(50) NULL,
        NewEquipmentApproverCode nvarchar(50) NULL,
        Status int NOT NULL CONSTRAINT DF_F03AccessChangeRequests_Status DEFAULT(0),
        SubmittedAt datetime2 NULL,
        ITCompletedAt datetime2 NULL,
        ITCompletedByUserId int NULL,
        ITNote nvarchar(1000) NULL,
        PreChangeSnapshotJson nvarchar(max) NULL,
        PostChangeResultJson nvarchar(max) NULL
    );
END;
GO

IF COL_LENGTH(N'dbo.F03AccessChangeRequests', N'PreChangeSnapshotJson') IS NULL
    ALTER TABLE dbo.F03AccessChangeRequests ADD PreChangeSnapshotJson nvarchar(max) NULL;
IF COL_LENGTH(N'dbo.F03AccessChangeRequests', N'PostChangeResultJson') IS NULL
    ALTER TABLE dbo.F03AccessChangeRequests ADD PostChangeResultJson nvarchar(max) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03AccessChangeRequests_Status' AND object_id=OBJECT_ID(N'dbo.F03AccessChangeRequests'))
    CREATE INDEX IX_F03AccessChangeRequests_Status ON dbo.F03AccessChangeRequests(Status,BusinessModule,SubmittedAt);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03AccessChangeRequests_Requester' AND object_id=OBJECT_ID(N'dbo.F03AccessChangeRequests'))
    CREATE INDEX IX_F03AccessChangeRequests_Requester ON dbo.F03AccessChangeRequests(RequesterEmployeeCode,CreatedAt DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03AccessChangeRequests_OldEmployee' AND object_id=OBJECT_ID(N'dbo.F03AccessChangeRequests'))
    CREATE INDEX IX_F03AccessChangeRequests_OldEmployee ON dbo.F03AccessChangeRequests(OldEmployeeCode,Status);
GO

PRINT N'Security access change workflow deployment completed.';
GO
