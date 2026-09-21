SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/*
    Approval Route v3
    -----------------
    Canonical source of requester position:
        F03Employees.PositionCode
            -> F03Positions.PositionCode
            -> F03ApprovalPolicies.PositionCode

    There is intentionally NO approval-position-group table.
    F03ApprovalPolicies is keyed directly by the HRM PositionCode.

    F03ApprovalPolicies determines WHICH approval levels are required.
    F03Approvers remains the candidate pool.
    F03ApprovalSelections stores the requester's selected candidate per
    required level until the immutable approval snapshot is created.
*/

IF OBJECT_ID(N'dbo.F03ApprovalPolicies', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03ApprovalPolicies
    (
        Id int IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_F03ApprovalPolicies PRIMARY KEY,
        IsActive bit NULL
            CONSTRAINT DF_F03ApprovalPolicies_IsActive DEFAULT 1,
        CreatedBy int NOT NULL
            CONSTRAINT DF_F03ApprovalPolicies_CreatedBy DEFAULT 0,
        LastModifiedSource nvarchar(50) NULL,
        CreatedAt datetime2(0) NOT NULL
            CONSTRAINT DF_F03ApprovalPolicies_CreatedAt DEFAULT GETDATE(),
        ModifiedBy int NULL,
        ModifiedAt datetime2(0) NULL,
        RequestType int NOT NULL,
        PositionCode nvarchar(20) NULL,
        Level int NOT NULL,
        Sequence int NOT NULL,
        LevelName nvarchar(100) NOT NULL,
        RoleName nvarchar(50) NOT NULL,
        Required bit NOT NULL
            CONSTRAINT DF_F03ApprovalPolicies_Required DEFAULT 1
    );
END;
GO

/*
    Legacy migration boundary.

    IMPORTANT:
    SQL Server compiles a batch before evaluating IF statements. Therefore
    legacy columns must be accessed through dynamic SQL; otherwise a database
    that already removed ApprovalGroupCode / RequesterPositionCode still gets
    Msg 207 at compile time.

    Canonical model after this block:
        F03ApprovalPolicies.PositionCode -> F03Positions.PositionCode

    ApprovalGroupCode and RequesterPositionCode are removed.
    F03ApprovalPositionGroups is removed below.
*/
IF COL_LENGTH(N'dbo.F03ApprovalPolicies', N'PositionCode') IS NULL
BEGIN
    ALTER TABLE dbo.F03ApprovalPolicies
        ADD PositionCode nvarchar(20) NULL;
END;
GO

IF COL_LENGTH(N'dbo.F03ApprovalPolicies', N'RequesterPositionCode') IS NOT NULL
BEGIN
    EXEC sys.sp_executesql N'
        UPDATE p
        SET p.PositionCode = p.RequesterPositionCode
        FROM dbo.F03ApprovalPolicies p
        WHERE p.PositionCode IS NULL
          AND p.RequesterPositionCode IS NOT NULL
          AND EXISTS
          (
              SELECT 1
              FROM dbo.F03Positions pos
              WHERE pos.PositionCode = p.RequesterPositionCode
          );';
END;
GO

IF COL_LENGTH(N'dbo.F03ApprovalPolicies', N'ApprovalGroupCode') IS NOT NULL
BEGIN
    EXEC sys.sp_executesql N'
        DECLARE @PositionGroupMap TABLE
        (
            PositionCode nvarchar(20) NOT NULL,
            ApprovalGroupCode nvarchar(30) NOT NULL
        );

        INSERT @PositionGroupMap(PositionCode, ApprovalGroupCode)
        VALUES
            (N''0001'',N''GM''),
            (N''0002'',N''SL''),
            (N''0003'',N''EMP''),
            (N''0004'',N''CHIEF''),
            (N''0005'',N''MGR''),
            (N''0006'',N''SL''),
            (N''0007'',N''EMP''),
            (N''0008'',N''EMP''),
            (N''0009'',N''MGR''),
            (N''0010'',N''CHIEF''),
            (N''0011'',N''MGR''),
            (N''0012'',N''EMP'');

        INSERT dbo.F03ApprovalPolicies
        (
            IsActive,
            CreatedBy,
            LastModifiedSource,
            CreatedAt,
            ModifiedBy,
            ModifiedAt,
            RequestType,
            PositionCode,
            Level,
            Sequence,
            LevelName,
            RoleName,
            Required
        )
        SELECT
            p.IsActive,
            p.CreatedBy,
            p.LastModifiedSource,
            p.CreatedAt,
            p.ModifiedBy,
            p.ModifiedAt,
            p.RequestType,
            m.PositionCode,
            p.Level,
            p.Sequence,
            p.LevelName,
            p.RoleName,
            p.Required
        FROM dbo.F03ApprovalPolicies p
        INNER JOIN @PositionGroupMap m
            ON m.ApprovalGroupCode = p.ApprovalGroupCode
        INNER JOIN dbo.F03Positions pos
            ON pos.PositionCode = m.PositionCode
        WHERE p.ApprovalGroupCode IS NOT NULL
          AND p.PositionCode IS NULL
          AND NOT EXISTS
          (
              SELECT 1
              FROM dbo.F03ApprovalPolicies x
              WHERE x.RequestType = p.RequestType
                AND x.PositionCode = m.PositionCode
                AND x.Level = p.Level
          );';
END;
GO

/*
    Legacy rows that could not be translated to a real HRM PositionCode
    cannot participate in the canonical FK and are removed.
*/
DELETE FROM dbo.F03ApprovalPolicies
WHERE PositionCode IS NULL;
GO

IF COL_LENGTH(N'dbo.F03ApprovalPolicies', N'RequesterPositionCode') IS NOT NULL
BEGIN
    EXEC sys.sp_executesql N'
        ALTER TABLE dbo.F03ApprovalPolicies
            DROP COLUMN RequesterPositionCode;';
END;
GO

IF COL_LENGTH(N'dbo.F03ApprovalPolicies', N'ApprovalGroupCode') IS NOT NULL
BEGIN
    EXEC sys.sp_executesql N'
        ALTER TABLE dbo.F03ApprovalPolicies
            DROP COLUMN ApprovalGroupCode;';
END;
GO

IF EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_F03ApprovalPolicies_Request_Group_Level'
      AND object_id = OBJECT_ID(N'dbo.F03ApprovalPolicies')
)
BEGIN
    DROP INDEX UX_F03ApprovalPolicies_Request_Group_Level
        ON dbo.F03ApprovalPolicies;
