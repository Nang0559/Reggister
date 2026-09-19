/*
===============================================================================
FVN_REGISTER - MASTER SQL DEPLOYMENT
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
  10 Triggers
  11 Automation contract
  12 Core verification
  13 HRM shift master / attendance resolver
  14 Application RBAC / authorization
  15 Public Information CMS
  16 Equipment flexible schema / import
  17 Documentation consistency verification
  18 OT / Leave limits
  19 Shared Work Calendar indexes / policy
  20 Reporting read models
  21 Reporting verification
  22 HRM-compatible attendance pipeline
  99 Final cross-layer/schema verification

IMPORTANT:
  06_Seed.sql is TEST/DEMO data.
  HRM remains READ ONLY from FVN_REGISTER.
  Work Calendar is a read projection shared by Leave, OT and Trip.
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
:r 12_Verify.sql
:r 13_HrmShiftMaster.sql
:r 14_SecurityAuthorization.sql
:r 15_PublicInformation.sql
:r 16_EquipmentFlexibleImport.sql
:r 17_DocumentationConsistency.sql
:r 18_OT_Leave_Limits.sql
:r 19_WorkCalendar.sql
:r 20_Reports.sql
:r 21_Verify_Reports.sql
:r 22_HrmCompatibleAttendance.sql
:r 99_Verify.sql

PRINT N'============================================================';
PRINT N'FVN_REGISTER SQL deployment 01..22 + 99 verification completed.';
PRINT N'============================================================';
GO

/* 99_Verify.sql is intentionally the final gate and must pass on the target database. */
