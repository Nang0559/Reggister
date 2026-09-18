USE [FVN_REGISTER];
GO
CREATE INDEX IX_UserFunction_UserId ON dbo.F03UserFunctions(IdUser);
CREATE INDEX IX_UserFunction_FunctionId ON dbo.F03UserFunctions(IdFunction);
CREATE INDEX IX_F03Session_UserId ON dbo.F03UserSessions(UserId);
CREATE INDEX IX_F03Session_DeviceId ON dbo.F03UserSessions(DeviceId);
CREATE INDEX IX_F03LeaveDay_StartTime ON dbo.F03LeaveDays(StartTime);
CREATE INDEX IX_F03LeaveDay_EmployeeCode ON dbo.F03LeaveDays(EmployeeCode);
CREATE INDEX IX_F03LeaveDayDetail_LeaveDate ON dbo.F03LeaveDayDetails(LeaveDate);
CREATE INDEX IX_F03Attendance_WorkDate ON dbo.F03AttendanceStaging(WorkDate);
CREATE INDEX IX_F03Attendance_EmployeeCode ON dbo.F03AttendanceStaging(EmployeeCode);
CREATE INDEX IX_F03OTRequest_Date ON dbo.F03OTRequests(OTDate);
CREATE INDEX IX_F03OTEmployee_RequestId ON dbo.F03OTEmployees(OTRequestId);
CREATE INDEX IX_F03OTEmployee_EmployeeCode ON dbo.F03OTEmployees(EmployeeCode);
CREATE INDEX IX_OTLimitRule_Lookup ON dbo.F03OTLimitRules(LimitType,PositionCode,DeptCode);
CREATE INDEX IX_F03TripRequest_Employee_Dates ON dbo.F03TripRequests(EmployeeCode,StartDate,EndDate);
CREATE INDEX IX_F03EquipmentAsset_Dept ON dbo.F03EquipmentAssets(DeptCode);
CREATE INDEX IX_F03EquipmentRequest_Employee_Created ON dbo.F03EquipmentRequests(EmployeeCode,CreatedAt);
CREATE INDEX IX_F03EquipmentRequest_Kind_Status ON dbo.F03EquipmentRequests(RequestKind,RequestStatus);
CREATE INDEX IX_F03EquipmentRepair_Asset_Date ON dbo.F03EquipmentRepairHistory(AssetId,RepairDate);
CREATE INDEX IX_ApprovalStep_Request ON dbo.F03ApprovalSteps(RequestType,RequestId);
CREATE INDEX IX_ApprovalStep_PendingReminder ON dbo.F03ApprovalSteps(Approved,ReminderSent);
CREATE INDEX IX_Approver_Lookup ON dbo.F03Approvers(RequestType,ApproveForDeptCode);
CREATE INDEX IX_Approver_UserId ON dbo.F03Approvers(UserId);
CREATE INDEX IX_ApprovalHistory_Request ON dbo.ApprovalHistories(RequestType,RequestId,StepId);
CREATE INDEX IX_Notification_User ON dbo.F03AppNotifications(UserId);
CREATE INDEX IX_Notification_IsRead ON dbo.F03AppNotifications(IsRead);
CREATE INDEX IX_EmailQueue_Status ON dbo.F03EmailQueues(Status);
CREATE INDEX IX_EmailLog_ToEmail ON dbo.F03EmailLogs(ToEmail);
CREATE INDEX IX_EmailLog_SentAt ON dbo.F03EmailLogs(SentAt);
CREATE INDEX IX_EscalationRule_Lookup ON dbo.F03EscalationRules(RequestModule,Level,DeptCode);
CREATE INDEX IX_EscalationLog_Request ON dbo.F03EscalationLogs(RequestModule,RequestId);
CREATE INDEX IX_AuditLog_UserId ON dbo.F03AuditLogs(UserId);
CREATE INDEX IX_AuditLog_Action ON dbo.F03AuditLogs(Action);
CREATE INDEX IX_AuditLog_CreatedAt ON dbo.F03AuditLogs(CreatedAt);
CREATE INDEX IX_StagingTrip_IsProcessed ON dbo.F03StagingTrips(IsProcessed);
CREATE INDEX IX_StagingReviewFlag_Entity ON dbo.F03SyncReviewFlag(EntityType,EntityKey,IsResolved);
CREATE INDEX IX_HrmLeaveTypeChangeLog_Pending ON dbo.HrmLeaveTypeChangeLogs(IsProcessed,ChangedAt);
GO

IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_F03AttendanceStaging_WorkDate_EmployeeCode' AND object_id=OBJECT_ID('dbo.F03AttendanceStaging'))
    CREATE INDEX IX_F03AttendanceStaging_WorkDate_EmployeeCode
    ON dbo.F03AttendanceStaging(WorkDate,EmployeeCode);
GO
