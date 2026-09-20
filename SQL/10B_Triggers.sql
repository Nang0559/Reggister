/*
FVN_REGISTER - SQL 10 Triggers

INTENTIONAL DESIGN DECISION:
There is NO trigger from HRM -> FVN_REGISTER.

A cross-database trigger would execute in the HRM transaction and could make
HRM INSERT/UPDATE/DELETE fail when FVN_REGISTER/network/downstream services
are unavailable.

Automatic synchronization is implemented by HrmSyncBackgroundWorker in the
application, using the same IHrmSyncService engine as manual synchronization.

This file is intentionally a no-op deployment marker so the 01..12 deployment
sequence remains explicit and auditable.
*/
USE [FVN_REGISTER];
GO
PRINT N'10_Triggers: intentionally no cross-database HRM trigger.';
GO
