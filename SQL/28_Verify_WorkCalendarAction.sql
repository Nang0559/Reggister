USE [FVN_REGISTER];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/*
===============================================================================
28_VERIFY_WORK_CALENDAR_ACTION
===============================================================================
Schema gate for the shared Calendar -> Action -> Notification contract.
This script is read-only and must return zero rows for failures.
===============================================================================
*/

DECLARE @Errors TABLE
(
    CheckName nvarchar(200) NOT NULL,
    Detail nvarchar(1000) NOT NULL
);

IF OBJECT_ID(N'dbo.F03CalendarModuleDefinitions',N'U') IS NULL
    INSERT @Errors VALUES(N'CalendarModuleDefinitions',N'Missing dbo.F03CalendarModuleDefinitions');

IF OBJECT_ID(N'dbo.F03CalendarModulePolicies',N'U') IS NULL
    INSERT @Errors VALUES(N'CalendarModulePolicies',N'Missing dbo.F03CalendarModulePolicies');

IF OBJECT_ID(N'dbo.F03CalendarProjection',N'U') IS NULL
    INSERT @Errors VALUES(N'CalendarProjection',N'Missing dbo.F03CalendarProjection');

IF OBJECT_ID(N'dbo.F03ActionItems',N'U') IS NULL
    INSERT @Errors VALUES(N'ActionItems',N'Missing dbo.F03ActionItems');

IF OBJECT_ID(N'dbo.F03ActionPolicies',N'U') IS NULL
    INSERT @Errors VALUES(N'ActionPolicies',N'Missing dbo.F03ActionPolicies');

IF OBJECT_ID(N'dbo.F03AppNotifications',N'U') IS NULL
    INSERT @Errors VALUES(N'Notifications',N'Missing existing dbo.F03AppNotifications');

IF COL_LENGTH(N'dbo.F03AppNotifications',N'ActionId') IS NULL
    INSERT @Errors VALUES(N'Notifications.ActionId',N'F03AppNotifications.ActionId was not added');

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name=N'UX_F03ActionItems_OpenLogicalKey'
      AND object_id=OBJECT_ID(N'dbo.F03ActionItems')
)
    INSERT @Errors VALUES(N'ActionDedup',N'Active Open/InProgress logical unique index is missing');



IF OBJECT_ID(N'dbo.F03ActionItems',N'U') IS NOT NULL
AND EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name=N'UX_F03ActionItems_OpenLogicalKey'
      AND object_id=OBJECT_ID(N'dbo.F03ActionItems')
)
BEGIN
    DECLARE @ActionDedupFilter nvarchar(4000) =
    (
        SELECT TOP (1) filter_definition
        FROM sys.indexes
        WHERE name=N'UX_F03ActionItems_OpenLogicalKey'
          AND object_id=OBJECT_ID(N'dbo.F03ActionItems')
    );

    IF @ActionDedupFilter IS NULL
       OR @ActionDedupFilter NOT LIKE N'%IsActive%'
       OR @ActionDedupFilter NOT LIKE N'%Status%'
        INSERT @Errors VALUES(
            N'ActionDedup.Filter',
            N'UX_F03ActionItems_OpenLogicalKey must filter active reusable Action rows (Open/InProgress).'
        );
END;

IF NOT EXISTS
(
    SELECT 1 FROM dbo.F03CalendarModuleDefinitions
    WHERE ModuleCode=N'OT' AND IsActive=1
)
    INSERT @Errors VALUES(N'CalendarSeed.OT',N'OT module definition missing');

IF NOT EXISTS
(
    SELECT 1 FROM dbo.F03CalendarModuleDefinitions
    WHERE ModuleCode=N'LEAVE' AND IsActive=1
)
    INSERT @Errors VALUES(N'CalendarSeed.LEAVE',N'LEAVE module definition missing');

IF NOT EXISTS
(
    SELECT 1 FROM dbo.F03CalendarModuleDefinitions
    WHERE ModuleCode=N'TRIP' AND IsActive=1
)
    INSERT @Errors VALUES(N'CalendarSeed.TRIP',N'TRIP module definition missing');

IF NOT EXISTS
(
    SELECT 1 FROM dbo.F03ActionPolicies
    WHERE ModuleCode=N'OT' AND ActionType=N'OT.ATTENDANCE_CONFIRMATION'
)
    INSERT @Errors VALUES(N'ActionSeed.OT',N'OT.ATTENDANCE_CONFIRMATION policy missing');

IF EXISTS
(
    SELECT 1
    FROM dbo.F03ActionItems
    WHERE Status NOT IN (0,10,20,30,40,90)
)
    INSERT @Errors VALUES(N'ActionStatus',N'F03ActionItems contains an unsupported Status value');

IF EXISTS
(
    SELECT 1
    FROM dbo.F03ActionItems
    WHERE AssignedToEmployeeId IS NULL
)
    INSERT @Errors VALUES(N'ActionAssignee',N'AssignedToEmployeeId must be NOT NULL');

SELECT CheckName,Detail
FROM @Errors
ORDER BY CheckName;

IF EXISTS (SELECT 1 FROM @Errors)
    THROW 51326, N'Work Calendar + Action schema verification failed.', 1;

PRINT N'WORK CALENDAR + ACTION SQL verification PASSED.';
GO
