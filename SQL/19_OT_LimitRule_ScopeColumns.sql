USE [FVN_REGISTER];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/*
    CANONICAL OT LIMIT SCOPE SCHEMA

    ScopeType is INT and maps to:
      1 = Employee
      2 = Department
      3 = Block

    F03Departments.BlockCode is persisted because Block rules are evaluated
    in SQL by OTValidator/OTQueryService.

    This file is the single owner of OT scope schema. Do not add another
    ScopeType migration elsewhere.
*/

IF OBJECT_ID(N'dbo.F03OTLimitRules', N'U') IS NULL
    THROW 50001, 'dbo.F03OTLimitRules does not exist. Run 03_Tables.sql first.', 1;

IF COL_LENGTH(N'dbo.F03Departments', N'BlockCode') IS NULL
BEGIN
    ALTER TABLE dbo.F03Departments ADD BlockCode nvarchar(20) NULL;
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name=N'IX_F03Department_BlockCode'
      AND object_id=OBJECT_ID(N'dbo.F03Departments')
)
BEGIN
    CREATE INDEX IX_F03Department_BlockCode ON dbo.F03Departments(BlockCode);
END;
GO

/*
  Fresh database: add canonical INT ScopeType.
  Existing databases from 26_OT_Limit_Scopes may have NVARCHAR values.
  Convert those values explicitly before dropping the legacy column.
*/
IF COL_LENGTH(N'dbo.F03OTLimitRules', N'ScopeType') IS NULL
BEGIN
    ALTER TABLE dbo.F03OTLimitRules
        ADD ScopeType int NOT NULL
            CONSTRAINT DF_F03OTLimitRules_ScopeType DEFAULT (1);
END
ELSE
BEGIN
    DECLARE @ScopeTypeSqlType sysname;

    SELECT @ScopeTypeSqlType=t.name
    FROM sys.columns c
    JOIN sys.types t ON t.user_type_id=c.user_type_id
    WHERE c.object_id=OBJECT_ID(N'dbo.F03OTLimitRules')
      AND c.name=N'ScopeType';

    IF @ScopeTypeSqlType IN (N'nvarchar',N'varchar',N'nchar',N'char')
    BEGIN
        IF COL_LENGTH(N'dbo.F03OTLimitRules',N'ScopeType_Canonical') IS NULL
            ALTER TABLE dbo.F03OTLimitRules
                ADD ScopeType_Canonical int NOT NULL
                    CONSTRAINT DF_F03OTLimitRules_ScopeType_Canonical DEFAULT (1);

        /* Dynamic SQL: ScopeType_Canonical does not exist when this batch is
           compiled, and a static reference fails with Msg 207 even though
           this branch is only taken on legacy databases. */
        EXEC sys.sp_executesql N'
        UPDATE r
        SET ScopeType_Canonical =
            CASE UPPER(LTRIM(RTRIM(r.ScopeType)))
                WHEN N''EMPLOYEE'' THEN 1
                WHEN N''DEPARTMENT'' THEN 2
                WHEN N''BLOCK'' THEN 3
                WHEN N''1'' THEN 1
                WHEN N''2'' THEN 2
                WHEN N''3'' THEN 3
                ELSE 1
            END
        FROM dbo.F03OTLimitRules r;';

        DECLARE @ScopeDefaultConstraint sysname;
        SELECT @ScopeDefaultConstraint=dc.name
        FROM sys.default_constraints dc
        JOIN sys.columns c
          ON c.object_id=dc.parent_object_id
         AND c.column_id=dc.parent_column_id
        WHERE dc.parent_object_id=OBJECT_ID(N'dbo.F03OTLimitRules')
          AND c.name=N'ScopeType';

        IF @ScopeDefaultConstraint IS NOT NULL
            EXEC(N'ALTER TABLE dbo.F03OTLimitRules DROP CONSTRAINT ' + QUOTENAME(@ScopeDefaultConstraint));

        ALTER TABLE dbo.F03OTLimitRules DROP COLUMN ScopeType;
        EXEC sys.sp_rename N'dbo.F03OTLimitRules.ScopeType_Canonical', N'ScopeType', N'COLUMN';
    END
    ELSE
    BEGIN
        UPDATE dbo.F03OTLimitRules
        SET ScopeType=1
        WHERE ScopeType NOT IN (1,2,3) OR ScopeType IS NULL;

        IF NOT EXISTS
        (
            SELECT 1 FROM sys.default_constraints dc
            JOIN sys.columns c
              ON c.object_id=dc.parent_object_id
             AND c.column_id=dc.parent_column_id
            WHERE dc.parent_object_id=OBJECT_ID(N'dbo.F03OTLimitRules')
              AND c.name=N'ScopeType'
        )
            ALTER TABLE dbo.F03OTLimitRules
                ADD CONSTRAINT DF_F03OTLimitRules_ScopeType DEFAULT (1) FOR ScopeType;
    END
END;
GO

IF COL_LENGTH(N'dbo.F03OTLimitRules', N'ScopeCode') IS NULL
    ALTER TABLE dbo.F03OTLimitRules ADD ScopeCode nvarchar(50) NULL;
GO

IF COL_LENGTH(N'dbo.F03OTLimitRules', N'EmployeeCode') IS NULL
    ALTER TABLE dbo.F03OTLimitRules ADD EmployeeCode nvarchar(50) NULL;
GO

/* Legacy baseline rules had NULL employee/department/position and were
   historically Employee-context rules. Their canonical ScopeType is 1. */
UPDATE dbo.F03OTLimitRules
SET ScopeType=1
WHERE EmployeeCode IS NULL
  AND DeptCode IS NULL
  AND PositionCode IS NULL
  AND (ScopeType IS NULL OR ScopeType=0);
GO

IF EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name=N'UX_OTLimitRule_ActiveScope'
      AND object_id=OBJECT_ID(N'dbo.F03OTLimitRules')
)
    DROP INDEX UX_OTLimitRule_ActiveScope ON dbo.F03OTLimitRules;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name=N'UX_OTLimitRule_ActiveScopeV2'
      AND object_id=OBJECT_ID(N'dbo.F03OTLimitRules')
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
    WHERE IsActive=1;
END;
GO

PRINT N'Canonical OT scope schema installed: ScopeType INT (1=Employee, 2=Department, 3=Block).';
GO
