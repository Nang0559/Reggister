USE [FVN_REGISTER];
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name=N'auth') EXEC(N'CREATE SCHEMA [auth]');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name=N'hr') EXEC(N'CREATE SCHEMA [hr]');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name=N'leave') EXEC(N'CREATE SCHEMA [leave]');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name=N'ot') EXEC(N'CREATE SCHEMA [ot]');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name=N'trip') EXEC(N'CREATE SCHEMA [trip]');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name=N'equipment') EXEC(N'CREATE SCHEMA [equipment]');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name=N'notify') EXEC(N'CREATE SCHEMA [notify]');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name=N'audit') EXEC(N'CREATE SCHEMA [audit]');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name=N'config') EXEC(N'CREATE SCHEMA [config]');
GO
