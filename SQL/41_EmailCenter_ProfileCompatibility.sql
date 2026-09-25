USE [FVN_REGISTER];
GO

/*
   Email Center profile compatibility
   - This script is intentionally safe to rerun.
   - F03EmailDispatchPolicies is a TABLE, so all table DML is written explicitly
     as UPDATE ... FROM/WHERE and never invoked as a procedure.
   - The dynamic UPDATE also avoids stale batch metadata when this compatibility
     script is executed immediately after an older schema migration.
*/

IF OBJECT_ID(N'dbo.F03EmailProfiles', N'U') IS NULL
BEGIN
    PRINT N'F03EmailCenter compatibility skipped: dbo.F03EmailProfiles does not exist.';
    RETURN;
END;

IF COL_LENGTH(N'dbo.F03EmailProfiles', N'IsDefault') IS NULL
BEGIN
    ALTER TABLE dbo.F03EmailProfiles
        ADD IsDefault bit NOT NULL
            CONSTRAINT DF_F03EmailProfiles_IsDefault_Compatibility DEFAULT 0;
END;
GO

/* Preserve the legacy IITSYS profile when it already exists. */
IF EXISTS (SELECT 1 FROM dbo.F03EmailProfiles WHERE Code = N'ITSYS' AND IsActive = 1)
BEGIN
    IF OBJECT_ID(N'dbo.F03EmailDispatchPolicies', N'U') IS NOT NULL
    BEGIN
        EXEC sys.sp_executesql N'
            UPDATE dbo.F03EmailDispatchPolicies
               SET EmailProfileCode = N''ITSYS''
             WHERE EmailProfileCode = N''SYSTEMSMTP'';';
    END;

    UPDATE dbo.F03EmailProfiles
       SET IsDefault = CASE WHEN Code = N'ITSYS' THEN 1 ELSE 0 END
     WHERE IsActive = 1;
END;

/* Never leave multiple active default profiles. */
;WITH Defaults AS
(
    SELECT Id,
           ROW_NUMBER() OVER
           (
               ORDER BY CASE WHEN Code = N'ITSYS' THEN 0 ELSE 1 END, Id
           ) AS rn
    FROM dbo.F03EmailProfiles
    WHERE IsActive = 1
      AND IsDefault = 1
)
UPDATE p
   SET IsDefault = CASE WHEN d.rn = 1 THEN 1 ELSE 0 END
FROM dbo.F03EmailProfiles p
JOIN Defaults d ON d.Id = p.Id;

GO
