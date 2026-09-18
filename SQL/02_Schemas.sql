/*
FVN_REGISTER uses the default dbo schema because the EF Core model maps
the F03* entities to dbo tables. Do not create domain schemas here.
*/
USE [FVN_REGISTER];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXEC(N'CREATE SCHEMA [dbo]');
GO
