/*
  Equipment Department Schema / versioned import metadata.
  Safe to re-run. Existing field definitions are migrated into v1 Active
  schemas per DepartmentCode; later versions can be cloned by the API.
*/

IF OBJECT_ID(N'dbo.F03EquipmentSchemas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03EquipmentSchemas
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03EquipmentSchemas PRIMARY KEY,
        DeptCode nvarchar(20) NOT NULL,
        SchemaName nvarchar(150) NOT NULL,
        Version int NOT NULL,
        Status nvarchar(20) NOT NULL CONSTRAINT DF_F03EquipmentSchemas_Status DEFAULT(N'Draft'),
        IsActive bit NULL CONSTRAINT DF_F03EquipmentSchemas_IsActive DEFAULT(1),
        CreatedBy int NOT NULL,
        LastModifiedSource nvarchar(200) NULL,
        CreatedAt datetime2 NOT NULL CONSTRAINT DF_F03EquipmentSchemas_CreatedAt DEFAULT(SYSDATETIME()),
        ModifiedBy int NULL,
        ModifiedAt datetime2 NULL
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_F03EquipmentSchemas_Dept_Version' AND object_id = OBJECT_ID(N'dbo.F03EquipmentSchemas'))
    CREATE UNIQUE INDEX UX_F03EquipmentSchemas_Dept_Version ON dbo.F03EquipmentSchemas(DeptCode, Version);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_F03EquipmentSchemas_Dept_Status' AND object_id = OBJECT_ID(N'dbo.F03EquipmentSchemas'))
    CREATE INDEX IX_F03EquipmentSchemas_Dept_Status ON dbo.F03EquipmentSchemas(DeptCode, Status);
GO

IF OBJECT_ID(N'dbo.F03EquipmentFieldDefinitions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03EquipmentFieldDefinitions
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03EquipmentFieldDefinitions PRIMARY KEY,
        SchemaId int NOT NULL,
        DeptCode nvarchar(20) NOT NULL,
        FieldKey nvarchar(60) NOT NULL,
        FieldLabel nvarchar(150) NOT NULL,
        DataType nvarchar(20) NOT NULL CONSTRAINT DF_F03EquipmentFieldDefinitions_DataType DEFAULT(N'Text'),
        IsRequired bit NOT NULL CONSTRAINT DF_F03EquipmentFieldDefinitions_IsRequired DEFAULT(0),
        IsImportable bit NOT NULL CONSTRAINT DF_F03EquipmentFieldDefinitions_IsImportable DEFAULT(1),
        IsSearchable bit NOT NULL CONSTRAINT DF_F03EquipmentFieldDefinitions_IsSearchable DEFAULT(0),
        IsActiveField bit NOT NULL CONSTRAINT DF_F03EquipmentFieldDefinitions_IsActiveField DEFAULT(1),
        DisplayOrder int NOT NULL CONSTRAINT DF_F03EquipmentFieldDefinitions_DisplayOrder DEFAULT(0),
        MaxLength int NULL,
        DefaultValue nvarchar(500) NULL,
        OptionsJson nvarchar(2000) NULL,
        IsActive bit NULL CONSTRAINT DF_F03EquipmentFieldDefinitions_IsActive DEFAULT(1),
        CreatedBy int NOT NULL,
        LastModifiedSource nvarchar(200) NULL,
        CreatedAt datetime2 NOT NULL CONSTRAINT DF_F03EquipmentFieldDefinitions_CreatedAt DEFAULT(SYSDATETIME()),
        ModifiedBy int NULL,
        ModifiedAt datetime2 NULL
    );
END
ELSE
BEGIN
    IF COL_LENGTH('dbo.F03EquipmentFieldDefinitions', 'SchemaId') IS NULL
        ALTER TABLE dbo.F03EquipmentFieldDefinitions ADD SchemaId int NULL;
    IF COL_LENGTH('dbo.F03EquipmentFieldDefinitions', 'MaxLength') IS NULL
        ALTER TABLE dbo.F03EquipmentFieldDefinitions ADD MaxLength int NULL;
    IF COL_LENGTH('dbo.F03EquipmentFieldDefinitions', 'DefaultValue') IS NULL
        ALTER TABLE dbo.F03EquipmentFieldDefinitions ADD DefaultValue nvarchar(500) NULL;
END;
GO

/* Bootstrap one active schema for each department represented by legacy fields. */
DECLARE @DeptCode nvarchar(20), @SchemaId int;
DECLARE dept_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT DISTINCT UPPER(LTRIM(RTRIM(DeptCode)))
    FROM dbo.F03EquipmentFieldDefinitions
    WHERE NULLIF(LTRIM(RTRIM(DeptCode)), N'') IS NOT NULL;
OPEN dept_cursor;
FETCH NEXT FROM dept_cursor INTO @DeptCode;
WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @SchemaId = Id
    FROM dbo.F03EquipmentSchemas
    WHERE DeptCode = @DeptCode AND Status = N'Active' AND IsActive = 1;

    IF @SchemaId IS NULL
    BEGIN
        INSERT dbo.F03EquipmentSchemas(DeptCode, SchemaName, Version, Status, IsActive, CreatedBy)
        VALUES(@DeptCode, CONCAT(@DeptCode, N' Equipment'), 1, N'Active', 1, 0);
        SET @SchemaId = SCOPE_IDENTITY();
    END;

    UPDATE dbo.F03EquipmentFieldDefinitions
    SET SchemaId = @SchemaId
    WHERE UPPER(LTRIM(RTRIM(DeptCode))) = @DeptCode
      AND (SchemaId IS NULL OR SchemaId = 0);

    FETCH NEXT FROM dept_cursor INTO @DeptCode;
END;
CLOSE dept_cursor;
DEALLOCATE dept_cursor;
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.F03EquipmentFieldDefinitions') AND name = N'SchemaId' AND is_nullable = 1)
    ALTER TABLE dbo.F03EquipmentFieldDefinitions ALTER COLUMN SchemaId int NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_F03EquipmentFieldDefinitions_F03EquipmentSchemas')
BEGIN
    ALTER TABLE dbo.F03EquipmentFieldDefinitions WITH CHECK
        ADD CONSTRAINT FK_F03EquipmentFieldDefinitions_F03EquipmentSchemas
        FOREIGN KEY(SchemaId) REFERENCES dbo.F03EquipmentSchemas(Id) ON DELETE CASCADE;
END;
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_F03EquipmentFieldDefinitions_Dept_FieldKey' AND object_id = OBJECT_ID(N'dbo.F03EquipmentFieldDefinitions'))
    DROP INDEX UX_F03EquipmentFieldDefinitions_Dept_FieldKey ON dbo.F03EquipmentFieldDefinitions;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_F03EquipmentFieldDefinitions_Schema_FieldKey' AND object_id = OBJECT_ID(N'dbo.F03EquipmentFieldDefinitions'))
    CREATE UNIQUE INDEX UX_F03EquipmentFieldDefinitions_Schema_FieldKey
        ON dbo.F03EquipmentFieldDefinitions(SchemaId, FieldKey);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_F03EquipmentFieldDefinitions_Dept_Active' AND object_id = OBJECT_ID(N'dbo.F03EquipmentFieldDefinitions'))
    CREATE INDEX IX_F03EquipmentFieldDefinitions_Dept_Active
        ON dbo.F03EquipmentFieldDefinitions(DeptCode, IsActive, IsActiveField, DisplayOrder);
GO

/* Keep legacy import queries compatible: only fields of the Active schema are importable. */
UPDATE f
SET f.IsActiveField = CASE WHEN s.Status = N'Active' THEN 1 ELSE 0 END
FROM dbo.F03EquipmentFieldDefinitions f
JOIN dbo.F03EquipmentSchemas s ON s.Id = f.SchemaId;
GO
