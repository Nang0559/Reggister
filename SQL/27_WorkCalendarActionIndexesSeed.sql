USE [FVN_REGISTER];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/*
===============================================================================
27_WORK_CALENDAR_ACTION_INDEXES_SEED
===============================================================================
Indexes / FK integrity / default shared module and action policy.
No demo business transactions are inserted here.
===============================================================================
*/

IF OBJECT_ID(N'dbo.F03CalendarModuleDefinitions',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03CalendarModuleDefinitions_ModuleCode'
               AND object_id=OBJECT_ID(N'dbo.F03CalendarModuleDefinitions'))
    CREATE UNIQUE INDEX UX_F03CalendarModuleDefinitions_ModuleCode
        ON dbo.F03CalendarModuleDefinitions(ModuleCode);
GO

IF OBJECT_ID(N'dbo.F03CalendarModulePolicies',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03CalendarModulePolicies_ModuleCode'
               AND object_id=OBJECT_ID(N'dbo.F03CalendarModulePolicies'))
    CREATE UNIQUE INDEX UX_F03CalendarModulePolicies_ModuleCode
        ON dbo.F03CalendarModulePolicies(ModuleCode);
GO

IF OBJECT_ID(N'dbo.F03CalendarProjection',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03CalendarProjection_EmployeeDateModuleSource'
               AND object_id=OBJECT_ID(N'dbo.F03CalendarProjection'))
    CREATE UNIQUE INDEX UX_F03CalendarProjection_EmployeeDateModuleSource
        ON dbo.F03CalendarProjection(EmployeeId,WorkDate,ModuleCode,SourceType,SourceId,ParticipantId);
GO

IF OBJECT_ID(N'dbo.F03CalendarProjection',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03CalendarProjection_EmployeeDate'
               AND object_id=OBJECT_ID(N'dbo.F03CalendarProjection'))
    CREATE INDEX IX_F03CalendarProjection_EmployeeDate
        ON dbo.F03CalendarProjection(EmployeeId,WorkDate)
        INCLUDE(ModuleCode,SourceType,SourceId,ParticipantId,StatusCode,Marker,Summary,Severity,RequiresAction,ActionId,DetailRoute);
GO

IF OBJECT_ID(N'dbo.F03CalendarProjection',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03CalendarProjection_RequiresAction'
               AND object_id=OBJECT_ID(N'dbo.F03CalendarProjection'))
    CREATE INDEX IX_F03CalendarProjection_RequiresAction
        ON dbo.F03CalendarProjection(EmployeeId,WorkDate,RequiresAction)
        INCLUDE(ModuleCode,SourceId,ActionId,Severity,Summary);
GO

IF OBJECT_ID(N'dbo.F03ActionItems',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03ActionItems_ActionId'
               AND object_id=OBJECT_ID(N'dbo.F03ActionItems'))
    CREATE UNIQUE INDEX UX_F03ActionItems_ActionId
        ON dbo.F03ActionItems(ActionId);
GO

/* One Open/InProgress action per logical issue and assignee. */
IF OBJECT_ID(N'dbo.F03ActionItems',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03ActionItems_OpenLogicalKey'
               AND object_id=OBJECT_ID(N'dbo.F03ActionItems'))
    CREATE UNIQUE INDEX UX_F03ActionItems_OpenLogicalKey
        ON dbo.F03ActionItems(ModuleCode,SourceType,SourceId,ParticipantId,ActionType,AssignedToEmployeeId)
        WHERE Status IN (0,10);
GO

IF OBJECT_ID(N'dbo.F03ActionItems',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03ActionItems_AssigneeStatus'
               AND object_id=OBJECT_ID(N'dbo.F03ActionItems'))
    CREATE INDEX IX_F03ActionItems_AssigneeStatus
        ON dbo.F03ActionItems(AssignedToUserId,Status,Priority,DueAt)
        INCLUDE(ActionId,ModuleCode,SourceId,ActionType,WorkDate,Title);
GO

IF OBJECT_ID(N'dbo.F03ActionItems',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03ActionItems_EmployeeStatus'
               AND object_id=OBJECT_ID(N'dbo.F03ActionItems'))
    CREATE INDEX IX_F03ActionItems_EmployeeStatus
        ON dbo.F03ActionItems(AssignedToEmployeeId,Status,Priority,DueAt)
        INCLUDE(ActionId,ModuleCode,SourceId,ActionType,WorkDate,Title);
GO

IF OBJECT_ID(N'dbo.F03ActionItems',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03ActionItems_DueAt'
               AND object_id=OBJECT_ID(N'dbo.F03ActionItems'))
    CREATE INDEX IX_F03ActionItems_DueAt
        ON dbo.F03ActionItems(Status,DueAt)
        INCLUDE(AssignedToUserId,AssignedToEmployeeId,ActionId,ModuleCode,ActionType);
