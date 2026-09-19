USE [FVN_REGISTER];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ============================================================================
   FVN_REGISTER - Leave balance schema upgrade
   Fixes existing databases created before annual-leave entitlement fields
   were added to dbo.F03LeaveBalances.
   Safe to run repeatedly.
   ============================================================================ */

IF OBJECT_ID(N'dbo.F03LeaveBalances', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03LeaveBalances
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03LeaveBalances PRIMARY KEY,
        IsActive bit NULL CONSTRAINT DF_F03LeaveBalances_IsActive DEFAULT 1,
        CreatedBy int NOT NULL CONSTRAINT DF_F03LeaveBalances_CreatedBy DEFAULT 0,
        LastModifiedSource nvarchar(50) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03LeaveBalances_CreatedAt DEFAULT GETDATE(),
        ModifiedBy int NULL,
        ModifiedAt datetime2(0) NULL,
        EmployeeCode nvarchar(50) NOT NULL,
        WorkYear int NOT NULL,
        BaseLeaveDays decimal(5,2) NOT NULL CONSTRAINT DF_F03LeaveBalances_BaseLeaveDays DEFAULT 12,
        SeniorityLeaveDays decimal(5,2) NOT NULL CONSTRAINT DF_F03LeaveBalances_SeniorityLeaveDays DEFAULT 0,
        TotalDays decimal(5,2) NOT NULL CONSTRAINT DF_F03LeaveBalances_TotalDays DEFAULT 12,
        YearsOfService int NOT NULL CONSTRAINT DF_F03LeaveBalances_YearsOfService DEFAULT 0,
        CalculatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03LeaveBalances_CalculatedAt DEFAULT GETDATE()
    );
END
ELSE
BEGIN
    IF COL_LENGTH(N'dbo.F03LeaveBalances', N'BaseLeaveDays') IS NULL
        ALTER TABLE dbo.F03LeaveBalances ADD BaseLeaveDays decimal(5,2) NOT NULL CONSTRAINT DF_F03LeaveBalances_BaseLeaveDays DEFAULT 12;

    IF COL_LENGTH(N'dbo.F03LeaveBalances', N'SeniorityLeaveDays') IS NULL
        ALTER TABLE dbo.F03LeaveBalances ADD SeniorityLeaveDays decimal(5,2) NOT NULL CONSTRAINT DF_F03LeaveBalances_SeniorityLeaveDays DEFAULT 0;

    IF COL_LENGTH(N'dbo.F03LeaveBalances', N'YearsOfService') IS NULL
        ALTER TABLE dbo.F03LeaveBalances ADD YearsOfService int NOT NULL CONSTRAINT DF_F03LeaveBalances_YearsOfService DEFAULT 0;

    IF COL_LENGTH(N'dbo.F03LeaveBalances', N'CalculatedAt') IS NULL
        ALTER TABLE dbo.F03LeaveBalances ADD CalculatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03LeaveBalances_CalculatedAt DEFAULT GETDATE();
END;
GO

/* Verify the exact columns required by LeaveEntitlementService. */
IF COL_LENGTH(N'dbo.F03LeaveBalances', N'BaseLeaveDays') IS NULL
   OR COL_LENGTH(N'dbo.F03LeaveBalances', N'SeniorityLeaveDays') IS NULL
   OR COL_LENGTH(N'dbo.F03LeaveBalances', N'YearsOfService') IS NULL
   OR COL_LENGTH(N'dbo.F03LeaveBalances', N'CalculatedAt') IS NULL
    THROW 51330, N'F03LeaveBalances vẫn thiếu cột entitlement bắt buộc.', 1;
GO