END;
GO

IF EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_F03ApprovalPolicies_Request_Position_Level'
      AND object_id = OBJECT_ID(N'dbo.F03ApprovalPolicies')
)
BEGIN
    DROP INDEX UX_F03ApprovalPolicies_Request_Position_Level
        ON dbo.F03ApprovalPolicies;
END;
GO

;WITH DuplicatePolicies AS
(
    SELECT
        Id,
        ROW_NUMBER() OVER
        (
            PARTITION BY RequestType, PositionCode, Level
            ORDER BY Id
        ) AS rn
    FROM dbo.F03ApprovalPolicies
    WHERE PositionCode IS NOT NULL
)
DELETE p
FROM dbo.F03ApprovalPolicies p
INNER JOIN DuplicatePolicies d
    ON d.Id = p.Id
WHERE d.rn > 1;
GO

ALTER TABLE dbo.F03ApprovalPolicies
    ALTER COLUMN PositionCode nvarchar(20) NOT NULL;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_F03ApprovalPolicies_F03Positions'
      AND parent_object_id = OBJECT_ID(N'dbo.F03ApprovalPolicies')
)
BEGIN
    ALTER TABLE dbo.F03ApprovalPolicies
        ADD CONSTRAINT FK_F03ApprovalPolicies_F03Positions
        FOREIGN KEY (PositionCode)
        REFERENCES dbo.F03Positions(PositionCode);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_F03ApprovalPolicies_Request_Position_Level'
      AND object_id = OBJECT_ID(N'dbo.F03ApprovalPolicies')
)
BEGIN
    CREATE UNIQUE INDEX UX_F03ApprovalPolicies_Request_Position_Level
        ON dbo.F03ApprovalPolicies(RequestType, PositionCode, Level);
END;
GO

