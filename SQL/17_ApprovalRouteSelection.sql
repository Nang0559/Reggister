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
        RequesterPositionCode nvarchar(20) NOT NULL,
        Level int NOT NULL,
        Sequence int NOT NULL,
        LevelName nvarchar(100) NOT NULL,
        RoleName nvarchar(50) NOT NULL,
        Required bit NOT NULL CONSTRAINT DF_F03ApprovalPolicies_Required DEFAULT 1
    );
END;
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
    WHERE name = N'UX_F03ApprovalPolicies_Request_Position_Level'
      AND object_id = OBJECT_ID(N'dbo.F03ApprovalPolicies')
)
BEGIN
    CREATE UNIQUE INDEX UX_F03ApprovalPolicies_Request_Position_Level
        ON dbo.F03ApprovalPolicies(RequestType, RequesterPositionCode, Level);
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
    Default position -> required approval levels.

    Requester position codes must match F03Employees/F03Users.Cvcode.
    The current application seed uses:
      EMP    Employee
      SL     Sub Leader
      CHIEF  Chief
      MGR    Manager
      GM     General Manager

    The requester NEVER chooses Level.
    The policy below determines the levels automatically.
*/

DECLARE @Policies TABLE
(
    RequestType int,
    RequesterPositionCode nvarchar(20),
    Level int,
    Sequence int,
    LevelName nvarchar(100),
    RoleName nvarchar(50)
);

-- Leave: Worker/Staff/Technician/Employee -> SubLeader -> Manager -> GM
INSERT @Policies VALUES
(0,N'0003',1,1,N'Sub-leader / Leader',N'SubLeader'),
(0,N'0003',2,2,N'Manager / Senior Manager',N'Manager'),
(0,N'0003',3,3,N'Giám đốc (GM)',N'GM'),
(0,N'0007',1,1,N'Sub-leader / Leader',N'SubLeader'),
(0,N'0007',2,2,N'Manager / Senior Manager',N'Manager'),
(0,N'0007',3,3,N'Giám đốc (GM)',N'GM'),
(0,N'0008',1,1,N'Sub-leader / Leader',N'SubLeader'),
(0,N'0008',2,2,N'Manager / Senior Manager',N'Manager'),
(0,N'0008',3,3,N'Giám đốc (GM)',N'GM'),
(0,N'0012',1,1,N'Sub-leader / Leader',N'SubLeader'),
(0,N'0012',2,2,N'Manager / Senior Manager',N'Manager'),
(0,N'0012',3,3,N'Giám đốc (GM)',N'GM'),

-- Leave: SubLeader/Leader/Chief -> Manager -> GM
(0,N'0002',2,1,N'Manager / Senior Manager',N'Manager'),
(0,N'0002',3,2,N'Giám đốc (GM)',N'GM'),
(0,N'0006',2,1,N'Manager / Senior Manager',N'Manager'),
(0,N'0006',3,2,N'Giám đốc (GM)',N'GM'),
(0,N'0004',2,1,N'Manager / Senior Manager',N'Manager'),
(0,N'0004',3,2,N'Giám đốc (GM)',N'GM'),
(0,N'0010',2,1,N'Manager / Senior Manager',N'Manager'),
(0,N'0010',3,2,N'Giám đốc (GM)',N'GM'),

-- Leave: Manager -> GM
(0,N'0005',3,1,N'Giám đốc (GM)',N'GM'),
(0,N'0009',3,1,N'Giám đốc (GM)',N'GM'),
(0,N'0011',3,1,N'Giám đốc (GM)',N'GM'),

