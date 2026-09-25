SET NOCOUNT ON;
IF COL_LENGTH('dbo.F03Users','TwoFactorRequired') IS NULL
    ALTER TABLE dbo.F03Users ADD TwoFactorRequired bit NOT NULL CONSTRAINT DF_F03Users_TwoFactorRequired DEFAULT(0);
IF COL_LENGTH('dbo.F03Users','TwoFactorRequiredAt') IS NULL
    ALTER TABLE dbo.F03Users ADD TwoFactorRequiredAt datetime2 NULL;
IF COL_LENGTH('dbo.F03Users','TwoFactorRequiredBy') IS NULL
    ALTER TABLE dbo.F03Users ADD TwoFactorRequiredBy int NULL;
IF COL_LENGTH('dbo.F03Users','TwoFactorEnabled') IS NULL
    ALTER TABLE dbo.F03Users ADD TwoFactorEnabled bit NOT NULL CONSTRAINT DF_F03Users_TwoFactorEnabled DEFAULT(0);
IF COL_LENGTH('dbo.F03Users','TwoFactorEnabledAt') IS NULL
    ALTER TABLE dbo.F03Users ADD TwoFactorEnabledAt datetime2 NULL;
IF COL_LENGTH('dbo.F03Users','TwoFactorSecretEncrypted') IS NULL
    ALTER TABLE dbo.F03Users ADD TwoFactorSecretEncrypted nvarchar(512) NULL;
GO

IF OBJECT_ID('dbo.F03TwoFactorChallenges','U') IS NULL
BEGIN
    CREATE TABLE dbo.F03TwoFactorChallenges
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03TwoFactorChallenges PRIMARY KEY,
        UserId int NOT NULL,
        ChallengeHash nvarchar(128) NOT NULL,
        ExpiresAt datetime2 NOT NULL,
        FailedAttempts int NOT NULL CONSTRAINT DF_F03TwoFactorChallenges_FailedAttempts DEFAULT(0),
        IsConsumed bit NOT NULL CONSTRAINT DF_F03TwoFactorChallenges_IsConsumed DEFAULT(0),
        ConsumedAt datetime2 NULL,
        Purpose nvarchar(20) NOT NULL CONSTRAINT DF_F03TwoFactorChallenges_Purpose DEFAULT('Login'),
        IsActive bit NULL CONSTRAINT DF_F03TwoFactorChallenges_IsActive DEFAULT(1),
        CreatedBy int NOT NULL CONSTRAINT DF_F03TwoFactorChallenges_CreatedBy DEFAULT(0),
        CreatedAt datetime2 NOT NULL CONSTRAINT DF_F03TwoFactorChallenges_CreatedAt DEFAULT(GETDATE()),
        ModifiedBy int NULL,
        ModifiedAt datetime2 NULL,
        LastModifiedSource nvarchar(255) NULL,
        CONSTRAINT FK_F03TwoFactorChallenges_User FOREIGN KEY(UserId) REFERENCES dbo.F03Users(Id)
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_F03TwoFactorChallenges_ChallengeHash' AND object_id=OBJECT_ID('dbo.F03TwoFactorChallenges'))
    CREATE UNIQUE INDEX UX_F03TwoFactorChallenges_ChallengeHash ON dbo.F03TwoFactorChallenges(ChallengeHash);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_F03TwoFactorChallenges_User_Active' AND object_id=OBJECT_ID('dbo.F03TwoFactorChallenges'))
    CREATE INDEX IX_F03TwoFactorChallenges_User_Active ON dbo.F03TwoFactorChallenges(UserId, IsConsumed, ExpiresAt);
GO
