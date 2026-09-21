/*
===============================================================================
FVN_REGISTER - MASTER SQL DEPLOYMENT
===============================================================================
Run from the repository SQL directory with SSMS Query -> SQLCMD Mode enabled.

EXPECTED WORKING DIRECTORY:
  <FVN_REGISTER>\SQL

This master intentionally uses relative paths so it is portable and does not
depend on a mapped drive such as H:.

Repository layout:
  SQL\*.sql
  Database\Equipment\*.sql
  Database\Trips\*.sql

IMPORTANT:
  06_Seed.sql is TEST/DEMO data and remains excluded.
  HRM remains READ ONLY from FVN_REGISTER.
  Existing F03AppNotifications is retained and upgraded with ActionId;
  no duplicate notification framework is introduced.
  Business modules remain source-of-truth.

HOW TO RUN:
  1. Open this file from the repository.
  2. Enable Query -> SQLCMD Mode.
  3. Make sure the SQLCMD working directory is the repository SQL folder.
     Example:
       H:\95 - Project\17.FVN _DANGKYNGHI\FVN_REGISTER\SQL
  4. Execute the whole file.
===============================================================================
*/

:setvar RepoRoot "."

:on error exit

/* ---------------------------------------------------------------------------
   Core database foundation
   --------------------------------------------------------------------------- */
:r "$(RepoRoot)\01_Database.sql"
:r "$(RepoRoot)\02A_Preflight.sql"
:r "$(RepoRoot)\02B_Schemas.sql"
:r "$(RepoRoot)\03_Tables.sql"
:r "$(RepoRoot)\04_Constraints.sql"
:r "$(RepoRoot)\05_Indexes.sql"
:rem 06_Seed.sql intentionally excluded; run only against a disposable/test database.
:r "$(RepoRoot)\07_Views.sql"
:r "$(RepoRoot)\08_Functions.sql"
:r "$(RepoRoot)\09_StoredProcedures.sql"
:r "$(RepoRoot)\10A_Audit.sql"
:r "$(RepoRoot)\10B_Triggers.sql"
:r "$(RepoRoot)\11A_Automation.sql"
:r "$(RepoRoot)\11B_Permissions.sql"
:r "$(RepoRoot)\12_Verify.sql"

/* ---------------------------------------------------------------------------
   Application/security/public/approval/reporting
   --------------------------------------------------------------------------- */
:r "$(RepoRoot)\13_HrmShiftMaster.sql"
:r "$(RepoRoot)\14_SecurityAuthorization.sql"
:r "$(RepoRoot)\15_PublicInformation.sql"
:r "$(RepoRoot)\16_EquipmentFlexibleImport.sql"
:r "$(RepoRoot)\17_ApprovalRouteSelection.sql"
:r "$(RepoRoot)\18_ApproverConfigurationReview.sql"
:r "$(RepoRoot)\19_OT_LimitRule_ScopeColumns.sql"
:r "$(RepoRoot)\20_Reports.sql"
:r "$(RepoRoot)\21_Verify_Reports.sql"

/* ---------------------------------------------------------------------------
   HRM attendance / leave / work-year
   --------------------------------------------------------------------------- */
:r "$(RepoRoot)\22_00_HrmAttendanceTables.sql"
:r "$(RepoRoot)\22_02_HrmCompatibleTimeKeepingForStaff.sql"
:r "$(RepoRoot)\22_03_CalculateHrmAttendance.sql"
:r "$(RepoRoot)\22_04_HrmAttendanceHistory.sql"
:r "$(RepoRoot)\23_LeaveBalanceUpgrade.sql"
:r "$(RepoRoot)\24_WorkYearUpgrade.sql"
:r "$(RepoRoot)\25_RemoveLegacyOTSync.sql"

/* ---------------------------------------------------------------------------
   Work Calendar / Action
   --------------------------------------------------------------------------- */
:r "$(RepoRoot)\26_WorkCalendarAction.sql"
:r "$(RepoRoot)\27_WorkCalendarActionIndexesSeed.sql"
:r "$(RepoRoot)\28_Verify_WorkCalendarAction.sql"

/* ---------------------------------------------------------------------------
   Equipment module database objects
   --------------------------------------------------------------------------- */
:r "$(RepoRoot)\..\Database\Equipment\001_Create_F03EquipmentAssets.sql"
:r "$(RepoRoot)\..\Database\Equipment\002_Create_F03EquipmentRequests.sql"
:r "$(RepoRoot)\..\Database\Equipment\003_Create_F03EquipmentRepairHistory.sql"
:r "$(RepoRoot)\..\Database\Equipment\004_Seed_EquipmentFunction.sql"

/* ---------------------------------------------------------------------------
   Trip module database objects
   --------------------------------------------------------------------------- */
:r "$(RepoRoot)\..\Database\Trips\001_Create_F03TripRequests.sql"
:r "$(RepoRoot)\..\Database\Trips\002_Create_F03TripActual.sql"

/* ---------------------------------------------------------------------------
   Execution reconciliation / payroll
   --------------------------------------------------------------------------- */
:r "$(RepoRoot)\29_ExecutionReconciliation.sql"
:r "$(RepoRoot)\30_Payroll.sql"
:r "$(RepoRoot)\31_Hrm_User_Approval_Provisioning.sql"
:r "$(RepoRoot)\32_ExecutionReviewSecurity.sql"
:r "$(RepoRoot)\33_DocumentationConsistency.sql"
:r "$(RepoRoot)\34_OT_Leave_Limits.sql"
:r "$(RepoRoot)\35_WorkCalendar.sql"

/* ---------------------------------------------------------------------------
   Final cross-foundation verification
   --------------------------------------------------------------------------- */
:r "$(RepoRoot)\99_Verify.sql"

PRINT N'============================================================';
PRINT N'FVN_REGISTER SQL deployment 01..35 + Database modules + 99 verification completed.';
PRINT N'============================================================';
GO

/* 99_Verify.sql remains the final cross-foundation/schema gate. */
