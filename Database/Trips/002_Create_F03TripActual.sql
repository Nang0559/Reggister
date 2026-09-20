USE [FVN_REGISTER];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/*
===============================================================================
F03TripActual
-------------------------------------------------------------------------------
Execution source-of-truth for approved Trip requests.

Business rule:
- When F03TripRequests becomes Approved, create exactly one F03TripActual.
- ActualStartDate/ActualEndDate initially equal the approved request period.
- EmployeeCode is copied from the approved request and is NOT reassigned.
- If the traveller changes, the business flow is a new TripRequest -> approval
  -> a new F03TripActual. The old approved request/actual remains its own history.
- Creation is idempotent by unique TripRequestId.
===============================================================================
*/

IF OBJECT_ID(N'dbo.F03TripActual', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03TripActual
    (
        Id                     INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_F03TripActual PRIMARY KEY,
        IsActive               BIT NULL
            CONSTRAINT DF_F03TripActual_IsActive DEFAULT (1),
        CreatedBy              INT NOT NULL,
        LastModifiedSource     NVARCHAR(50) NULL,
        CreatedAt              DATETIME2 NOT NULL
            CONSTRAINT DF_F03TripActual_CreatedAt DEFAULT (GETDATE()),
        ModifiedBy             INT NULL,
        ModifiedAt             DATETIME2 NULL,

        TripRequestId          INT NOT NULL,
        TripCode               NVARCHAR(30) NOT NULL,
        EmployeeCode           NVARCHAR(50) NOT NULL,
        ActualStartDate        DATETIME2 NOT NULL,
        ActualEndDate          DATETIME2 NOT NULL,
        Destination            NVARCHAR(250) NOT NULL,
        Purpose                NVARCHAR(1000) NOT NULL,
        CustomerOrPartner      NVARCHAR(250) NULL,
        TransportMethod        NVARCHAR(100) NULL,
        CompanionEmployeeCodes NVARCHAR(500) NULL,
        Accommodation          NVARCHAR(500) NULL,
        Note                   NVARCHAR(1000) NULL,
        ActualStatus           NVARCHAR(30) NOT NULL
            CONSTRAINT DF_F03TripActual_ActualStatus DEFAULT (N'Scheduled'),
        ApprovedAt             DATETIME2 NOT NULL
            CONSTRAINT DF_F03TripActual_ApprovedAt DEFAULT (GETDATE())
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_F03TripActual_TripRequest'
      AND object_id = OBJECT_ID(N'dbo.F03TripActual')
)
BEGIN
    CREATE UNIQUE INDEX UX_F03TripActual_TripRequest
        ON dbo.F03TripActual(TripRequestId);
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_F03TripActual_EmployeePeriod'
      AND object_id = OBJECT_ID(N'dbo.F03TripActual')
)
BEGIN
    CREATE INDEX IX_F03TripActual_EmployeePeriod
        ON dbo.F03TripActual(EmployeeCode, ActualStartDate, ActualEndDate);
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_F03TripActual_EmployeeStatus'
      AND object_id = OBJECT_ID(N'dbo.F03TripActual')
)
BEGIN
    CREATE INDEX IX_F03TripActual_EmployeeStatus
        ON dbo.F03TripActual(EmployeeCode, ActualStartDate, ActualStatus);
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_F03TripActual_TripRequest'
)
BEGIN
    ALTER TABLE dbo.F03TripActual
        ADD CONSTRAINT FK_F03TripActual_TripRequest
        FOREIGN KEY (TripRequestId)
        REFERENCES dbo.F03TripRequests(Id);
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.check_constraints
    WHERE name = N'CK_F03TripActual_DateRange'
      AND parent_object_id = OBJECT_ID(N'dbo.F03TripActual')
)
BEGIN
    ALTER TABLE dbo.F03TripActual
        ADD CONSTRAINT CK_F03TripActual_DateRange
        CHECK (ActualEndDate >= ActualStartDate);
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.check_constraints
    WHERE name = N'CK_F03TripActual_Status'
      AND parent_object_id = OBJECT_ID(N'dbo.F03TripActual')
)
BEGIN
    ALTER TABLE dbo.F03TripActual
        ADD CONSTRAINT CK_F03TripActual_Status
        CHECK (ActualStatus IN
            (N'Scheduled', N'InProgress', N'Completed', N'Cancelled'));
END;
GO

PRINT N'F03TripActual schema completed.';
GO
