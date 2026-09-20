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
    Codes such as EMP / SL / CHIEF / MGR / GM are legacy seed values and
    must not be used as HRM PositionCode.

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
    Migration boundary from the previously introduced, non-canonical model.
    If ApprovalGroupCode / RequesterPositionCode exist, convert the existing
    group policies into direct HRM PositionCode policies before removing them.
*/
IF COL_LENGTH(N'dbo.F03ApprovalPolicies', N'PositionCode') IS NULL
BEGIN
    ALTER TABLE dbo.F03ApprovalPolicies
        ADD PositionCode nvarchar(20) NULL;
END;
GO

DECLARE @PositionGroupMap TABLE
(
    PositionCode nvarchar(20) NOT NULL,
    ApprovalGroupCode nvarchar(30) NOT NULL
);

INSERT @PositionGroupMap(PositionCode, ApprovalGroupCode)
VALUES
    (N'0001',N'GM'),
    (N'0002',N'SL'),
    (N'0003',N'EMP'),
    (N'0004',N'CHIEF'),
    (N'0005',N'MGR'),
    (N'0006',N'SL'),
    (N'0007',N'EMP'),
    (N'0008',N'EMP'),
    (N'0009',N'MGR'),
    (N'0010',N'CHIEF'),
    (N'0011',N'MGR'),
    (N'0012',N'EMP');

IF COL_LENGTH(N'dbo.F03ApprovalPolicies', N'ApprovalGroupCode') IS NOT NULL
BEGIN
    /*
        Copy group policies to every canonical HRM position that belongs
        to that former group. Existing direct PositionCode policies win.
    */
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
    WHERE p.ApprovalGroupCode IS NOT NULL
      AND p.PositionCode IS NULL
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.F03ApprovalPolicies x
          WHERE x.RequestType = p.RequestType
            AND x.PositionCode = m.PositionCode
            AND x.Level = p.Level
      );
END;
GO

IF COL_LENGTH(N'dbo.F03ApprovalPolicies', N'RequesterPositionCode') IS NOT NULL
BEGIN
    /*
        Preserve any old direct 000x requester-position policies.
        Legacy EMP/SL/CHIEF/MGR/GM values are intentionally ignored.
    */
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
      );
END;
GO

/*
    Remove the legacy policy rows that cannot represent the HRM master.
*/
IF COL_LENGTH(N'dbo.F03ApprovalPolicies', N'ApprovalGroupCode') IS NOT NULL
BEGIN
    DELETE FROM dbo.F03ApprovalPolicies
    WHERE PositionCode IS NULL;
END;
GO

IF COL_LENGTH(N'dbo.F03ApprovalPolicies', N'RequesterPositionCode') IS NOT NULL
BEGIN
    ALTER TABLE dbo.F03ApprovalPolicies
        DROP COLUMN RequesterPositionCode;
END;
GO

IF COL_LENGTH(N'dbo.F03ApprovalPolicies', N'ApprovalGroupCode') IS NOT NULL
BEGIN
    ALTER TABLE dbo.F03ApprovalPolicies
        DROP COLUMN ApprovalGroupCode;
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
    PositionCode is the HRM source-of-truth.
    Policies below are explicitly expanded to the real HRM PositionCode values.
*/
DECLARE @Policies TABLE
(
    RequestType int,
    PositionCode nvarchar(20),
    Level int,
    Sequence int,
    LevelName nvarchar(100),
    RoleName nvarchar(50)
);

