USE [FVN_REGISTER];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/*
    OT limit scopes:
      Employee   = limit for each employee (exact EmployeeCode or legacy
                   Position/Department context).
      Department = aggregate OT of the whole department.
      Block      = aggregate OT of all departments mapped to the block.

    IMPORTANT:
      Only IsActive = 1 rules participate in validation/preview.
      Department.BlockCode maps departments to blocks such as SX / OFFICE.
*/

IF COL_LENGTH(N'dbo.F03Departments', N'BlockCode') IS NULL
BEGIN
    ALTER TABLE dbo.F03Departments
        ADD BlockCode nvarchar(20) NULL;
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_F03Department_BlockCode'
      AND object_id = OBJECT_ID(N'dbo.F03Departments')
)
BEGIN
    CREATE INDEX IX_F03Department_BlockCode
        ON dbo.F03Departments(BlockCode);
END;
GO

IF COL_LENGTH(N'dbo.F03OTLimitRules', N'ScopeType') IS NULL
BEGIN
    ALTER TABLE dbo.F03OTLimitRules
        ADD ScopeType nvarchar(20) NOT NULL
            CONSTRAINT DF_F03OTLimitRules_ScopeType DEFAULT(N'Employee');
END;
GO

IF COL_LENGTH(N'dbo.F03OTLimitRules', N'ScopeCode') IS NULL
BEGIN
    ALTER TABLE dbo.F03OTLimitRules
        ADD ScopeCode nvarchar(50) NULL;
END;
GO

IF COL_LENGTH(N'dbo.F03OTLimitRules', N'EmployeeCode') IS NULL
BEGIN
    ALTER TABLE dbo.F03OTLimitRules
        ADD EmployeeCode nvarchar(50) NULL;
END;
GO

IF EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_OTLimitRule_ActiveScope'
      AND object_id = OBJECT_ID(N'dbo.F03OTLimitRules')
)
BEGIN
    DROP INDEX UX_OTLimitRule_ActiveScope
        ON dbo.F03OTLimitRules;
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_OTLimitRule_ActiveScopeV2'
      AND object_id = OBJECT_ID(N'dbo.F03OTLimitRules')
)
BEGIN
    CREATE UNIQUE INDEX UX_OTLimitRule_ActiveScopeV2
        ON dbo.F03OTLimitRules
        (
            LimitType,
            ScopeType,
            ScopeCode,
            EmployeeCode,
            DeptCode,
            PositionCode
        )
        WHERE IsActive = 1;
END;
GO

/*
    Existing rules are legacy employee-context rules.
    Do not enable any new Department/Block rule here.
    Example configuration:
      Department: ScopeType='Department', ScopeCode='PROD'
      Block:      ScopeType='Block',      ScopeCode='SX'
*/

PRINT N'OT scoped limits installed. Only active Employee/Department/Block rules are enforced.';
GO
