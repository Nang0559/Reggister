USE [FVN_REGISTER];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ============================================================================
   FVN_REGISTER - Work year schema upgrade
   EF configuration maps F03WorkYear -> dbo.F03WorkYears.
   Existing databases may not have this table.
   ============================================================================ */

IF OBJECT_ID(N'dbo.F03WorkYears', N'U') IS NULL
   AND OBJECT_ID(N'dbo.F03WorkYear', N'U') IS NOT NULL
BEGIN
    EXEC sys.sp_rename N'dbo.F03WorkYear', N'F03WorkYears';
END;
GO

IF OBJECT_ID(N'dbo.F03WorkYears', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03WorkYears
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03WorkYears PRIMARY KEY,
        IsActive bit NULL CONSTRAINT DF_F03WorkYears_IsActive DEFAULT 1,
        CreatedBy int NOT NULL CONSTRAINT DF_F03WorkYears_CreatedBy DEFAULT 0,
        LastModifiedSource nvarchar(50) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03WorkYears_CreatedAt DEFAULT GETDATE(),
        ModifiedBy int NULL,
        ModifiedAt datetime2(0) NULL,
        WorkYear int NOT NULL,
        StartDate date NOT NULL,
        EndDate date NOT NULL,
        Remark nvarchar(500) NULL
    );

    CREATE UNIQUE INDEX IX_F03WorkYear_Year
        ON dbo.F03WorkYears(WorkYear);
END;
GO

IF COL_LENGTH(N'dbo.F03WorkYears', N'WorkYear') IS NULL
   OR COL_LENGTH(N'dbo.F03WorkYears', N'StartDate') IS NULL
   OR COL_LENGTH(N'dbo.F03WorkYears', N'EndDate') IS NULL
    THROW 51331, N'F03WorkYears thiếu cột bắt buộc.', 1;
GO

/* Ensure the current work year exists for the leave entitlement calculation. */
IF NOT EXISTS
(
    SELECT 1
    FROM dbo.F03WorkYears
    WHERE WorkYear = YEAR(GETDATE())
      AND ISNULL(IsActive, 1) = 1
)
BEGIN
    INSERT INTO dbo.F03WorkYears
    (
        IsActive, CreatedBy, CreatedAt,
        WorkYear, StartDate, EndDate, Remark
    )
    VALUES
    (
        1, 0, GETDATE(),
        YEAR(GETDATE()),
        DATEFROMPARTS(YEAR(GETDATE()), 1, 1),
        DATEFROMPARTS(YEAR(GETDATE()), 12, 31),
        N'Tự tạo bởi schema upgrade'
    );
END;
GO
