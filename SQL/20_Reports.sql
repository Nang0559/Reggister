USE [FVN_REGISTER];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

/*
  FVN_REGISTER REPORTING READ MODEL
  ---------------------------------
  These views are reporting projections only.
  They do not replace service authorization and do not grant access by
  themselves. API/service layer MUST apply the current user's capability
  and data scope before querying/returning report data.
*/

CREATE OR ALTER VIEW dbo.VF03Report_LeaveRequests
AS
SELECT
    l.Id AS RequestId,
    l.EmployeeCode,
    l.DeptCode,
    l.WorkYear,
    l.StartTime,
    l.EndTime,
    l.TotalDay,
    l.TotalLeaveDay,
    l.LeaveTypeCode,
    l.RequestStatus,
    l.IsActive,
    l.CreatedAt,
    l.ModifiedAt
FROM dbo.F03LeaveDays l;
GO

CREATE OR ALTER VIEW dbo.VF03Report_OTRequests
AS
SELECT
    o.Id AS RequestId,
    o.EmployeeCode,
    o.DeptCode,
    o.OTCode,
    o.OTDate,
    o.StartTime,
    o.EndTime,
    o.PlannedHours,
    o.TotalOTHours,
    o.OTTypeCode,
    o.OTReasonSummary,
    o.RequestStatus,
    o.IsActive,
    o.CreatedAt,
    o.ModifiedAt
FROM dbo.F03OTRequests o;
GO

CREATE OR ALTER VIEW dbo.VF03Report_Trips
AS
SELECT
    t.Id AS RequestId,
    t.EmployeeCode,
    t.DeptCode,
    t.TripCode,
    t.StartDate,
    t.EndDate,
    t.Destination,
    t.Purpose,
    t.CustomerOrPartner,
    t.TransportMethod,
    t.EstimatedCost,
    t.RequestStatus,
    t.IsActive,
    t.CreatedAt,
    t.ModifiedAt
FROM dbo.F03TripRequests t;
GO

CREATE OR ALTER VIEW dbo.VF03Report_Equipment
AS
SELECT
    e.Id AS AssetId,
    e.EquipmentCode,
    e.EquipmentName,
    e.DeptCode,
    e.SerialNumber,
    e.AssetCode,
    e.PurchasePrice,
    e.PurchaseDate,
    e.ExpectedDepreciationDate,
    e.Location,
    e.IsQrActive,
    e.IsActive,
    e.CreatedAt,
    e.ModifiedAt
FROM dbo.F03EquipmentAssets e;
GO

CREATE OR ALTER VIEW dbo.VF03Report_Attendance
AS
SELECT
    a.Id,
    a.WorkDate,
    a.EmployeeCode,
    a.DeptCode,
    a.DeptName,
    a.FullName,
    a.ShiftCode,
    a.ShiftName,
    a.CheckInDateTime,
    a.CheckOutDateTime,
    a.TotalHours,
    a.OtHours,
    a.IsHoliday,
    a.HolidayType,
    a.ShiftType,
    a.SyncedAt
FROM dbo.F03AttendanceStaging a;
GO

/* Supporting indexes are created only when the target columns exist. */
IF OBJECT_ID(N'dbo.F03TripRequests',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03TripRequests_Report' AND object_id=OBJECT_ID(N'dbo.F03TripRequests'))
    CREATE INDEX IX_F03TripRequests_Report ON dbo.F03TripRequests(DeptCode,StartDate,EndDate,RequestStatus,IsActive) INCLUDE(EmployeeCode,EstimatedCost);
GO

IF OBJECT_ID(N'dbo.F03AttendanceStaging',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03AttendanceStaging_Report' AND object_id=OBJECT_ID(N'dbo.F03AttendanceStaging'))
    CREATE INDEX IX_F03AttendanceStaging_Report ON dbo.F03AttendanceStaging(DeptCode,WorkDate,EmployeeCode) INCLUDE(TotalHours,OtHours,CheckInDateTime,CheckOutDateTime);
GO

PRINT N'Reporting views/indexes created.';
GO