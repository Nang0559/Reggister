/* FVN_REGISTER — remove legacy OT synchronization procedure
   OT actual synchronization is now owned by usp_CalculateHrmAttendance.
*/
IF OBJECT_ID(N'dbo.usp_SyncOTActualHours',N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_SyncOTActualHours;
GO
