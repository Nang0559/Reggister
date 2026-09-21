/* FVN_REGISTER - Public Registration Forms v1. Separate from F03PublicInformation. */
SET NOCOUNT ON;
IF OBJECT_ID(N'dbo.F03PublicForms', N'U') IS NULL
BEGIN
CREATE TABLE dbo.F03PublicForms(
 Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03PublicForms PRIMARY KEY,
 FormCode nvarchar(50) NOT NULL, Title nvarchar(300) NOT NULL, Description nvarchar(2000) NULL, CategoryCode nvarchar(50) NULL,
 Status nvarchar(20) NOT NULL CONSTRAINT DF_F03PublicForms_Status DEFAULT N'Draft',
 StartAt datetime2 NULL, EndAt datetime2 NULL,
 AllowMultipleSubmit bit NOT NULL CONSTRAINT DF_F03PublicForms_AllowMultiple DEFAULT 0,
 RequireApproval bit NOT NULL CONSTRAINT DF_F03PublicForms_RequireApproval DEFAULT 0,
 MaxSubmissions int NULL, Version int NOT NULL CONSTRAINT DF_F03PublicForms_Version DEFAULT 1,
 CreatedBy int NOT NULL CONSTRAINT DF_F03PublicForms_CreatedBy DEFAULT(0), CreatedAt datetime2 NOT NULL CONSTRAINT DF_F03PublicForms_CreatedAt DEFAULT GETDATE(),
 ModifiedBy int NULL, ModifiedAt datetime2 NULL, PublishedAt datetime2 NULL, ClosedAt datetime2 NULL,
 IsActive bit NOT NULL CONSTRAINT DF_F03PublicForms_IsActive DEFAULT 1,
 CONSTRAINT UQ_F03PublicForms_FormCode UNIQUE(FormCode),
 CONSTRAINT CK_F03PublicForms_Status CHECK(Status IN (N'Draft',N'Published',N'Closed',N'Archived')),
 CONSTRAINT CK_F03PublicForms_DateRange CHECK(EndAt IS NULL OR StartAt IS NULL OR EndAt >= StartAt),
 CONSTRAINT CK_F03PublicForms_Max CHECK(MaxSubmissions IS NULL OR MaxSubmissions > 0)
); END;
IF OBJECT_ID(N'dbo.F03PublicFormQuestions', N'U') IS NULL
BEGIN
CREATE TABLE dbo.F03PublicFormQuestions(
 Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03PublicFormQuestions PRIMARY KEY,
 FormId int NOT NULL, QuestionCode nvarchar(50) NOT NULL, QuestionText nvarchar(1000) NOT NULL,
 QuestionType nvarchar(30) NOT NULL, HelpText nvarchar(1000) NULL, Placeholder nvarchar(300) NULL,
 IsRequired bit NOT NULL CONSTRAINT DF_F03PublicFormQuestions_IsRequired DEFAULT 0,
 Sequence int NOT NULL CONSTRAINT DF_F03PublicFormQuestions_Sequence DEFAULT 1,
 IsActive bit NOT NULL CONSTRAINT DF_F03PublicFormQuestions_IsActive DEFAULT 1,
 CONSTRAINT FK_F03PublicFormQuestions_Form FOREIGN KEY(FormId) REFERENCES dbo.F03PublicForms(Id) ON DELETE CASCADE,
 CONSTRAINT UQ_F03PublicFormQuestions_Code UNIQUE(FormId,QuestionCode),
 CONSTRAINT CK_F03PublicFormQuestions_Type CHECK(QuestionType IN (N'Text',N'Textarea',N'Number',N'Date',N'Time',N'DateTime',N'SingleChoice',N'MultiChoice',N'YesNo',N'Department',N'Employee',N'File'))
); END;
IF OBJECT_ID(N'dbo.F03PublicFormQuestionOptions', N'U') IS NULL
BEGIN
CREATE TABLE dbo.F03PublicFormQuestionOptions(
 Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03PublicFormQuestionOptions PRIMARY KEY,
 QuestionId int NOT NULL, OptionCode nvarchar(50) NOT NULL, OptionText nvarchar(300) NOT NULL,
 Sequence int NOT NULL CONSTRAINT DF_F03PublicFormQuestionOptions_Sequence DEFAULT 1,
 IsActive bit NOT NULL CONSTRAINT DF_F03PublicFormQuestionOptions_IsActive DEFAULT 1,
 CONSTRAINT FK_F03PublicFormQuestionOptions_Question FOREIGN KEY(QuestionId) REFERENCES dbo.F03PublicFormQuestions(Id) ON DELETE CASCADE,
 CONSTRAINT UQ_F03PublicFormQuestionOptions_Code UNIQUE(QuestionId,OptionCode)
); END;
IF OBJECT_ID(N'dbo.F03PublicFormAudiences', N'U') IS NULL
BEGIN
CREATE TABLE dbo.F03PublicFormAudiences(
 Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03PublicFormAudiences PRIMARY KEY,
 FormId int NOT NULL, ScopeType nvarchar(20) NOT NULL, ScopeValue nvarchar(100) NULL,
 IsActive bit NOT NULL CONSTRAINT DF_F03PublicFormAudiences_IsActive DEFAULT 1,
 CONSTRAINT FK_F03PublicFormAudiences_Form FOREIGN KEY(FormId) REFERENCES dbo.F03PublicForms(Id) ON DELETE CASCADE,
 CONSTRAINT CK_F03PublicFormAudiences_Type CHECK(ScopeType IN (N'AllCompany',N'Department',N'Position',N'Employee'))
); END;
IF OBJECT_ID(N'dbo.F03PublicFormSubmissions', N'U') IS NULL
BEGIN
CREATE TABLE dbo.F03PublicFormSubmissions(
 Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03PublicFormSubmissions PRIMARY KEY,
 FormId int NOT NULL, EmployeeCode nvarchar(50) NOT NULL,
 SubmittedAt datetime2 NOT NULL CONSTRAINT DF_F03PublicFormSubmissions_SubmittedAt DEFAULT GETDATE(),
 Status nvarchar(20) NOT NULL CONSTRAINT DF_F03PublicFormSubmissions_Status DEFAULT N'Submitted',
 FormVersion int NOT NULL CONSTRAINT DF_F03PublicFormSubmissions_Version DEFAULT 1,
 IsCancelled bit NOT NULL CONSTRAINT DF_F03PublicFormSubmissions_IsCancelled DEFAULT 0,
 CancelledAt datetime2 NULL, CancelledBy int NULL,
 CONSTRAINT FK_F03PublicFormSubmissions_Form FOREIGN KEY(FormId) REFERENCES dbo.F03PublicForms(Id) ON DELETE CASCADE,
 CONSTRAINT CK_F03PublicFormSubmissions_Status CHECK(Status IN (N'Submitted',N'Approved',N'Rejected',N'Cancelled'))
); END;
IF OBJECT_ID(N'dbo.F03PublicFormAnswers', N'U') IS NULL
BEGIN
CREATE TABLE dbo.F03PublicFormAnswers(
 Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03PublicFormAnswers PRIMARY KEY,
 SubmissionId int NOT NULL, QuestionId int NOT NULL, TextValue nvarchar(max) NULL,
 NumberValue decimal(18,4) NULL, DateValue datetime2 NULL, BoolValue bit NULL, JsonValue nvarchar(max) NULL,
 CONSTRAINT FK_F03PublicFormAnswers_Submission FOREIGN KEY(SubmissionId) REFERENCES dbo.F03PublicFormSubmissions(Id) ON DELETE CASCADE,
 CONSTRAINT FK_F03PublicFormAnswers_Question FOREIGN KEY(QuestionId) REFERENCES dbo.F03PublicFormQuestions(Id),
 CONSTRAINT UQ_F03PublicFormAnswers UNIQUE(SubmissionId,QuestionId)
); END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03PublicForms_StatusWindow' AND object_id=OBJECT_ID(N'dbo.F03PublicForms')) CREATE INDEX IX_F03PublicForms_StatusWindow ON dbo.F03PublicForms(Status,StartAt,EndAt,IsActive);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03PublicFormAudiences_Form' AND object_id=OBJECT_ID(N'dbo.F03PublicFormAudiences')) CREATE INDEX IX_F03PublicFormAudiences_Form ON dbo.F03PublicFormAudiences(FormId,ScopeType,ScopeValue,IsActive);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03PublicFormSubmissions_FormEmployee' AND object_id=OBJECT_ID(N'dbo.F03PublicFormSubmissions')) CREATE INDEX IX_F03PublicFormSubmissions_FormEmployee ON dbo.F03PublicFormSubmissions(FormId,EmployeeCode,Status);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_F03PublicFormAnswers_Submission' AND object_id=OBJECT_ID(N'dbo.F03PublicFormAnswers')) CREATE INDEX IX_F03PublicFormAnswers_Submission ON dbo.F03PublicFormAnswers(SubmissionId);

IF OBJECT_ID(N'dbo.F03Functions',N'U') IS NOT NULL
BEGIN
 IF NOT EXISTS(SELECT 1 FROM dbo.F03Functions WHERE FunctionCode=2807)
 INSERT INTO dbo.F03Functions(FunctionCode,FunctionName,Detail,ModuleCode,ActionCode,ScopeCode,DisplayOrder,IsActive)
 VALUES(2807,N'Public Registration Form - Manage',N'Tạo, thiết kế, publish, đóng và quản lý biểu mẫu đăng ký động',N'PublicForm',N'Manage',N'All',2807,1);
 ELSE
 UPDATE dbo.F03Functions SET FunctionName=N'Public Registration Form - Manage',Detail=N'Tạo, thiết kế, publish, đóng và quản lý biểu mẫu đăng ký động',ModuleCode=N'PublicForm',ActionCode=N'Manage',ScopeCode=N'All',DisplayOrder=2807,IsActive=1 WHERE FunctionCode=2807;
END;
