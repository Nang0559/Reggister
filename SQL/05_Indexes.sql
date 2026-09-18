USE [FVN_REGISTER];
GO
CREATE INDEX IX_Employee_Department ON hr.Employee(DepartmentId,IsActive) WHERE DepartmentId IS NOT NULL;
CREATE INDEX IX_LeaveRequest_Employee_Status ON leave.LeaveRequest(EmployeeCode,Status,StartDate DESC);
CREATE INDEX IX_LeaveRequest_Status_Submitted ON leave.LeaveRequest(Status,SubmittedAt DESC);
CREATE INDEX IX_LeaveApproval_Approver_Decision ON leave.Approval(ApproverCode,Decision,LevelNo);
CREATE INDEX IX_OTRequest_Employee_Status ON ot.OTRequest(EmployeeCode,Status,WorkDate DESC);
CREATE INDEX IX_OTApproval_Approver_Decision ON ot.Approval(ApproverCode,Decision,LevelNo);
CREATE INDEX IX_TripRequest_Employee_Status ON trip.TripRequest(EmployeeCode,Status,FromDate DESC);
CREATE INDEX IX_TripApproval_Approver_Decision ON trip.Approval(ApproverCode,Decision,LevelNo);
CREATE INDEX IX_Notification_Employee_Read ON notify.Notification(EmployeeCode,IsRead,CreatedAt DESC);
CREATE INDEX IX_EmailQueue_Status_Created ON notify.EmailQueue(Status,CreatedAt);
GO
