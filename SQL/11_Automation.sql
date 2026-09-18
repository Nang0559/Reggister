/*
FVN_REGISTER - SQL 11 Automation

The application BackgroundService is the scheduler:
    HrmSyncBackgroundWorker -> IHrmSyncService -> Importer -> SyncJob

No SQL Agent job is created because running a second scheduler would race the
application worker. Manual sync and automatic sync share the same semaphore.

Email is similarly asynchronous:
    F03EmailQueues -> EmailBackgroundWorker -> SMTP

SQL remains a durable data layer; scheduling belongs to the application.
*/
USE [FVN_REGISTER];
GO
IF OBJECT_ID(N'dbo.F03HrmUserRoleRules',N'U') IS NULL
    THROW 51101, 'F03HrmUserRoleRules must exist before automation is enabled.', 1;
IF OBJECT_ID(N'dbo.F03EmailQueues',N'U') IS NULL
    THROW 51102, 'F03EmailQueues must exist before email automation is enabled.', 1;
PRINT N'11_Automation: application worker contract verified.';
GO
