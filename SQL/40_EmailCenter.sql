USE [FVN_REGISTER];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ============================================================================
   FVN_REGISTER - Email Center
   - Dynamic SMTP profiles
   - Template -> dispatch policy -> outbox
   - AutoSend / QueueForApproval / Disabled
   - Existing email queue remains compatible
   ============================================================================ */

IF OBJECT_ID(N'dbo.F03EmailProfiles', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03EmailProfiles', N'SecurityMode') IS NULL ALTER TABLE dbo.F03EmailProfiles ADD SecurityMode nvarchar(20) NOT NULL CONSTRAINT DF_F03EmailProfiles_SecurityMode DEFAULT 'STARTTLS';
    IF COL_LENGTH(N'dbo.F03EmailProfiles', N'AuthenticationType') IS NULL ALTER TABLE dbo.F03EmailProfiles ADD AuthenticationType nvarchar(20) NOT NULL CONSTRAINT DF_F03EmailProfiles_AuthenticationType DEFAULT 'Basic';
    IF COL_LENGTH(N'dbo.F03EmailProfiles', N'FromName') IS NULL ALTER TABLE dbo.F03EmailProfiles ADD FromName nvarchar(100) NULL;
    IF COL_LENGTH(N'dbo.F03EmailProfiles', N'ReplyTo') IS NULL ALTER TABLE dbo.F03EmailProfiles ADD ReplyTo nvarchar(100) NULL;
    IF COL_LENGTH(N'dbo.F03EmailProfiles', N'IsDefault') IS NULL ALTER TABLE dbo.F03EmailProfiles ADD IsDefault bit NOT NULL CONSTRAINT DF_F03EmailProfiles_IsDefault DEFAULT 0;
    IF COL_LENGTH(N'dbo.F03EmailProfiles', N'TimeoutSeconds') IS NULL ALTER TABLE dbo.F03EmailProfiles ADD TimeoutSeconds int NOT NULL CONSTRAINT DF_F03EmailProfiles_TimeoutSeconds DEFAULT 30;
END
ELSE
BEGIN
    CREATE TABLE dbo.F03EmailProfiles(
        Id int IDENTITY PRIMARY KEY,
        IsActive bit NULL DEFAULT 1,
        CreatedBy int NOT NULL DEFAULT 0,
        LastModifiedSource nvarchar(50) NULL,
        CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),
        ModifiedBy int NULL,
        ModifiedAt datetime2(0) NULL,
        ParentId int NOT NULL DEFAULT 0,
        IsGroup bit NOT NULL DEFAULT 0,
        Code nvarchar(20) NOT NULL,
        Name nvarchar(100) NOT NULL,
        NameEn nvarchar(100) NULL,
        EmailServerName nvarchar(100) NOT NULL,
        EmailServerType nvarchar(20) NOT NULL DEFAULT 'SMTP',
        EmailServerPort int NOT NULL DEFAULT 587,
        EmailServerEnableSsl bit NOT NULL DEFAULT 1,
        SecurityMode nvarchar(20) NOT NULL DEFAULT 'STARTTLS',
        AuthenticationType nvarchar(20) NOT NULL DEFAULT 'Basic',
        EmailAccountName nvarchar(100) NOT NULL,
        EmailAddress nvarchar(100) NOT NULL,
        FromName nvarchar(100) NULL,
        ReplyTo nvarchar(100) NULL,
        EmailPassword nvarchar(255) NULL,
        SiteUrl nvarchar(255) NULL,
        IsDefault bit NOT NULL DEFAULT 0,
        TimeoutSeconds int NOT NULL DEFAULT 30,
        Timestamp rowversion NOT NULL
    );
END;

