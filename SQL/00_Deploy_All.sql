/*
===============================================================================
FVN_REGISTER - MASTER SQL DEPLOYMENT
===============================================================================
Run from the repository root in SSMS with SQLCMD Mode enabled.

Recommended:
  1. Clone/pull the repository.
  2. Open this file from the local repository:
       SQL\\00_Deploy_All.sql
  3. In SSMS enable Query -> SQLCMD Mode.
  4. Execute while the SQLCMD working directory is the repository root.

All deployment scripts are referenced through SQL\\ so the master script has
one consistent path base. The Trip actual script is also referenced explicitly
from Database\\Trips.

IMPORTANT:
  06_Seed.sql is TEST/DEMO data and remains excluded.
  HRM remains READ ONLY from FVN_REGISTER.
  Existing F03AppNotifications is retained and upgraded with ActionId;
  no duplicate notification framework is introduced.
  Business modules remain source-of-truth.
===============================================================================
*/

:on error exit

:r SQL\\01_Database.sql
:r SQL\\02A_Preflight.sql
:r SQL\\02B_Schemas.sql
:r SQL\\03_Tables.sql
:r SQL\\04_Constraints.sql
:r SQL\\05_Indexes.sql
:rem 06_Seed.sql intentionally excluded; run only against a disposable/test database.
:r SQL\\07_Views.sql
:r SQL\\08_Functions.sql
:r SQL\\09_StoredProcedures.sql
:r SQL\\10A_Audit.sql
:r SQL\\10B_Triggers.sql
:r SQL\\11A_Automation.sql
:r SQL\\11B_Permissions.sql
:r SQL\\12_Verify.sql
:r SQL\\13_HrmShiftMaster.sql
:r SQL\\14_SecurityAuthorization.sql
:r SQL\\15_PublicInformation.sql
:r SQL\\16_EquipmentFlexibleImport.sql
:r SQL\\17_ApprovalRouteSelection.sql
:r SQL\\18_ApproverConfigurationReview.sql
:r SQL\\19_OT_LimitRule_ScopeColumns.sql
:r SQL\\20_Reports.sql
:r SQL\\21_Verify_Reports.sql
:r SQL\\22_00_HrmAttendanceTables.sql
:r SQL\\22_02_HrmCompatibleTimeKeepingForStaff.sql
:r SQL\\22_03_CalculateHrmAttendance.sql
:r SQL\\22_04_HrmAttendanceHistory.sql
:r SQL\\23_LeaveBalanceUpgrade.sql
:r SQL\\24_WorkYearUpgrade.sql
:r SQL\\25_RemoveLegacyOTSync.sql
:r SQL\\26_WorkCalendarAction.sql
:r SQL\\27_WorkCalendarActionIndexesSeed.sql
:r SQL\\28_Verify_WorkCalendarAction.sql
:r Database\\Trips\\002_Create_F03TripActual.sql
:r SQL\\29_ExecutionReconciliation.sql
:r SQL\\30_Payroll.sql
:r SQL\\31_Hrm_User_Approval_Provisioning.sql
:r SQL\\32_ExecutionReviewSecurity.sql
:r SQL\\33_DocumentationConsistency.sql
:r SQL\\34_OT_Leave_Limits.sql
:r SQL\\35_WorkCalendar.sql
:r SQL\\99_Verify.sql

PRINT N'============================================================';
PRINT N'FVN_REGISTER SQL deployment 01..35 + 99 verification completed.';
PRINT N'============================================================';
GO

/* 99_Verify.sql remains the final cross-foundation/schema gate. */