-- OT: Worker/Staff/Technician/Employee -> 3 -> 5 -> 6 -> 7
(1,N'0003',3,1,N'Sub-leader / Leader',N'SubLeader'),
(1,N'0003',5,2,N'Ast. Chief / Chief',N'Chief'),
(1,N'0003',6,3,N'A.MG / MG',N'Manager'),
(1,N'0003',7,4,N'Giám đốc (GM)',N'GM'),
(1,N'0007',3,1,N'Sub-leader / Leader',N'SubLeader'),
(1,N'0007',5,2,N'Ast. Chief / Chief',N'Chief'),
(1,N'0007',6,3,N'A.MG / MG',N'Manager'),
(1,N'0007',7,4,N'Giám đốc (GM)',N'GM'),
(1,N'0008',3,1,N'Sub-leader / Leader',N'SubLeader'),
(1,N'0008',5,2,N'Ast. Chief / Chief',N'Chief'),
(1,N'0008',6,3,N'A.MG / MG',N'Manager'),
(1,N'0008',7,4,N'Giám đốc (GM)',N'GM'),
(1,N'0012',3,1,N'Sub-leader / Leader',N'SubLeader'),
(1,N'0012',5,2,N'Ast. Chief / Chief',N'Chief'),
(1,N'0012',6,3,N'A.MG / MG',N'Manager'),
(1,N'0012',7,4,N'Giám đốc (GM)',N'GM'),

-- OT: SubLeader/Leader -> Chief -> Manager -> GM
(1,N'0002',5,1,N'Ast. Chief / Chief',N'Chief'),
(1,N'0002',6,2,N'A.MG / MG',N'Manager'),
(1,N'0002',7,3,N'Giám đốc (GM)',N'GM'),
(1,N'0006',5,1,N'Ast. Chief / Chief',N'Chief'),
(1,N'0006',6,2,N'A.MG / MG',N'Manager'),
(1,N'0006',7,3,N'Giám đốc (GM)',N'GM'),

-- OT: Chief -> Manager -> GM
(1,N'0004',6,1,N'A.MG / MG',N'Manager'),
(1,N'0004',7,2,N'Giám đốc (GM)',N'GM'),
(1,N'0010',6,1,N'A.MG / MG',N'Manager'),
(1,N'0010',7,2,N'Giám đốc (GM)',N'GM'),

-- OT: Manager -> GM
(1,N'0005',7,1,N'Giám đốc (GM)',N'GM'),
(1,N'0009',7,1,N'Giám đốc (GM)',N'GM'),
(1,N'0011',7,1,N'Giám đốc (GM)',N'GM'),

-- Trip: same hierarchy as Leave
(2,N'0003',1,1,N'Sub-leader / Leader',N'SubLeader'),
(2,N'0003',2,2,N'Manager / Senior Manager',N'Manager'),
(2,N'0003',3,3,N'Giám đốc (GM)',N'GM'),
(2,N'0007',1,1,N'Sub-leader / Leader',N'SubLeader'),
(2,N'0007',2,2,N'Manager / Senior Manager',N'Manager'),
(2,N'0007',3,3,N'Giám đốc (GM)',N'GM'),
(2,N'0008',1,1,N'Sub-leader / Leader',N'SubLeader'),
(2,N'0008',2,2,N'Manager / Senior Manager',N'Manager'),
(2,N'0008',3,3,N'Giám đốc (GM)',N'GM'),
(2,N'0012',1,1,N'Sub-leader / Leader',N'SubLeader'),
(2,N'0012',2,2,N'Manager / Senior Manager',N'Manager'),
(2,N'0012',3,3,N'Giám đốc (GM)',N'GM'),
(2,N'0002',2,1,N'Manager / Senior Manager',N'Manager'),
(2,N'0002',3,2,N'Giám đốc (GM)',N'GM'),
(2,N'0006',2,1,N'Manager / Senior Manager',N'Manager'),
(2,N'0006',3,2,N'Giám đốc (GM)',N'GM'),
(2,N'0004',2,1,N'Manager / Senior Manager',N'Manager'),
(2,N'0004',3,2,N'Giám đốc (GM)',N'GM'),
(2,N'0010',2,1,N'Manager / Senior Manager',N'Manager'),
(2,N'0010',3,2,N'Giám đốc (GM)',N'GM'),
(2,N'0005',3,1,N'Giám đốc (GM)',N'GM'),
(2,N'0009',3,1,N'Giám đốc (GM)',N'GM'),
(2,N'0011',3,1,N'Giám đốc (GM)',N'GM'),

