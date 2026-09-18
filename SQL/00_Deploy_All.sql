/*
===============================================================================
FVN_REGISTER - MASTER SQL DEPLOYMENT
===============================================================================
Run in SSMS with "SQLCMD Mode" enabled.

Actual SQL directory in the repository currently contains:
  01_Database.sql
  03_Tables.sql
  05_Indexes.sql
  06_Seed.sql
  07_Views.sql
  08_Functions.sql
  09_StoredProcedures.sql
  99_Verify.sql

There are currently no separate 02/04/10/11/12 SQL files in the repository.
This runner therefore executes every existing numbered deployment file in the
correct dependency order.

WARNING:
  06_Seed.sql inserts TEST/DEMO data and resets the password hash of E0001..E0005.
  Do not run 06_Seed.sql in production unless that is explicitly intended.
===============================================================================
*/

:r 01_Database.sql
:r 03_Tables.sql
:r 05_Indexes.sql
:r 06_Seed.sql
:r 07_Views.sql
:r 08_Functions.sql
:r 09_StoredProcedures.sql
:r 99_Verify.sql

PRINT N'============================================================';
PRINT N'FVN_REGISTER SQL deployment completed.';
PRINT N'============================================================';
GO
