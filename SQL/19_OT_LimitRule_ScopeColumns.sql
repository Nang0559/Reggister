USE [FVN_REGISTER];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/*
===============================================================================
19_OT_LIMITRULE_SCOPECOLUMNS
===============================================================================

CANONICAL OT LIMIT SCOPE SCHEMA

  ScopeType:
      1 = Employee
      2 = Department
      3 = Block

  Canonical identity:
      LimitType + ScopeType + ScopeCode + EmployeeCode + DeptCode + PositionCode

  F03Departments.BlockCode is persisted because Block rules are evaluated by
  the OT application/query layer.

IMPORTANT
---------
This file is the SINGLE owner of the OT scope schema.

Legacy databases may contain:
  - ScopeType as NVARCHAR
  - ScopeType_Canonical
  - UX_OTLimitRule_ActiveScope
  - legacy ScopeCode/EmployeeCode omissions

The migration is deliberately batch-safe. All operations which depend on the
runtime type/existence of ScopeType are executed dynamically so SQL Server does
not compile legacy-column references against the canonical schema.
===============================================================================
*/

IF OBJECT_ID(N'dbo.F03OTLimitRules', N'U') IS NULL
    THROW 50001, N'dbo.F03OTLimitRules does not exist. Run 03_Tables.sql first.', 1;

IF OBJECT_ID(N'dbo.F03Departments', N'U') IS NULL
    THROW 50002, N'dbo.F03Departments does not exist. Run 03_Tables.sql first.', 1;

IF COL_LENGTH(N'dbo.F03OTLimitRules', N'LimitType') IS NULL
    THROW 50003, N'dbo.F03OTLimitRules.LimitType is required.', 1;

IF COL_LENGTH(N'dbo.F03OTLimitRules', N'IsActive') IS NULL
    THROW 50004, N'dbo.F03OTLimitRules.IsActive is required.', 1;

IF COL_LENGTH(N'dbo.F03OTLimitRules', N'DeptCode') IS NULL
    THROW 50005, N'dbo.F03OTLimitRules.DeptCode is required.', 1;

IF COL_LENGTH(N'dbo.F03OTLimitRules', N'PositionCode') IS NULL
    THROW 50006, N'dbo.F03OTLimitRules.PositionCode is required.', 1;
GO

/* ---------------------------------------------------------------------------
   Department block scope
--------------------------------------------------------------------------- */

IF COL_LENGTH(N'dbo.F03Departments', N'BlockCode') IS NULL
BEGIN
    ALTER TABLE dbo.F03Departments
        ADD BlockCode nvarchar(20) NULL;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_F03Department_BlockCode'
      AND object_id = OBJECT_ID(N'dbo.F03Departments')
)
BEGIN
    CREATE INDEX IX_F03Department_BlockCode
        ON dbo.F03Departments(BlockCode);
END;
GO

/* ---------------------------------------------------------------------------
   ScopeType migration
---------------------------------------------------------------------------

   Do NOT reference ScopeType with a hard-coded data type in the legacy branch.
   SQL Server compiles a complete batch before executing IF branches. Dynamic
   SQL is therefore required when the existing column can be NVARCHAR.
*/

DECLARE @ScopeTypeExists bit =
    CASE WHEN COL_LENGTH(N'dbo.F03OTLimitRules', N'ScopeType') IS NULL
         THEN 0 ELSE 1 END;

IF @ScopeTypeExists = 0
BEGIN
    ALTER TABLE dbo.F03OTLimitRules
        ADD ScopeType int NOT NULL
            CONSTRAINT DF_F03OTLimitRules_ScopeType DEFAULT (1);
