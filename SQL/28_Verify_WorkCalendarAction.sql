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
    INSERT @Errors VALUES(N'ActionDedup',N'Open/InProgress logical unique index is missing');

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


/* Generic execution reconciliation verification */
IF OBJECT_ID(N'dbo.F03ExecutionPolicies',N'U') IS NULL THROW 52029, N'Missing F03ExecutionPolicies', 1;
IF OBJECT_ID(N'dbo.F03ExecutionReconciliations',N'U') IS NULL THROW 52029, N'Missing F03ExecutionReconciliations', 1;
IF OBJECT_ID(N'dbo.F03ExecutionConfirmations',N'U') IS NULL THROW 52029, N'Missing F03ExecutionConfirmations', 1;
IF OBJECT_ID(N'dbo.F03ExecutionConfirmationEvidence',N'U') IS NULL THROW 52029, N'Missing F03ExecutionConfirmationEvidence', 1;
IF OBJECT_ID(N'dbo.F03ExecutionReconciliationHistory',N'U') IS NULL THROW 52029, N'Missing F03ExecutionReconciliationHistory', 1;

/* Participant/source identity and shared evidence contract. */
IF COL_LENGTH(N'dbo.F03CalendarProjection',N'SourceType') IS NULL
    THROW 52030, N'Missing F03CalendarProjection.SourceType', 1;
IF COL_LENGTH(N'dbo.F03CalendarProjection',N'ParticipantId') IS NULL
    THROW 52031, N'Missing F03CalendarProjection.ParticipantId', 1;
IF COL_LENGTH(N'dbo.F03ActionItems',N'SourceType') IS NULL
    THROW 52032, N'Missing F03ActionItems.SourceType', 1;
IF COL_LENGTH(N'dbo.F03ActionItems',N'ParticipantId') IS NULL
    THROW 52033, N'Missing F03ActionItems.ParticipantId', 1;
IF COL_LENGTH(N'dbo.F03ExecutionReconciliations',N'SourceType') IS NULL
    THROW 52034, N'Missing F03ExecutionReconciliations.SourceType', 1;
IF COL_LENGTH(N'dbo.F03ExecutionReconciliations',N'ParticipantId') IS NULL
    THROW 52035, N'Missing F03ExecutionReconciliations.ParticipantId', 1;
IF COL_LENGTH(N'dbo.F03ExecutionConfirmations',N'SourceType') IS NULL
    THROW 52036, N'Missing F03ExecutionConfirmations.SourceType', 1;
IF COL_LENGTH(N'dbo.F03ExecutionConfirmations',N'ParticipantId') IS NULL
    THROW 52037, N'Missing F03ExecutionConfirmations.ParticipantId', 1;
IF NOT EXISTS
(
    SELECT 1 FROM sys.foreign_keys
    WHERE name=N'FK_F03ExecutionEvidence_Attachment'
)
    THROW 52038, N'Execution evidence must reference F03Attachment.', 1;
IF NOT EXISTS
(
    SELECT 1 FROM sys.foreign_keys
    WHERE name=N'FK_F03ExecutionReconciliations_Action'
)
    THROW 52039, N'Execution reconciliation must reference shared ActionItem.', 1;

PRINT N'Generic execution reconciliation schema verified.';
GO
