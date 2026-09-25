USE [FVN_REGISTER];
GO

/* Preserve the legacy IITSYS profile when it already exists. */
IF EXISTS (SELECT 1 FROM dbo.F03EmailProfiles WHERE Code = 'ITSYS' AND IsActive = 1)
BEGIN
    UPDATE dbo.F03EmailDispatchPolicies
       SET EmailProfileCode = 'ITSYS'
     WHERE EmailProfileCode = 'SYSTEMSMTP';

    UPDATE dbo.F03EmailProfiles
       SET IsDefault = CASE WHEN Code = 'ITSYS' THEN 1 ELSE 0 END
     WHERE IsActive = 1;
END;

/* Never leave multiple active default profiles. */
;WITH Defaults AS
(
    SELECT Id, ROW_NUMBER() OVER (ORDER BY CASE WHEN Code = 'ITSYS' THEN 0 ELSE 1 END, Id) AS rn
    FROM dbo.F03EmailProfiles
    WHERE IsActive = 1 AND IsDefault = 1
)
UPDATE p SET IsDefault = CASE WHEN d.rn = 1 THEN 1 ELSE 0 END
FROM dbo.F03EmailProfiles p
JOIN Defaults d ON d.Id = p.Id;

GO