/*
    Approval Policy seed is intentionally not created here.

    The canonical v4 policy requires DeptCode and ApprovalPositionCode,
    which cannot be inferred safely from the legacy position-only seed.
    SQL/36_ApprovalPolicyDepartmentPosition.sql owns that migration boundary;
    administrators then configure department-scoped policies from the UI.
*/

IF OBJECT_ID(N'dbo.F03ApprovalSelections', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03ApprovalSelections
    (
        Id int IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_F03ApprovalSelections PRIMARY KEY,
        IsActive bit NULL
            CONSTRAINT DF_F03ApprovalSelections_IsActive DEFAULT 1,
        CreatedBy int NOT NULL
            CONSTRAINT DF_F03ApprovalSelections_CreatedBy DEFAULT 0,
        LastModifiedSource nvarchar(50) NULL,
        CreatedAt datetime2(0) NOT NULL
            CONSTRAINT DF_F03ApprovalSelections_CreatedAt DEFAULT GETDATE(),
        ModifiedBy int NULL,
        ModifiedAt datetime2(0) NULL,
        RequestType int NOT NULL,
        RequestId int NOT NULL,
        Level int NOT NULL,
        ApproverCode nvarchar(50) NOT NULL
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_F03ApprovalSelections_Request_Level'
      AND object_id = OBJECT_ID(N'dbo.F03ApprovalSelections')
)
BEGIN
    CREATE UNIQUE INDEX UX_F03ApprovalSelections_Request_Level
        ON dbo.F03ApprovalSelections(RequestType, RequestId, Level);
END;
GO

/*
    Remove the obsolete pre-Snapshot approval step table.

    The canonical approval runtime uses:
        F03ApprovalSnapshots
        F03ApprovalStepSnapshots
        F03ApprovalHistories

    F03ApprovalSteps has no writer in the current application and must not
    remain as a second source of approval truth.
*/
IF OBJECT_ID(N'dbo.F03ApprovalSteps', N'U') IS NOT NULL
BEGIN
    DECLARE @sql nvarchar(max) = N'';

    SELECT @sql = @sql +
        N'ALTER TABLE ' +
        QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + N'.' +
        QUOTENAME(OBJECT_NAME(parent_object_id)) +
        N' DROP CONSTRAINT ' + QUOTENAME(name) + N';'
    FROM sys.foreign_keys
    WHERE referenced_object_id = OBJECT_ID(N'dbo.F03ApprovalSteps');

    IF @sql <> N''
        EXEC sp_executesql @sql;

    DROP TABLE dbo.F03ApprovalSteps;
END;
GO

/*
    Remove the obsolete non-canonical mapping table.
    PositionCode is the only requester-position key.
*/
IF OBJECT_ID(N'dbo.F03ApprovalPositionGroups', N'U') IS NOT NULL
BEGIN
    DECLARE @dropGroupSql nvarchar(max) = N'';

    SELECT @dropGroupSql = @dropGroupSql +
        N'ALTER TABLE ' +
        QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + N'.' +
        QUOTENAME(OBJECT_NAME(parent_object_id)) +
        N' DROP CONSTRAINT ' + QUOTENAME(name) + N';'
    FROM sys.foreign_keys
    WHERE parent_object_id = OBJECT_ID(N'dbo.F03ApprovalPositionGroups')
       OR referenced_object_id = OBJECT_ID(N'dbo.F03ApprovalPositionGroups');

    IF @dropGroupSql <> N''
        EXEC sp_executesql @dropGroupSql;

    DROP TABLE dbo.F03ApprovalPositionGroups;
END;
GO

SELECT
    RequestType,
    PositionCode,
    Level,
    Sequence,
    LevelName,
    RoleName,
    Required
FROM dbo.F03ApprovalPolicies
WHERE IsActive = 1
ORDER BY RequestType, PositionCode, Sequence;

SELECT
    RequestType,
    Level,
    ApproveForDeptCode,
    COUNT(*) AS CandidateCount
FROM dbo.F03Approvers
WHERE IsActive = 1
GROUP BY RequestType, Level, ApproveForDeptCode
ORDER BY RequestType, Level, ApproveForDeptCode;
GO
