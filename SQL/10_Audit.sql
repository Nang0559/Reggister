USE [FVN_REGISTER];
GO
IF OBJECT_ID(N'audit.ChangeLog','U') IS NULL
CREATE TABLE audit.ChangeLog(
    Id bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_ChangeLog PRIMARY KEY,
    EntityName nvarchar(200) NOT NULL,
    EntityId nvarchar(100) NOT NULL,
    Action nvarchar(50) NOT NULL,
    ChangedBy nvarchar(100) NULL,
    ChangedAt datetime2(0) NOT NULL CONSTRAINT DF_ChangeLog_ChangedAt DEFAULT(SYSDATETIME()),
    DataJson nvarchar(max) NULL,
    CONSTRAINT CK_ChangeLog_Json CHECK(DataJson IS NULL OR ISJSON(DataJson)=1)
);
CREATE INDEX IX_ChangeLog_Entity ON audit.ChangeLog(EntityName,EntityId,ChangedAt DESC);
CREATE INDEX IX_ChangeLog_ChangedAt ON audit.ChangeLog(ChangedAt DESC);
GO
