SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/*
    Approval Route v2
    -----------------
    1. F03ApprovalPolicies determines WHICH approval levels are required
       from the requester's PositionCode.
    2. F03Approvers remains the candidate pool. Multiple active people may
       exist at the same RequestType + Level + ApproveForDeptCode.
    3. F03ApprovalSelections stores the requester's selected candidate per
       required level until the immutable approval snapshot is created.
    4. Do NOT delete existing F03Approvers. Re-run this script safely.
*/

IF OBJECT_ID(N'dbo.F03ApprovalPolicies', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03ApprovalPolicies
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03ApprovalPolicies PRIMARY KEY,
        IsActive bit NULL CONSTRAINT DF_F03ApprovalPolicies_IsActive DEFAULT 1,
        CreatedBy int NOT NULL CONSTRAINT DF_F03ApprovalPolicies_CreatedBy DEFAULT 0,
        LastModifiedSource nvarchar(50) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03ApprovalPolicies_CreatedAt DEFAULT GETDATE(),
        ModifiedBy int NULL,
        ModifiedAt datetime2(0) NULL,

        RequestType int NOT NULL,
        ApprovalGroupCode nvarchar(30) NULL,
        RequesterPositionCode nvarchar(20) NULL,
        Level int NOT NULL,
        Sequence int NOT NULL,
        LevelName nvarchar(100) NOT NULL,
        RoleName nvarchar(50) NOT NULL,
        Required bit NOT NULL CONSTRAINT DF_F03ApprovalPolicies_Required DEFAULT 1
    );
END;
GO

/*
    Canonical approval routing uses HRM PositionCode -> ApprovalGroupCode.
    RequesterPositionCode is retained only as a nullable migration column;
    runtime routing must never use it.
*/
IF COL_LENGTH(N'dbo.F03ApprovalPolicies', N'ApprovalGroupCode') IS NULL
BEGIN
    ALTER TABLE dbo.F03ApprovalPolicies
        ADD ApprovalGroupCode nvarchar(30) NULL;
END;
GO

IF COL_LENGTH(N'dbo.F03ApprovalPolicies', N'RequesterPositionCode') IS NOT NULL
BEGIN
    ALTER TABLE dbo.F03ApprovalPolicies
        ALTER COLUMN RequesterPositionCode nvarchar(20) NULL;
END;
GO

IF OBJECT_ID(N'dbo.F03ApprovalPositionGroups', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03ApprovalPositionGroups
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03ApprovalPositionGroups PRIMARY KEY,
        IsActive bit NULL CONSTRAINT DF_F03ApprovalPositionGroups_IsActive DEFAULT 1,
        CreatedBy int NOT NULL CONSTRAINT DF_F03ApprovalPositionGroups_CreatedBy DEFAULT 0,
        LastModifiedSource nvarchar(50) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03ApprovalPositionGroups_CreatedAt DEFAULT GETDATE(),
        ModifiedBy int NULL,
        ModifiedAt datetime2(0) NULL,
        PositionCode nvarchar(20) NOT NULL,
        ApprovalGroupCode nvarchar(30) NOT NULL,
        ApprovalGroupName nvarchar(100) NOT NULL
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_F03ApprovalPositionGroups_PositionCode'
      AND object_id = OBJECT_ID(N'dbo.F03ApprovalPositionGroups')
)
BEGIN
    CREATE UNIQUE INDEX UX_F03ApprovalPositionGroups_PositionCode
        ON dbo.F03ApprovalPositionGroups(PositionCode);
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_F03ApprovalPositionGroups_ApprovalGroupCode'
      AND object_id = OBJECT_ID(N'dbo.F03ApprovalPositionGroups')
)
BEGIN
    CREATE INDEX IX_F03ApprovalPositionGroups_ApprovalGroupCode
        ON dbo.F03ApprovalPositionGroups(ApprovalGroupCode);
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_F03ApprovalPositionGroups_F03Positions'
)
BEGIN
    ALTER TABLE dbo.F03ApprovalPositionGroups
        ADD CONSTRAINT FK_F03ApprovalPositionGroups_F03Positions
        FOREIGN KEY (PositionCode) REFERENCES dbo.F03Positions(PositionCode);
END;
GO