END
ELSE
BEGIN
    DECLARE @ScopeTypeSqlType sysname;

    SELECT @ScopeTypeSqlType = t.name
    FROM sys.columns c
    INNER JOIN sys.types t
        ON t.user_type_id = c.user_type_id
       AND t.system_type_id = c.system_type_id
    WHERE c.object_id = OBJECT_ID(N'dbo.F03OTLimitRules')
      AND c.name = N'ScopeType';

    IF @ScopeTypeSqlType IN (N'nvarchar', N'varchar', N'nchar', N'char')
    BEGIN
        /* Existing indexes cannot survive DROP COLUMN ScopeType. */
        IF EXISTS
        (
            SELECT 1
            FROM sys.indexes
            WHERE name = N'UX_OTLimitRule_ActiveScope'
              AND object_id = OBJECT_ID(N'dbo.F03OTLimitRules')
        )
            DROP INDEX UX_OTLimitRule_ActiveScope
                ON dbo.F03OTLimitRules;

        IF EXISTS
        (
            SELECT 1
            FROM sys.indexes
            WHERE name = N'UX_OTLimitRule_ActiveScopeV2'
              AND object_id = OBJECT_ID(N'dbo.F03OTLimitRules')
        )
            DROP INDEX UX_OTLimitRule_ActiveScopeV2
                ON dbo.F03OTLimitRules;

        /*
          The canonical temporary column is created with an unnamed DEFAULT so
          reruns cannot collide with an old DF_* constraint left by a partial
          deployment.
        */
        IF COL_LENGTH(N'dbo.F03OTLimitRules', N'ScopeType_Canonical') IS NULL
        BEGIN
            ALTER TABLE dbo.F03OTLimitRules
                ADD ScopeType_Canonical int NOT NULL DEFAULT (1);
        END;

        EXEC sys.sp_executesql N'
            UPDATE r
            SET ScopeType_Canonical =
                CASE UPPER(LTRIM(RTRIM(CONVERT(nvarchar(50), r.ScopeType))))
                    WHEN N''EMPLOYEE''   THEN 1
                    WHEN N''DEPARTMENT'' THEN 2
                    WHEN N''BLOCK''      THEN 3
                    WHEN N''1''          THEN 1
                    WHEN N''2''          THEN 2
                    WHEN N''3''          THEN 3
                    ELSE 1
                END
            FROM dbo.F03OTLimitRules AS r;';

        /*
          Remove every DEFAULT constraint attached to the legacy ScopeType.
          This is safer than relying on one historical constraint name.
        */
        DECLARE @Sql nvarchar(max) = N'';

        SELECT @Sql = @Sql +
            N'ALTER TABLE dbo.F03OTLimitRules DROP CONSTRAINT ' +
            QUOTENAME(dc.name) + N';' + CHAR(13) + CHAR(10)
        FROM sys.default_constraints dc
        INNER JOIN sys.columns c
            ON c.object_id = dc.parent_object_id
           AND c.column_id = dc.parent_column_id
        WHERE dc.parent_object_id = OBJECT_ID(N'dbo.F03OTLimitRules')
          AND c.name = N'ScopeType';

        IF @Sql <> N''
            EXEC sys.sp_executesql @Sql;

        ALTER TABLE dbo.F03OTLimitRules
            DROP COLUMN ScopeType;

        EXEC sys.sp_rename
            N'dbo.F03OTLimitRules.ScopeType_Canonical',
            N'ScopeType',
            N'COLUMN';
    END
    ELSE
    BEGIN
        /*
          Canonical INT ScopeType already exists. Keep only the supported
          values and repair NULL/invalid legacy values to Employee scope.
        */
        EXEC sys.sp_executesql N'
            UPDATE dbo.F03OTLimitRules
            SET ScopeType = 1
            WHERE ScopeType IS NULL
               OR ScopeType NOT IN (1,2,3);';
    END
END;
GO

/* ---------------------------------------------------------------------------
   Canonical defaults / null repair
--------------------------------------------------------------------------- */

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

/*
  Legacy baseline rules without an explicit employee/department/position
  context are treated as Employee-scope compatibility rules.
*/
EXEC sys.sp_executesql N'
    UPDATE dbo.F03OTLimitRules
    SET ScopeType = 1
    WHERE EmployeeCode IS NULL
      AND DeptCode IS NULL
      AND PositionCode IS NULL
      AND (ScopeType IS NULL OR ScopeType = 0);';
GO

/* ---------------------------------------------------------------------------
   Canonical unique active-scope index
--------------------------------------------------------------------------- */

IF EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_OTLimitRule_ActiveScope'
      AND object_id = OBJECT_ID(N'dbo.F03OTLimitRules')
)
BEGIN
    DROP INDEX UX_OTLimitRule_ActiveScope
        ON dbo.F03OTLimitRules;
END;
GO

IF EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_OTLimitRule_ActiveScopeV2'
      AND object_id = OBJECT_ID(N'dbo.F03OTLimitRules')
)
BEGIN
    DROP INDEX UX_OTLimitRule_ActiveScopeV2
        ON dbo.F03OTLimitRules;
END;
GO

/*
  The filtered unique index is the canonical physical constraint.
  It deliberately includes all scope discriminators so Employee, Department
  and Block rules cannot collide with one another.
*/
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
GO

PRINT N'Canonical OT scope schema installed successfully.';
PRINT N'ScopeType: INT (1=Employee, 2=Department, 3=Block).';
PRINT N'Legacy NVARCHAR ScopeType values are migrated before the old column is removed.';
GO
