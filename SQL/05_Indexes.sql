USE [FVN_REGISTER];
GO
SET NOCOUNT ON;
GO
IF OBJECT_ID(N'dbo.F03UserFunctions',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_UserFunction_UserId' AND object_id=OBJECT_ID(N'dbo.F03UserFunctions',N'U'))
    CREATE INDEX [IX_UserFunction_UserId] ON dbo.[F03UserFunctions](IdUser);
GO
IF OBJECT_ID(N'dbo.F03UserFunctions',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_UserFunction_FunctionId' AND object_id=OBJECT_ID(N'dbo.F03UserFunctions',N'U'))
    CREATE INDEX [IX_UserFunction_FunctionId] ON dbo.[F03UserFunctions](IdFunction);
GO
IF OBJECT_ID(N'dbo.F03UserSessions',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_F03Session_UserId' AND object_id=OBJECT_ID(N'dbo.F03UserSessions',N'U'))
    CREATE INDEX [IX_F03Session_UserId] ON dbo.[F03UserSessions](UserId);
GO
IF OBJECT_ID(N'dbo.F03UserSessions',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_F03Session_DeviceId' AND object_id=OBJECT_ID(N'dbo.F03UserSessions',N'U'))
    CREATE INDEX [IX_F03Session_DeviceId] ON dbo.[F03UserSessions](DeviceId);
GO
IF OBJECT_ID(N'dbo.F03LeaveDays',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_F03LeaveDay_StartTime' AND object_id=OBJECT_ID(N'dbo.F03LeaveDays',N'U'))
    CREATE INDEX [IX_F03LeaveDay_StartTime] ON dbo.[F03LeaveDays](StartTime);
GO
IF OBJECT_ID(N'dbo.F03LeaveDays',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_F03LeaveDay_EmployeeCode' AND object_id=OBJECT_ID(N'dbo.F03LeaveDays',N'U'))
    CREATE INDEX [IX_F03LeaveDay_EmployeeCode] ON dbo.[F03LeaveDays](EmployeeCode);
GO
IF OBJECT_ID(N'dbo.F03LeaveDayDetails',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_F03LeaveDayDetail_LeaveDate' AND object_id=OBJECT_ID(N'dbo.F03LeaveDayDetails',N'U'))
    CREATE INDEX [IX_F03LeaveDayDetail_LeaveDate] ON dbo.[F03LeaveDayDetails](LeaveDate);
GO
IF OBJECT_ID(N'dbo.F03AttendanceStaging',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_F03Attendance_WorkDate' AND object_id=OBJECT_ID(N'dbo.F03AttendanceStaging',N'U'))
    CREATE INDEX [IX_F03Attendance_WorkDate] ON dbo.[F03AttendanceStaging](WorkDate);
GO
IF OBJECT_ID(N'dbo.F03AttendanceStaging',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_F03Attendance_EmployeeCode' AND object_id=OBJECT_ID(N'dbo.F03AttendanceStaging',N'U'))
    CREATE INDEX [IX_F03Attendance_EmployeeCode] ON dbo.[F03AttendanceStaging](EmployeeCode);
GO
IF OBJECT_ID(N'dbo.F03OTRequests',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_F03OTRequest_Date' AND object_id=OBJECT_ID(N'dbo.F03OTRequests',N'U'))
    CREATE INDEX [IX_F03OTRequest_Date] ON dbo.[F03OTRequests](OTDate);
GO
IF OBJECT_ID(N'dbo.F03OTEmployees',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_F03OTEmployee_RequestId' AND object_id=OBJECT_ID(N'dbo.F03OTEmployees',N'U'))
    CREATE INDEX [IX_F03OTEmployee_RequestId] ON dbo.[F03OTEmployees](OTRequestId);
GO
IF OBJECT_ID(N'dbo.F03OTEmployees',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_F03OTEmployee_EmployeeCode' AND object_id=OBJECT_ID(N'dbo.F03OTEmployees',N'U'))
    CREATE INDEX [IX_F03OTEmployee_EmployeeCode] ON dbo.[F03OTEmployees](EmployeeCode);
GO
IF OBJECT_ID(N'dbo.F03OTLimitRules',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_OTLimitRule_Lookup' AND object_id=OBJECT_ID(N'dbo.F03OTLimitRules',N'U'))
    CREATE INDEX [IX_OTLimitRule_Lookup] ON dbo.[F03OTLimitRules](LimitType,PositionCode,DeptCode);
GO
IF OBJECT_ID(N'dbo.F03TripRequests',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_F03TripRequest_Employee_Dates' AND object_id=OBJECT_ID(N'dbo.F03TripRequests',N'U'))
    CREATE INDEX [IX_F03TripRequest_Employee_Dates] ON dbo.[F03TripRequests](EmployeeCode,StartDate,EndDate);
GO
IF OBJECT_ID(N'dbo.F03EquipmentAssets',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_F03EquipmentAsset_Dept' AND object_id=OBJECT_ID(N'dbo.F03EquipmentAssets',N'U'))
    CREATE INDEX [IX_F03EquipmentAsset_Dept] ON dbo.[F03EquipmentAssets](DeptCode);
GO
IF OBJECT_ID(N'dbo.F03EquipmentRequests',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_F03EquipmentRequest_Employee_Created' AND object_id=OBJECT_ID(N'dbo.F03EquipmentRequests',N'U'))
    CREATE INDEX [IX_F03EquipmentRequest_Employee_Created] ON dbo.[F03EquipmentRequests](EmployeeCode,CreatedAt);
GO
IF OBJECT_ID(N'dbo.F03EquipmentRequests',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_F03EquipmentRequest_Kind_Status' AND object_id=OBJECT_ID(N'dbo.F03EquipmentRequests',N'U'))
    CREATE INDEX [IX_F03EquipmentRequest_Kind_Status] ON dbo.[F03EquipmentRequests](RequestKind,RequestStatus);
GO
IF OBJECT_ID(N'dbo.F03EquipmentRepairHistory',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_F03EquipmentRepair_Asset_Date' AND object_id=OBJECT_ID(N'dbo.F03EquipmentRepairHistory',N'U'))
    CREATE INDEX [IX_F03EquipmentRepair_Asset_Date] ON dbo.[F03EquipmentRepairHistory](AssetId,RepairDate);
GO
IF OBJECT_ID(N'dbo.F03ApprovalSteps',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_ApprovalStep_Request' AND object_id=OBJECT_ID(N'dbo.F03ApprovalSteps',N'U'))
    CREATE INDEX [IX_ApprovalStep_Request] ON dbo.[F03ApprovalSteps](RequestType,RequestId);
GO
IF OBJECT_ID(N'dbo.F03ApprovalSteps',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_ApprovalStep_PendingReminder' AND object_id=OBJECT_ID(N'dbo.F03ApprovalSteps',N'U'))
    CREATE INDEX [IX_ApprovalStep_PendingReminder] ON dbo.[F03ApprovalSteps](Approved,ReminderSent);
GO
IF OBJECT_ID(N'dbo.F03Approvers',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_Approver_Lookup' AND object_id=OBJECT_ID(N'dbo.F03Approvers',N'U'))
    CREATE INDEX [IX_Approver_Lookup] ON dbo.[F03Approvers](RequestType,ApproveForDeptCode);
GO
IF OBJECT_ID(N'dbo.F03Approvers',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_Approver_UserId' AND object_id=OBJECT_ID(N'dbo.F03Approvers',N'U'))
    CREATE INDEX [IX_Approver_UserId] ON dbo.[F03Approvers](UserId);
GO
IF OBJECT_ID(N'dbo.ApprovalHistories',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_ApprovalHistory_Request' AND object_id=OBJECT_ID(N'dbo.ApprovalHistories',N'U'))
    CREATE INDEX [IX_ApprovalHistory_Request] ON dbo.[ApprovalHistories](RequestType,RequestId,StepId);
GO
IF OBJECT_ID(N'dbo.F03AppNotifications',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_Notification_User' AND object_id=OBJECT_ID(N'dbo.F03AppNotifications',N'U'))
    CREATE INDEX [IX_Notification_User] ON dbo.[F03AppNotifications](UserId);
GO
IF OBJECT_ID(N'dbo.F03AppNotifications',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_Notification_IsRead' AND object_id=OBJECT_ID(N'dbo.F03AppNotifications',N'U'))
    CREATE INDEX [IX_Notification_IsRead] ON dbo.[F03AppNotifications](IsRead);
GO
IF OBJECT_ID(N'dbo.F03EmailQueues',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_EmailQueue_Status' AND object_id=OBJECT_ID(N'dbo.F03EmailQueues',N'U'))
    CREATE INDEX [IX_EmailQueue_Status] ON dbo.[F03EmailQueues](Status);
GO
IF OBJECT_ID(N'dbo.F03EmailLogs',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_EmailLog_ToEmail' AND object_id=OBJECT_ID(N'dbo.F03EmailLogs',N'U'))
    CREATE INDEX [IX_EmailLog_ToEmail] ON dbo.[F03EmailLogs](ToEmail);
GO
IF OBJECT_ID(N'dbo.F03EmailLogs',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_EmailLog_SentAt' AND object_id=OBJECT_ID(N'dbo.F03EmailLogs',N'U'))
    CREATE INDEX [IX_EmailLog_SentAt] ON dbo.[F03EmailLogs](SentAt);
GO
IF OBJECT_ID(N'dbo.F03EscalationRules',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_EscalationRule_Lookup' AND object_id=OBJECT_ID(N'dbo.F03EscalationRules',N'U'))
    CREATE INDEX [IX_EscalationRule_Lookup] ON dbo.[F03EscalationRules](RequestModule,Level,DeptCode);
GO
IF OBJECT_ID(N'dbo.F03EscalationLogs',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_EscalationLog_Request' AND object_id=OBJECT_ID(N'dbo.F03EscalationLogs',N'U'))
    CREATE INDEX [IX_EscalationLog_Request] ON dbo.[F03EscalationLogs](RequestModule,RequestId);
GO
IF OBJECT_ID(N'dbo.F03AuditLogs',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_AuditLog_UserId' AND object_id=OBJECT_ID(N'dbo.F03AuditLogs',N'U'))
    CREATE INDEX [IX_AuditLog_UserId] ON dbo.[F03AuditLogs](UserId);
GO
IF OBJECT_ID(N'dbo.F03AuditLogs',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_AuditLog_Action' AND object_id=OBJECT_ID(N'dbo.F03AuditLogs',N'U'))
    CREATE INDEX [IX_AuditLog_Action] ON dbo.[F03AuditLogs](Action);
GO
IF OBJECT_ID(N'dbo.F03AuditLogs',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_AuditLog_CreatedAt' AND object_id=OBJECT_ID(N'dbo.F03AuditLogs',N'U'))
    CREATE INDEX [IX_AuditLog_CreatedAt] ON dbo.[F03AuditLogs](CreatedAt);
GO
IF OBJECT_ID(N'dbo.F03StagingTrips',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_StagingTrip_IsProcessed' AND object_id=OBJECT_ID(N'dbo.F03StagingTrips',N'U'))
    CREATE INDEX [IX_StagingTrip_IsProcessed] ON dbo.[F03StagingTrips](IsProcessed);
GO
IF OBJECT_ID(N'dbo.F03SyncReviewFlag',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_StagingReviewFlag_Entity' AND object_id=OBJECT_ID(N'dbo.F03SyncReviewFlag',N'U'))
    CREATE INDEX [IX_StagingReviewFlag_Entity] ON dbo.[F03SyncReviewFlag](EntityType,EntityKey,IsResolved);
GO
IF OBJECT_ID(N'dbo.HrmLeaveTypeChangeLogs',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_HrmLeaveTypeChangeLog_Pending' AND object_id=OBJECT_ID(N'dbo.HrmLeaveTypeChangeLogs',N'U'))
    CREATE INDEX [IX_HrmLeaveTypeChangeLog_Pending] ON dbo.[HrmLeaveTypeChangeLogs](IsProcessed,ChangedAt);
GO
IF OBJECT_ID(N'dbo.F03AttendanceStaging',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_F03AttendanceStaging_WorkDate_EmployeeCode' AND object_id=OBJECT_ID(N'dbo.F03AttendanceStaging'))
    CREATE INDEX IX_F03AttendanceStaging_WorkDate_EmployeeCode ON dbo.F03AttendanceStaging(WorkDate,EmployeeCode);
GO
IF OBJECT_ID(N'dbo.F03StagingLeaveType',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_F03StagingLeaveType_Pending_Key' AND object_id=OBJECT_ID(N'dbo.F03StagingLeaveType'))
    CREATE INDEX [IX_F03StagingLeaveType_Pending_Key] ON dbo.[F03StagingLeaveType](IsProcessed,EntityKey,CreatedAt,Id);
GO
IF OBJECT_ID(N'dbo.F03StagingDepartment',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_F03StagingDepartment_Pending_Key' AND object_id=OBJECT_ID(N'dbo.F03StagingDepartment'))
    CREATE INDEX [IX_F03StagingDepartment_Pending_Key] ON dbo.[F03StagingDepartment](IsProcessed,EntityKey,CreatedAt,Id);
GO
IF OBJECT_ID(N'dbo.F03StagingPosition',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_F03StagingPosition_Pending_Key' AND object_id=OBJECT_ID(N'dbo.F03StagingPosition'))
    CREATE INDEX [IX_F03StagingPosition_Pending_Key] ON dbo.[F03StagingPosition](IsProcessed,EntityKey,CreatedAt,Id);
GO
IF OBJECT_ID(N'dbo.F03StagingEmployee',N'U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_F03StagingEmployee_Pending_Key' AND object_id=OBJECT_ID(N'dbo.F03StagingEmployee'))
    CREATE INDEX [IX_F03StagingEmployee_Pending_Key] ON dbo.[F03StagingEmployee](IsProcessed,EntityKey,CreatedAt,Id);
GO


