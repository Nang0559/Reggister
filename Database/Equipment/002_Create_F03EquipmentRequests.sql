IF OBJECT_ID(N'dbo.F03EquipmentRequests', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.F03EquipmentRequests (
  Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03EquipmentRequests PRIMARY KEY,
  EmployeeCode NVARCHAR(50) NOT NULL, DeptCode NVARCHAR(20) NOT NULL,
  RequestStatus INT NOT NULL CONSTRAINT DF_F03EquipmentRequests_Status DEFAULT(0),
  IsActive BIT NULL CONSTRAINT DF_F03EquipmentRequests_Active DEFAULT(1),
  CreatedBy INT NOT NULL, LastModifiedSource NVARCHAR(MAX) NULL, CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_F03EquipmentRequests_CreatedAt DEFAULT(GETDATE()), ModifiedBy INT NULL, ModifiedAt DATETIME2 NULL,
  RequestKind INT NOT NULL, AssetId INT NULL, SelectedApproverCode NVARCHAR(50) NOT NULL, QrToken NVARCHAR(128) NOT NULL, OperatorUserId INT NOT NULL,
  EquipmentName NVARCHAR(250) NOT NULL, Specification NVARCHAR(1000) NULL, SerialNumber NVARCHAR(100) NULL, AssetCode NVARCHAR(50) NULL,
  PurchasePrice DECIMAL(18,2) NOT NULL, PurchaseDate DATETIME2 NULL, ExpectedDepreciationDate DATETIME2 NULL, Location NVARCHAR(250) NULL, Note NVARCHAR(1000) NULL,
  RepairDate DATETIME2 NULL, RepairContent NVARCHAR(1000) NULL, RepairVendor NVARCHAR(250) NULL, RepairCost DECIMAL(18,2) NULL, RepairResult NVARCHAR(1000) NULL,
  CONSTRAINT FK_F03EquipmentRequests_Asset FOREIGN KEY(AssetId) REFERENCES dbo.F03EquipmentAssets(Id)
 );
 CREATE INDEX IX_F03EquipmentRequests_Status ON dbo.F03EquipmentRequests(RequestKind, RequestStatus);
 CREATE INDEX IX_F03EquipmentRequests_Employee ON dbo.F03EquipmentRequests(EmployeeCode, CreatedAt);
 CREATE INDEX IX_F03EquipmentRequests_Qr ON dbo.F03EquipmentRequests(QrToken);
END;
GO
