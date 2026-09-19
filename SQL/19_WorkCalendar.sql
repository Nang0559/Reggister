USE [FVN_REGISTER];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/*
===============================================================================
WORK CALENDAR + ANNUAL LEAVE ENTITLEMENT
===============================================================================
Canonical master data:
  dbo.F03WorkYears
      - defines the active work year and its date range.

  dbo.F03CompanyHolidays
      - defines company non-working dates.
      - TinhPhep controls whether the holiday is counted in leave entitlement
        calculations; it does not turn a holiday into a working day.

Calendar rule:
  - Monday-Friday is a working day.
  - Saturday/Sunday is a weekend/non-working day.
  - A date in F03CompanyHoliday is a company holiday/non-working day.
  - The application never hard-codes company holidays.

Annual leave rule:
  - Base entitlement = 12 days/year.
  - Every complete 5 years from F03Employees.FirstWorkingDate adds 1 day.
  - No first-year proration is applied by this rule.
  - The calculation date for a WorkYear is WorkYear.EndDate.
  - F03LeaveBalances stores the calculated annual entitlement snapshot.

Legacy compatibility:
  - F03WorkYear -> F03WorkYears
  - F03CompanyHoliday -> F03CompanyHolidays
  - IsPaidLeave -> TinhPhep
===============================================================================
*/

IF OBJECT_ID(N'dbo.F03WorkYears',N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.F03WorkYear',N'U') IS NULL
BEGIN
    EXEC sys.sp_rename N'dbo.F03WorkYears', N'F03WorkYear';
END;
GO

IF OBJECT_ID(N'dbo.F03CompanyHolidays',N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.F03CompanyHoliday',N'U') IS NULL
BEGIN
    EXEC sys.sp_rename N'dbo.F03CompanyHolidays', N'F03CompanyHoliday';
END;
GO

IF OBJECT_ID(N'dbo.F03CompanyHoliday',N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.F03CompanyHoliday',N'TinhPhep') IS NULL
   AND COL_LENGTH(N'dbo.F03CompanyHoliday',N'IsPaidLeave') IS NOT NULL
BEGIN
    EXEC sys.sp_rename N'dbo.F03CompanyHoliday.IsPaidLeave', N'TinhPhep', N'COLUMN';
END;
GO

IF OBJECT_ID(N'dbo.F03WorkYear',N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM sys.indexes
       WHERE name=N'UX_F03WorkYears_WorkYear'
         AND object_id=OBJECT_ID(N'dbo.F03WorkYear'))
BEGIN
    CREATE UNIQUE INDEX UX_F03WorkYear_WorkYear
        ON dbo.F03WorkYear(WorkYear);
END;
GO

IF OBJECT_ID(N'dbo.F03CompanyHoliday',N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM sys.indexes
       WHERE name=N'UX_F03CompanyHolidays_Date'
         AND object_id=OBJECT_ID(N'dbo.F03CompanyHoliday'))
BEGIN
    CREATE UNIQUE INDEX UX_F03CompanyHoliday_Date
        ON dbo.F03CompanyHoliday(HolidayDate);
END;
GO

IF OBJECT_ID(N'dbo.F03CompanyHoliday',N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM sys.indexes
       WHERE name=N'IX_F03CompanyHolidays_YearDate'
         AND object_id=OBJECT_ID(N'dbo.F03CompanyHoliday'))
BEGIN
    CREATE INDEX IX_F03CompanyHoliday_YearDate
        ON dbo.F03CompanyHoliday(Year,HolidayDate)
        INCLUDE (Description,TinhPhep);
END;
GO

IF OBJECT_ID(N'dbo.F03LeaveBalances',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03LeaveBalances',N'BaseLeaveDays') IS NULL
        ALTER TABLE dbo.F03LeaveBalances ADD BaseLeaveDays decimal(5,2) NOT NULL
            CONSTRAINT DF_F03LeaveBalances_BaseLeaveDays_19 DEFAULT 12;

    IF COL_LENGTH(N'dbo.F03LeaveBalances',N'SeniorityLeaveDays') IS NULL
        ALTER TABLE dbo.F03LeaveBalances ADD SeniorityLeaveDays decimal(5,2) NOT NULL
            CONSTRAINT DF_F03LeaveBalances_SeniorityLeaveDays_19 DEFAULT 0;

    IF COL_LENGTH(N'dbo.F03LeaveBalances',N'YearsOfService') IS NULL
        ALTER TABLE dbo.F03LeaveBalances ADD YearsOfService int NOT NULL
            CONSTRAINT DF_F03LeaveBalances_YearsOfService_19 DEFAULT 0;

    IF COL_LENGTH(N'dbo.F03LeaveBalances',N'CalculatedAt') IS NULL
        ALTER TABLE dbo.F03LeaveBalances ADD CalculatedAt datetime2(0) NOT NULL
            CONSTRAINT DF_F03LeaveBalances_CalculatedAt_19 DEFAULT GETDATE();

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name=N'UX_F03LeaveBalances_EmployeeYear'
          AND object_id=OBJECT_ID(N'dbo.F03LeaveBalances'))
    BEGIN
        CREATE UNIQUE INDEX UX_F03LeaveBalances_EmployeeYear
            ON dbo.F03LeaveBalances(EmployeeCode,WorkYear);
    END;
END;
GO

PRINT N'Canonical Work Calendar and Annual Leave entitlement schema verified.';
GO
