USE [FVN_REGISTER];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

/* Equipment ownership / approver handover.
   Current responsibility lives on F03EquipmentAssets.
   Every change is appended to an immutable handover history row.
   Approved inspection tasks are never rewritten by this process. */

IF COL_LENGTH(N'dbo.F03EquipmentAssets', N'ResponsibleEmployeeCode') IS NULL
    ALTER TABLE dbo.F03EquipmentAssets ADD ResponsibleEmployeeCode nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.F03EquipmentAssets', N'ResponsibleApproverEmployeeCode') IS NULL
    ALTER TABLE dbo.F03EquipmentAssets ADD ResponsibleApproverEmployeeCode nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.F03EquipmentAssets', N'ResponsibleAssignedAt') IS NULL
    ALTER TABLE dbo.F03EquipmentAssets ADD ResponsibleAssignedAt datetime2 NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03EquipmentAssets_ResponsibleEmployeeCode' AND object_id=OBJECT_ID(N'dbo.F03EquipmentAssets'))
    CREATE INDEX IX_F03EquipmentAssets_ResponsibleEmployeeCode ON dbo.F03EquipmentAssets(ResponsibleEmployeeCode);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03EquipmentAssets_ResponsibleApproverEmployeeCode' AND object_id=OBJECT_ID(N'dbo.F03EquipmentAssets'))
    CREATE INDEX IX_F03EquipmentAssets_ResponsibleApproverEmployeeCode ON dbo.F03EquipmentAssets(ResponsibleApproverEmployeeCode);
GO
/* Backfill responsibility for assets created before the handover feature.
   Registration owner/selected approver are the authoritative initial assignment. */
UPDATE a
SET
    a.ResponsibleEmployeeCode = r.EmployeeCode,
    a.ResponsibleApproverEmployeeCode = NULLIF(r.SelectedApproverCode, N''),
    a.ResponsibleAssignedAt = COALESCE(a.ResponsibleAssignedAt, r.ModifiedAt, r.CreatedAt)
FROM dbo.F03EquipmentAssets a
CROSS APPLY
(
    SELECT TOP (1) r.EmployeeCode, r.SelectedApproverCode, r.ModifiedAt, r.CreatedAt
    FROM dbo.F03EquipmentRequests r
    WHERE r.AssetId = a.Id
      AND r.RequestKind = 1
    ORDER BY r.Id DESC
) r
WHERE a.ResponsibleEmployeeCode IS NULL;
GO


IF OBJECT_ID(N'dbo.F03EquipmentAssignmentHistory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03EquipmentAssignmentHistory(
        Id int IDENTITY PRIMARY KEY,
        IsActive bit NULL DEFAULT 1,
        CreatedBy int NOT NULL DEFAULT 0,
        LastModifiedSource nvarchar(200) NULL,
        CreatedAt datetime2 NOT NULL DEFAULT SYSDATETIME(),
        ModifiedBy int NULL,
        ModifiedAt datetime2 NULL,
        AssetId int NOT NULL,
        PreviousResponsibleEmployeeCode nvarchar(50) NULL,
        NewResponsibleEmployeeCode nvarchar(50) NULL,
        PreviousApproverEmployeeCode nvarchar(50) NULL,
        NewApproverEmployeeCode nvarchar(50) NULL,
        Reason nvarchar(500) NOT NULL,
        HandoverAt datetime2 NOT NULL DEFAULT SYSDATETIME(),
        HandoverByUserId int NOT NULL,
        CONSTRAINT FK_F03EquipmentAssignmentHistory_Asset FOREIGN KEY(AssetId)
            REFERENCES dbo.F03EquipmentAssets(Id) ON DELETE NO ACTION
    );
END;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03EquipmentAssignmentHistory_Asset' AND object_id=OBJECT_ID(N'dbo.F03EquipmentAssignmentHistory'))
    CREATE INDEX IX_F03EquipmentAssignmentHistory_Asset ON dbo.F03EquipmentAssignmentHistory(AssetId,HandoverAt DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03EquipmentAssignmentHistory_PreviousResponsible' AND object_id=OBJECT_ID(N'dbo.F03EquipmentAssignmentHistory'))
    CREATE INDEX IX_F03EquipmentAssignmentHistory_PreviousResponsible ON dbo.F03EquipmentAssignmentHistory(PreviousResponsibleEmployeeCode,HandoverAt DESC);
GO

PRINT N'Equipment ownership handover deployment completed.';
GO

GO
CREATE OR ALTER TRIGGER dbo.TR_F03EquipmentAssignmentHistory_Immutable
ON dbo.F03EquipmentAssignmentHistory
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM deleted)
        THROW 51041, N'Lịch sử bàn giao thiết bị là audit record và không được sửa/xóa.', 1;
END;
GO
