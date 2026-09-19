/*
FVN_REGISTER - SQL 13 HRM User / Approval Provisioning
Canonical relationship:

HRM PositionCode (000x)
    -> F03ApprovalPositionGroups
    -> F03ApprovalGroups
    -> F03ApprovalPolicies
    -> F03Approvers

F03Employees
    -> F03Users

This script is deliberately idempotent. It does NOT create an HRM cross-database
trigger. Application HRM sync remains the authoritative scheduler; these
procedures are durable reconciliation/repair commands used by that service
or an admin repair action.
*/
USE [FVN_REGISTER];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* ============================================================
   1. ApprovalGroup master
   ============================================================ */
IF OBJECT_ID(N'dbo.F03ApprovalGroups',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03ApprovalGroups
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03ApprovalGroups PRIMARY KEY,
        IsActive bit NULL CONSTRAINT DF_F03ApprovalGroups_IsActive DEFAULT 1,
        CreatedBy int NOT NULL CONSTRAINT DF_F03ApprovalGroups_CreatedBy DEFAULT 0,
        LastModifiedSource nvarchar(50) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03ApprovalGroups_CreatedAt DEFAULT GETDATE(),
        ModifiedBy int NULL,
        ModifiedAt datetime2(0) NULL,
        GroupCode nvarchar(50) NOT NULL,
        GroupName nvarchar(100) NOT NULL,
        Note nvarchar(500) NULL
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03ApprovalGroups_GroupCode' AND object_id=OBJECT_ID(N'dbo.F03ApprovalGroups'))
    CREATE UNIQUE INDEX UX_F03ApprovalGroups_GroupCode
        ON dbo.F03ApprovalGroups(GroupCode);
GO

/* ============================================================
   2. PositionCode -> ApprovalGroup
   PositionCode remains the HRM identity. No hard-coded EMP/SL...
   in application code.
   ============================================================ */
IF OBJECT_ID(N'dbo.F03ApprovalPositionGroups',N'U') IS NULL
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
        GroupCode nvarchar(50) NOT NULL,
        Note nvarchar(500) NULL
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03ApprovalPositionGroups_PositionCode' AND object_id=OBJECT_ID(N'dbo.F03ApprovalPositionGroups'))
    CREATE UNIQUE INDEX UX_F03ApprovalPositionGroups_PositionCode
        ON dbo.F03ApprovalPositionGroups(PositionCode);
GO

/* ============================================================
   3. Approval policy: which level is required for a group/module
   ============================================================ */
IF OBJECT_ID(N'dbo.F03ApprovalPolicies',N'U') IS NULL
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
        RequestType nvarchar(20) NOT NULL,
        GroupCode nvarchar(50) NOT NULL,
        Level int NOT NULL,
        Sequence int NOT NULL,
        LevelName nvarchar(100) NULL,
        RoleName nvarchar(50) NOT NULL,
        Required bit NOT NULL CONSTRAINT DF_F03ApprovalPolicies_Required DEFAULT 1
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03ApprovalPolicies_Group_Request_Level' AND object_id=OBJECT_ID(N'dbo.F03ApprovalPolicies'))
    CREATE UNIQUE INDEX UX_F03ApprovalPolicies_Group_Request_Level
        ON dbo.F03ApprovalPolicies(GroupCode,RequestType,Level);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03ApprovalPolicies_Route' AND object_id=OBJECT_ID(N'dbo.F03ApprovalPolicies'))
    CREATE INDEX IX_F03ApprovalPolicies_Route
        ON dbo.F03ApprovalPolicies(RequestType,GroupCode,Sequence)
        INCLUDE(Level,RoleName,Required);
GO

/* ============================================================
   4. Canonical PositionCode seed
   ============================================================ */
INSERT dbo.F03ApprovalGroups(IsActive,CreatedBy,LastModifiedSource,GroupCode,GroupName,Note)
SELECT 1,0,N'Seed',v.GroupCode,v.GroupName,v.Note
FROM (VALUES
    (N'GM',    N'General Manager', N'Position group for GM'),
    (N'MGR',   N'Manager',        N'Position group for Manager'),
    (N'CHIEF', N'Chief',          N'Position group for Chief / Assistant Chief'),
    (N'SL',    N'SubLeader',      N'Position group for Sub-Leader / Leader'),
    (N'EMP',   N'Employee',       N'Default employee group')
) v(GroupCode,GroupName,Note)
WHERE NOT EXISTS (SELECT 1 FROM dbo.F03ApprovalGroups g WHERE g.GroupCode=v.GroupCode);
GO

INSERT dbo.F03ApprovalPositionGroups(IsActive,CreatedBy,LastModifiedSource,PositionCode,GroupCode,Note)
SELECT 1,0,N'Seed',v.PositionCode,v.GroupCode,v.Note
FROM (VALUES
    (N'0001',N'GM',    N'Giám đốc'),
    (N'0002',N'SL',    N'Sub-Leader'),
    (N'0004',N'CHIEF', N'Chief'),
    (N'0005',N'MGR',   N'Manager'),
    (N'0006',N'SL',    N'Leader'),
    (N'0009',N'MGR',   N'Senior Manager'),
    (N'0010',N'CHIEF', N'Assistant Chief'),
    (N'0011',N'MGR',   N'Assistant Manager'),
    (N'0003',N'EMP',   N'Worker / Staff'),
    (N'0007',N'EMP',   N'Worker / Staff'),
    (N'0008',N'EMP',   N'Worker / Staff'),
    (N'0012',N'EMP',   N'Worker / Staff')
) v(PositionCode,GroupCode,Note)
WHERE NOT EXISTS (SELECT 1 FROM dbo.F03ApprovalPositionGroups x WHERE x.PositionCode=v.PositionCode);
GO

/* ============================================================
   5. Canonical Leave / OT policy
   Level values are business identifiers; Sequence is only route order.
   ============================================================ */
INSERT dbo.F03ApprovalPolicies
(IsActive,CreatedBy,LastModifiedSource,RequestType,GroupCode,Level,Sequence,LevelName,RoleName,Required)
SELECT 1,0,N'Seed',v.RequestType,v.GroupCode,v.Level,v.Sequence,v.LevelName,v.RoleName,1
FROM (VALUES
    (N'Leave',N'EMP',   1,1,N'Sub-Leader / Leader',       N'SubLeader'),
    (N'Leave',N'EMP',   3,2,N'Manager / Senior Manager',  N'Manager'),
    (N'Leave',N'EMP',   4,3,N'General Manager',           N'GM'),

    (N'Leave',N'SL',    2,1,N'Chief / Assistant Chief',   N'Chief'),
    (N'Leave',N'SL',    3,2,N'Manager / Senior Manager',  N'Manager'),
    (N'Leave',N'SL',    4,3,N'General Manager',           N'GM'),

    (N'Leave',N'CHIEF', 3,1,N'Manager / Senior Manager',  N'Manager'),
    (N'Leave',N'CHIEF', 4,2,N'General Manager',            N'GM'),

    (N'Leave',N'MGR',   4,1,N'General Manager',            N'GM'),

    (N'OT',N'EMP',      1,1,N'Sub-Leader / Leader',       N'SubLeader'),
    (N'OT',N'EMP',      3,2,N'Manager / Senior Manager',  N'Manager'),
    (N'OT',N'EMP',      4,3,N'General Manager',           N'GM'),
    (N'OT',N'EMP',      5,4,N'BCH Công đoàn',              N'Union'),

    (N'OT',N'SL',       2,1,N'Chief / Assistant Chief',   N'Chief'),
    (N'OT',N'SL',       3,2,N'Manager / Senior Manager',  N'Manager'),
    (N'OT',N'SL',       4,3,N'General Manager',            N'GM'),
    (N'OT',N'SL',       5,4,N'BCH Công đoàn',              N'Union'),

    (N'OT',N'CHIEF',    3,1,N'Manager / Senior Manager',  N'Manager'),
    (N'OT',N'CHIEF',    4,2,N'General Manager',            N'GM'),
    (N'OT',N'CHIEF',    5,3,N'BCH Công đoàn',              N'Union'),

    (N'OT',N'MGR',      4,1,N'General Manager',            N'GM'),
    (N'OT',N'MGR',      5,2,N'BCH Công đoàn',              N'Union'),

    (N'OT',N'GM',       5,1,N'BCH Công đoàn',              N'Union')
) v(RequestType,GroupCode,Level,Sequence,LevelName,RoleName)
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.F03ApprovalPolicies p
    WHERE p.RequestType=v.RequestType
      AND p.GroupCode=v.GroupCode
      AND p.Level=v.Level
);
GO