-- Equipment: Worker-like -> Equipment level 1 -> 2 -> GM.
(3,N'0003',1,1,N'Cấp 1 - Người phụ trách',N'EquipmentApprover'),
(3,N'0003',2,2,N'Cấp 2 - Quản lý thiết bị',N'EquipmentManager'),
(3,N'0003',3,3,N'Giám đốc (GM)',N'GM'),
(3,N'0007',1,1,N'Cấp 1 - Người phụ trách',N'EquipmentApprover'),
(3,N'0007',2,2,N'Cấp 2 - Quản lý thiết bị',N'EquipmentManager'),
(3,N'0007',3,3,N'Giám đốc (GM)',N'GM'),
(3,N'0008',1,1,N'Cấp 1 - Người phụ trách',N'EquipmentApprover'),
(3,N'0008',2,2,N'Cấp 2 - Quản lý thiết bị',N'EquipmentManager'),
(3,N'0008',3,3,N'Giám đốc (GM)',N'GM'),
(3,N'0012',1,1,N'Cấp 1 - Người phụ trách',N'EquipmentApprover'),
(3,N'0012',2,2,N'Cấp 2 - Quản lý thiết bị',N'EquipmentManager'),
(3,N'0012',3,3,N'Giám đốc (GM)',N'GM'),
(3,N'0002',2,1,N'Cấp 2 - Quản lý thiết bị',N'EquipmentManager'),
(3,N'0002',3,2,N'Giám đốc (GM)',N'GM'),
(3,N'0006',2,1,N'Cấp 2 - Quản lý thiết bị',N'EquipmentManager'),
(3,N'0006',3,2,N'Giám đốc (GM)',N'GM'),
(3,N'0004',2,1,N'Cấp 2 - Quản lý thiết bị',N'EquipmentManager'),
(3,N'0004',3,2,N'Giám đốc (GM)',N'GM'),
(3,N'0010',2,1,N'Cấp 2 - Quản lý thiết bị',N'EquipmentManager'),
(3,N'0010',3,2,N'Giám đốc (GM)',N'GM'),
(3,N'0005',3,1,N'Giám đốc (GM)',N'GM'),
(3,N'0009',3,1,N'Giám đốc (GM)',N'GM'),
(3,N'0011',3,1,N'Giám đốc (GM)',N'GM');

INSERT dbo.F03ApprovalPolicies
(
    IsActive, CreatedBy, RequestType, RequesterPositionCode,
    Level, Sequence, LevelName, RoleName, Required
)
SELECT 1, 0, p.RequestType, p.RequesterPositionCode,
       p.Level, p.Sequence, p.LevelName, p.RoleName, 1
FROM @Policies p
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.F03ApprovalPolicies x
    WHERE x.RequestType = p.RequestType
      AND x.RequesterPositionCode = p.RequesterPositionCode
      AND x.Level = p.Level
);

GO

