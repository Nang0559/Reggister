IF OBJECT_ID(N'dbo.F03TripRequests', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03TripRequests
    (
        Id                     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03TripRequests PRIMARY KEY,
        EmployeeCode           NVARCHAR(50) NOT NULL,
        DeptCode               NVARCHAR(20) NULL,
        RequestStatus          INT NOT NULL CONSTRAINT DF_F03TripRequests_RequestStatus DEFAULT (0),
        IsActive               BIT NULL CONSTRAINT DF_F03TripRequests_IsActive DEFAULT (1),
        CreatedBy              INT NOT NULL,
        LastModifiedSource     NVARCHAR(MAX) NULL,
        CreatedAt              DATETIME2 NOT NULL CONSTRAINT DF_F03TripRequests_CreatedAt DEFAULT (GETDATE()),
        ModifiedBy             INT NULL,
        ModifiedAt             DATETIME2 NULL,

        TripCode               NVARCHAR(30) NOT NULL,
        StartDate              DATETIME2 NOT NULL,
        EndDate                DATETIME2 NOT NULL,
        Destination            NVARCHAR(250) NOT NULL,
        Purpose                NVARCHAR(1000) NOT NULL,
        CustomerOrPartner      NVARCHAR(250) NULL,
        TransportMethod        NVARCHAR(100) NULL,
        CompanionEmployeeCodes NVARCHAR(500) NULL,
        EstimatedCost          DECIMAL(18,2) NULL,
        Accommodation          NVARCHAR(500) NULL,
        Note                   NVARCHAR(1000) NULL
    );

    CREATE UNIQUE INDEX UX_F03TripRequests_TripCode
        ON dbo.F03TripRequests(TripCode);

    CREATE INDEX IX_F03TripRequests_Employee_Period
        ON dbo.F03TripRequests(EmployeeCode, StartDate, EndDate);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_F03TripRequests_DateRange'
      AND parent_object_id = OBJECT_ID(N'dbo.F03TripRequests')
)
BEGIN
    ALTER TABLE dbo.F03TripRequests
        ADD CONSTRAINT CK_F03TripRequests_DateRange CHECK (EndDate >= StartDate);
END;
GO