/* ============================================================
   6. Repair/reconcile USER from Employee
   This is intentionally a procedure, not a cross-database trigger.
   Existing PermissionCode is preserved; HRM only supplies identity/scope.
   ============================================================ */
CREATE OR ALTER PROCEDURE dbo.usp_ReconcileEmployeeUsers
    @EmployeeCode nvarchar(50)=NULL,
    @CreatedBy int=0,
    @DefaultPasswordHash nvarchar(255)=N'f925916e2754e5e03f75dd58a5733251'
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    ;WITH SourceEmployee AS
    (
        SELECT e.EmployeeCode,e.EmployeeName,e.DeptCode,e.PositionCode,
               e.LevelApprove,e.IsActive
        FROM dbo.F03Employees e
        WHERE (@EmployeeCode IS NULL OR e.EmployeeCode=@EmployeeCode)
    )
    INSERT dbo.F03Users
    (
        IsActive,CreatedBy,LastModifiedSource,Password,EmployeeCode,FullName,
        PermissionCode,LockoutEnable,NumLoginFailed,LevelApprove,DeptCode,Cvcode
    )
    SELECT
        CASE WHEN s.IsActive=1 THEN 1 ELSE 0 END,
        @CreatedBy,N'HRM',
        @DefaultPasswordHash,s.EmployeeCode,s.EmployeeName,
        ISNULL(
            (
                SELECT TOP(1) r.PermissionCode
                FROM dbo.F03HrmUserRoleRules r
                WHERE (r.DeptCode=s.DeptCode OR r.DeptCode IS NULL)
                  AND (r.PositionCode=s.PositionCode OR r.PositionCode IS NULL)
                ORDER BY
                    CASE WHEN r.DeptCode=s.DeptCode AND r.PositionCode=s.PositionCode THEN 0
                         WHEN r.DeptCode=s.DeptCode THEN 1
                         WHEN r.PositionCode=s.PositionCode THEN 2
                         ELSE 3 END,
                    r.Priority,
                    r.Id
            ),5),
        1,0,ISNULL(s.LevelApprove,0),s.DeptCode,s.PositionCode
    FROM SourceEmployee s
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dbo.F03Users u WHERE u.EmployeeCode=s.EmployeeCode
    );

    UPDATE u
       SET u.IsActive=CASE WHEN e.IsActive=1 THEN 1 ELSE 0 END,
           u.FullName=e.EmployeeName,
           u.LevelApprove=ISNULL(e.LevelApprove,0),
           u.DeptCode=e.DeptCode,
           u.Cvcode=e.PositionCode,
           u.ModifiedBy=@CreatedBy,
           u.ModifiedAt=GETDATE(),
           u.LastModifiedSource=N'HRM'
    FROM dbo.F03Users u
    JOIN dbo.F03Employees e ON e.EmployeeCode=u.EmployeeCode
    WHERE (@EmployeeCode IS NULL OR e.EmployeeCode=@EmployeeCode);

    SELECT
        CreatedUsers=@@ROWCOUNT;