/*
    Canonical application position codes.

    These rows are explicit and intentionally do not depend on the legacy
    numeric CvCodeRules values above. F03Employees/F03Users.Cvcode is the
    runtime source used by ApprovalRouteService.
*/
INSERT dbo.F03ApprovalPolicies
(
    IsActive, CreatedBy, RequestType, RequesterPositionCode,
    Level, Sequence, LevelName, RoleName, Required
)
SELECT 1,0,v.RequestType,v.PositionCode,v.Level,v.Sequence,v.LevelName,v.RoleName,1
FROM (VALUES
    -- Leave: EMP -> SubLeader -> Manager -> GM
    (0,N'EMP',1,1,N'Sub-leader / Leader',N'SubLeader'),
    (0,N'EMP',2,2,N'Manager / Senior Manager',N'Manager'),
    (0,N'EMP',3,3,N'Giám đốc (GM)',N'GM'),

    -- Leave: SL / CHIEF -> Manager -> GM
    (0,N'SL',2,1,N'Manager / Senior Manager',N'Manager'),
    (0,N'SL',3,2,N'Giám đốc (GM)',N'GM'),
    (0,N'CHIEF',2,1,N'Manager / Senior Manager',N'Manager'),
    (0,N'CHIEF',3,2,N'Giám đốc (GM)',N'GM'),

    -- Leave: MGR -> GM; GM has no higher approval level
    (0,N'MGR',3,1,N'Giám đốc (GM)',N'GM'),

    -- OT: EMP -> SubLeader -> Chief -> Manager -> GM
    (1,N'EMP',3,1,N'Sub-leader / Leader',N'SubLeader'),
    (1,N'EMP',5,2,N'Ast. Chief / Chief',N'Chief'),
    (1,N'EMP',6,3,N'A.MG / MG',N'Manager'),
    (1,N'EMP',7,4,N'Giám đốc (GM)',N'GM'),

    -- OT: SL -> Chief -> Manager -> GM
    (1,N'SL',5,1,N'Ast. Chief / Chief',N'Chief'),
    (1,N'SL',6,2,N'A.MG / MG',N'Manager'),
    (1,N'SL',7,3,N'Giám đốc (GM)',N'GM'),

    -- OT: CHIEF -> Manager -> GM
    (1,N'CHIEF',6,1,N'A.MG / MG',N'Manager'),
    (1,N'CHIEF',7,2,N'Giám đốc (GM)',N'GM'),

    -- OT: MGR -> GM
    (1,N'MGR',7,1,N'Giám đốc (GM)',N'GM'),

    -- Trip: same hierarchy as Leave
    (2,N'EMP',1,1,N'Sub-leader / Leader',N'SubLeader'),
    (2,N'EMP',2,2,N'Manager / Senior Manager',N'Manager'),
    (2,N'EMP',3,3,N'Giám đốc (GM)',N'GM'),
    (2,N'SL',2,1,N'Manager / Senior Manager',N'Manager'),
    (2,N'SL',3,2,N'Giám đốc (GM)',N'GM'),
    (2,N'CHIEF',2,1,N'Manager / Senior Manager',N'Manager'),
    (2,N'CHIEF',3,2,N'Giám đốc (GM)',N'GM'),
    (2,N'MGR',3,1,N'Giám đốc (GM)',N'GM'),

    -- Equipment: EMP -> EquipmentApprover -> EquipmentManager -> GM
    (3,N'EMP',1,1,N'Cấp 1 - Người phụ trách',N'EquipmentApprover'),
    (3,N'EMP',2,2,N'Cấp 2 - Quản lý thiết bị',N'EquipmentManager'),
    (3,N'EMP',3,3,N'Giám đốc (GM)',N'GM'),
    (3,N'SL',2,1,N'Cấp 2 - Quản lý thiết bị',N'EquipmentManager'),
    (3,N'SL',3,2,N'Giám đốc (GM)',N'GM'),
    (3,N'CHIEF',2,1,N'Cấp 2 - Quản lý thiết bị',N'EquipmentManager'),
    (3,N'CHIEF',3,2,N'Giám đốc (GM)',N'GM'),
    (3,N'MGR',3,1,N'Giám đốc (GM)',N'GM')
) AS v(RequestType,PositionCode,Level,Sequence,LevelName,RoleName)
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.F03ApprovalPolicies x
    WHERE x.RequestType=v.RequestType
      AND x.RequesterPositionCode=v.PositionCode
      AND x.Level=v.Level
);


GO

/*
    Verification after running this script.
*/
SELECT RequestType, RequesterPositionCode, Level, Sequence, LevelName, RoleName, Required
FROM dbo.F03ApprovalPolicies
WHERE IsActive = 1
ORDER BY RequestType, RequesterPositionCode, Sequence;

SELECT RequestType, Level, ApproveForDeptCode, COUNT(*) AS CandidateCount
FROM dbo.F03Approvers
WHERE IsActive = 1
GROUP BY RequestType, Level, ApproveForDeptCode
ORDER BY RequestType, Level, ApproveForDeptCode;
GO
