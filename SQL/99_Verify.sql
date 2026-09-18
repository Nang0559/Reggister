USE [FVN_REGISTER];
GO
SET NOCOUNT ON;
SELECT DB_NAME() AS DatabaseName;
SELECT name,type_desc FROM sys.tables WHERE name LIKE N'F03%' OR name LIKE N'Approval%' OR name LIKE N'Hrm%' ORDER BY name;
SELECT TABLE_SCHEMA,TABLE_NAME,COLUMN_NAME,DATA_TYPE,CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA=N'dbo' AND (TABLE_NAME LIKE N'F03%' OR TABLE_NAME LIKE N'Approval%' OR TABLE_NAME LIKE N'Hrm%') ORDER BY TABLE_NAME,ORDINAL_POSITION;
SELECT name,type_desc FROM sys.views WHERE name IN(N'vEmployeeApprover',N'vF03Employee',N'vF03leaveType',N'vF03LeaveRequest',N'vF03LeaveRequestDetail',N'vF03LeaveBalance',N'vF03OTRequest',N'vF03OTRequestDetail',N'vF03OTSummary',N'vF03Users',N'vw_CurrentlyPresentEmployees',N'vF03EmployeeAttendance',N'VwShiftCheckInOut');
SELECT name,type_desc FROM sys.procedures WHERE name IN(N'usp_GetPendingApproval',N'usp_DecideApproval',N'usp_SyncAttendanceStaging',N'usp_SyncOTActualHours',N'usp_DequeueEmail',N'usp_ProcessApprovalEscalation',N'usp_WriteAuditLog',N'usp_WriteUserLog');
SELECT fk.name,OBJECT_NAME(fk.parent_object_id) AS ParentTable,OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable FROM sys.foreign_keys fk ORDER BY ParentTable,fk.name;
GO
