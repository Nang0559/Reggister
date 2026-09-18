USE [FVN_REGISTER];
GO
IF OBJECT_ID(N'auth.UserRole','U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_UserRole_User') ALTER TABLE auth.UserRole ADD CONSTRAINT FK_UserRole_User FOREIGN KEY(UserId) REFERENCES auth.UserAccount(Id);
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_UserRole_Role') ALTER TABLE auth.UserRole ADD CONSTRAINT FK_UserRole_Role FOREIGN KEY(RoleId) REFERENCES auth.Role(Id);
END
IF OBJECT_ID(N'auth.RolePermission','U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_RolePermission_Role') ALTER TABLE auth.RolePermission ADD CONSTRAINT FK_RolePermission_Role FOREIGN KEY(RoleId) REFERENCES auth.Role(Id);
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_RolePermission_Permission') ALTER TABLE auth.RolePermission ADD CONSTRAINT FK_RolePermission_Permission FOREIGN KEY(PermissionId) REFERENCES auth.Permission(Id);
END
IF OBJECT_ID(N'hr.Department','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_Department_Parent') ALTER TABLE hr.Department ADD CONSTRAINT FK_Department_Parent FOREIGN KEY(ParentId) REFERENCES hr.Department(Id);
IF OBJECT_ID(N'hr.Employee','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_Employee_Department') ALTER TABLE hr.Employee ADD CONSTRAINT FK_Employee_Department FOREIGN KEY(DepartmentId) REFERENCES hr.Department(Id);
IF OBJECT_ID(N'config.Approver','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_Approver_Level') ALTER TABLE config.Approver ADD CONSTRAINT FK_Approver_Level FOREIGN KEY(ApprovalLevelId) REFERENCES config.ApprovalLevel(Id);
IF OBJECT_ID(N'leave.LeaveRequest','U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_LeaveRequest_LeaveType') ALTER TABLE leave.LeaveRequest ADD CONSTRAINT FK_LeaveRequest_LeaveType FOREIGN KEY(LeaveTypeId) REFERENCES config.LeaveType(Id);
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_LeaveRequest_WorkYear') ALTER TABLE leave.LeaveRequest ADD CONSTRAINT FK_LeaveRequest_WorkYear FOREIGN KEY(WorkYearId) REFERENCES config.WorkYear(Id);
END
IF OBJECT_ID(N'leave.LeaveAttachment','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_LeaveAttachment_Request') ALTER TABLE leave.LeaveAttachment ADD CONSTRAINT FK_LeaveAttachment_Request FOREIGN KEY(LeaveRequestId) REFERENCES leave.LeaveRequest(Id) ON DELETE CASCADE;
IF OBJECT_ID(N'leave.Approval','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_LeaveApproval_Request') ALTER TABLE leave.Approval ADD CONSTRAINT FK_LeaveApproval_Request FOREIGN KEY(LeaveRequestId) REFERENCES leave.LeaveRequest(Id) ON DELETE CASCADE;
IF OBJECT_ID(N'ot.Approval','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_OTApproval_Request') ALTER TABLE ot.Approval ADD CONSTRAINT FK_OTApproval_Request FOREIGN KEY(OTRequestId) REFERENCES ot.OTRequest(Id) ON DELETE CASCADE;
IF OBJECT_ID(N'trip.Approval','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_TripApproval_Request') ALTER TABLE trip.Approval ADD CONSTRAINT FK_TripApproval_Request FOREIGN KEY(TripRequestId) REFERENCES trip.TripRequest(Id) ON DELETE CASCADE;
GO