IF OBJECT_ID(N'dbo.F03EmailQueues', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.F03EmailQueues', N'EmailProfileCode') IS NULL ALTER TABLE dbo.F03EmailQueues ADD EmailProfileCode nvarchar(20) NULL;
    IF COL_LENGTH(N'dbo.F03EmailQueues', N'DispatchMode') IS NULL ALTER TABLE dbo.F03EmailQueues ADD DispatchMode nvarchar(30) NOT NULL CONSTRAINT DF_F03EmailQueues_DispatchMode DEFAULT 'AutoSend';
    IF COL_LENGTH(N'dbo.F03EmailQueues', N'LastAttemptAt') IS NULL ALTER TABLE dbo.F03EmailQueues ADD LastAttemptAt datetime2(0) NULL;
    IF COL_LENGTH(N'dbo.F03EmailQueues', N'ApprovedAt') IS NULL ALTER TABLE dbo.F03EmailQueues ADD ApprovedAt datetime2(0) NULL;
    IF COL_LENGTH(N'dbo.F03EmailQueues', N'ApprovedBy') IS NULL ALTER TABLE dbo.F03EmailQueues ADD ApprovedBy int NULL;
    IF COL_LENGTH(N'dbo.F03EmailQueues', N'CancelledAt') IS NULL ALTER TABLE dbo.F03EmailQueues ADD CancelledAt datetime2(0) NULL;
    IF COL_LENGTH(N'dbo.F03EmailQueues', N'CancelledBy') IS NULL ALTER TABLE dbo.F03EmailQueues ADD CancelledBy int NULL;
END
ELSE
BEGIN
    CREATE TABLE dbo.F03EmailQueues(
        Id int IDENTITY PRIMARY KEY,
        IsActive bit NULL DEFAULT 1,
        CreatedBy int NOT NULL DEFAULT 0,
        LastModifiedSource nvarchar(50) NULL,
        CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),
        ModifiedBy int NULL,
        ModifiedAt datetime2(0) NULL,
        ToEmail nvarchar(255) NOT NULL,
        Subject nvarchar(255) NOT NULL,
        Body nvarchar(max) NOT NULL,
        TemplateCode nvarchar(50) NULL,
        EmailProfileCode nvarchar(20) NULL,
        DispatchMode nvarchar(30) NOT NULL DEFAULT 'AutoSend',
        Payload nvarchar(max) NULL,
        Status nvarchar(30) NOT NULL DEFAULT 'Pending',
        RetryCount int NOT NULL DEFAULT 0,
        MaxRetry int NOT NULL DEFAULT 3,
        ErrorMessage nvarchar(1000) NULL,
        LastAttemptAt datetime2(0) NULL,
        ApprovedAt datetime2(0) NULL,
        ApprovedBy int NULL,
        CancelledAt datetime2(0) NULL,
        CancelledBy int NULL,
        SentAt datetime2(0) NULL
    );
END;

