USE [FVN_REGISTER];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/*
===============================================================================
26_WORK_CALENDAR_ACTION
===============================================================================
Canonical shared projection/action layer.

IMPORTANT:
- F03Employees / F03Users remain existing identity sources.
- Existing F03AppNotifications is upgraded; no duplicate notification table.
- Business modules remain source-of-truth for OT / Leave / Trip / Approval.
- Calendar is projection/navigation.
- ActionItem is the shared unit of work.
- Notification is delivery/read state.
- Dashboard reads projections/actions; it does not own workflow.

Status codes:
  ActionItem: Open=0, InProgress=10, Completed=20, Dismissed=30,
              Expired=40, Cancelled=90

Calendar policy:
  DisplayMode: None=0, Marker=1, Summary=2, MarkerAndSummary=3
  NoteMode: None=0, AlertsOnly=1, AllImportant=2
  ConfirmationMode: None=0, Employee=1, EmployeeThenHr=2, Admin=3
  ReconciliationMode: None=0, PlannedVsActual=1,
                       ApprovedVsExecution=2, Custom=3
===============================================================================
*/

IF OBJECT_ID(N'dbo.F03CalendarModuleDefinitions',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03CalendarModuleDefinitions
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03CalendarModuleDefinitions PRIMARY KEY,
        IsActive bit NOT NULL CONSTRAINT DF_F03CalendarModuleDefinitions_IsActive DEFAULT 1,
        CreatedBy int NOT NULL CONSTRAINT DF_F03CalendarModuleDefinitions_CreatedBy DEFAULT 0,
        LastModifiedSource nvarchar(50) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03CalendarModuleDefinitions_CreatedAt DEFAULT GETDATE(),
        ModifiedBy int NULL,
        ModifiedAt datetime2(0) NULL,
        ModuleCode nvarchar(50) NOT NULL,
        ModuleName nvarchar(100) NOT NULL,
        SupportsCalendar bit NOT NULL CONSTRAINT DF_F03CalendarModuleDefinitions_SupportsCalendar DEFAULT 1,
        DefaultEnabled bit NOT NULL CONSTRAINT DF_F03CalendarModuleDefinitions_DefaultEnabled DEFAULT 1
    );
END;
GO

IF OBJECT_ID(N'dbo.F03CalendarModulePolicies',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03CalendarModulePolicies
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03CalendarModulePolicies PRIMARY KEY,
        IsActive bit NOT NULL CONSTRAINT DF_F03CalendarModulePolicies_IsActive DEFAULT 1,
        CreatedBy int NOT NULL CONSTRAINT DF_F03CalendarModulePolicies_CreatedBy DEFAULT 0,
        LastModifiedSource nvarchar(50) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03CalendarModulePolicies_CreatedAt DEFAULT GETDATE(),
        ModifiedBy int NULL,
        ModifiedAt datetime2(0) NULL,
        ModuleCode nvarchar(50) NOT NULL,
        IsEnabled bit NOT NULL CONSTRAINT DF_F03CalendarModulePolicies_IsEnabled DEFAULT 1,
        DisplayMode tinyint NOT NULL CONSTRAINT DF_F03CalendarModulePolicies_DisplayMode DEFAULT 3,
        NoteMode tinyint NOT NULL CONSTRAINT DF_F03CalendarModulePolicies_NoteMode DEFAULT 1,
        ConfirmationMode tinyint NOT NULL CONSTRAINT DF_F03CalendarModulePolicies_ConfirmationMode DEFAULT 0,
        ReconciliationMode tinyint NOT NULL CONSTRAINT DF_F03CalendarModulePolicies_ReconciliationMode DEFAULT 0,
        Priority int NOT NULL CONSTRAINT DF_F03CalendarModulePolicies_Priority DEFAULT 100,
        SummaryTemplate nvarchar(500) NULL,
        DetailTemplate nvarchar(500) NULL,
        UpdatedBy int NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.F03CalendarProjection',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03CalendarProjection
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03CalendarProjection PRIMARY KEY,
        IsActive bit NOT NULL CONSTRAINT DF_F03CalendarProjection_IsActive DEFAULT 1,
        CreatedBy int NOT NULL CONSTRAINT DF_F03CalendarProjection_CreatedBy DEFAULT 0,
        LastModifiedSource nvarchar(50) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03CalendarProjection_CreatedAt DEFAULT GETDATE(),
        ModifiedBy int NULL,
        ModifiedAt datetime2(0) NULL,
        EmployeeId int NOT NULL,
        WorkDate date NOT NULL,
        ModuleCode nvarchar(50) NOT NULL,
        SourceType nvarchar(50) NOT NULL CONSTRAINT DF_F03CalendarProjection_SourceType DEFAULT N'MODULE',
        SourceId nvarchar(100) NOT NULL,
        ParticipantId nvarchar(100) NULL,
        StatusCode nvarchar(50) NOT NULL,
        Marker nvarchar(20) NULL,
        Summary nvarchar(500) NULL,
        Severity tinyint NOT NULL CONSTRAINT DF_F03CalendarProjection_Severity DEFAULT 0,
        RequiresAction bit NOT NULL CONSTRAINT DF_F03CalendarProjection_RequiresAction DEFAULT 0,
        ActionId uniqueidentifier NULL,
        DetailRoute nvarchar(500) NULL,
        PayloadJson nvarchar(max) NULL,
        CalculatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03CalendarProjection_CalculatedAt DEFAULT GETDATE()
    );
END;
GO

