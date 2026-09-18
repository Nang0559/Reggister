/* ============================================================
   16 - Flexible Equipment Schema + Excel Import
   ============================================================ */

IF COL_LENGTH(N'dbo.F03EquipmentAssets', N'CustomDataJson') IS NULL
    ALTER TABLE dbo.F03EquipmentAssets ADD CustomDataJson NVARCHAR(MAX) NULL;
GO

IF OBJECT_ID(N'dbo.F03EquipmentFieldDefinitions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03EquipmentFieldDefinitions
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03EquipmentFieldDefinitions PRIMARY KEY,
        DeptCode NVARCHAR(20) NOT NULL,
        FieldKey NVARCHAR(60) NOT NULL,
        FieldLabel NVARCHAR(150) NOT NULL,
        DataType NVARCHAR(20) NOT NULL CONSTRAINT DF_F03EquipmentFieldDefinitions_DataType DEFAULT N'Text',
        IsRequired BIT NOT NULL CONSTRAINT DF_F03EquipmentFieldDefinitions_IsRequired DEFAULT 0,
        IsImportable BIT NOT NULL CONSTRAINT DF_F03EquipmentFieldDefinitions_IsImportable DEFAULT 1,
        IsSearchable BIT NOT NULL CONSTRAINT DF_F03EquipmentFieldDefinitions_IsSearchable DEFAULT 0,
        IsActiveField BIT NOT NULL CONSTRAINT DF_F03EquipmentFieldDefinitions_IsActiveField DEFAULT 1,
        DisplayOrder INT NOT NULL CONSTRAINT DF_F03EquipmentFieldDefinitions_DisplayOrder DEFAULT 0,
        OptionsJson NVARCHAR(2000) NULL,
        IsActive BIT NULL CONSTRAINT DF_F03EquipmentFieldDefinitions_IsActive DEFAULT 1,
        CreatedBy INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_F03EquipmentFieldDefinitions_CreatedAt DEFAULT SYSDATETIME(),
        ModifiedBy INT NULL,
        ModifiedAt DATETIME2 NULL,
        LastModifiedSource NVARCHAR(100) NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03EquipmentFieldDefinitions_Dept_FieldKey' AND object_id=OBJECT_ID(N'dbo.F03EquipmentFieldDefinitions'))
    CREATE UNIQUE INDEX UX_F03EquipmentFieldDefinitions_Dept_FieldKey
    ON dbo.F03EquipmentFieldDefinitions(DeptCode,FieldKey);
GO

IF OBJECT_ID(N'dbo.F03EquipmentImportBatches', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03EquipmentImportBatches
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03EquipmentImportBatches PRIMARY KEY,
        DeptCode NVARCHAR(20) NOT NULL,
        FileName NVARCHAR(260) NOT NULL,
        Status NVARCHAR(30) NOT NULL,
        TotalRows INT NOT NULL CONSTRAINT DF_F03EquipmentImportBatches_TotalRows DEFAULT 0,
        ValidRows INT NOT NULL CONSTRAINT DF_F03EquipmentImportBatches_ValidRows DEFAULT 0,
        InvalidRows INT NOT NULL CONSTRAINT DF_F03EquipmentImportBatches_InvalidRows DEFAULT 0,
        ImportedRows INT NOT NULL CONSTRAINT DF_F03EquipmentImportBatches_ImportedRows DEFAULT 0,
        CompletedAt DATETIME2 NULL,
        IsActive BIT NULL CONSTRAINT DF_F03EquipmentImportBatches_IsActive DEFAULT 1,
        CreatedBy INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_F03EquipmentImportBatches_CreatedAt DEFAULT SYSDATETIME(),
        ModifiedBy INT NULL,
        ModifiedAt DATETIME2 NULL,
        LastModifiedSource NVARCHAR(100) NULL
    );
END
GO

IF OBJECT_ID(N'dbo.F03EquipmentImportRows', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03EquipmentImportRows
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03EquipmentImportRows PRIMARY KEY,
        BatchId INT NOT NULL,
        RowNumber INT NOT NULL,
        RawJson NVARCHAR(MAX) NOT NULL,
        Status NVARCHAR(30) NOT NULL,
        ErrorMessage NVARCHAR(2000) NULL,
        AssetId INT NULL,
        IsActive BIT NULL CONSTRAINT DF_F03EquipmentImportRows_IsActive DEFAULT 1,
        CreatedBy INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_F03EquipmentImportRows_CreatedAt DEFAULT SYSDATETIME(),
        ModifiedBy INT NULL,
        ModifiedAt DATETIME2 NULL,
        LastModifiedSource NVARCHAR(100) NULL,
        CONSTRAINT FK_F03EquipmentImportRows_Batch FOREIGN KEY(BatchId) REFERENCES dbo.F03EquipmentImportBatches(Id)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03EquipmentImportRows_Batch_Status' AND object_id=OBJECT_ID(N'dbo.F03EquipmentImportRows'))
    CREATE INDEX IX_F03EquipmentImportRows_Batch_Status ON dbo.F03EquipmentImportRows(BatchId,Status);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.F03Functions WHERE FunctionCode=2306)
BEGIN
    INSERT dbo.F03Functions(FunctionCode,FunctionName,Detail,ModuleCode,ActionCode,ScopeCode,DisplayOrder)
    VALUES(2306,N'Equipment.Import',N'Import thiết bị từ Excel',N'Equipment',N'Import',N'Department',360);
END
ELSE
BEGIN
    UPDATE dbo.F03Functions
    SET FunctionName=N'Equipment.Import',Detail=N'Import thiết bị từ Excel',ModuleCode=N'Equipment',ActionCode=N'Import',ScopeCode=N'Department',DisplayOrder=360
    WHERE FunctionCode=2306;
END
GO

/* Admin/SuperAdmin and Equipment approvers receive the import capability.
   Existing security data is not overwritten except this additive capability. */
INSERT dbo.F03RoleFunctions(IdRole,IdFunction)
SELECT r.IdRole,f.IdFunction
FROM dbo.F03Roles r CROSS JOIN dbo.F03Functions f
WHERE r.RoleCode IN(1,2,4)
  AND f.FunctionCode=2306
  AND NOT EXISTS(
      SELECT 1 FROM dbo.F03RoleFunctions rf WHERE rf.IdRole=r.IdRole AND rf.IdFunction=f.IdFunction
  );
GO
