USE [FVN_REGISTER];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/*
===============================================================================
WORK CALENDAR FOUNDATION
===============================================================================
F03CompanyHolidays is the source of company non-working dates.
The Work Calendar is a READ projection; it does not replace Leave/OT/Trip
business rules and it does not write to HRM.

The application resolves:
  COMPANY  -> F03CompanyHolidays
  LEAVE    -> VF03LeaveRequest
  OT       -> VF03OTRequest
  TRIP     -> F03TripRequests

The following indexes support the shared calendar range queries.
===============================================================================
*/

IF OBJECT_ID(N'dbo.F03CompanyHolidays', N'U') IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_F03CompanyHolidays_HolidayDate'
      AND object_id = OBJECT_ID(N'dbo.F03CompanyHolidays'))
BEGIN
    CREATE INDEX IX_F03CompanyHolidays_HolidayDate
        ON dbo.F03CompanyHolidays(HolidayDate)
        INCLUDE (Description, Year, IsPaidLeave);
END;
GO

IF OBJECT_ID(N'dbo.F03LeaveDays', N'U') IS NOT NULL
AND COL_LENGTH(N'dbo.F03LeaveDays', N'EmployeeCode') IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_F03LeaveDays_Calendar'
      AND object_id = OBJECT_ID(N'dbo.F03LeaveDays'))
BEGIN
    CREATE INDEX IX_F03LeaveDays_Calendar
        ON dbo.F03LeaveDays(EmployeeCode, StartTime, EndTime, IsActive, RequestStatus)
        INCLUDE (LeaveTypeCode, TotalDay, TotalLeaveDay);
END;
GO

IF OBJECT_ID(N'dbo.F03OTRequests', N'U') IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_F03OTRequests_Calendar'
      AND object_id = OBJECT_ID(N'dbo.F03OTRequests'))
BEGIN
    CREATE INDEX IX_F03OTRequests_Calendar
        ON dbo.F03OTRequests(EmployeeCode, OTDate, IsActive, RequestStatus)
        INCLUDE (StartTime, EndTime, TotalOTHours, OTTypeCode);
END;
GO

IF OBJECT_ID(N'dbo.F03TripRequests', N'U') IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_F03TripRequests_Calendar'
      AND object_id = OBJECT_ID(N'dbo.F03TripRequests'))
BEGIN
    CREATE INDEX IX_F03TripRequests_Calendar
        ON dbo.F03TripRequests(EmployeeCode, StartDate, EndDate, IsActive, RequestStatus)
        INCLUDE (TripCode, Destination, Purpose);
END;
GO

/*
Calendar semantics:
  - Weekend is a presentation/default working-day rule.
  - Company holiday is calendar information.
  - Leave/OT/Trip registration remains subject to its own validator and
    authorization scope.
  - Therefore this script intentionally does NOT add a blanket constraint
    "holiday => all modules blocked".
*/
PRINT N'Work Calendar foundation indexes verified.';
GO