INSERT @Policies
(
    RequestType,
    PositionCode,
    Level,
    Sequence,
    LevelName,
    RoleName
)
VALUES
-- Leave
(0,N'0002',2,1,N'Manager / Senior Manager',N'Manager'),
(0,N'0002',3,2,N'Giám đốc (GM)',N'GM'),
(0,N'0003',1,1,N'Sub-leader / Leader',N'SubLeader'),
(0,N'0003',2,2,N'Manager / Senior Manager',N'Manager'),
(0,N'0003',3,3,N'Giám đốc (GM)',N'GM'),
(0,N'0004',2,1,N'Manager / Senior Manager',N'Manager'),
(0,N'0004',3,2,N'Giám đốc (GM)',N'GM'),
(0,N'0005',3,1,N'Giám đốc (GM)',N'GM'),
(0,N'0006',2,1,N'Manager / Senior Manager',N'Manager'),
(0,N'0006',3,2,N'Giám đốc (GM)',N'GM'),
(0,N'0007',1,1,N'Sub-leader / Leader',N'SubLeader'),
(0,N'0007',2,2,N'Manager / Senior Manager',N'Manager'),
(0,N'0007',3,3,N'Giám đốc (GM)',N'GM'),
(0,N'0008',1,1,N'Sub-leader / Leader',N'SubLeader'),
(0,N'0008',2,2,N'Manager / Senior Manager',N'Manager'),
(0,N'0008',3,3,N'Giám đốc (GM)',N'GM'),
(0,N'0009',3,1,N'Giám đốc (GM)',N'GM'),
(0,N'0010',2,1,N'Manager / Senior Manager',N'Manager'),
(0,N'0010',3,2,N'Giám đốc (GM)',N'GM'),
(0,N'0011',3,1,N'Giám đốc (GM)',N'GM'),
(0,N'0012',1,1,N'Sub-leader / Leader',N'SubLeader'),
(0,N'0012',2,2,N'Manager / Senior Manager',N'Manager'),
(0,N'0012',3,3,N'Giám đốc (GM)',N'GM'),

-- OT: business levels 3 -> 5 -> 6 -> 7
(1,N'0002',5,1,N'Ast. Chief / Chief',N'Chief'),
(1,N'0002',6,2,N'A.MG / MG',N'Manager'),
(1,N'0002',7,3,N'Giám đốc (GM)',N'GM'),
(1,N'0003',3,1,N'Sub-leader / Leader',N'SubLeader'),
(1,N'0003',5,2,N'Ast. Chief / Chief',N'Chief'),
(1,N'0003',6,3,N'A.MG / MG',N'Manager'),
(1,N'0003',7,4,N'Giám đốc (GM)',N'GM'),
(1,N'0004',6,1,N'A.MG / MG',N'Manager'),
(1,N'0004',7,2,N'Giám đốc (GM)',N'GM'),
(1,N'0005',7,1,N'Giám đốc (GM)',N'GM'),
(1,N'0006',5,1,N'Ast. Chief / Chief',N'Chief'),
(1,N'0006',6,2,N'A.MG / MG',N'Manager'),
(1,N'0006',7,3,N'Giám đốc (GM)',N'GM'),
(1,N'0007',3,1,N'Sub-leader / Leader',N'SubLeader'),
(1,N'0007',5,2,N'Ast. Chief / Chief',N'Chief'),
(1,N'0007',6,3,N'A.MG / MG',N'Manager'),
(1,N'0007',7,4,N'Giám đốc (GM)',N'GM'),
(1,N'0008',3,1,N'Sub-leader / Leader',N'SubLeader'),
(1,N'0008',5,2,N'Ast. Chief / Chief',N'Chief'),
(1,N'0008',6,3,N'A.MG / MG',N'Manager'),
(1,N'0008',7,4,N'Giám đốc (GM)',N'GM'),
(1,N'0009',7,1,N'Giám đốc (GM)',N'GM'),
(1,N'0010',6,1,N'A.MG / MG',N'Manager'),
(1,N'0010',7,2,N'Giám đốc (GM)',N'GM'),
(1,N'0011',7,1,N'Giám đốc (GM)',N'GM'),
(1,N'0012',3,1,N'Sub-leader / Leader',N'SubLeader'),
(1,N'0012',5,2,N'Ast. Chief / Chief',N'Chief'),
(1,N'0012',6,3,N'A.MG / MG',N'Manager'),
(1,N'0012',7,4,N'Giám đốc (GM)',N'GM'),