IF EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_F03ApprovalPolicies_Request_Position_Level'
      AND object_id = OBJECT_ID(N'dbo.F03ApprovalPolicies')
)
BEGIN
    DROP INDEX UX_F03ApprovalPolicies_Request_Position_Level
        ON dbo.F03ApprovalPolicies;
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_F03ApprovalPolicies_Request_Group_Level'
      AND object_id = OBJECT_ID(N'dbo.F03ApprovalPolicies')
)
BEGIN
    CREATE UNIQUE INDEX UX_F03ApprovalPolicies_Request_Group_Level
        ON dbo.F03ApprovalPolicies(RequestType, ApprovalGroupCode, Level)
        WHERE ApprovalGroupCode IS NOT NULL;
END;
GO

/* Canonical HRM PositionCode -> ApprovalGroup mapping. */
MERGE dbo.F03ApprovalPositionGroups AS target
USING
(
    VALUES
        (N'0001',N'GM',N'General Manager'),
        (N'0002',N'SL',N'Sub Leader / Leader'),
        (N'0003',N'EMP',N'Employee / Worker'),
        (N'0004',N'CHIEF',N'Chief / Assistant Chief'),
        (N'0005',N'MGR',N'Manager / Senior Manager'),
        (N'0006',N'SL',N'Sub Leader / Leader'),
        (N'0007',N'EMP',N'Employee / Worker'),
        (N'0008',N'EMP',N'Employee / Worker'),
        (N'0009',N'MGR',N'Manager / Senior Manager'),
        (N'0010',N'CHIEF',N'Chief / Assistant Chief'),
        (N'0011',N'MGR',N'Manager / Senior Manager'),
        (N'0012',N'EMP',N'Employee / Worker')
) AS source(PositionCode,ApprovalGroupCode,ApprovalGroupName)
ON target.PositionCode = source.PositionCode
WHEN MATCHED THEN
    UPDATE SET
        target.ApprovalGroupCode = source.ApprovalGroupCode,
        target.ApprovalGroupName = source.ApprovalGroupName,
        target.IsActive = 1
WHEN NOT MATCHED THEN
    INSERT (IsActive,CreatedBy,PositionCode,ApprovalGroupCode,ApprovalGroupName)
    VALUES (1,0,source.PositionCode,source.ApprovalGroupCode,source.ApprovalGroupName);
GO

/* Migrate existing 000x policies to ApprovalGroupCode. */
UPDATE p
SET p.ApprovalGroupCode = g.ApprovalGroupCode
FROM dbo.F03ApprovalPolicies p
INNER JOIN dbo.F03ApprovalPositionGroups g
    ON g.PositionCode = p.RequesterPositionCode
WHERE p.RequesterPositionCode IS NOT NULL;
GO

/* Remove the obsolete EMP/SL/CHIEF/MGR/GM requester-position model. */
DELETE p
FROM dbo.F03ApprovalPolicies p
WHERE p.RequesterPositionCode IN (N'EMP',N'SL',N'CHIEF',N'MGR',N'GM');
GO

IF OBJECT_ID(N'dbo.F03ApprovalSelections', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03ApprovalSelections
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03ApprovalSelections PRIMARY KEY,
        IsActive bit NULL CONSTRAINT DF_F03ApprovalSelections_IsActive DEFAULT 1,
        CreatedBy int NOT NULL CONSTRAINT DF_F03ApprovalSelections_CreatedBy DEFAULT 0,
        LastModifiedSource nvarchar(50) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03ApprovalSelections_CreatedAt DEFAULT GETDATE(),
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
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_F03ApprovalSelections_Request_Level'
      AND object_id = OBJECT_ID(N'dbo.F03ApprovalSelections')
)
BEGIN
    CREATE UNIQUE INDEX UX_F03ApprovalSelections_Request_Level
        ON dbo.F03ApprovalSelections(RequestType, RequestId, Level);
END;
GO

/*
    Canonical policy: ApprovalGroupCode -> ApprovalLevel.
    HRM PositionCode is resolved to ApprovalGroupCode by F03ApprovalPositionGroups.
    The requester never selects a Level.
*/
DECLARE @Policies TABLE
(
    RequestType int,
    ApprovalGroupCode nvarchar(30),
    Level int,
    Sequence int,
    LevelName nvarchar(100),
    RoleName nvarchar(50)
);