END;
GO

/* ============================================================
   7. Repair/reconcile APPROVERS from Employee + PositionCode policy.
   One employee can become an approver at one or more policy levels.
   ============================================================ */
CREATE OR ALTER PROCEDURE dbo.usp_ReconcileEmployeeApprovers
    @EmployeeCode nvarchar(50)=NULL,
    @CreatedBy int=0
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    /* New/updated approvers */
    MERGE dbo.F03Approvers AS target
    USING
    (
        SELECT
            e.EmployeeCode,
            e.PositionCode,
            e.EmployeeName,
            e.EmailAddress,
            e.DeptCode,
            d.DeptName,
            p.RequestType,
            p.Level,
            p.RoleName,
            CASE WHEN p.GroupCode=N'GM' THEN N'ALL' ELSE e.DeptCode END AS ApproveForDeptCode
        FROM dbo.F03Employees e
        JOIN dbo.F03ApprovalPositionGroups pg
          ON pg.PositionCode=e.PositionCode AND pg.IsActive=1
        JOIN dbo.F03ApprovalPolicies p
          ON p.GroupCode=pg.GroupCode AND p.IsActive=1
        LEFT JOIN dbo.F03Departments d
          ON d.DeptCode=e.DeptCode
        WHERE e.IsActive=1
          AND (@EmployeeCode IS NULL OR e.EmployeeCode=@EmployeeCode)
          AND EXISTS
          (
              SELECT 1
              FROM dbo.F03Positions pos
              WHERE pos.PositionCode=e.PositionCode
                AND pos.IsApprove=1
                AND pos.IsAllowApprove=1
          )
    ) AS src
    ON target.ApproverCode=src.EmployeeCode
   AND target.RequestType=src.RequestType
   AND target.Level=src.Level
   AND target.ApproveForDeptCode=src.ApproveForDeptCode
    WHEN MATCHED THEN
        UPDATE SET
            target.IsActive=1,
            target.PositionCode=src.PositionCode,
            target.ApproverName=src.EmployeeName,
            target.ApproverEmail=src.EmailAddress,
            target.ApproverDeptCode=src.DeptCode,
            target.ApproverDeptName=ISNULL(src.DeptName,src.DeptCode),
            target.ApproveForDeptName=CASE WHEN src.ApproveForDeptCode=N'ALL' THEN N'Toàn công ty' ELSE ISNULL(src.DeptName,src.DeptCode) END,
            target.RoleName=src.RoleName,
            target.ModifiedBy=@CreatedBy,
            target.ModifiedAt=GETDATE(),
            target.LastModifiedSource=N'HRM'
    WHEN NOT MATCHED THEN
        INSERT
        (
            IsActive,CreatedBy,LastModifiedSource,UserId,RequestType,ApproverCode,
            PositionCode,ApproverName,ApproverEmail,ApproverDeptCode,ApproverDeptName,
            ApproveForDeptCode,ApproveForDeptName,Level,RoleName
        )
        VALUES
        (
            1,@CreatedBy,N'HRM',
            (SELECT TOP(1) u.Id FROM dbo.F03Users u WHERE u.EmployeeCode=src.EmployeeCode),
            src.RequestType,src.EmployeeCode,src.PositionCode,src.EmployeeName,src.EmailAddress,
            src.DeptCode,ISNULL(src.DeptName,src.DeptCode),
            src.ApproveForDeptCode,
            CASE WHEN src.ApproveForDeptCode=N'ALL' THEN N'Toàn công ty' ELSE ISNULL(src.DeptName,src.DeptCode) END,
            src.Level,src.RoleName
        );

    /* Deactivate stale HRM-derived approvers for affected employee(s). */
    UPDATE a
       SET a.IsActive=0,
           a.ModifiedBy=@CreatedBy,
           a.ModifiedAt=GETDATE(),
           a.LastModifiedSource=N'HRM'
    FROM dbo.F03Approvers a
    JOIN dbo.F03Employees e ON e.EmployeeCode=a.ApproverCode
    WHERE (@EmployeeCode IS NULL OR e.EmployeeCode=@EmployeeCode)
      AND
      (
          e.IsActive=0
          OR NOT EXISTS
          (
              SELECT 1
              FROM dbo.F03ApprovalPositionGroups pg
              JOIN dbo.F03ApprovalPolicies pol
                ON pol.GroupCode=pg.GroupCode AND pol.IsActive=1
              JOIN dbo.F03Positions pos
                ON pos.PositionCode=e.PositionCode
               AND pos.IsApprove=1
               AND pos.IsAllowApprove=1
              WHERE pg.PositionCode=e.PositionCode
                AND pg.IsActive=1
                AND pol.RequestType=a.RequestType
                AND pol.Level=a.Level
                AND a.ApproveForDeptCode=CASE WHEN pg.GroupCode=N'GM' THEN N'ALL' ELSE e.DeptCode END
          )
      );

    SELECT
        ActiveApprovers=(SELECT COUNT(*) FROM dbo.F03Approvers WHERE IsActive=1),
        AffectedEmployee=@EmployeeCode;
