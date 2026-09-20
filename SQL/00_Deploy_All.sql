/*
===============================================================================
FVN_REGISTER - MASTER SQL DEPLOYMENT
===============================================================================
Run in SSMS with SQLCMD Mode enabled.

Order:
  01..25 existing database/application foundations
  26 Shared Work Calendar + Action schema
  27 Shared Work Calendar + Action indexes / policies / seeds
  28 Shared Work Calendar + Action verification
  99 Final cross-layer/schema verification

IMPORTANT:
  06_Seed.sql is TEST/DEMO data and remains excluded.
  HRM remains READ ONLY from FVN_REGISTER.
  Existing F03AppNotifications is retained and upgraded with ActionId;
  no duplicate notification framework is introduced.
  Business modules remain source-of-truth.
===============================================================================
*/

:on error exit

:r 01_Database.sql
:r 02_Preflight.sql
:r 02_Schemas.sql
:r 03_Tables.sql
:r 04_Constraints.sql
:r 05_Indexes.sql
:rem 06_Seed.sql intentionally excluded; run only against a disposable/test database.
:r 07_Views.sql
:r 08_Functions.sql
:r 09_StoredProcedures.sql
:r 10_Audit.sql
:r 10_Triggers.sql
:r 11_Automation.sql
:r 11_Permissions.sql
:r 13_HrmShiftMaster.sql
:r 14_SecurityAuthorization.sql
:r 15_PublicInformation.sql
:r 16_EquipmentFlexibleImport.sql
:r 17_ApprovalRouteSelection.sql
:r 18_ApproverConfigurationReview.sql
:r 19_OT_LimitRule_ScopeColumns.sql
:r 13_Hrm_User_Approval_Provisioning.sql
:r 14_ExecutionReviewSecurity.sql
:r 17_DocumentationConsistency.sql
:r 18_OT_Leave_Limits.sql
:r 19_WorkCalendar.sql
:r 12_Verify.sql
:r 20_Reports.sql
:r 21_Verify_Reports.sql
:r 22_00_HrmAttendanceTables.sql
:r 22_02_HrmCompatibleTimeKeepingForStaff.sql
:r 22_03_CalculateHrmAttendance.sql
:r 22_04_HrmAttendanceHistory.sql
:r 23_LeaveBalanceUpgrade.sql
:r 24_WorkYearUpgrade.sql
:r 25_RemoveLegacyOTSync.sql
:r 26_WorkCalendarAction.sql
:r 27_WorkCalendarActionIndexesSeed.sql
:r 28_Verify_WorkCalendarAction.sql
:r 29_ExecutionReconciliation.sql
:r ../Database/Trips/002_Create_F03TripActual.sql
:r 30_Payroll.sql
:r 99_Verify.sql

PRINT N'============================================================';
PRINT N'FVN_REGISTER SQL deployment 01..29 + 99 verification completed.';
PRINT N'============================================================';
GO

/* 99_Verify.sql remains the final cross-layer/schema gate. */