GO

IF OBJECT_ID(N'dbo.F03ActionItems',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03ActionItems_ModuleSource'
               AND object_id=OBJECT_ID(N'dbo.F03ActionItems'))
    CREATE INDEX IX_F03ActionItems_ModuleSource
        ON dbo.F03ActionItems(ModuleCode,SourceType,SourceId,ActionType)
        INCLUDE(ActionId,ParticipantId,AssignedToEmployeeId,Status,DueAt);
GO

IF OBJECT_ID(N'dbo.F03ActionPolicies',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03ActionPolicies_ModuleActionType'
               AND object_id=OBJECT_ID(N'dbo.F03ActionPolicies'))
    CREATE UNIQUE INDEX UX_F03ActionPolicies_ModuleActionType
        ON dbo.F03ActionPolicies(ModuleCode,ActionType);
GO

IF OBJECT_ID(N'dbo.F03AppNotifications',N'U') IS NOT NULL
AND COL_LENGTH(N'dbo.F03AppNotifications',N'ActionId') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03AppNotifications_ActionId'
               AND object_id=OBJECT_ID(N'dbo.F03AppNotifications'))
    CREATE INDEX IX_F03AppNotifications_ActionId
        ON dbo.F03AppNotifications(ActionId,UserId,IsRead,CreatedAt);
GO

IF OBJECT_ID(N'dbo.F03AppNotifications',N'U') IS NOT NULL
AND COL_LENGTH(N'dbo.F03AppNotifications',N'NotificationType') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03AppNotifications_UserUnread'
               AND object_id=OBJECT_ID(N'dbo.F03AppNotifications'))
    CREATE INDEX IX_F03AppNotifications_UserUnread
        ON dbo.F03AppNotifications(UserId,IsRead,CreatedAt)
        INCLUDE(ActionId,NotificationType,Title,ActionUrl);
GO

