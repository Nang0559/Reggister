/*
FVN_REGISTER - SQL 02 Preflight
Purpose: validate the deployment environment before changing schema.
*/
SET NOCOUNT ON;
IF DB_ID(N'FVN_REGISTER') IS NULL
    THROW 51000, 'Database FVN_REGISTER does not exist. Run 01_Database.sql first.', 1;
USE [FVN_REGISTER];
GO
IF SERVERPROPERTY('ProductMajorVersion') IS NULL
    THROW 51001, 'Cannot determine SQL Server version.', 1;
PRINT N'02_Preflight: OK';
GO
