USE [FVN_REGISTER];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* 30_PAYROLL — canonical 21st -> 20th payroll input snapshot. */

IF OBJECT_ID(N'dbo.F03PayrollCalculationPeriods',N'U') IS NULL
BEGIN
 CREATE TABLE dbo.F03PayrollCalculationPeriods(
  Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03PayrollCalculationPeriods PRIMARY KEY,
  IsActive bit NOT NULL CONSTRAINT DF_F03PayrollPeriods_IsActive DEFAULT 1,
  CreatedBy int NOT NULL CONSTRAINT DF_F03PayrollPeriods_CreatedBy DEFAULT 0,
  CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03PayrollPeriods_CreatedAt DEFAULT GETDATE(),
  ModifiedBy int NULL, ModifiedAt datetime2(0) NULL, LastModifiedSource nvarchar(50) NULL,
  PeriodCode nvarchar(20) NOT NULL, FromDate date NOT NULL, ToDate date NOT NULL,
  Status nvarchar(20) NOT NULL CONSTRAINT DF_F03PayrollPeriods_Status DEFAULT N'Open',
  CalculatedAt datetime2(0) NULL, CalculatedBy int NULL,
  LockedAt datetime2(0) NULL, LockedBy int NULL,
  ExportedAt datetime2(0) NULL, ExportedBy int NULL
 );
END;
GO
IF COL_LENGTH(N'dbo.F03PayrollCalculationPeriods',N'LastModifiedSource') IS NULL ALTER TABLE dbo.F03PayrollCalculationPeriods ADD LastModifiedSource nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.F03PayrollCalculationPeriods',N'CalculatedBy') IS NULL ALTER TABLE dbo.F03PayrollCalculationPeriods ADD CalculatedBy int NULL;
IF COL_LENGTH(N'dbo.F03PayrollCalculationPeriods',N'LockedBy') IS NULL ALTER TABLE dbo.F03PayrollCalculationPeriods ADD LockedBy int NULL;
IF COL_LENGTH(N'dbo.F03PayrollCalculationPeriods',N'ExportedBy') IS NULL ALTER TABLE dbo.F03PayrollCalculationPeriods ADD ExportedBy int NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03PayrollCalculationPeriods_PeriodCode' AND object_id=OBJECT_ID(N'dbo.F03PayrollCalculationPeriods'))
 CREATE UNIQUE INDEX UX_F03PayrollCalculationPeriods_PeriodCode ON dbo.F03PayrollCalculationPeriods(PeriodCode);
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name=N'CK_F03PayrollCalculationPeriods_21_20')
 ALTER TABLE dbo.F03PayrollCalculationPeriods WITH NOCHECK ADD CONSTRAINT CK_F03PayrollCalculationPeriods_21_20 CHECK (DAY(FromDate)=21 AND ToDate=DATEADD(DAY,-1,DATEADD(MONTH,1,FromDate)));
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name=N'CK_F03PayrollCalculationPeriods_Status')
 ALTER TABLE dbo.F03PayrollCalculationPeriods ADD CONSTRAINT CK_F03PayrollCalculationPeriods_Status CHECK(Status IN(N'Open',N'Calculated',N'Locked',N'Exported'));
GO

IF OBJECT_ID(N'dbo.F03PayrollInputs',N'U') IS NULL
BEGIN
 CREATE TABLE dbo.F03PayrollInputs(
  Id bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03PayrollInputs PRIMARY KEY,
  IsActive bit NOT NULL CONSTRAINT DF_F03PayrollInputs_IsActive DEFAULT 1,
  CreatedBy int NOT NULL CONSTRAINT DF_F03PayrollInputs_CreatedBy DEFAULT 0,
  CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03PayrollInputs_CreatedAt DEFAULT GETDATE(),
  ModifiedBy int NULL, ModifiedAt datetime2(0) NULL, LastModifiedSource nvarchar(50) NULL,
  PayrollPeriodId int NOT NULL, EmployeeId int NOT NULL, WorkDate date NOT NULL,
  WorkMinutes decimal(10,2) NOT NULL CONSTRAINT DF_F03PayrollInputs_WorkMinutes DEFAULT 0,
  LeaveTotal decimal(10,2) NOT NULL CONSTRAINT DF_F03PayrollInputs_LeaveTotal DEFAULT 0,
  OTMinutes decimal(10,2) NOT NULL CONSTRAINT DF_F03PayrollInputs_OTMinutes DEFAULT 0,
  Source nvarchar(30) NOT NULL CONSTRAINT DF_F03PayrollInputs_Source DEFAULT N'HRM_CALCULATION',
  SnapshotAt datetime2(0) NOT NULL CONSTRAINT DF_F03PayrollInputs_SnapshotAt DEFAULT GETDATE()
 );
END;
GO
IF COL_LENGTH(N'dbo.F03PayrollInputs',N'LastModifiedSource') IS NULL ALTER TABLE dbo.F03PayrollInputs ADD LastModifiedSource nvarchar(50) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_F03PayrollInputs_PeriodEmployeeDate' AND object_id=OBJECT_ID(N'dbo.F03PayrollInputs'))
 CREATE UNIQUE INDEX UX_F03PayrollInputs_PeriodEmployeeDate ON dbo.F03PayrollInputs(PayrollPeriodId,EmployeeId,WorkDate);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_F03PayrollInputs_Period')
 ALTER TABLE dbo.F03PayrollInputs ADD CONSTRAINT FK_F03PayrollInputs_Period FOREIGN KEY(PayrollPeriodId) REFERENCES dbo.F03PayrollCalculationPeriods(Id);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_F03PayrollInputs_Employee')
 ALTER TABLE dbo.F03PayrollInputs WITH NOCHECK ADD CONSTRAINT FK_F03PayrollInputs_Employee FOREIGN KEY(EmployeeId) REFERENCES dbo.F03Employees(Id);
