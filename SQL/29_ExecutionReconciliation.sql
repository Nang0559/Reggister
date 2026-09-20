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

        ModuleCode nvarchar(50) NOT NULL,
        SourceId nvarchar(100) NOT NULL,
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

        ReconciliationId bigint NOT NULL,
        ModuleCode nvarchar(50) NOT NULL,
        SourceId nvarchar(100) NOT NULL,
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
        ON dbo.F03ExecutionReconciliations(ModuleCode,SourceId,EmployeeId,WorkDate);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03ExecutionReconciliations_EmployeeDate' AND object_id=OBJECT_ID(N'dbo.F03ExecutionReconciliations'))
    CREATE INDEX IX_F03ExecutionReconciliations_EmployeeDate
        ON dbo.F03ExecutionReconciliations(EmployeeId,WorkDate,ReconciliationStatus)
        INCLUDE(ModuleCode,SourceId,RequiresConfirmation,RequiresEvidence,ConfirmationId,ActionId);
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
        ON dbo.F03ExecutionConfirmations(EmployeeId,Status,WorkDate);
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