INSERT @Policies VALUES
-- Leave
(0,N'EMP',1,1,N'Sub-leader / Leader',N'SubLeader'),
(0,N'EMP',2,2,N'Manager / Senior Manager',N'Manager'),
(0,N'EMP',3,3,N'Giám đốc (GM)',N'GM'),
(0,N'SL',2,1,N'Manager / Senior Manager',N'Manager'),
(0,N'SL',3,2,N'Giám đốc (GM)',N'GM'),
(0,N'CHIEF',2,1,N'Manager / Senior Manager',N'Manager'),
(0,N'CHIEF',3,2,N'Giám đốc (GM)',N'GM'),
(0,N'MGR',3,1,N'Giám đốc (GM)',N'GM'),
-- OT: business levels 3 -> 5 -> 6 -> 7
(1,N'EMP',3,1,N'Sub-leader / Leader',N'SubLeader'),
(1,N'EMP',5,2,N'Ast. Chief / Chief',N'Chief'),
(1,N'EMP',6,3,N'A.MG / MG',N'Manager'),
(1,N'EMP',7,4,N'Giám đốc (GM)',N'GM'),
(1,N'SL',5,1,N'Ast. Chief / Chief',N'Chief'),
(1,N'SL',6,2,N'A.MG / MG',N'Manager'),
(1,N'SL',7,3,N'Giám đốc (GM)',N'GM'),
(1,N'CHIEF',6,1,N'A.MG / MG',N'Manager'),
(1,N'CHIEF',7,2,N'Giám đốc (GM)',N'GM'),
(1,N'MGR',7,1,N'Giám đốc (GM)',N'GM'),
-- Trip = Leave hierarchy
(2,N'EMP',1,1,N'Sub-leader / Leader',N'SubLeader'),
(2,N'EMP',2,2,N'Manager / Senior Manager',N'Manager'),
(2,N'EMP',3,3,N'Giám đốc (GM)',N'GM'),
(2,N'SL',2,1,N'Manager / Senior Manager',N'Manager'),
(2,N'SL',3,2,N'Giám đốc (GM)',N'GM'),
(2,N'CHIEF',2,1,N'Manager / Senior Manager',N'Manager'),
(2,N'CHIEF',3,2,N'Giám đốc (GM)',N'GM'),
(2,N'MGR',3,1,N'Giám đốc (GM)',N'GM'),
-- Equipment
(3,N'EMP',1,1,N'Cấp 1 - Người phụ trách',N'EquipmentApprover'),
(3,N'EMP',2,2,N'Cấp 2 - Quản lý thiết bị',N'EquipmentManager'),
(3,N'EMP',3,3,N'Giám đốc (GM)',N'GM'),
(3,N'SL',2,1,N'Cấp 2 - Quản lý thiết bị',N'EquipmentManager'),
(3,N'SL',3,2,N'Giám đốc (GM)',N'GM'),
(3,N'CHIEF',2,1,N'Cấp 2 - Quản lý thiết bị',N'EquipmentManager'),
(3,N'CHIEF',3,2,N'Giám đốc (GM)',N'GM'),
(3,N'MGR',3,1,N'Giám đốc (GM)',N'GM');

INSERT dbo.F03ApprovalPolicies
(
    IsActive, CreatedBy, RequestType, ApprovalGroupCode,
    RequesterPositionCode, Level, Sequence, LevelName, RoleName, Required
)
SELECT 1,0,p.RequestType,p.ApprovalGroupCode,NULL,p.Level,p.Sequence,p.LevelName,p.RoleName,1
FROM @Policies p
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.F03ApprovalPolicies x
    WHERE x.RequestType = p.RequestType
      AND x.ApprovalGroupCode = p.ApprovalGroupCode
      AND x.Level = p.Level
);
GO

/*
    Verification after running this script.
*/
SELECT RequestType, ApprovalGroupCode, Level, Sequence, LevelName, RoleName, Required
FROM dbo.F03ApprovalPolicies
WHERE IsActive = 1
ORDER BY RequestType, ApprovalGroupCode, Sequence;

SELECT RequestType, Level, ApproveForDeptCode, COUNT(*) AS CandidateCount
FROM dbo.F03Approvers
WHERE IsActive = 1
GROUP BY RequestType, Level, ApproveForDeptCode
ORDER BY RequestType, Level, ApproveForDeptCode;
GO
