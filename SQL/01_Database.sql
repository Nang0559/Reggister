/*
FVN_REGISTER - SQL Server deployment entry point.
Run this file first in SSMS/sqlcmd.
*/
IF DB_ID(N'FVN_REGISTER') IS NULL
BEGIN
    CREATE DATABASE [FVN_REGISTER];
END
GO

USE [FVN_REGISTER];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'dbo')
    EXEC(N'CREATE SCHEMA [dbo]');
GO
