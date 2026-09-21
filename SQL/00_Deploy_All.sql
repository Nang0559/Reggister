/*
===============================================================================
FVN_REGISTER - MASTER SQL DEPLOYMENT
===============================================================================
Run in SSMS with Query -> SQLCMD Mode enabled.

LOCAL REPOSITORY:
  H:\95 - Project\17.FVN _DANGKYNGHI\FVN_REGISTER

The paths below match the actual local repository structure:
  FVN_REGISTER\SQL\*.sql
  FVN_REGISTER\Database\Equipment\*.sql
  FVN_REGISTER\Database\Trips\*.sql

IMPORTANT:
  06_Seed.sql is TEST/DEMO data and remains excluded.
  HRM remains READ ONLY from FVN_REGISTER.
  Existing F03AppNotifications is retained and upgraded with ActionId;
  no duplicate notification framework is introduced.
  Business modules remain source-of-truth.
===============================================================================
*/

:setvar RepoRoot "H:\95 - Project\17.FVN _DANGKYNGHI\FVN_REGISTER"

:on error exit

/* ---------------------------------------------------------------------------
   Core database foundation
   --------------------------------------------------------------------------- */
:r "$(RepoRoot)\SQL\01_Database.sql"
:r "$(RepoRoot)\SQL\02A_Preflight.sql"
:r "$(RepoRoot)\SQL\02B_Schemas.sql"
:r "$(RepoRoot)\SQL\03_Tables.sql"
:r "$(RepoRoot)\SQL\04_Constraints.sql"
:r "$(RepoRoot)\SQL\05_Indexes.sql"
:rem 06_Seed.sql intentionally excluded; run only against a disposable/test database.
:r "$(RepoRoot)\SQL\07_Views.sql"
:r "$(RepoRoot)\SQL\08_Functions.sql"
:r "$(RepoRoot)\SQL\09_StoredProcedures.sql"
:r "$(RepoRoot)\SQL\10A_Audit.sql"
:r "$(RepoRoot)\SQL\10B_Triggers.sql"
:r "$(RepoRoot)\SQL\11A_Automation.sql"
:r "$(RepoRoot)\SQL\11B_Permissions.sql"
:r "$(RepoRoot)\SQL\12_Verify.sql"

/* ---------------------------------------------------------------------------
   Application/security/public/approval/reporting
   --------------------------------------------------------------------------- */
:r "$(RepoRoot)\SQL\13_HrmShiftMaster.sql"
:r "$(RepoRoot)\SQL\14_SecurityAuthorization.sql"
:r "$(RepoRoot)\SQL\15_PublicInformation.sql"
:r "$(RepoRoot)\SQL\16_EquipmentFlexibleImport.sql"
:r "$(RepoRoot)\SQL\17_ApprovalRouteSelection.sql"
:r "$(RepoRoot)\SQL\18_ApproverConfigurationReview.sql"
:r "$(RepoRoot)\SQL\19_OT_LimitRule_ScopeColumns.sql"
:r "$(RepoRoot)\SQL\20_Reports.sql"
:r "$(RepoRoot)\SQL\21_Verify_Reports.sql"

/* ---------------------------------------------------------------------------
   HRM attendance / leave / work-year
   --------------------------------------------------------------------------- */
:r "$(RepoRoot)\SQL\22_00_HrmAttendanceTables.sql"
:r "$(RepoRoot)\SQL\22_02_HrmCompatibleTimeKeepingForStaff.sql"
:r "$(RepoRoot)\SQL\22_03_CalculateHrmAttendance.sql"
:r "$(RepoRoot)\SQL\22_04_HrmAttendanceHistory.sql"
:r "$(RepoRoot)\SQL\23_LeaveBalanceUpgrade.sql"
:r "$(RepoRoot)\SQL\24_WorkYearUpgrade.sql"
:r "$(RepoRoot)\SQL\25_RemoveLegacyOTSync.sql"

/* ---------------------------------------------------------------------------
   Work Calendar / Action
   --------------------------------------------------------------------------- */
:r "$(RepoRoot)\SQL\26_WorkCalendarAction.sql"
:r "$(RepoRoot)\SQL\27_WorkCalendarActionIndexesSeed.sql"
:r "$(RepoRoot)\SQL\28_Verify_WorkCalendarAction.sql"

/* ---------------------------------------------------------------------------
   Equipment module database objects
   --------------------------------------------------------------------------- */
:r "$(RepoRoot)\Database\Equipment\001_Create_F03EquipmentAssets.sql"
:r "$(RepoRoot)\Database\Equipment\002_Create_F03EquipmentRequests.sql"
:r "$(RepoRoot)\Database\Equipment\003_Create_F03EquipmentRepairHistory.sql"
:r "$(RepoRoot)\Database\Equipment\004_Seed_EquipmentFunction.sql"

/* ---------------------------------------------------------------------------
   Trip module database objects
   --------------------------------------------------------------------------- */
:r "$(RepoRoot)\Database\Trips\001_Create_F03TripRequests.sql"
:r "$(RepoRoot)\Database\Trips\002_Create_F03TripActual.sql"

/* ---------------------------------------------------------------------------
   Execution reconciliation / payroll
   --------------------------------------------------------------------------- */
:r "$(RepoRoot)\SQL\29_ExecutionReconciliation.sql"
:r "$(RepoRoot)\SQL\30_Payroll.sql"
:r "$(RepoRoot)\SQL\31_Hrm_User_Approval_Provisioning.sql"
:r "$(RepoRoot)\SQL\32_ExecutionReviewSecurity.sql"
:r "$(RepoRoot)\SQL\33_DocumentationConsistency.sql"
:r "$(RepoRoot)\SQL\34_OT_Leave_Limits.sql"
:r "$(RepoRoot)\SQL\35_WorkCalendar.sql"

/* ---------------------------------------------------------------------------
   Final cross-foundation verification
   --------------------------------------------------------------------------- */
:r "$(RepoRoot)\SQL\99_Verify.sql"

PRINT N'============================================================';
PRINT N'FVN_REGISTER SQL deployment 01..35 + Database modules + 99 verification completed.';
PRINT N'============================================================';
GO

/* 99_Verify.sql remains the final cross-foundation/schema gate. */
