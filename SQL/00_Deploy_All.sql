/*
===============================================================================
FVN_REGISTER - MASTER SQL DEPLOYMENT 01..14
===============================================================================
Run in SSMS with SQLCMD Mode enabled.

Order:
  01 Database
  02 Preflight
  03 Tables / schema
  04 Constraints / business keys
  05 Performance indexes
  06 Seed / TEST data
  07 Views
  08 Functions
  09 Stored procedures
  10 Trigger policy (intentionally no HRM cross-db trigger)
  11 Automation contract (application worker)
  12 Final verification
  13 HRM shift master / attendance resolver
  14 Application RBAC / authorization

IMPORTANT:
  06_Seed.sql is TEST/DEMO data. Do NOT run it on production unless intended.
  Automatic HRM synchronization is performed by HrmSyncBackgroundWorker in the
  application, not by a cross-database trigger or SQL Agent job.
===============================================================================
*/

:r 01_Database.sql
:r 02_Preflight.sql
:r 03_Tables.sql
:r 04_Constraints.sql
:r 05_Indexes.sql
:r 06_Seed.sql
:r 07_Views.sql
:r 08_Functions.sql
:r 09_StoredProcedures.sql
:r 10_Triggers.sql
:r 11_Automation.sql
:r 13_HrmShiftMaster.sql
:r 14_SecurityAuthorization.sql
:r 12_Verify.sql

PRINT N'============================================================';
PRINT N'FVN_REGISTER SQL deployment 01..14 completed.';
PRINT N'============================================================';
GO