IF OBJECT_ID(N'dbo.F03ActionItems',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03ActionItems
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03ActionItems PRIMARY KEY,
        IsActive bit NOT NULL CONSTRAINT DF_F03ActionItems_IsActive DEFAULT 1,
        CreatedBy int NOT NULL CONSTRAINT DF_F03ActionItems_CreatedBy DEFAULT 0,
        LastModifiedSource nvarchar(50) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03ActionItems_CreatedAt DEFAULT GETDATE(),
        ModifiedBy int NULL,
        ModifiedAt datetime2(0) NULL,
        ActionId uniqueidentifier NOT NULL CONSTRAINT DF_F03ActionItems_ActionId DEFAULT NEWSEQUENTIALID(),
        ModuleCode nvarchar(50) NOT NULL,
        SourceType nvarchar(50) NOT NULL CONSTRAINT DF_F03ActionItems_SourceType DEFAULT N'MODULE',
        SourceId nvarchar(100) NOT NULL,
        ParticipantId nvarchar(100) NULL,
        EmployeeId int NOT NULL,
        AssignedToUserId int NULL,
        AssignedToEmployeeId int NOT NULL,
        WorkDate date NULL,
        ActionType nvarchar(100) NOT NULL,
        Title nvarchar(200) NOT NULL,
        Summary nvarchar(1000) NULL,
        Severity tinyint NOT NULL CONSTRAINT DF_F03ActionItems_Severity DEFAULT 0,
        Priority int NOT NULL CONSTRAINT DF_F03ActionItems_Priority DEFAULT 100,
        Status tinyint NOT NULL CONSTRAINT DF_F03ActionItems_Status DEFAULT 0,
        DueAt datetime2(0) NULL,
        DetailRoute nvarchar(500) NULL,
        ReferenceNo nvarchar(100) NULL,
        PayloadJson nvarchar(max) NULL,
        CompletedAt datetime2(0) NULL,
        DismissedAt datetime2(0) NULL,
        ExpiredAt datetime2(0) NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.F03ActionPolicies',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03ActionPolicies
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03ActionPolicies PRIMARY KEY,
        IsActive bit NOT NULL CONSTRAINT DF_F03ActionPolicies_IsActive DEFAULT 1,
        CreatedBy int NOT NULL CONSTRAINT DF_F03ActionPolicies_CreatedBy DEFAULT 0,
        LastModifiedSource nvarchar(50) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03ActionPolicies_CreatedAt DEFAULT GETDATE(),
        ModifiedBy int NULL,
        ModifiedAt datetime2(0) NULL,
        ModuleCode nvarchar(50) NOT NULL,
        ActionType nvarchar(100) NOT NULL,
        IsEnabled bit NOT NULL CONSTRAINT DF_F03ActionPolicies_IsEnabled DEFAULT 1,
        DefaultPriority int NOT NULL CONSTRAINT DF_F03ActionPolicies_DefaultPriority DEFAULT 100,
        DueHours int NULL,
        NotificationEnabled bit NOT NULL CONSTRAINT DF_F03ActionPolicies_NotificationEnabled DEFAULT 1,
        EscalationEnabled bit NOT NULL CONSTRAINT DF_F03ActionPolicies_EscalationEnabled DEFAULT 0,
        TitleTemplate nvarchar(200) NULL,
        SummaryTemplate nvarchar(1000) NULL
    );
END;
GO

/* Existing notification framework is retained and upgraded to reference ActionId. */
IF OBJECT_ID(N'dbo.F03AppNotifications',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03AppNotifications',N'ActionId') IS NULL
        ALTER TABLE dbo.F03AppNotifications ADD ActionId uniqueidentifier NULL;

    IF COL_LENGTH(N'dbo.F03AppNotifications',N'NotificationType') IS NULL
        ALTER TABLE dbo.F03AppNotifications ADD NotificationType nvarchar(50) NULL;
END;
GO

/* Separate batch on purpose: a column added by ALTER TABLE is not visible to
   statements compiled in the same batch (Msg 207: Invalid column name). */
IF COL_LENGTH(N'dbo.F03AppNotifications',N'NotificationType') IS NOT NULL
    UPDATE dbo.F03AppNotifications
    SET NotificationType = N'Legacy'
    WHERE NotificationType IS NULL;
GO

/* Backfill ActionId into calendar projection is intentionally not automatic:
   existing calendar rows have no canonical ActionItem identity. */
GO
PRINT N'26_WORK_CALENDAR_ACTION schema upgrade completed.';
GO

/* Canonical source identity for shared projections/actions.
   SourceType identifies the business aggregate/participant shape.
   ParticipantId isolates multi-employee modules (e.g. OT employees).
   These fields do not replace the business module source-of-truth. */
IF OBJECT_ID(N'dbo.F03CalendarProjection',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03CalendarProjection',N'SourceType') IS NULL
        ALTER TABLE dbo.F03CalendarProjection ADD SourceType nvarchar(50) NOT NULL
            CONSTRAINT DF_F03CalendarProjection_SourceType DEFAULT N'MODULE' WITH VALUES;
    IF COL_LENGTH(N'dbo.F03CalendarProjection',N'ParticipantId') IS NULL
        ALTER TABLE dbo.F03CalendarProjection ADD ParticipantId nvarchar(100) NULL;
END;
GO

IF OBJECT_ID(N'dbo.F03ActionItems',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03ActionItems',N'SourceType') IS NULL
        ALTER TABLE dbo.F03ActionItems ADD SourceType nvarchar(50) NOT NULL
            CONSTRAINT DF_F03ActionItems_SourceType DEFAULT N'MODULE' WITH VALUES;
    IF COL_LENGTH(N'dbo.F03ActionItems',N'ParticipantId') IS NULL
        ALTER TABLE dbo.F03ActionItems ADD ParticipantId nvarchar(100) NULL;
END;
GO

PRINT N'26_WORK_CALENDAR_ACTION canonical source identity upgrade completed.';
GO
