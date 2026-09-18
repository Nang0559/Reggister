USE [FVN_REGISTER];
GO
CREATE OR ALTER VIEW leave.vw_LeaveRequest
AS
SELECT r.Id,r.RequestCode,r.EmployeeCode,e.FullName EmployeeName,
       e.DepartmentId,d.Code DeptCode,d.Name DeptName,
       r.LeaveTypeId,lt.Code LeaveTypeCode,lt.Name LeaveTypeName,
       r.WorkYearId,r.StartDate,r.EndDate,r.TotalUnits,
       r.LeavePaymentType,r.IsHalfDay,r.HalfDayOption,
       r.Status,r.SubmittedAt,r.CreatedAt,r.UpdatedAt
FROM leave.LeaveRequest r
LEFT JOIN hr.Employee e ON e.EmployeeCode=r.EmployeeCode
LEFT JOIN hr.Department d ON d.Id=e.DepartmentId
LEFT JOIN config.LeaveType lt ON lt.Id=r.LeaveTypeId;
GO
CREATE OR ALTER VIEW leave.vw_PendingApproval
AS
SELECT r.Id RequestId,r.RequestCode,r.EmployeeCode,e.FullName EmployeeName,
       a.LevelNo,a.ApproverCode,a.ApproverName,a.Decision,
       r.StartDate,r.EndDate,r.TotalUnits,r.SubmittedAt
FROM leave.LeaveRequest r
JOIN leave.Approval a ON a.LeaveRequestId=r.Id
LEFT JOIN hr.Employee e ON e.EmployeeCode=r.EmployeeCode
WHERE r.Status IN (0,1) AND a.IsRequired=1 AND a.Decision=0;
GO
CREATE OR ALTER VIEW ot.vw_OTRequest
AS
SELECT r.*,e.FullName EmployeeName,d.Code DeptCode,d.Name DeptName
FROM ot.OTRequest r
LEFT JOIN hr.Employee e ON e.EmployeeCode=r.EmployeeCode
LEFT JOIN hr.Department d ON d.Id=e.DepartmentId;
GO
CREATE OR ALTER VIEW trip.vw_TripRequest
AS
SELECT r.*,e.FullName EmployeeName,d.Code DeptCode,d.Name DeptName
FROM trip.TripRequest r
LEFT JOIN hr.Employee e ON e.EmployeeCode=r.EmployeeCode
LEFT JOIN hr.Department d ON d.Id=e.DepartmentId;
GO