-- Trip = Leave hierarchy
(2,N'0002',2,1,N'Manager / Senior Manager',N'Manager'),
(2,N'0002',3,2,N'Giám đốc (GM)',N'GM'),
(2,N'0003',1,1,N'Sub-leader / Leader',N'SubLeader'),
(2,N'0003',2,2,N'Manager / Senior Manager',N'Manager'),
(2,N'0003',3,3,N'Giám đốc (GM)',N'GM'),
(2,N'0004',2,1,N'Manager / Senior Manager',N'Manager'),
(2,N'0004',3,2,N'Giám đốc (GM)',N'GM'),
(2,N'0005',3,1,N'Giám đốc (GM)',N'GM'),
(2,N'0006',2,1,N'Manager / Senior Manager',N'Manager'),
(2,N'0006',3,2,N'Giám đốc (GM)',N'GM'),
(2,N'0007',1,1,N'Sub-leader / Leader',N'SubLeader'),
(2,N'0007',2,2,N'Manager / Senior Manager',N'Manager'),
(2,N'0007',3,3,N'Giám đốc (GM)',N'GM'),
(2,N'0008',1,1,N'Sub-leader / Leader',N'SubLeader'),
(2,N'0008',2,2,N'Manager / Senior Manager',N'Manager'),
(2,N'0008',3,3,N'Giám đốc (GM)',N'GM'),
(2,N'0009',3,1,N'Giám đốc (GM)',N'GM'),
(2,N'0010',2,1,N'Manager / Senior Manager',N'Manager'),
(2,N'0010',3,2,N'Giám đốc (GM)',N'GM'),
(2,N'0011',3,1,N'Giám đốc (GM)',N'GM'),
(2,N'0012',1,1,N'Sub-leader / Leader',N'SubLeader'),
(2,N'0012',2,2,N'Manager / Senior Manager',N'Manager'),
(2,N'0012',3,3,N'Giám đốc (GM)',N'GM'),

-- Equipment
(3,N'0002',2,1,N'Cấp 2 - Quản lý thiết bị',N'EquipmentManager'),
(3,N'0002',3,2,N'Giám đốc (GM)',N'GM'),
(3,N'0003',1,1,N'Cấp 1 - Người phụ trách',N'EquipmentApprover'),
(3,N'0003',2,2,N'Cấp 2 - Quản lý thiết bị',N'EquipmentManager'),
(3,N'0003',3,3,N'Giám đốc (GM)',N'GM'),
(3,N'0004',2,1,N'Cấp 2 - Quản lý thiết bị',N'EquipmentManager'),
(3,N'0004',3,2,N'Giám đốc (GM)',N'GM'),
(3,N'0005',3,1,N'Giám đốc (GM)',N'GM'),
(3,N'0006',2,1,N'Cấp 2 - Quản lý thiết bị',N'EquipmentManager'),
(3,N'0006',3,2,N'Giám đốc (GM)',N'GM'),
(3,N'0007',1,1,N'Cấp 1 - Người phụ trách',N'EquipmentApprover'),
(3,N'0007',2,2,N'Cấp 2 - Quản lý thiết bị',N'EquipmentManager'),
(3,N'0007',3,3,N'Giám đốc (GM)',N'GM'),
(3,N'0008',1,1,N'Cấp 1 - Người phụ trách',N'EquipmentApprover'),
(3,N'0008',2,2,N'Cấp 2 - Quản lý thiết bị',N'EquipmentManager'),
(3,N'0008',3,3,N'Giám đốc (GM)',N'GM'),
(3,N'0009',3,1,N'Giám đốc (GM)',N'GM'),
(3,N'0010',2,1,N'Cấp 2 - Quản lý thiết bị',N'EquipmentManager'),
(3,N'0010',3,2,N'Giám đốc (GM)',N'GM'),
(3,N'0011',3,1,N'Giám đốc (GM)',N'GM'),
(3,N'0012',1,1,N'Cấp 1 - Người phụ trách',N'EquipmentApprover'),
(3,N'0012',2,2,N'Cấp 2 - Quản lý thiết bị',N'EquipmentManager'),
(3,N'0012',3,3,N'Giám đốc (GM)',N'GM');

INSERT dbo.F03ApprovalPolicies
(
    IsActive,
    CreatedBy,
    RequestType,
    PositionCode,
    Level,
    Sequence,
    LevelName,
    RoleName,
    Required
)
SELECT
    1,
    0,
    p.RequestType,
    p.PositionCode,
    p.Level,
    p.Sequence,
    p.LevelName,
    p.RoleName,
    1
FROM @Policies p
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.F03ApprovalPolicies x
    WHERE x.RequestType = p.RequestType
      AND x.PositionCode = p.PositionCode
      AND x.Level = p.Level
);
GO

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
    Remove the previously introduced non-canonical mapping table.
    PositionCode is now the only requester-position key.
*/
IF OBJECT_ID(N'dbo.F03ApprovalPositionGroups', N'U') IS NOT NULL
BEGIN
    IF EXISTS
    (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = N'FK_F03ApprovalPositionGroups_F03Positions'
          AND parent_object_id = OBJECT_ID(N'dbo.F03ApprovalPositionGroups')
    )
    BEGIN
        ALTER TABLE dbo.F03ApprovalPositionGroups
            DROP CONSTRAINT FK_F03ApprovalPositionGroups_F03Positions;
    END;

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
