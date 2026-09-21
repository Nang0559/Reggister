SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/*
    Approval Policy v4
    ------------------
    Canonical scope:
        RequestType (required)
        DeptCode    (required)
        PositionCode (optional requester position refinement)
        ApprovalPositionCode (required approver position)

    The selected approval position is resolved against F03Positions.
    Its DefaultApproveLevel supplies Level; RoleName is resolved by the
    application from that level/request type. F03Approvers then supplies
    the actual employee candidates for that position and department.

    Existing v3 rows cannot be safely inferred into a department or an
    approval position. They are therefore moved to an inactive quarantine
    scope instead of silently inventing organization data. Admin can recreate
    them using the new policy UI.
*/

IF OBJECT_ID(N'dbo.F03ApprovalPolicies', N'U') IS NULL
BEGIN
    THROW 51001, 'F03ApprovalPolicies must exist before applying Approval Policy v4.', 1;
END;
GO

IF COL_LENGTH(N'dbo.F03ApprovalPolicies', N'DeptCode') IS NULL
BEGIN
    ALTER TABLE dbo.F03ApprovalPolicies
        ADD DeptCode nvarchar(20) NULL;
END;
GO

IF COL_LENGTH(N'dbo.F03ApprovalPolicies', N'ApprovalPositionCode') IS NULL
BEGIN
    ALTER TABLE dbo.F03ApprovalPolicies
        ADD ApprovalPositionCode nvarchar(20) NULL;
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

/*
    Old rows have no reliable department / approver-position information.
    Keep them for audit, but make them non-applicable to the new resolver.
*/
UPDATE dbo.F03ApprovalPolicies
SET
    DeptCode = COALESCE(NULLIF(DeptCode, N''), N'__LEGACY_UNCONFIGURED__'),
    ApprovalPositionCode = COALESCE(
        NULLIF(ApprovalPositionCode, N''),
        N'__LEGACY_UNCONFIGURED__'
    ),
    IsActive = 0,
    LastModifiedSource = N'Migration:ApprovalPolicyV4'
WHERE
    DeptCode IS NULL
    OR LTRIM(RTRIM(DeptCode)) = N''
    OR ApprovalPositionCode IS NULL
    OR LTRIM(RTRIM(ApprovalPositionCode)) = N'';
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_F03ApprovalPolicies_F03Departments'
      AND parent_object_id = OBJECT_ID(N'dbo.F03ApprovalPolicies')
)
BEGIN
    ALTER TABLE dbo.F03ApprovalPolicies
        ADD CONSTRAINT FK_F03ApprovalPolicies_F03Departments
        FOREIGN KEY (DeptCode)
        REFERENCES dbo.F03Departments(DeptCode);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_F03ApprovalPolicies_ApprovalPosition'
      AND parent_object_id = OBJECT_ID(N'dbo.F03ApprovalPolicies')
)
BEGIN
    ALTER TABLE dbo.F03ApprovalPolicies
        ADD CONSTRAINT FK_F03ApprovalPolicies_ApprovalPosition
        FOREIGN KEY (ApprovalPositionCode)
        REFERENCES dbo.F03Positions(PositionCode);
END;
GO

/*
    The quarantine sentinel is intentionally not a real department/position.
    FK constraints therefore require us to create no sentinel rows; instead,
    old rows must be removed if the database has referential constraints.
*/
DELETE p
FROM dbo.F03ApprovalPolicies p
WHERE p.DeptCode = N'__LEGACY_UNCONFIGURED__'
   OR p.ApprovalPositionCode = N'__LEGACY_UNCONFIGURED__';
GO

ALTER TABLE dbo.F03ApprovalPolicies
    ALTER COLUMN DeptCode nvarchar(20) NOT NULL;
GO

ALTER TABLE dbo.F03ApprovalPolicies
    ALTER COLUMN ApprovalPositionCode nvarchar(20) NOT NULL;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_F03ApprovalPolicies_Request_Dept_Position_Level'
      AND object_id = OBJECT_ID(N'dbo.F03ApprovalPolicies')
)
BEGIN
    CREATE UNIQUE INDEX UX_F03ApprovalPolicies_Request_Dept_Position_Level
        ON dbo.F03ApprovalPolicies
        (
            RequestType,
            DeptCode,
            PositionCode,
            Level
        );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_F03ApprovalPolicies_Route'
      AND object_id = OBJECT_ID(N'dbo.F03ApprovalPolicies')
)
BEGIN
    CREATE INDEX IX_F03ApprovalPolicies_Route
        ON dbo.F03ApprovalPolicies
        (
            RequestType,
            DeptCode,
            PositionCode,
            IsActive,
            Sequence,
            Level
        );
END;
GO
