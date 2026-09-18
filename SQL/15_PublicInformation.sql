USE [FVN_REGISTER];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.F03PublicInformation', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03PublicInformation
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03PublicInformation PRIMARY KEY,
        Type nvarchar(30) NOT NULL,
        Title nvarchar(300) NOT NULL,
        Summary nvarchar(1000) NULL,
        Content nvarchar(max) NULL,
        Status nvarchar(30) NOT NULL CONSTRAINT DF_F03PublicInformation_Status DEFAULT(N'Draft'),
        EffectiveFrom datetime2(0) NULL,
        EffectiveTo datetime2(0) NULL,
        IsImportant bit NOT NULL CONSTRAINT DF_F03PublicInformation_IsImportant DEFAULT(0),
        AttachmentUrl nvarchar(1000) NULL,
        PublishedBy int NULL,
        PublishedAt datetime2(0) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03PublicInformation_CreatedAt DEFAULT(GETDATE()),
        CreatedBy int NOT NULL CONSTRAINT DF_F03PublicInformation_CreatedBy DEFAULT(0),
        ModifiedAt datetime2(0) NULL,
        ModifiedBy int NULL
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03PublicInformation_Published' AND object_id=OBJECT_ID(N'dbo.F03PublicInformation'))
    CREATE INDEX IX_F03PublicInformation_Published ON dbo.F03PublicInformation(Status, IsImportant, PublishedAt);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.F03Functions WHERE FunctionCode=2801)
BEGIN
    INSERT INTO dbo.F03Functions(FunctionCode, FunctionName, Detail, ModuleCode, ActionCode, ScopeCode, DisplayOrder, IsActive)
    VALUES(2801,N'Public Information - Manage',N'Tạo, sửa, publish và archive thông báo/quy định công khai',N'PublicInformation',N'Manage',N'All',2801,1);
END
ELSE
BEGIN
    UPDATE dbo.F03Functions SET FunctionName=N'Public Information - Manage',Detail=N'Tạo, sửa, publish và archive thông báo/quy định công khai',ModuleCode=N'PublicInformation',ActionCode=N'Manage',ScopeCode=N'All',DisplayOrder=2801,IsActive=1 WHERE FunctionCode=2801;
END;
GO