/* New tables only: safe FK enforcement. */
IF OBJECT_ID(N'dbo.F03CalendarProjection',N'U') IS NOT NULL
AND OBJECT_ID(N'dbo.F03Employees',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_F03CalendarProjection_Employee')
    ALTER TABLE dbo.F03CalendarProjection
        ADD CONSTRAINT FK_F03CalendarProjection_Employee
        FOREIGN KEY(EmployeeId) REFERENCES dbo.F03Employees(Id);
GO

IF OBJECT_ID(N'dbo.F03ActionItems',N'U') IS NOT NULL
AND OBJECT_ID(N'dbo.F03Employees',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_F03ActionItems_Employee')
    ALTER TABLE dbo.F03ActionItems
        ADD CONSTRAINT FK_F03ActionItems_Employee
        FOREIGN KEY(EmployeeId) REFERENCES dbo.F03Employees(Id);
GO

IF OBJECT_ID(N'dbo.F03ActionItems',N'U') IS NOT NULL
AND OBJECT_ID(N'dbo.F03Employees',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_F03ActionItems_AssignedEmployee')
    ALTER TABLE dbo.F03ActionItems
        ADD CONSTRAINT FK_F03ActionItems_AssignedEmployee
        FOREIGN KEY(AssignedToEmployeeId) REFERENCES dbo.F03Employees(Id);
GO

IF OBJECT_ID(N'dbo.F03CalendarProjection',N'U') IS NOT NULL
AND OBJECT_ID(N'dbo.F03ActionItems',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_F03CalendarProjection_Action')
    ALTER TABLE dbo.F03CalendarProjection
        ADD CONSTRAINT FK_F03CalendarProjection_Action
        FOREIGN KEY(ActionId) REFERENCES dbo.F03ActionItems(ActionId);
GO

IF OBJECT_ID(N'dbo.F03ActionItems',N'U') IS NOT NULL
AND OBJECT_ID(N'dbo.F03Users',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_F03ActionItems_AssignedUser')
    ALTER TABLE dbo.F03ActionItems
        ADD CONSTRAINT FK_F03ActionItems_AssignedUser
        FOREIGN KEY(AssignedToUserId) REFERENCES dbo.F03Users(Id);
GO

IF OBJECT_ID(N'dbo.F03AppNotifications',N'U') IS NOT NULL
AND COL_LENGTH(N'dbo.F03AppNotifications',N'ActionId') IS NOT NULL
AND OBJECT_ID(N'dbo.F03ActionItems',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_F03AppNotifications_Action')
    ALTER TABLE dbo.F03AppNotifications
        ADD CONSTRAINT FK_F03AppNotifications_Action
        FOREIGN KEY(ActionId) REFERENCES dbo.F03ActionItems(ActionId);
GO

/* Attendance is a read-only calendar module backed by F03HrmAttendanceCalculated. */
INSERT dbo.F03CalendarModuleDefinitions
    (IsActive,CreatedBy,ModuleCode,ModuleName,SupportsCalendar,DefaultEnabled)
SELECT 1,0,N'ATTENDANCE',N'Attendance',1,1
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.F03CalendarModuleDefinitions x
    WHERE x.ModuleCode=N'ATTENDANCE'
);
GO

INSERT dbo.F03CalendarModulePolicies
    (IsActive,CreatedBy,ModuleCode,IsEnabled,DisplayMode,NoteMode,
     ConfirmationMode,ReconciliationMode,Priority,SummaryTemplate,DetailTemplate)
SELECT 1,0,N'ATTENDANCE',1,3,1,0,0,50,
       N'Attendance: {StatusCode}',NULL
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.F03CalendarModulePolicies x
    WHERE x.ModuleCode=N'ATTENDANCE'
);
GO

/* Canonical shared modules. */
INSERT dbo.F03CalendarModuleDefinitions
    (IsActive,CreatedBy,ModuleCode,ModuleName,SupportsCalendar,DefaultEnabled)
SELECT 1,0,v.ModuleCode,v.ModuleName,1,1
FROM (VALUES
    (N'OT',    N'Overtime'),
    (N'LEAVE', N'Leave'),
    (N'TRIP',  N'Business Trip')
) v(ModuleCode,ModuleName)
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.F03CalendarModuleDefinitions x
    WHERE x.ModuleCode=v.ModuleCode
);
GO

INSERT dbo.F03CalendarModulePolicies
    (IsActive,CreatedBy,ModuleCode,IsEnabled,DisplayMode,NoteMode,
     ConfirmationMode,ReconciliationMode,Priority,SummaryTemplate,DetailTemplate)
SELECT 1,0,v.ModuleCode,1,3,1,0,v.ReconciliationMode,v.Priority,v.SummaryTemplate,v.DetailTemplate
FROM (VALUES
    (N'OT',N'ApprovedVsExecution',2,100,N'OT: {StatusCode}',N'/ot/{SourceId}'),
    (N'LEAVE',N'PlannedVsActual',1,100,N'Leave: {StatusCode}',N'/leave/{SourceId}'),
    (N'TRIP',N'ApprovedVsExecution',2,100,N'Trip: {StatusCode}',N'/trips/{SourceId}')
) v(ModuleCode,ReconciliationName,ReconciliationMode,Priority,SummaryTemplate,DetailTemplate)
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.F03CalendarModulePolicies x
    WHERE x.ModuleCode=v.ModuleCode
);
GO

INSERT dbo.F03ActionPolicies
    (IsActive,CreatedBy,ModuleCode,ActionType,IsEnabled,DefaultPriority,
     DueHours,NotificationEnabled,EscalationEnabled,TitleTemplate,SummaryTemplate)
SELECT 1,0,v.ModuleCode,v.ActionType,1,v.Priority,v.DueHours,1,0,v.TitleTemplate,v.SummaryTemplate
FROM (VALUES
    (N'OT',N'OT.ATTENDANCE_CONFIRMATION',80,48,N'Xác nhận OT thiếu chấm công',N'Xác nhận kết quả OT ngày {WorkDate}'),
    (N'OT',N'OT.REVIEW_MISMATCH',90,24,N'Kiểm tra chênh lệch OT',N'Kiểm tra Approved/Actual cho {WorkDate}'),
    (N'LEAVE',N'LEAVE.CONFIRMATION',80,48,N'Xác nhận nghỉ phép',N'Xác nhận thông tin nghỉ ngày {WorkDate}'),
    (N'LEAVE',N'LEAVE.REVIEW',90,24,N'Kiểm tra đơn nghỉ phép',N'Có yêu cầu cần HR xử lý'),
    (N'TRIP',N'TRIP.CONFIRMATION',80,48,N'Xác nhận công tác',N'Xác nhận thông tin công tác ngày {WorkDate}'),
    (N'TRIP',N'TRIP.REVIEW',90,24,N'Kiểm tra công tác',N'Có yêu cầu công tác cần xử lý'),
    (N'APPROVAL',N'APPROVAL.REVIEW',70,24,N'Có phê duyệt cần xử lý',N'Có bước phê duyệt đang chờ')
) v(ModuleCode,ActionType,Priority,DueHours,TitleTemplate,SummaryTemplate)
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.F03ActionPolicies x
    WHERE x.ModuleCode=v.ModuleCode AND x.ActionType=v.ActionType
);
GO

PRINT N'27_WORK_CALENDAR_ACTION indexes, FKs and canonical seeds completed.';
GO
