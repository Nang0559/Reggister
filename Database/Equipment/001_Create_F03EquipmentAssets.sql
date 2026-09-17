IF OBJECT_ID(N'dbo.F03EquipmentAssets', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.F03EquipmentAssets (
  Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03EquipmentAssets PRIMARY KEY,
  EquipmentCode NVARCHAR(30) NOT NULL,
  EquipmentName NVARCHAR(250) NOT NULL,
  Specification NVARCHAR(1000) NULL,
  SerialNumber NVARCHAR(100) NULL,
  AssetCode NVARCHAR(50) NULL,
  PurchasePrice DECIMAL(18,2) NOT NULL,
  PurchaseDate DATETIME2 NOT NULL,
  ExpectedDepreciationDate DATETIME2 NOT NULL,
  DeptCode NVARCHAR(20) NOT NULL,
  Location NVARCHAR(250) NULL,
  QrToken NVARCHAR(128) NOT NULL,
  IsQrActive BIT NOT NULL CONSTRAINT DF_F03EquipmentAssets_Qr DEFAULT(0),
  IsActive BIT NULL CONSTRAINT DF_F03EquipmentAssets_Active DEFAULT(1),
  Note NVARCHAR(1000) NULL,
  CreatedBy INT NOT NULL,
  LastModifiedSource NVARCHAR(MAX) NULL,
  CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_F03EquipmentAssets_CreatedAt DEFAULT(GETDATE()),
  ModifiedBy INT NULL, ModifiedAt DATETIME2 NULL
 );
 CREATE UNIQUE INDEX UX_F03EquipmentAssets_Code ON dbo.F03EquipmentAssets(EquipmentCode);
 CREATE UNIQUE INDEX UX_F03EquipmentAssets_Qr ON dbo.F03EquipmentAssets(QrToken);
 CREATE INDEX IX_F03EquipmentAssets_Dept ON dbo.F03EquipmentAssets(DeptCode);
END;
GO
