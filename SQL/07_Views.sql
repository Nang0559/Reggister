USE [FVN_REGISTER];
GO
CREATE OR ALTER VIEW dbo.vEmployeeApprover AS
SELECT a.Id,a.ApproveForDeptCode AS DeptCode,d.DeptName,a.Level AS ApproveLevel,a.RoleName,a.ApproverName,a.ApproverEmail,CONVERT(varchar(30),a.Level) AS ApproveLevelCode,a.IsActive
FROM dbo.F03Approvers a LEFT JOIN dbo.F03Departments d ON d.DeptCode=a.ApproveForDeptCode WHERE a.IsActive=1;
GO
CREATE OR ALTER VIEW dbo.vF03Employee AS
SELECT e.Id,e.EmployeeCode,e.EmployeeName,d.DeptName,e.BirthDate,e.GenderCode,g.GenderName,e.EmailAddress,e.PhoneNumber,e.FirstWorkingDate,e.EndWorkingDate,
e.TotalLeaveDays AS TongPhep,e.EmployeeNo,e.CreatedBy,e.CreatedAt,ISNULL(e.ModifiedBy,0) ModifiedBy,ISNULL(e.ModifiedAt,e.CreatedAt) ModifiedAt,
p.PositionName AS CVName,p.PositionCode AS CVCode,e.DeptCode,e.IsActive
FROM dbo.F03Employees e LEFT JOIN dbo.F03Departments d ON d.DeptCode=e.DeptCode LEFT JOIN dbo.F03Genders g ON g.Id=e.GenderCode LEFT JOIN dbo.F03Positions p ON p.PositionCode=e.PositionCode;
GO
CREATE OR ALTER VIEW dbo.vF03leaveType AS
SELECT Id AS LeaveTypeId,LeaveTypeCode,LeaveTypeName,LeaveTypeName2,IsCountedAsLeave,
CASE WHEN IsCountedAsLeave=1 THEN N'Có tính phép' ELSE N'Không tính phép' END AS CountedAsLeaveName,HRMCode AS Hrmcode,IsActive,CreatedBy,CreatedAt,ModifiedBy,ModifiedAt FROM dbo.F03LeaveType;
GO
CREATE OR ALTER VIEW dbo.vF03LeaveRequest AS
SELECT l.Id,l.Id AS LeaveCode,l.WorkYear,l.EmployeeCode,e.EmployeeName,g.GenderName,l.DeptCode,d.DeptName,e.EmailAddress,l.CreatedAt RegisterDate,l.StartTime StartDate,l.EndTime EndDate,
l.TotalDay,l.TotalLeaveDay,l.LeaveTypeCode,lt.LeaveTypeName,l.LeaveReason,
CASE l.RequestStatus WHEN 0 THEN N'Draft' WHEN 1 THEN N'Pending' WHEN 2 THEN N'InProgress' WHEN 3 THEN N'Approved' WHEN 4 THEN N'Rejected' WHEN 5 THEN N'Cancelled' WHEN 6 THEN N'Escalated' WHEN 7 THEN N'NeedsRevision' END RequestStatus,
p.PositionCode,p.PositionName,l.IsActive,l.CreatedAt,l.ModifiedAt,
(SELECT STUFF((SELECT N', ' + a2.FileName FROM dbo.F03Attachment a2 WHERE a2.Module=0 AND a2.RequestId=l.Id AND a2.IsActive=1 FOR XML PATH(N''), TYPE).value(N'.',N'nvarchar(max)'),1,2,N'')) AttachedDocuments
FROM dbo.F03LeaveDays l LEFT JOIN dbo.F03Employees e ON e.EmployeeCode=l.EmployeeCode LEFT JOIN dbo.F03Departments d ON d.DeptCode=l.DeptCode LEFT JOIN dbo.F03Genders g ON g.Id=e.GenderCode LEFT JOIN dbo.F03Positions p ON p.PositionCode=e.PositionCode LEFT JOIN dbo.F03LeaveType lt ON lt.LeaveTypeCode=l.LeaveTypeCode;
GO
CREATE OR ALTER VIEW dbo.vF03LeaveRequestDetail AS
SELECT CAST(ROW_NUMBER() OVER(ORDER BY l.Id,dd.Id) AS bigint) RowId,l.Id LeaveId,l.EmployeeCode,e.EmployeeName,l.DeptCode,d.DeptName,
CASE l.RequestStatus WHEN 0 THEN N'Draft' WHEN 1 THEN N'Pending' WHEN 2 THEN N'InProgress' WHEN 3 THEN N'Approved' WHEN 4 THEN N'Rejected' WHEN 5 THEN N'Cancelled' WHEN 6 THEN N'Escalated' WHEN 7 THEN N'NeedsRevision' END RequestStatus,
l.StartTime StartDate,l.EndTime EndDate,dd.Id DetailId,CAST(dd.LeaveDate AS date) LeaveDate,dd.LeaveTypeCode,dd.LeaveTypeName,dd.IsCountedAsLeave,dd.IsHalfDay,dd.HalfDayOption,dd.DayValue,dd.CreatedAt DetailCreatedAt,dd.CreatedBy DetailCreatedBy
FROM dbo.F03LeaveDays l JOIN dbo.F03LeaveDayDetails dd ON dd.LeaveDaysId=l.Id LEFT JOIN dbo.F03Employees e ON e.EmployeeCode=l.EmployeeCode LEFT JOIN dbo.F03Departments d ON d.DeptCode=l.DeptCode;
GO
CREATE OR ALTER VIEW dbo.vF03LeaveBalance AS
SELECT b.Id,b.EmployeeCode,e.EmployeeName,e.DeptCode,d.DeptName,e.GenderCode,g.GenderName,b.WorkYear,b.TotalDays TotalEntitledLeave,
ISNULL((SELECT SUM(x.DayValue) FROM dbo.F03LeaveDayDetails x JOIN dbo.F03LeaveDays r ON r.Id=x.LeaveDaysId WHERE r.EmployeeCode=b.EmployeeCode AND r.WorkYear=b.WorkYear AND r.RequestStatus=3),0) TotalDaysOff,
ISNULL((SELECT SUM(x.DayValue) FROM dbo.F03LeaveDayDetails x JOIN dbo.F03LeaveDays r ON r.Id=x.LeaveDaysId WHERE r.EmployeeCode=b.EmployeeCode AND r.WorkYear=b.WorkYear AND r.RequestStatus=3 AND x.IsCountedAsLeave=1),0) LeaveDaysUsed,
b.TotalDays-ISNULL((SELECT SUM(x.DayValue) FROM dbo.F03LeaveDayDetails x JOIN dbo.F03LeaveDays r ON r.Id=x.LeaveDaysId WHERE r.EmployeeCode=b.EmployeeCode AND r.WorkYear=b.WorkYear AND r.RequestStatus=3 AND x.IsCountedAsLeave=1),0) RemainingLeave,
b.IsActive,b.CreatedBy,b.CreatedAt,ISNULL(b.ModifiedBy,0) ModifiedBy,ISNULL(b.ModifiedAt,b.CreatedAt) ModifiedAt
FROM dbo.F03LeaveBalances b LEFT JOIN dbo.F03Employees e ON e.EmployeeCode=b.EmployeeCode LEFT JOIN dbo.F03Departments d ON d.DeptCode=e.DeptCode LEFT JOIN dbo.F03Genders g ON g.Id=e.GenderCode;
GO
CREATE OR ALTER VIEW dbo.vF03OTRequest AS
SELECT r.Id,r.OTCode,2 RequestType,r.EmployeeCode,e.EmployeeName,r.CreatedByEmail,r.DeptCode,d.DeptName,r.OTDate,CAST(r.StartTime AS time) StartTime,CAST(r.EndTime AS time) EndTime,
r.PlannedHours,r.TotalOTHours,r.OTTypeCode,t.OTTypeName,t.RateMultiplier,r.OTReasonSummary,r.ScopeType,r.RequestStatus,
(SELECT COUNT(*) FROM dbo.F03OTEmployees x WHERE x.OTRequestId=r.Id) EmployeeCount,r.CreatedAt,r.IsActive
FROM dbo.F03OTRequests r LEFT JOIN dbo.F03Employees e ON e.EmployeeCode=r.EmployeeCode LEFT JOIN dbo.F03Departments d ON d.DeptCode=r.DeptCode LEFT JOIN dbo.F03OTTypes t ON t.OTTypeCode=r.OTTypeCode;
GO
CREATE OR ALTER VIEW dbo.vF03OTRequestDetail AS
SELECT CAST(ROW_NUMBER() OVER(ORDER BY r.Id,o.Id) AS bigint) RowId,r.Id OTRequestId,r.OTCode,o.EmployeeCode,o.EmployeeName,o.DeptCode,o.DeptName,o.Id DetailId,CAST(r.OTDate AS date) OTDate,
CAST(o.StartTime AS time) StartTime,CAST(o.EndTime AS time) EndTime,o.OTHours,o.CvCode,o.OTTypeCode EmpOTTypeCode,o.OTRateMultiplier,o.ActualHours,CAST(o.ActualStartTime AS time) ActualStartTime,CAST(o.ActualEndTime AS time) ActualEndTime,
CONVERT(varchar(20),o.ValidationStatus) ValidationStatus,o.ValidationMessage,o.Note,o.OTReasonCategoryCode,o.OTReasonDetail,r.RequestStatus,o.IsActive,o.CreatedAt,o.CreatedBy
FROM dbo.F03OTRequests r JOIN dbo.F03OTEmployees o ON o.OTRequestId=r.Id;
GO
CREATE OR ALTER VIEW dbo.vF03OTSummary AS
SELECT o.EmployeeCode,MAX(o.EmployeeName) EmployeeName,MAX(o.DeptCode) DeptCode,MAX(o.DeptName) DeptName,YEAR(r.OTDate) WorkYear,
SUM(CASE WHEN r.RequestStatus=3 THEN o.OTHours ELSE 0 END) TotalApprovedHours,SUM(CASE WHEN r.RequestStatus IN(1,2,6,7) THEN o.OTHours ELSE 0 END) TotalPendingHours
FROM dbo.F03OTEmployees o JOIN dbo.F03OTRequests r ON r.Id=o.OTRequestId GROUP BY o.EmployeeCode,YEAR(r.OTDate);
GO
CREATE OR ALTER VIEW dbo.vF03Users AS
SELECT u.IdUser,COALESCE(u.EmployeeCode,CONVERT(nvarchar(50),u.IdUser)) UserName,u.Password,u.Avatar,u.EmployeeCode,e.EmployeeName,u.DeptCode,d.DeptName,e.GenderCode,g.GenderName,e.BirthDate,e.EmailAddress,e.PhoneNumber,
u.PermissionCode,p.PermissionName,u.LastLogin,u.LockoutEnable,u.LockoutEndDate,u.NumLoginFailed,u.IsActive,u.CreatedBy,u.CreatedAt,u.ModifiedBy,u.ModifiedAt,u.Cvcode,u.LevelApprove
FROM dbo.F03Users u LEFT JOIN dbo.F03Employees e ON e.EmployeeCode=u.EmployeeCode LEFT JOIN dbo.F03Departments d ON d.DeptCode=u.DeptCode LEFT JOIN dbo.F03Genders g ON g.Id=e.GenderCode LEFT JOIN dbo.F03Permissions p ON p.PermissionCode=u.PermissionCode;
GO
CREATE OR ALTER VIEW dbo.vw_CurrentlyPresentEmployees AS
SELECT a.EmployeeCode EmployeeID,COALESCE(a.FullName,e.EmployeeName) FullName,a.DeptCode,d.DeptName,CAST(a.WorkDate AS date) Date,a.CheckInDateTime CheckInTime
FROM dbo.F03AttendanceStaging a LEFT JOIN dbo.F03Employees e ON e.EmployeeCode=a.EmployeeCode LEFT JOIN dbo.F03Departments d ON d.DeptCode=a.DeptCode
WHERE a.CheckInDateTime IS NOT NULL AND a.CheckOutDateTime IS NULL;
GO
CREATE OR ALTER VIEW dbo.vF03EmployeeAttendance AS
SELECT a.EmployeeCode EmployeeID,COALESCE(a.FullName,e.EmployeeName) FullName,COALESCE(TRY_CONVERT(int,a.DeptCode),0) Department,CAST(a.WorkDate AS date) Date,a.CheckInDateTime CheckInTime,a.CheckOutDateTime CheckOutTime
FROM dbo.F03AttendanceStaging a LEFT JOIN dbo.F03Employees e ON e.EmployeeCode=a.EmployeeCode;
GO
CREATE OR ALTER VIEW dbo.VwShiftCheckInOut AS
SELECT a.EmployeeCode EmployeeID,COALESCE(a.FullName,e.EmployeeName) FullName,COALESCE(TRY_CONVERT(int,a.DeptCode),0) Department,CAST(a.WorkDate AS date) Date,a.CheckInDateTime CheckInTime,a.CheckOutDateTime CheckOutTime
FROM dbo.F03AttendanceStaging a LEFT JOIN dbo.F03Employees e ON e.EmployeeCode=a.EmployeeCode;
GO
