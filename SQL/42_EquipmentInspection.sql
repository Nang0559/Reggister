USE [FVN_REGISTER];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

/*
  Equipment Inspection / Checklist
  - Checklist is optional per registered equipment.
  - An inspection assignment can only reference an active F03EquipmentAssets row.
  - Published checklist versions are immutable from the application.
  - Task history is preserved by TemplateId + Version.
*/

IF OBJECT_ID(N'dbo.F03EquipmentInspectionTemplates', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.F03EquipmentInspectionTemplates(
   Id int IDENTITY PRIMARY KEY,
   IsActive bit NULL DEFAULT 1,
   CreatedBy int NOT NULL DEFAULT 0,
   LastModifiedSource nvarchar(200) NULL,
   CreatedAt datetime2 NOT NULL DEFAULT SYSDATETIME(),
   ModifiedBy int NULL,
   ModifiedAt datetime2 NULL,
   TemplateCode nvarchar(50) NOT NULL,
   TemplateName nvarchar(200) NOT NULL,
   DeptCode nvarchar(20) NOT NULL,
   Frequency nvarchar(20) NOT NULL DEFAULT 'Daily',
   Version int NOT NULL DEFAULT 1,
   Status nvarchar(20) NOT NULL DEFAULT 'Draft',
   Description nvarchar(2000) NULL
 );
END;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03EquipmentInspectionTemplates_CodeVersion' AND object_id=OBJECT_ID(N'dbo.F03EquipmentInspectionTemplates'))
 CREATE UNIQUE INDEX UX_F03EquipmentInspectionTemplates_CodeVersion ON dbo.F03EquipmentInspectionTemplates(DeptCode,TemplateCode,Version);
GO

IF OBJECT_ID(N'dbo.F03EquipmentInspectionItems', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.F03EquipmentInspectionItems(
   Id int IDENTITY PRIMARY KEY,
   IsActive bit NULL DEFAULT 1,
   CreatedBy int NOT NULL DEFAULT 0,
   LastModifiedSource nvarchar(200) NULL,
   CreatedAt datetime2 NOT NULL DEFAULT SYSDATETIME(),
   ModifiedBy int NULL,
   ModifiedAt datetime2 NULL,
   TemplateId int NOT NULL,
   ItemCode nvarchar(60) NOT NULL,
   ItemLabel nvarchar(250) NOT NULL,
   InputType nvarchar(30) NOT NULL DEFAULT 'PassFail',
   IsRequired bit NOT NULL DEFAULT 0,
   RequireImage bit NOT NULL DEFAULT 0,
   MinImages int NOT NULL DEFAULT 0,
   MaxImages int NOT NULL DEFAULT 3,
   MinValue decimal(18,4) NULL,
   MaxValue decimal(18,4) NULL,
   Unit nvarchar(30) NULL,
   OptionsJson nvarchar(2000) NULL,
   DisplayOrder int NOT NULL DEFAULT 0,
   CONSTRAINT FK_F03EquipmentInspectionItems_Template FOREIGN KEY(TemplateId) REFERENCES dbo.F03EquipmentInspectionTemplates(Id) ON DELETE CASCADE
 );
END;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03EquipmentInspectionItems_TemplateCode' AND object_id=OBJECT_ID(N'dbo.F03EquipmentInspectionItems'))
 CREATE UNIQUE INDEX UX_F03EquipmentInspectionItems_TemplateCode ON dbo.F03EquipmentInspectionItems(TemplateId,ItemCode);
GO

IF OBJECT_ID(N'dbo.F03EquipmentInspectionAssignments', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.F03EquipmentInspectionAssignments(
   Id int IDENTITY PRIMARY KEY,
   IsActive bit NULL DEFAULT 1,
   CreatedBy int NOT NULL DEFAULT 0,
   LastModifiedSource nvarchar(200) NULL,
   CreatedAt datetime2 NOT NULL DEFAULT SYSDATETIME(),
   ModifiedBy int NULL,
   ModifiedAt datetime2 NULL,
   EquipmentId int NOT NULL,
   TemplateId int NOT NULL,
   Frequency nvarchar(20) NOT NULL,
   DueTime time NOT NULL DEFAULT '08:00',
   ReminderHoursBefore int NOT NULL DEFAULT 24,
   ScheduleDayOfWeek int NULL,
   ScheduleDayOfMonth int NULL,
   ScheduleMonth int NULL,
   InspectorEmployeeCode nvarchar(50) NOT NULL,
   ApproverEmployeeCode nvarchar(50) NOT NULL,
   EffectiveFrom date NOT NULL,
   EffectiveTo date NULL,
   CONSTRAINT FK_F03EquipmentInspectionAssignments_Equipment FOREIGN KEY(EquipmentId) REFERENCES dbo.F03EquipmentAssets(Id),
   CONSTRAINT FK_F03EquipmentInspectionAssignments_Template FOREIGN KEY(TemplateId) REFERENCES dbo.F03EquipmentInspectionTemplates(Id)
 );
END;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03EquipmentInspectionAssignments_Equipment' AND object_id=OBJECT_ID(N'dbo.F03EquipmentInspectionAssignments')) CREATE INDEX IX_F03EquipmentInspectionAssignments_Equipment ON dbo.F03EquipmentInspectionAssignments(EquipmentId,IsActive,EffectiveFrom,EffectiveTo);
GO

IF OBJECT_ID(N'dbo.F03EquipmentInspectionTasks', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.F03EquipmentInspectionTasks(
   Id int IDENTITY PRIMARY KEY,
   IsActive bit NULL DEFAULT 1,
   CreatedBy int NOT NULL DEFAULT 0,
   LastModifiedSource nvarchar(200) NULL,
   CreatedAt datetime2 NOT NULL DEFAULT SYSDATETIME(),
   ModifiedBy int NULL,
   ModifiedAt datetime2 NULL,
   EquipmentId int NOT NULL,
   TemplateId int NOT NULL,
   AssignmentId int NOT NULL,
   ActionId uniqueidentifier NULL,
   ApprovalActionId uniqueidentifier NULL,
   ScheduledDate date NOT NULL,
   DueAt datetime2 NOT NULL,
   Status nvarchar(30) NOT NULL DEFAULT 'Scheduled',
   Result nvarchar(30) NULL,
   InspectorEmployeeCode nvarchar(50) NOT NULL,
   ApproverEmployeeCode nvarchar(50) NOT NULL,
   StartedAt datetime2 NULL,
   SubmittedAt datetime2 NULL,
   ApprovedAt datetime2 NULL,
   RejectedAt datetime2 NULL,
   ReminderSentAt datetime2 NULL,
   OverdueReminderSentAt datetime2 NULL,
   RejectReason nvarchar(1000) NULL,
   CONSTRAINT FK_F03EquipmentInspectionTasks_Equipment FOREIGN KEY(EquipmentId) REFERENCES dbo.F03EquipmentAssets(Id),
   CONSTRAINT FK_F03EquipmentInspectionTasks_Template FOREIGN KEY(TemplateId) REFERENCES dbo.F03EquipmentInspectionTemplates(Id),
   CONSTRAINT FK_F03EquipmentInspectionTasks_Assignment FOREIGN KEY(AssignmentId) REFERENCES dbo.F03EquipmentInspectionAssignments(Id)
 );
END;
GO
IF COL_LENGTH(N'dbo.F03EquipmentInspectionTasks', N'ActionId') IS NULL
    ALTER TABLE dbo.F03EquipmentInspectionTasks ADD ActionId uniqueidentifier NULL;
IF COL_LENGTH(N'dbo.F03EquipmentInspectionTasks', N'ApprovalActionId') IS NULL
    ALTER TABLE dbo.F03EquipmentInspectionTasks ADD ApprovalActionId uniqueidentifier NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03EquipmentInspectionTasks_AssignmentDate' AND object_id=OBJECT_ID(N'dbo.F03EquipmentInspectionTasks'))
 CREATE UNIQUE INDEX UX_F03EquipmentInspectionTasks_AssignmentDate ON dbo.F03EquipmentInspectionTasks(AssignmentId,ScheduledDate);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03EquipmentInspectionTasks_InspectorStatusDue' AND object_id=OBJECT_ID(N'dbo.F03EquipmentInspectionTasks')) CREATE INDEX IX_F03EquipmentInspectionTasks_InspectorStatusDue ON dbo.F03EquipmentInspectionTasks(InspectorEmployeeCode,Status,DueAt);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03EquipmentInspectionTasks_ActionId' AND object_id=OBJECT_ID(N'dbo.F03EquipmentInspectionTasks')) CREATE UNIQUE INDEX UX_F03EquipmentInspectionTasks_ActionId ON dbo.F03EquipmentInspectionTasks(ActionId) WHERE ActionId IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03EquipmentInspectionTasks_ApprovalActionId' AND object_id=OBJECT_ID(N'dbo.F03EquipmentInspectionTasks')) CREATE UNIQUE INDEX UX_F03EquipmentInspectionTasks_ApprovalActionId ON dbo.F03EquipmentInspectionTasks(ApprovalActionId) WHERE ApprovalActionId IS NOT NULL;
GO

IF OBJECT_ID(N'dbo.F03EquipmentInspectionItemResults', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.F03EquipmentInspectionItemResults(
   Id int IDENTITY PRIMARY KEY,
   IsActive bit NULL DEFAULT 1,
   CreatedBy int NOT NULL DEFAULT 0,
   LastModifiedSource nvarchar(200) NULL,
   CreatedAt datetime2 NOT NULL DEFAULT SYSDATETIME(),
   ModifiedBy int NULL,
   ModifiedAt datetime2 NULL,
   TaskId int NOT NULL,
   ItemId int NOT NULL,
   ValueText nvarchar(2000) NULL,
   ValueNumber decimal(18,4) NULL,
   Passed bit NULL,
   Note nvarchar(1000) NULL,
   CONSTRAINT FK_F03EquipmentInspectionItemResults_Task FOREIGN KEY(TaskId) REFERENCES dbo.F03EquipmentInspectionTasks(Id) ON DELETE CASCADE,
   CONSTRAINT FK_F03EquipmentInspectionItemResults_Item FOREIGN KEY(ItemId) REFERENCES dbo.F03EquipmentInspectionItems(Id)
 );
END;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03EquipmentInspectionItemResults_TaskItem' AND object_id=OBJECT_ID(N'dbo.F03EquipmentInspectionItemResults'))
 CREATE UNIQUE INDEX UX_F03EquipmentInspectionItemResults_TaskItem ON dbo.F03EquipmentInspectionItemResults(TaskId,ItemId);
GO

IF OBJECT_ID(N'dbo.F03EquipmentInspectionEvidence', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.F03EquipmentInspectionEvidence(
   Id int IDENTITY PRIMARY KEY,
   IsActive bit NULL DEFAULT 1,
   CreatedBy int NOT NULL DEFAULT 0,
   LastModifiedSource nvarchar(200) NULL,
   CreatedAt datetime2 NOT NULL DEFAULT SYSDATETIME(),
   ModifiedBy int NULL,
   ModifiedAt datetime2 NULL,
   TaskId int NOT NULL,
   ItemResultId int NULL,
   FileName nvarchar(255) NOT NULL,
   ContentType nvarchar(100) NOT NULL,
   FileSize bigint NOT NULL,
   StoragePath nvarchar(500) NOT NULL,
   CONSTRAINT FK_F03EquipmentInspectionEvidence_Task FOREIGN KEY(TaskId) REFERENCES dbo.F03EquipmentInspectionTasks(Id) ON DELETE CASCADE,
   CONSTRAINT FK_F03EquipmentInspectionEvidence_ItemResult FOREIGN KEY(ItemResultId) REFERENCES dbo.F03EquipmentInspectionItemResults(Id) ON DELETE NO ACTION
 );
END;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03EquipmentInspectionEvidence_TaskItem' AND object_id=OBJECT_ID(N'dbo.F03EquipmentInspectionEvidence')) CREATE INDEX IX_F03EquipmentInspectionEvidence_TaskItem ON dbo.F03EquipmentInspectionEvidence(TaskId,ItemResultId);
GO

/* Inspection permissions */
INSERT dbo.F03Functions(IsActive,CreatedBy,FunctionCode,FunctionName,Detail,ModuleCode,ActionCode,ScopeCode,DisplayOrder)
SELECT 1,0,v.FunctionCode,v.Name,v.Detail,N'EquipmentInspection',v.ActionCode,v.ScopeCode,v.SortNo
FROM (VALUES
 (2315,N'EquipmentInspection.Manage',N'Thiết kế và phân công checklist',N'Manage',N'All',371),
 (2316,N'EquipmentInspection.Execute',N'Thực hiện checklist',N'Execute',N'Own',372),
 (2317,N'EquipmentInspection.Approve',N'Phê duyệt checklist',N'Approve',N'Department',373),
 (2318,N'EquipmentInspection.Report',N'Báo cáo kiểm tra thiết bị',N'Report',N'Department',374)
) v(FunctionCode,Name,Detail,ActionCode,ScopeCode,SortNo)
WHERE NOT EXISTS(SELECT 1 FROM dbo.F03Functions f WHERE f.FunctionCode=v.FunctionCode);
GO
INSERT dbo.F03RoleFunctions(IdRole,IdFunction)
SELECT r.Id,f.Id
FROM dbo.F03Roles r CROSS JOIN dbo.F03Functions f
WHERE ((r.RoleCode IN(1,2) AND f.FunctionCode IN(2315,2316,2317,2318))
   OR (r.RoleCode IN(3,4) AND f.FunctionCode IN(2316,2317))
   OR (r.RoleCode=5 AND f.FunctionCode=2316))
AND NOT EXISTS(SELECT 1 FROM dbo.F03RoleFunctions rf WHERE rf.IdRole=r.Id AND rf.IdFunction=f.Id);
GO

/* Email templates + default auto-send policies. */
IF NOT EXISTS(SELECT 1 FROM dbo.F03EmailTemplates WHERE Code=N'EQUIPMENT_INSPECTION_REMINDER')
INSERT dbo.F03EmailTemplates(IsActive,CreatedBy,CreatedAt,Code,Subject,Body,Description)
VALUES(1,0,SYSDATETIME(),N'EQUIPMENT_INSPECTION_REMINDER',
N'[FCC Smart Portal] Nhắc kiểm tra thiết bị {{EquipmentCode}}',
N'<p>Kính gửi {{InspectorName}},</p><p>Bạn có checklist kiểm tra thiết bị <b>{{EquipmentCode}} - {{EquipmentName}}</b> đến hạn.</p><p>Checklist: <b>{{ChecklistName}}</b><br/>Hạn kiểm tra: <b>{{DueAt}}</b></p><p>Vui lòng đăng nhập FCC Smart Portal để thực hiện.</p>',
N'Nhắc người được phân công kiểm tra thiết bị');
IF NOT EXISTS(SELECT 1 FROM dbo.F03EmailTemplates WHERE Code=N'EQUIPMENT_INSPECTION_OVERDUE')
INSERT dbo.F03EmailTemplates(IsActive,CreatedBy,CreatedAt,Code,Subject,Body,Description)
VALUES(1,0,SYSDATETIME(),N'EQUIPMENT_INSPECTION_OVERDUE',
N'[FCC Smart Portal] Checklist thiết bị đã quá hạn {{EquipmentCode}}',
N'<p>Kính gửi {{InspectorName}},</p><p>Checklist <b>{{ChecklistName}}</b> của thiết bị <b>{{EquipmentCode}} - {{EquipmentName}}</b> đã quá hạn.</p><p>Hạn kiểm tra: <b>{{DueAt}}</b></p>',
N'Nhắc checklist thiết bị quá hạn');
IF NOT EXISTS(SELECT 1 FROM dbo.F03EmailTemplates WHERE Code=N'EQUIPMENT_INSPECTION_APPROVAL_REQUEST')
INSERT dbo.F03EmailTemplates(IsActive,CreatedBy,CreatedAt,Code,Subject,Body,Description)
VALUES(1,0,SYSDATETIME(),N'EQUIPMENT_INSPECTION_APPROVAL_REQUEST',
N'[FCC Smart Portal] Chờ phê duyệt kiểm tra thiết bị {{EquipmentCode}}',
N'<p>Checklist <b>{{ChecklistName}}</b> của thiết bị <b>{{EquipmentCode}} - {{EquipmentName}}</b> đã được người kiểm tra gửi.</p><p>Ngày kiểm tra: {{ScheduledDate}}<br/>Người kiểm tra: {{InspectorEmployeeCode}}</p><p>Vui lòng đăng nhập để phê duyệt.</p>',
N'Yêu cầu phê duyệt kết quả kiểm tra thiết bị');
IF NOT EXISTS(SELECT 1 FROM dbo.F03EmailTemplates WHERE Code=N'EQUIPMENT_INSPECTION_APPROVED')
INSERT dbo.F03EmailTemplates(IsActive,CreatedBy,CreatedAt,Code,Subject,Body,Description)
VALUES(1,0,SYSDATETIME(),N'EQUIPMENT_INSPECTION_APPROVED',
N'[FCC Smart Portal] Checklist thiết bị đã được phê duyệt',
N'<p>Checklist <b>{{ChecklistName}}</b> của thiết bị <b>{{EquipmentCode}} - {{EquipmentName}}</b> đã được phê duyệt.</p>',
N'Thông báo kết quả kiểm tra thiết bị được phê duyệt');
IF NOT EXISTS(SELECT 1 FROM dbo.F03EmailTemplates WHERE Code=N'EQUIPMENT_INSPECTION_REJECTED')
INSERT dbo.F03EmailTemplates(IsActive,CreatedBy,CreatedAt,Code,Subject,Body,Description)
VALUES(1,0,SYSDATETIME(),N'EQUIPMENT_INSPECTION_REJECTED',
N'[FCC Smart Portal] Checklist thiết bị cần kiểm tra lại',
N'<p>Checklist <b>{{ChecklistName}}</b> của thiết bị <b>{{EquipmentCode}} - {{EquipmentName}}</b> bị từ chối.</p><p>Lý do: {{RejectReason}}</p>',
N'Thông báo checklist cần kiểm tra lại');
GO

IF NOT EXISTS(SELECT 1 FROM dbo.F03EmailDispatchPolicies WHERE TemplateCode=N'EQUIPMENT_INSPECTION_REMINDER' AND Priority=100)
INSERT dbo.F03EmailDispatchPolicies(IsActive,CreatedBy,CreatedAt,TemplateCode,EmailProfileCode,DispatchMode,Priority,Description)
VALUES(1,0,SYSDATETIME(),N'EQUIPMENT_INSPECTION_REMINDER',N'SYSTEMSMTP',N'AutoSend',100,N'Nhắc kiểm tra thiết bị');
IF NOT EXISTS(SELECT 1 FROM dbo.F03EmailDispatchPolicies WHERE TemplateCode=N'EQUIPMENT_INSPECTION_OVERDUE' AND Priority=100)
INSERT dbo.F03EmailDispatchPolicies(IsActive,CreatedBy,CreatedAt,TemplateCode,EmailProfileCode,DispatchMode,Priority,Description)
VALUES(1,0,SYSDATETIME(),N'EQUIPMENT_INSPECTION_OVERDUE',N'SYSTEMSMTP',N'AutoSend',100,N'Nhắc checklist quá hạn');
IF NOT EXISTS(SELECT 1 FROM dbo.F03EmailDispatchPolicies WHERE TemplateCode=N'EQUIPMENT_INSPECTION_APPROVAL_REQUEST' AND Priority=100)
INSERT dbo.F03EmailDispatchPolicies(IsActive,CreatedBy,CreatedAt,TemplateCode,EmailProfileCode,DispatchMode,Priority,Description)
VALUES(1,0,SYSDATETIME(),N'EQUIPMENT_INSPECTION_APPROVAL_REQUEST',N'SYSTEMSMTP',N'AutoSend',100,N'Yêu cầu duyệt checklist');
IF NOT EXISTS(SELECT 1 FROM dbo.F03EmailDispatchPolicies WHERE TemplateCode=N'EQUIPMENT_INSPECTION_APPROVED' AND Priority=100)
INSERT dbo.F03EmailDispatchPolicies(IsActive,CreatedBy,CreatedAt,TemplateCode,EmailProfileCode,DispatchMode,Priority,Description)
VALUES(1,0,SYSDATETIME(),N'EQUIPMENT_INSPECTION_APPROVED',N'SYSTEMSMTP',N'AutoSend',100,N'Checklist đã duyệt');
IF NOT EXISTS(SELECT 1 FROM dbo.F03EmailDispatchPolicies WHERE TemplateCode=N'EQUIPMENT_INSPECTION_REJECTED' AND Priority=100)
INSERT dbo.F03EmailDispatchPolicies(IsActive,CreatedBy,CreatedAt,TemplateCode,EmailProfileCode,DispatchMode,Priority,Description)
VALUES(1,0,SYSDATETIME(),N'EQUIPMENT_INSPECTION_REJECTED',N'SYSTEMSMTP',N'AutoSend',100,N'Checklist bị từ chối');
GO

PRINT N'Equipment Inspection deployment completed.';
GO

/* ============================================================
   SECURITY: APPROVED INSPECTION IMMUTABILITY
   Once an inspection task is Approved, nobody (including users
   with Manage/Approve/Report permissions) may mutate its task,
   item results, or evidence. New inspection periods must create
   a new task; corrections require a controlled rejection before
   approval, never editing an approved record.
   ============================================================ */
GO
CREATE OR ALTER TRIGGER dbo.TR_F03EquipmentInspectionTasks_ApprovedImmutable
ON dbo.F03EquipmentInspectionTasks
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    /* Any UPDATE or DELETE whose previous state was Approved is forbidden.
       This also protects audit columns and prevents privileged direct SQL edits. */
    IF EXISTS (SELECT 1 FROM deleted WHERE Status = N'Approved')
    BEGIN
        THROW 51031, N'Inspection task đã Approved là immutable; không được sửa hoặc xóa.', 1;
    END
END;

CREATE OR ALTER TRIGGER dbo.TR_F03EquipmentInspectionItemResults_ApprovedImmutable
ON dbo.F03EquipmentInspectionItemResults
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS
    (
        SELECT 1
        FROM
        (
            SELECT TaskId FROM inserted
            UNION
            SELECT TaskId FROM deleted
        ) x
        INNER JOIN dbo.F03EquipmentInspectionTasks t ON t.Id = x.TaskId
        WHERE t.Status = N'Approved'
    )
    BEGIN
        THROW 51032, N'Kết quả checklist của task đã Approved là immutable.', 1;
    END
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_F03EquipmentInspectionEvidence_ApprovedImmutable
ON dbo.F03EquipmentInspectionEvidence
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS
    (
        SELECT 1
        FROM
        (
            SELECT TaskId FROM inserted
            UNION
            SELECT TaskId FROM deleted
        ) x
        INNER JOIN dbo.F03EquipmentInspectionTasks t ON t.Id = x.TaskId
        WHERE t.Status = N'Approved'
    )
    BEGIN
        THROW 51033, N'Hình ảnh/evidence của task đã Approved là immutable.', 1;
    END
END;
GO