GO

IF NOT EXISTS (
    SELECT 1 FROM dbo.F03PayrollCalculationPeriods
    WHERE DAY(FromDate)<>21 OR ToDate<>DATEADD(DAY,-1,DATEADD(MONTH,1,FromDate))
)
    ALTER TABLE dbo.F03PayrollCalculationPeriods WITH CHECK CHECK CONSTRAINT CK_F03PayrollCalculationPeriods_21_20;
ELSE
    PRINT N'WARNING: legacy payroll periods violate 21->20 and date constraint remains untrusted.';
GO

IF NOT EXISTS (
    SELECT 1 FROM dbo.F03PayrollInputs p
    LEFT JOIN dbo.F03Employees e ON e.Id=p.EmployeeId
    WHERE e.Id IS NULL
)
    ALTER TABLE dbo.F03PayrollInputs WITH CHECK CHECK CONSTRAINT FK_F03PayrollInputs_Employee;
ELSE
    PRINT N'WARNING: orphan payroll input employees exist; FK_F03PayrollInputs_Employee remains untrusted.';
GO

CREATE OR ALTER PROCEDURE dbo.usp_EnsurePayrollPeriod
 @AsOfDate date,
 @ActorUserId int = 0
AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON;
 DECLARE @FromDate date = CASE WHEN DAY(@AsOfDate)>=21
     THEN DATEFROMPARTS(YEAR(@AsOfDate),MONTH(@AsOfDate),21)
     ELSE DATEADD(MONTH,-1,DATEFROMPARTS(YEAR(@AsOfDate),MONTH(@AsOfDate),21)) END;
 DECLARE @ToDate date = DATEADD(DAY,-1,DATEADD(MONTH,1,@FromDate));
 DECLARE @PeriodCode nvarchar(20) = CONVERT(nvarchar(10),@FromDate,23)+N'_'+CONVERT(nvarchar(10),@ToDate,23);
 BEGIN TRAN;
 DECLARE @Id int;
 SELECT @Id=Id FROM dbo.F03PayrollCalculationPeriods WITH (UPDLOCK,HOLDLOCK)
 WHERE IsActive=1 AND FromDate=@FromDate AND ToDate=@ToDate;
 IF @Id IS NULL
 BEGIN
   INSERT dbo.F03PayrollCalculationPeriods
     (CreatedBy,PeriodCode,FromDate,ToDate,Status,LastModifiedSource)
   VALUES(@ActorUserId,@PeriodCode,@FromDate,@ToDate,N'Open',N'PAYROLL_PERIOD_CREATE');
   SET @Id=SCOPE_IDENTITY();
 END;
 COMMIT;
 SELECT * FROM dbo.F03PayrollCalculationPeriods WHERE Id=@Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_PreparePayrollPeriod
 @PeriodId int,
 @ActorUserId int = 0
AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON;
 BEGIN TRAN;
 DECLARE @FromDate date,@ToDate date,@Status nvarchar(20);
 SELECT @FromDate=FromDate,@ToDate=ToDate,@Status=Status
 FROM dbo.F03PayrollCalculationPeriods WITH (UPDLOCK,HOLDLOCK)
 WHERE Id=@PeriodId AND IsActive=1;

 IF @FromDate IS NULL THROW 52200,N'Không tìm thấy kỳ lương.',1;
 IF DAY(@FromDate)<>21 OR @ToDate<>DATEADD(DAY,-1,DATEADD(MONTH,1,@FromDate))
     THROW 52203,N'Kỳ lương phải theo chu kỳ ngày 21 đến ngày 20.',1;
 IF @Status IN(N'Locked',N'Exported') THROW 52201,N'Kỳ lương đã khóa/xuất, không được thay đổi.',1;

 DELETE FROM dbo.F03PayrollInputs WHERE PayrollPeriodId=@PeriodId;

 INSERT dbo.F03PayrollInputs(PayrollPeriodId,EmployeeId,WorkDate,WorkMinutes,LeaveTotal,OTMinutes,Source,SnapshotAt,CreatedBy,LastModifiedSource)
 SELECT @PeriodId,e.Id,a.WorkDate,
        CAST(ISNULL(a.WorkMinutesDay,0)+ISNULL(a.WorkMinutesNight,0) AS decimal(10,2)),
        CAST(ISNULL(a.LeaveTotal,0) AS decimal(10,2)),
        CAST(ISNULL(o.RecognizedOTMinutes,0) AS decimal(10,2)),
        N'HRM_CALCULATION',GETDATE(),@ActorUserId,N'PAYROLL_PREPARE'
 FROM dbo.F03HrmAttendanceCalculated a
 INNER JOIN dbo.F03Employees e ON e.EmployeeCode=a.EmployeeCode AND e.IsActive=1
 LEFT JOIN dbo.F03HrmOTActual o ON o.HrmEmployeeId=a.HrmEmployeeId AND o.WorkDate=a.WorkDate
 WHERE a.WorkDate BETWEEN @FromDate AND @ToDate;

 DECLARE @InputRows int=@@ROWCOUNT;
 UPDATE dbo.F03PayrollCalculationPeriods
 SET Status=N'Calculated',CalculatedAt=GETDATE(),CalculatedBy=@ActorUserId,ModifiedBy=@ActorUserId,ModifiedAt=GETDATE(),LastModifiedSource=N'PAYROLL_PREPARE'
 WHERE Id=@PeriodId AND Status NOT IN(N'Locked',N'Exported');

 COMMIT;
 SELECT @PeriodId AS PeriodId,@InputRows AS InputRows;
END;
GO

PRINT N'30_PAYROLL schema/procedure ready.';
GO
