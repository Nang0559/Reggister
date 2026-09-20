USE [FVN_REGISTER];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/*
===============================================================================
29_EXECUTION_RECONCILIATION
Generic Planned/Approved -> Actual -> Reconciliation -> Confirmation ->
Evidence/Review workflow shared by OT, Leave, Trip and future modules.

Calendar/Action remain projections/orchestration. Business modules remain
source-of-truth for their own planned and actual records.
===============================================================================
*/

IF OBJECT_ID(N'dbo.F03ExecutionPolicies',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03ExecutionPolicies
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03ExecutionPolicies PRIMARY KEY,
        IsActive bit NOT NULL CONSTRAINT DF_F03ExecutionPolicies_IsActive DEFAULT 1,
        CreatedBy int NOT NULL CONSTRAINT DF_F03ExecutionPolicies_CreatedBy DEFAULT 0,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03ExecutionPolicies_CreatedAt DEFAULT GETDATE(),
        ModifiedBy int NULL,
        ModifiedAt datetime2(0) NULL,
        ModuleCode nvarchar(50) NOT NULL,
        ReconciliationMode tinyint NOT NULL CONSTRAINT DF_F03ExecutionPolicies_ReconciliationMode DEFAULT 1,
        ConfirmationMode tinyint NOT NULL CONSTRAINT DF_F03ExecutionPolicies_ConfirmationMode DEFAULT 0,
        EvidenceMode tinyint NOT NULL CONSTRAINT DF_F03ExecutionPolicies_EvidenceMode DEFAULT 2,
        ReviewMode tinyint NOT NULL CONSTRAINT DF_F03ExecutionPolicies_ReviewMode DEFAULT 1,
        DueHours int NULL,
        AutoResolveMode tinyint NOT NULL CONSTRAINT DF_F03ExecutionPolicies_AutoResolveMode DEFAULT 0
    );
END;
GO

