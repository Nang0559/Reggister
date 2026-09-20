USE [FVN_REGISTER];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/*
    Align dbo.F03OTLimitRules with FVN_REGISTER.Core.Entities.OT.F03OTLimitRule.

    The application now resolves OT rules by:
      - ScopeType
      - ScopeCode
      - EmployeeCode

    Older databases can still contain F03OTLimitRules without these columns.
    This migration is intentionally idempotent so it is safe to run on an
    already-updated database.
*/

IF OBJECT_ID(N'dbo.F03OTLimitRules', N'U') IS NULL
BEGIN
    THROW 50001, 'dbo.F03OTLimitRules does not exist. Run the OT limit baseline/schema deployment first.', 1;
END;
GO

IF COL_LENGTH(N'dbo.F03OTLimitRules', N'ScopeType') IS NULL
BEGIN
    ALTER TABLE dbo.F03OTLimitRules
        ADD ScopeType int NOT NULL
            CONSTRAINT DF_F03OTLimitRules_ScopeType DEFAULT (0);
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

PRINT N'F03OTLimitRules schema aligned: ScopeType, ScopeCode, EmployeeCode are present.';
GO
