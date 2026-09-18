USE [FVN_REGISTER];
GO
SET NOCOUNT ON;

PRINT N'=== REPORTING VERIFICATION ===';

SELECT FunctionCode, FunctionName, ModuleCode, ActionCode, ScopeCode
FROM dbo.F03Functions
WHERE FunctionCode IN
(2001,2006,2101,2107,2201,2206,2301,2307,2901,2902)
ORDER BY FunctionCode;

SELECT v.ObjectName, v.ObjectType
FROM (VALUES
(N'VF03Report_LeaveRequests'),
(N'VF03Report_OTRequests'),
(N'VF03Report_Trips'),
(N'VF03Report_Equipment'),
(N'VF03Report_Attendance')
) v(ObjectName)
LEFT JOIN sys.views s ON s.name=v.ObjectName AND s.schema_id=SCHEMA_ID(N'dbo')
CROSS APPLY (SELECT CASE WHEN s.object_id IS NULL THEN N'MISSING' ELSE N'VIEW' END AS ObjectType) x
WHERE 1=1
ORDER BY v.ObjectName;

SELECT i.name AS IndexName, OBJECT_NAME(i.object_id) AS TableName
FROM sys.indexes i
WHERE i.name IN
(N'IX_F03TripRequests_Report',
 N'IX_F03AttendanceStaging_Report',
 N'IX_F03CompanyHolidays_HolidayDate',
 N'IX_F03LeaveDays_Calendar',
 N'IX_F03OTRequests_Calendar')
ORDER BY TableName, IndexName;
GO