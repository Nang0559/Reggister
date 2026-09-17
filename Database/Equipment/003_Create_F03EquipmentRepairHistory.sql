IF OBJECT_ID(N'dbo.F03EquipmentRepairHistory', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.F03EquipmentRepairHistory (
  Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03EquipmentRepairHistory PRIMARY KEY,
  AssetId INT NOT NULL, RequestId INT NOT NULL, RepairDate DATETIME2 NOT NULL, OperatorUserId INT NOT NULL,
  RepairCost DECIMAL(18,2) NULL, RepairContent NVARCHAR(1000) NOT NULL, RepairVendor NVARCHAR(250) NULL, RepairResult NVARCHAR(1000) NULL, Note NVARCHAR(1000) NULL,
  IsApproved BIT NOT NULL CONSTRAINT DF_F03EquipmentRepairHistory_Approved DEFAULT(0),
  CreatedBy INT NOT NULL, LastModifiedSource NVARCHAR(MAX) NULL, CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_F03EquipmentRepairHistory_CreatedAt DEFAULT(GETDATE()), ModifiedBy INT NULL, ModifiedAt DATETIME2 NULL,
  CONSTRAINT FK_F03EquipmentRepairHistory_Asset FOREIGN KEY(AssetId) REFERENCES dbo.F03EquipmentAssets(Id),
  CONSTRAINT FK_F03EquipmentRepairHistory_Request FOREIGN KEY(RequestId) REFERENCES dbo.F03EquipmentRequests(Id)
 );
 CREATE UNIQUE INDEX UX_F03EquipmentRepairHistory_Request ON dbo.F03EquipmentRepairHistory(RequestId);
 CREATE INDEX IX_F03EquipmentRepairHistory_AssetDate ON dbo.F03EquipmentRepairHistory(AssetId, RepairDate);
END;
GO