IF OBJECT_ID(N'dbo.F03ExecutionReconciliations',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03ExecutionReconciliations
    (
        Id bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03ExecutionReconciliations PRIMARY KEY,
        IsActive bit NOT NULL CONSTRAINT DF_F03ExecutionReconciliations_IsActive DEFAULT 1,
        CreatedBy int NOT NULL CONSTRAINT DF_F03ExecutionReconciliations_CreatedBy DEFAULT 0,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03ExecutionReconciliations_CreatedAt DEFAULT GETDATE(),
        ModifiedBy int NULL,
        ModifiedAt datetime2(0) NULL,
        LastModifiedSource nvarchar(50) NULL,

        ModuleCode nvarchar(50) NOT NULL,
        SourceType nvarchar(50) NOT NULL CONSTRAINT DF_F03ExecutionReconciliations_SourceType DEFAULT N'MODULE',
        SourceId nvarchar(100) NOT NULL,
        ParticipantId nvarchar(100) NULL,
        EmployeeId int NOT NULL,
        WorkDate date NOT NULL,

        PlannedState nvarchar(50) NULL,
        ActualState nvarchar(50) NULL,
        ReconciliationStatus nvarchar(50) NOT NULL
            CONSTRAINT DF_F03ExecutionReconciliations_Status DEFAULT N'None',

        RequiresConfirmation bit NOT NULL
            CONSTRAINT DF_F03ExecutionReconciliations_RequiresConfirmation DEFAULT 0,
        RequiresEvidence bit NOT NULL
            CONSTRAINT DF_F03ExecutionReconciliations_RequiresEvidence DEFAULT 0,

        ConfirmationId bigint NULL,
        ActionId uniqueidentifier NULL,

        DetailJson nvarchar(max) NULL,
        ResolvedAt datetime2(0) NULL,
        ResolvedBy int NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.F03ExecutionConfirmations',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03ExecutionConfirmations
    (
        Id bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03ExecutionConfirmations PRIMARY KEY,
        IsActive bit NOT NULL CONSTRAINT DF_F03ExecutionConfirmations_IsActive DEFAULT 1,
        CreatedBy int NOT NULL CONSTRAINT DF_F03ExecutionConfirmations_CreatedBy DEFAULT 0,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03ExecutionConfirmations_CreatedAt DEFAULT GETDATE(),
        ModifiedBy int NULL,
        ModifiedAt datetime2(0) NULL,
        LastModifiedSource nvarchar(50) NULL,

        ReconciliationId bigint NOT NULL,
        ModuleCode nvarchar(50) NOT NULL,
        SourceType nvarchar(50) NOT NULL CONSTRAINT DF_F03ExecutionConfirmations_SourceType DEFAULT N'MODULE',
        SourceId nvarchar(100) NOT NULL,
        ParticipantId nvarchar(100) NULL,
        EmployeeId int NOT NULL,
        WorkDate date NOT NULL,

        Decision nvarchar(50) NULL,
        Status nvarchar(50) NOT NULL
            CONSTRAINT DF_F03ExecutionConfirmations_Status DEFAULT N'Pending',

        Comment nvarchar(2000) NULL,
        EvidenceRequired bit NOT NULL
            CONSTRAINT DF_F03ExecutionConfirmations_EvidenceRequired DEFAULT 0,

        SubmittedAt datetime2(0) NULL,
        ReviewedBy int NULL,
        ReviewedAt datetime2(0) NULL,
        ReviewNote nvarchar(2000) NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.F03ExecutionConfirmationEvidence',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03ExecutionConfirmationEvidence
    (
        Id bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03ExecutionConfirmationEvidence PRIMARY KEY,
        IsActive bit NOT NULL CONSTRAINT DF_F03ExecutionConfirmationEvidence_IsActive DEFAULT 1,
        CreatedBy int NOT NULL CONSTRAINT DF_F03ExecutionConfirmationEvidence_CreatedBy DEFAULT 0,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03ExecutionConfirmationEvidence_CreatedAt DEFAULT GETDATE(),
        ModifiedBy int NULL,
        ModifiedAt datetime2(0) NULL,
        LastModifiedSource nvarchar(50) NULL,

        ConfirmationId bigint NOT NULL,
        EvidenceType nvarchar(50) NOT NULL,
        FileId int NULL,
        ReferenceNo nvarchar(200) NULL,
        ExternalUrl nvarchar(1000) NULL,
        Description nvarchar(2000) NULL,

        SubmittedBy int NULL,
        SubmittedAt datetime2(0) NOT NULL
            CONSTRAINT DF_F03ExecutionConfirmationEvidence_SubmittedAt DEFAULT GETDATE(),

        ReviewStatus nvarchar(50) NOT NULL
            CONSTRAINT DF_F03ExecutionConfirmationEvidence_ReviewStatus DEFAULT N'Pending',
        ReviewedBy int NULL,
        ReviewedAt datetime2(0) NULL,
        ReviewNote nvarchar(2000) NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.F03ExecutionReconciliationHistory',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03ExecutionReconciliationHistory
    (
        Id bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03ExecutionReconciliationHistory PRIMARY KEY,
        ReconciliationId bigint NOT NULL,
        FromStatus nvarchar(50) NULL,
        ToStatus nvarchar(50) NOT NULL,
        EventType nvarchar(100) NOT NULL,
        Reason nvarchar(2000) NULL,
        ActorUserId int NULL,
        ActorEmployeeId int NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03ExecutionReconciliationHistory_CreatedAt DEFAULT GETDATE()
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03ExecutionPolicies_ModuleCode' AND object_id=OBJECT_ID(N'dbo.F03ExecutionPolicies'))
    CREATE UNIQUE INDEX UX_F03ExecutionPolicies_ModuleCode ON dbo.F03ExecutionPolicies(ModuleCode);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03ExecutionReconciliations_Key' AND object_id=OBJECT_ID(N'dbo.F03ExecutionReconciliations'))
    CREATE UNIQUE INDEX UX_F03ExecutionReconciliations_Key
        ON dbo.F03ExecutionReconciliations(ModuleCode,SourceType,SourceId,ParticipantId,EmployeeId,WorkDate);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03ExecutionReconciliations_EmployeeDate' AND object_id=OBJECT_ID(N'dbo.F03ExecutionReconciliations'))
    CREATE INDEX IX_F03ExecutionReconciliations_EmployeeDate
        ON dbo.F03ExecutionReconciliations(EmployeeId,WorkDate,ReconciliationStatus)
        INCLUDE(ModuleCode,SourceType,SourceId,ParticipantId,RequiresConfirmation,RequiresEvidence,ConfirmationId,ActionId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03ExecutionReconciliations_Action' AND object_id=OBJECT_ID(N'dbo.F03ExecutionReconciliations'))
    CREATE INDEX IX_F03ExecutionReconciliations_Action
        ON dbo.F03ExecutionReconciliations(ActionId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03ExecutionConfirmations_Reconciliation' AND object_id=OBJECT_ID(N'dbo.F03ExecutionConfirmations'))
    CREATE UNIQUE INDEX UX_F03ExecutionConfirmations_Reconciliation
        ON dbo.F03ExecutionConfirmations(ReconciliationId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03ExecutionConfirmations_EmployeeStatus' AND object_id=OBJECT_ID(N'dbo.F03ExecutionConfirmations'))
    CREATE INDEX IX_F03ExecutionConfirmations_EmployeeStatus
        ON dbo.F03ExecutionConfirmations(EmployeeId,Status,WorkDate)
        INCLUDE(ModuleCode,SourceType,SourceId,ParticipantId,ReconciliationId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03ExecutionEvidence_Confirmation' AND object_id=OBJECT_ID(N'dbo.F03ExecutionConfirmationEvidence'))
    CREATE INDEX IX_F03ExecutionEvidence_Confirmation
        ON dbo.F03ExecutionConfirmationEvidence(ConfirmationId,ReviewStatus,SubmittedAt);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03ExecutionHistory_Reconciliation' AND object_id=OBJECT_ID(N'dbo.F03ExecutionReconciliationHistory'))
    CREATE INDEX IX_F03ExecutionHistory_Reconciliation
        ON dbo.F03ExecutionReconciliationHistory(ReconciliationId,CreatedAt);
GO

IF OBJECT_ID(N'dbo.F03Employees',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_F03ExecutionReconciliations_Employee')
    ALTER TABLE dbo.F03ExecutionReconciliations ADD CONSTRAINT FK_F03ExecutionReconciliations_Employee
    FOREIGN KEY(EmployeeId) REFERENCES dbo.F03Employees(Id);
GO

IF OBJECT_ID(N'dbo.F03ExecutionReconciliations',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_F03ExecutionConfirmations_Reconciliation')
    ALTER TABLE dbo.F03ExecutionConfirmations ADD CONSTRAINT FK_F03ExecutionConfirmations_Reconciliation
    FOREIGN KEY(ReconciliationId) REFERENCES dbo.F03ExecutionReconciliations(Id);
GO

IF OBJECT_ID(N'dbo.F03ExecutionConfirmations',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_F03ExecutionEvidence_Confirmation')
    ALTER TABLE dbo.F03ExecutionConfirmationEvidence ADD CONSTRAINT FK_F03ExecutionEvidence_Confirmation
    FOREIGN KEY(ConfirmationId) REFERENCES dbo.F03ExecutionConfirmations(Id);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.F03ExecutionPolicies WHERE ModuleCode=N'OT')
    INSERT dbo.F03ExecutionPolicies(ModuleCode,ReconciliationMode,ConfirmationMode,EvidenceMode,ReviewMode,DueHours)
    VALUES(N'OT',2,1,2,1,48);
IF NOT EXISTS (SELECT 1 FROM dbo.F03ExecutionPolicies WHERE ModuleCode=N'LEAVE')
    INSERT dbo.F03ExecutionPolicies(ModuleCode,ReconciliationMode,ConfirmationMode,EvidenceMode,ReviewMode,DueHours)
    VALUES(N'LEAVE',1,1,2,1,48);
IF NOT EXISTS (SELECT 1 FROM dbo.F03ExecutionPolicies WHERE ModuleCode=N'TRIP')
    INSERT dbo.F03ExecutionPolicies(ModuleCode,ReconciliationMode,ConfirmationMode,EvidenceMode,ReviewMode,DueHours)
    VALUES(N'TRIP',2,1,2,1,48);
GO

PRINT N'29_EXECUTION_RECONCILIATION schema completed.';
GO

/* Canonical source identity upgrade for existing installations. */
IF OBJECT_ID(N'dbo.F03ExecutionReconciliations',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03ExecutionReconciliations',N'SourceType') IS NULL
        ALTER TABLE dbo.F03ExecutionReconciliations ADD SourceType nvarchar(50) NOT NULL
            CONSTRAINT DF_F03ExecutionReconciliations_SourceType DEFAULT N'MODULE' WITH VALUES;
    IF COL_LENGTH(N'dbo.F03ExecutionReconciliations',N'ParticipantId') IS NULL
        ALTER TABLE dbo.F03ExecutionReconciliations ADD ParticipantId nvarchar(100) NULL;
END;
GO

IF OBJECT_ID(N'dbo.F03ExecutionConfirmations',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03ExecutionConfirmations',N'SourceType') IS NULL
        ALTER TABLE dbo.F03ExecutionConfirmations ADD SourceType nvarchar(50) NOT NULL
            CONSTRAINT DF_F03ExecutionConfirmations_SourceType DEFAULT N'MODULE' WITH VALUES;
    IF COL_LENGTH(N'dbo.F03ExecutionConfirmations',N'ParticipantId') IS NULL
        ALTER TABLE dbo.F03ExecutionConfirmations ADD ParticipantId nvarchar(100) NULL;
END;
GO

/* Evidence uses the existing shared attachment/file store. */
IF OBJECT_ID(N'dbo.F03ExecutionConfirmationEvidence',N'U') IS NOT NULL
AND OBJECT_ID(N'dbo.F03Attachment',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_F03ExecutionEvidence_Attachment')
    ALTER TABLE dbo.F03ExecutionConfirmationEvidence
        ADD CONSTRAINT FK_F03ExecutionEvidence_Attachment
        FOREIGN KEY(FileId) REFERENCES dbo.F03Attachment(Id);
GO

/* History belongs to the reconciliation lifecycle. */
IF OBJECT_ID(N'dbo.F03ExecutionReconciliationHistory',N'U') IS NOT NULL
AND OBJECT_ID(N'dbo.F03ExecutionReconciliations',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_F03ExecutionHistory_Reconciliation')
    ALTER TABLE dbo.F03ExecutionReconciliationHistory
        ADD CONSTRAINT FK_F03ExecutionHistory_Reconciliation
        FOREIGN KEY(ReconciliationId) REFERENCES dbo.F03ExecutionReconciliations(Id);
GO

/* Action is orchestration output; it never becomes business source-of-truth. */
IF OBJECT_ID(N'dbo.F03ExecutionReconciliations',N'U') IS NOT NULL
AND OBJECT_ID(N'dbo.F03ActionItems',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_F03ExecutionReconciliations_Action')
    ALTER TABLE dbo.F03ExecutionReconciliations
        ADD CONSTRAINT FK_F03ExecutionReconciliations_Action
        FOREIGN KEY(ActionId) REFERENCES dbo.F03ActionItems(ActionId);
GO

/* Canonical employee isolation indexes. */
IF OBJECT_ID(N'dbo.F03ExecutionReconciliations',N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03ExecutionReconciliations_ModuleSourceParticipant'
               AND object_id=OBJECT_ID(N'dbo.F03ExecutionReconciliations'))
    CREATE INDEX IX_F03ExecutionReconciliations_ModuleSourceParticipant
        ON dbo.F03ExecutionReconciliations(ModuleCode,SourceType,SourceId,ParticipantId,EmployeeId,WorkDate)
        INCLUDE(ReconciliationStatus,RequiresConfirmation,RequiresEvidence,ConfirmationId,ActionId);
GO

PRINT N'29_EXECUTION_RECONCILIATION canonical source identity and shared references completed.';
GO


/* 28 runs before this file on a new database; execution-specific checks therefore live here. */
IF OBJECT_ID(N'dbo.F03ExecutionPolicies',N'U') IS NULL THROW 52029, N'Missing F03ExecutionPolicies', 1;
IF OBJECT_ID(N'dbo.F03ExecutionReconciliations',N'U') IS NULL THROW 52029, N'Missing F03ExecutionReconciliations', 1;
IF OBJECT_ID(N'dbo.F03ExecutionConfirmations',N'U') IS NULL THROW 52029, N'Missing F03ExecutionConfirmations', 1;
IF OBJECT_ID(N'dbo.F03ExecutionConfirmationEvidence',N'U') IS NULL THROW 52029, N'Missing F03ExecutionConfirmationEvidence', 1;
IF OBJECT_ID(N'dbo.F03ExecutionReconciliationHistory',N'U') IS NULL THROW 52029, N'Missing F03ExecutionReconciliationHistory', 1;

IF COL_LENGTH(N'dbo.F03ExecutionReconciliations',N'LastModifiedSource') IS NULL THROW 52040, N'Missing F03ExecutionReconciliations.LastModifiedSource', 1;
IF COL_LENGTH(N'dbo.F03ExecutionConfirmations',N'LastModifiedSource') IS NULL THROW 52041, N'Missing F03ExecutionConfirmations.LastModifiedSource', 1;
IF COL_LENGTH(N'dbo.F03ExecutionConfirmationEvidence',N'LastModifiedSource') IS NULL THROW 52042, N'Missing F03ExecutionConfirmationEvidence.LastModifiedSource', 1;

IF COL_LENGTH(N'dbo.F03CalendarProjection',N'SourceType') IS NULL THROW 52030, N'Missing F03CalendarProjection.SourceType', 1;
IF COL_LENGTH(N'dbo.F03CalendarProjection',N'ParticipantId') IS NULL THROW 52031, N'Missing F03CalendarProjection.ParticipantId', 1;
IF COL_LENGTH(N'dbo.F03ActionItems',N'SourceType') IS NULL THROW 52032, N'Missing F03ActionItems.SourceType', 1;
IF COL_LENGTH(N'dbo.F03ActionItems',N'ParticipantId') IS NULL THROW 52033, N'Missing F03ActionItems.ParticipantId', 1;
IF COL_LENGTH(N'dbo.F03ExecutionReconciliations',N'SourceType') IS NULL THROW 52034, N'Missing F03ExecutionReconciliations.SourceType', 1;
IF COL_LENGTH(N'dbo.F03ExecutionReconciliations',N'ParticipantId') IS NULL THROW 52035, N'Missing F03ExecutionReconciliations.ParticipantId', 1;
IF COL_LENGTH(N'dbo.F03ExecutionConfirmations',N'SourceType') IS NULL THROW 52036, N'Missing F03ExecutionConfirmations.SourceType', 1;
IF COL_LENGTH(N'dbo.F03ExecutionConfirmations',N'ParticipantId') IS NULL THROW 52037, N'Missing F03ExecutionConfirmations.ParticipantId', 1;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_F03ExecutionEvidence_Attachment') THROW 52038, N'Execution evidence must reference F03Attachment.', 1;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_F03ExecutionReconciliations_Action') THROW 52039, N'Execution reconciliation must reference shared ActionItem.', 1;

PRINT N'GENERIC EXECUTION RECONCILIATION SCHEMA VERIFIED.';
GO
