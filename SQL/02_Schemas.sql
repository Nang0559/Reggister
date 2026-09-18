/*
EF Core maps the current FVN_REGISTER model to the dbo schema.
Domain schemas such as auth/leave/ot are intentionally not used.
*/
USE [FVN_REGISTER];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXEC(N'CREATE SCHEMA [dbo]');
GO