IF OBJECT_ID(N'dbo.F03EmailDispatchPolicies', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03EmailDispatchPolicies(
        Id int IDENTITY PRIMARY KEY,
        IsActive bit NULL DEFAULT 1,
        CreatedBy int NOT NULL DEFAULT 0,
        LastModifiedSource nvarchar(50) NULL,
        CreatedAt datetime2(0) NOT NULL DEFAULT GETDATE(),
        ModifiedBy int NULL,
        ModifiedAt datetime2(0) NULL,
        TemplateCode nvarchar(50) NOT NULL,
        EmailProfileCode nvarchar(20) NOT NULL,
        DispatchMode nvarchar(30) NOT NULL DEFAULT 'AutoSend',
        Priority int NOT NULL DEFAULT 100,
        Description nvarchar(500) NULL
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_F03EmailDispatchPolicies_Template_Priority' AND object_id = OBJECT_ID(N'dbo.F03EmailDispatchPolicies'))
    CREATE UNIQUE INDEX UX_F03EmailDispatchPolicies_Template_Priority ON dbo.F03EmailDispatchPolicies(TemplateCode, Priority);

IF OBJECT_ID(N'dbo.F03EmailProfiles', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.F03EmailProfiles WHERE Code = 'ITSYS')
    UPDATE dbo.F03EmailProfiles SET IsDefault = 1 WHERE Code = 'ITSYS' AND IsDefault = 0 AND NOT EXISTS (SELECT 1 FROM dbo.F03EmailProfiles WHERE IsDefault = 1 AND IsActive = 1);

/* Seed a safe inactive profile only when no profile exists. Admin must configure SMTP credentials. */
IF NOT EXISTS (SELECT 1 FROM dbo.F03EmailProfiles WHERE Code = 'SYSTEMSMTP')
BEGIN
    INSERT dbo.F03EmailProfiles(IsActive, CreatedBy, CreatedAt, ParentId, IsGroup, Code, Name, EmailServerName, EmailServerType, EmailServerPort, EmailServerEnableSsl, SecurityMode, AuthenticationType, EmailAccountName, EmailAddress, IsDefault, TimeoutSeconds)
    VALUES (0, 0, GETDATE(), 0, 0, 'SYSTEMSMTP', 'FCC System Mail', 'smtp.example.local', 'SMTP', 587, 1, 'STARTTLS', 'Basic', 'system@example.local', 'system@example.local', 0, 30);
END;

/* Template helper. Existing templates are preserved; new templates are inserted only when missing. */
IF NOT EXISTS (SELECT 1 FROM dbo.F03EmailTemplates WHERE Code = 'USER_ACCOUNT_CREATED')
INSERT dbo.F03EmailTemplates(IsActive, CreatedBy, CreatedAt, Code, Subject, Body, Description)
VALUES (1,0,GETDATE(),'USER_ACCOUNT_CREATED',N'[FCC Smart Portal] Tài khoản của bạn đã được tạo',N'<p>Kính gửi {{EmployeeName}},</p><p>Tài khoản FCC Smart Portal của bạn đã được tạo.</p><p><b>Mã nhân viên:</b> {{EmployeeCode}}<br/><b>Tên đăng nhập:</b> {{UserName}}<br/><b>Phòng ban:</b> {{DepartmentCode}}</p><p>Vui lòng liên hệ bộ phận quản trị/IT để nhận thông tin xác thực nếu cần.</p><p>Trân trọng,<br/><b>FCC Smart Portal</b></p>',N'Thông báo tạo tài khoản người dùng');

IF NOT EXISTS (SELECT 1 FROM dbo.F03EmailTemplates WHERE Code = 'PASSWORD_CHANGED')
INSERT dbo.F03EmailTemplates(IsActive, CreatedBy, CreatedAt, Code, Subject, Body, Description)
VALUES (1,0,GETDATE(),'PASSWORD_CHANGED',N'[FCC Smart Portal] Mật khẩu đã được thay đổi',N'<p>Kính gửi {{EmployeeName}},</p><p>Mật khẩu tài khoản <b>{{UserName}}</b> vừa được thay đổi.</p><p>Nếu bạn không thực hiện thao tác này, vui lòng liên hệ IT ngay.</p><p>Trân trọng,<br/><b>FCC Smart Portal</b></p>',N'Thông báo đổi mật khẩu');

IF NOT EXISTS (SELECT 1 FROM dbo.F03EmailTemplates WHERE Code = 'PASSWORD_RESET')
INSERT dbo.F03EmailTemplates(IsActive, CreatedBy, CreatedAt, Code, Subject, Body, Description)
VALUES (1,0,GETDATE(),'PASSWORD_RESET',N'[FCC Smart Portal] Mật khẩu đã được đặt lại',N'<p>Kính gửi {{EmployeeName}},</p><p>Mật khẩu tài khoản <b>{{UserName}}</b> đã được quản trị viên đặt lại.</p><p>Vui lòng đăng nhập và đổi mật khẩu sau khi nhận được thông báo này.</p><p>Trân trọng,<br/><b>FCC Smart Portal</b></p>',N'Thông báo reset mật khẩu');

IF NOT EXISTS (SELECT 1 FROM dbo.F03EmailTemplates WHERE Code = 'LEAVE_APPROVAL_REQUEST')
INSERT dbo.F03EmailTemplates(IsActive, CreatedBy, CreatedAt, Code, Subject, Body, Description)
VALUES (1,0,GETDATE(),'LEAVE_APPROVAL_REQUEST',N'[FCC Smart Portal] Yêu cầu phê duyệt nghỉ phép - {{EmployeeName}}',N'<p>Kính gửi {{Role}},</p><p>Nhân viên <b>{{EmployeeName}}</b> có yêu cầu nghỉ phép đang chờ phê duyệt.</p><p>Từ {{StartDate}} đến {{EndDate}} - {{TotalDay}} ngày.</p><p>Lý do: {{Reason}}</p><p>Vui lòng đăng nhập FCC Smart Portal để xử lý.</p>',N'Yêu cầu phê duyệt nghỉ phép');

IF NOT EXISTS (SELECT 1 FROM dbo.F03EmailTemplates WHERE Code = 'OT_APPROVAL_REQUEST')
INSERT dbo.F03EmailTemplates(IsActive, CreatedBy, CreatedAt, Code, Subject, Body, Description)
VALUES (1,0,GETDATE(),'OT_APPROVAL_REQUEST',N'[FCC Smart Portal] Yêu cầu phê duyệt OT - {{EmployeeName}}',N'<p>Yêu cầu OT của <b>{{EmployeeName}}</b> đang chờ phê duyệt.</p><p>Vui lòng đăng nhập FCC Smart Portal để xử lý.</p>',N'Yêu cầu phê duyệt OT');

IF NOT EXISTS (SELECT 1 FROM dbo.F03EmailTemplates WHERE Code = 'EQUIPMENT_APPROVAL_REQUEST')
INSERT dbo.F03EmailTemplates(IsActive, CreatedBy, CreatedAt, Code, Subject, Body, Description)
VALUES (1,0,GETDATE(),'EQUIPMENT_APPROVAL_REQUEST',N'[FCC Smart Portal] Yêu cầu phê duyệt thiết bị',N'<p>Có một yêu cầu thiết bị đang chờ phê duyệt.</p><p>Vui lòng đăng nhập FCC Smart Portal để kiểm tra.</p>',N'Yêu cầu phê duyệt thiết bị');

/* Default policies. AutoSend is used for system notifications; approval-heavy events can be changed to QueueForApproval in Admin. */
IF NOT EXISTS (SELECT 1 FROM dbo.F03EmailDispatchPolicies WHERE TemplateCode = 'USER_ACCOUNT_CREATED' AND Priority = 100)
INSERT dbo.F03EmailDispatchPolicies(IsActive,CreatedBy,CreatedAt,TemplateCode,EmailProfileCode,DispatchMode,Priority,Description)
VALUES (1,0,GETDATE(),'USER_ACCOUNT_CREATED','SYSTEMSMTP','AutoSend',100,N'Thông báo tạo tài khoản');

IF NOT EXISTS (SELECT 1 FROM dbo.F03EmailDispatchPolicies WHERE TemplateCode = 'PASSWORD_CHANGED' AND Priority = 100)
INSERT dbo.F03EmailDispatchPolicies(IsActive,CreatedBy,CreatedAt,TemplateCode,EmailProfileCode,DispatchMode,Priority,Description)
VALUES (1,0,GETDATE(),'PASSWORD_CHANGED','SYSTEMSMTP','AutoSend',100,N'Thông báo đổi mật khẩu');

IF NOT EXISTS (SELECT 1 FROM dbo.F03EmailDispatchPolicies WHERE TemplateCode = 'PASSWORD_RESET' AND Priority = 100)
INSERT dbo.F03EmailDispatchPolicies(IsActive,CreatedBy,CreatedAt,TemplateCode,EmailProfileCode,DispatchMode,Priority,Description)
VALUES (1,0,GETDATE(),'PASSWORD_RESET','SYSTEMSMTP','AutoSend',100,N'Thông báo reset mật khẩu');

IF NOT EXISTS (SELECT 1 FROM dbo.F03EmailDispatchPolicies WHERE TemplateCode = 'LEAVE_APPROVAL_REQUEST' AND Priority = 100)
INSERT dbo.F03EmailDispatchPolicies(IsActive,CreatedBy,CreatedAt,TemplateCode,EmailProfileCode,DispatchMode,Priority,Description)
VALUES (1,0,GETDATE(),'LEAVE_APPROVAL_REQUEST','SYSTEMSMTP','AutoSend',100,N'Yêu cầu duyệt nghỉ phép');

IF NOT EXISTS (SELECT 1 FROM dbo.F03EmailDispatchPolicies WHERE TemplateCode = 'OT_APPROVAL_REQUEST' AND Priority = 100)
INSERT dbo.F03EmailDispatchPolicies(IsActive,CreatedBy,CreatedAt,TemplateCode,EmailProfileCode,DispatchMode,Priority,Description)
VALUES (1,0,GETDATE(),'OT_APPROVAL_REQUEST','SYSTEMSMTP','AutoSend',100,N'Yêu cầu duyệt OT');

IF NOT EXISTS (SELECT 1 FROM dbo.F03EmailDispatchPolicies WHERE TemplateCode = 'EQUIPMENT_APPROVAL_REQUEST' AND Priority = 100)
INSERT dbo.F03EmailDispatchPolicies(IsActive,CreatedBy,CreatedAt,TemplateCode,EmailProfileCode,DispatchMode,Priority,Description)
VALUES (1,0,GETDATE(),'EQUIPMENT_APPROVAL_REQUEST','SYSTEMSMTP','AutoSend',100,N'Yêu cầu duyệt thiết bị');

PRINT N'Email Center deployment completed.';
GO
