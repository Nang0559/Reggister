IF OBJECT_ID(N'dbo.F03TripActual',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03TripActual
    (
        Id               BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03TripActual PRIMARY KEY,
        TripRequestId    INT NOT NULL,
        EmployeeCode     NVARCHAR(50) NOT NULL,
        ActualStartDate  DATETIME2 NOT NULL,
        ActualEndDate    DATETIME2 NOT NULL,
        Status           NVARCHAR(30) NOT NULL CONSTRAINT DF_F03TripActual_Status DEFAULT N'Scheduled',
        IsActive         BIT NOT NULL CONSTRAINT DF_F03TripActual_IsActive DEFAULT 1,
        CreatedBy        INT NOT NULL CONSTRAINT DF_F03TripActual_CreatedBy DEFAULT 0,
        CreatedAt        DATETIME2(0) NOT NULL CONSTRAINT DF_F03TripActual_CreatedAt DEFAULT GETDATE(),
        ModifiedBy       INT NULL,
        ModifiedAt       DATETIME2(0) NULL,
        LastModifiedSource NVARCHAR(50) NULL
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03TripActual_TripRequest' AND object_id=OBJECT_ID(N'dbo.F03TripActual'))
    CREATE UNIQUE INDEX UX_F03TripActual_TripRequest ON dbo.F03TripActual(TripRequestId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03TripActual_EmployeePeriod' AND object_id=OBJECT_ID(N'dbo.F03TripActual'))
    CREATE INDEX IX_F03TripActual_EmployeePeriod ON dbo.F03TripActual(EmployeeCode,ActualStartDate,ActualEndDate,Status);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_F03TripActual_TripRequest')
    ALTER TABLE dbo.F03TripActual WITH NOCHECK
        ADD CONSTRAINT FK_F03TripActual_TripRequest
        FOREIGN KEY(TripRequestId) REFERENCES dbo.F03TripRequests(Id);
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name=N'CK_F03TripActual_DateRange')
    ALTER TABLE dbo.F03TripActual WITH NOCHECK
        ADD CONSTRAINT CK_F03TripActual_DateRange CHECK(ActualEndDate >= ActualStartDate);
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name=N'CK_F03TripActual_Status')
    ALTER TABLE dbo.F03TripActual WITH NOCHECK
        ADD CONSTRAINT CK_F03TripActual_Status CHECK(Status IN(N'Scheduled',N'InProgress',N'Completed',N'Cancelled'));
GO