END;
GO

/* ============================================================
   8. Repair all currently missing users/approvers.
   Safe to run repeatedly.
   ============================================================ */
CREATE OR ALTER PROCEDURE dbo.usp_ReconcileHrmSecurity
    @CreatedBy int=0
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    EXEC dbo.usp_ReconcileEmployeeUsers @CreatedBy=@CreatedBy;
    EXEC dbo.usp_ReconcileEmployeeApprovers @CreatedBy=@CreatedBy;
END;
GO

/* ============================================================
   9. Verification queries
   ============================================================ */
SELECT
    MissingUsers = COUNT(*)
FROM dbo.F03Employees e
LEFT JOIN dbo.F03Users u ON u.EmployeeCode=e.EmployeeCode
WHERE e.IsActive=1 AND u.Id IS NULL;

SELECT
    MissingApprovers = COUNT(*)
FROM dbo.F03Employees e
JOIN dbo.F03ApprovalPositionGroups pg ON pg.PositionCode=e.PositionCode AND pg.IsActive=1
JOIN dbo.F03ApprovalPolicies pol ON pol.GroupCode=pg.GroupCode AND pol.IsActive=1
LEFT JOIN dbo.F03Approvers a
  ON a.ApproverCode=e.EmployeeCode
 AND a.RequestType=pol.RequestType
 AND a.Level=pol.Level
 AND a.ApproveForDeptCode=CASE WHEN pg.GroupCode=N'GM' THEN N'ALL' ELSE e.DeptCode END
WHERE e.IsActive=1
  AND EXISTS (SELECT 1 FROM dbo.F03Positions p WHERE p.PositionCode=e.PositionCode AND p.IsApprove=1 AND p.IsAllowApprove=1)
  AND a.Id IS NULL;
GO
