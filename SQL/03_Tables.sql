USE [FVN_REGISTER];
GO

/* Identity / authorization */
IF OBJECT_ID(N'auth.UserAccount','U') IS NULL
CREATE TABLE auth.UserAccount(
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_UserAccount PRIMARY KEY,
    EmployeeCode nvarchar(50) NOT NULL,
    UserName nvarchar(100) NULL,
    PasswordHash nvarchar(500) NULL,
    Email nvarchar(255) NULL,
    IsActive bit NOT NULL CONSTRAINT DF_UserAccount_IsActive DEFAULT(1),
    CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_UserAccount_CreatedAt DEFAULT(SYSDATETIME()),
    UpdatedAt datetime2(0) NULL,
    CONSTRAINT UQ_UserAccount_EmployeeCode UNIQUE(EmployeeCode)
);
IF OBJECT_ID(N'auth.Role','U') IS NULL
CREATE TABLE auth.Role(
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Role PRIMARY KEY,
    Code nvarchar(50) NOT NULL,
    Name nvarchar(150) NOT NULL,
    IsActive bit NOT NULL CONSTRAINT DF_Role_IsActive DEFAULT(1),
    CONSTRAINT UQ_Role_Code UNIQUE(Code)
);
IF OBJECT_ID(N'auth.Permission','U') IS NULL
CREATE TABLE auth.Permission(
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Permission PRIMARY KEY,
    Code nvarchar(100) NOT NULL,
    Name nvarchar(200) NOT NULL,
    CONSTRAINT UQ_Permission_Code UNIQUE(Code)
);
IF OBJECT_ID(N'auth.UserRole','U') IS NULL
CREATE TABLE auth.UserRole(
    UserId int NOT NULL,
    RoleId int NOT NULL,
    AssignedAt datetime2(0) NOT NULL CONSTRAINT DF_UserRole_AssignedAt DEFAULT(SYSDATETIME()),
    CONSTRAINT PK_UserRole PRIMARY KEY(UserId,RoleId)
);
IF OBJECT_ID(N'auth.RolePermission','U') IS NULL
CREATE TABLE auth.RolePermission(
    RoleId int NOT NULL,
    PermissionId int NOT NULL,
    CONSTRAINT PK_RolePermission PRIMARY KEY(RoleId,PermissionId)
);

/* HR master */
IF OBJECT_ID(N'hr.Department','U') IS NULL
CREATE TABLE hr.Department(
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Department PRIMARY KEY,
    Code nvarchar(50) NOT NULL,
    Name nvarchar(200) NOT NULL,
    ParentId int NULL,
    IsActive bit NOT NULL CONSTRAINT DF_Department_IsActive DEFAULT(1),
    CONSTRAINT UQ_Department_Code UNIQUE(Code)
);
IF OBJECT_ID(N'hr.Employee','U') IS NULL
CREATE TABLE hr.Employee(
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Employee PRIMARY KEY,
    EmployeeCode nvarchar(50) NOT NULL,
    FullName nvarchar(200) NOT NULL,
    Email nvarchar(255) NULL,
    DepartmentId int NULL,
    PositionCode nvarchar(50) NULL,
    PositionName nvarchar(150) NULL,
    IsActive bit NOT NULL CONSTRAINT DF_Employee_IsActive DEFAULT(1),
    CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_Employee_CreatedAt DEFAULT(SYSDATETIME()),
    UpdatedAt datetime2(0) NULL,
    CONSTRAINT UQ_Employee_Code UNIQUE(EmployeeCode)
);

/* Configuration */
IF OBJECT_ID(N'config.WorkYear','U') IS NULL
CREATE TABLE config.WorkYear(
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_WorkYear PRIMARY KEY,
    YearValue int NOT NULL,
    StartDate date NOT NULL,
    EndDate date NOT NULL,
    IsCurrent bit NOT NULL CONSTRAINT DF_WorkYear_IsCurrent DEFAULT(0),
    CONSTRAINT UQ_WorkYear_Year UNIQUE(YearValue),
    CONSTRAINT CK_WorkYear_Date CHECK(StartDate<=EndDate)
);
IF OBJECT_ID(N'config.CompanyHoliday','U') IS NULL
CREATE TABLE config.CompanyHoliday(
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_CompanyHoliday PRIMARY KEY,
    HolidayDate date NOT NULL,
    Name nvarchar(200) NOT NULL,
    IsActive bit NOT NULL CONSTRAINT DF_CompanyHoliday_IsActive DEFAULT(1),
    CONSTRAINT UQ_CompanyHoliday_Date UNIQUE(HolidayDate)
);
IF OBJECT_ID(N'config.LeaveType','U') IS NULL
CREATE TABLE config.LeaveType(
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_LeaveType PRIMARY KEY,
    Code nvarchar(50) NOT NULL,
    Name nvarchar(200) NOT NULL,
    IsCountedAsLeave bit NOT NULL CONSTRAINT DF_LeaveType_IsCountedAsLeave DEFAULT(1),
    IsPaid bit NOT NULL CONSTRAINT DF_LeaveType_IsPaid DEFAULT(1),
    IsActive bit NOT NULL CONSTRAINT DF_LeaveType_IsActive DEFAULT(1),
    CONSTRAINT UQ_LeaveType_Code UNIQUE(Code)
);

/* Approval configuration + immutable request snapshot */
IF OBJECT_ID(N'config.ApprovalLevel','U') IS NULL
CREATE TABLE config.ApprovalLevel(
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ApprovalLevel PRIMARY KEY,
    LevelNo int NOT NULL,
    Code nvarchar(50) NOT NULL,
    Name nvarchar(150) NOT NULL,
    IsActive bit NOT NULL CONSTRAINT DF_ApprovalLevel_IsActive DEFAULT(1),
    CONSTRAINT UQ_ApprovalLevel_Level UNIQUE(LevelNo),
    CONSTRAINT UQ_ApprovalLevel_Code UNIQUE(Code)
);
IF OBJECT_ID(N'config.Approver','U') IS NULL
CREATE TABLE config.Approver(
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Approver PRIMARY KEY,
    EmployeeCode nvarchar(50) NOT NULL,
    PositionCode nvarchar(50) NULL,
    ApprovalLevelId int NOT NULL,
    IsActive bit NOT NULL CONSTRAINT DF_Approver_IsActive DEFAULT(1),
    CONSTRAINT UQ_Approver_Employee_Level UNIQUE(EmployeeCode,ApprovalLevelId)
);

/* Leave */
IF OBJECT_ID(N'leave.LeaveRequest','U') IS NULL
CREATE TABLE leave.LeaveRequest(
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_LeaveRequest PRIMARY KEY,
    RequestCode nvarchar(50) NOT NULL,
    EmployeeCode nvarchar(50) NOT NULL,
    LeaveTypeId int NOT NULL,
    WorkYearId int NOT NULL,
    StartDate date NOT NULL,
    EndDate date NOT NULL,
    TotalUnits decimal(10,2) NOT NULL,
    IsHalfDay bit NOT NULL CONSTRAINT DF_LeaveRequest_IsHalfDay DEFAULT(0),
    HalfDayOption tinyint NULL,
    LeavePaymentType tinyint NOT NULL,
    Reason nvarchar(1000) NULL,
    Status tinyint NOT NULL CONSTRAINT DF_LeaveRequest_Status DEFAULT(0),
    SubmittedAt datetime2(0) NULL,
    CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_LeaveRequest_CreatedAt DEFAULT(SYSDATETIME()),
    UpdatedAt datetime2(0) NULL,
    RowVersion rowversion NOT NULL,
    CONSTRAINT UQ_LeaveRequest_Code UNIQUE(RequestCode),
    CONSTRAINT CK_LeaveRequest_Date CHECK(StartDate<=EndDate),
    CONSTRAINT CK_LeaveRequest_Total CHECK(TotalUnits>=0)
);
IF OBJECT_ID(N'leave.LeaveAttachment','U') IS NULL
CREATE TABLE leave.LeaveAttachment(
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_LeaveAttachment PRIMARY KEY,
    LeaveRequestId int NOT NULL,
    FileName nvarchar(255) NOT NULL,
    ContentType nvarchar(100) NOT NULL,
    FileSize bigint NOT NULL,
    StoragePath nvarchar(1000) NULL,
    CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_LeaveAttachment_CreatedAt DEFAULT(SYSDATETIME())
);
IF OBJECT_ID(N'leave.Approval','U') IS NULL
CREATE TABLE leave.Approval(
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_LeaveApproval PRIMARY KEY,
    LeaveRequestId int NOT NULL,
    LevelNo int NOT NULL,
    ApproverCode nvarchar(50) NULL,
    ApproverName nvarchar(200) NULL,
    ApproverEmail nvarchar(255) NULL,
    Decision tinyint NOT NULL CONSTRAINT DF_LeaveApproval_Decision DEFAULT(0),
    ApproveTime datetime2(0) NULL,
    Comment nvarchar(1000) NULL,
    IsRequired bit NOT NULL CONSTRAINT DF_LeaveApproval_IsRequired DEFAULT(1),
    IsOverriddenByAdmin bit NOT NULL CONSTRAINT DF_LeaveApproval_IsOverridden DEFAULT(0),
    OverriddenByName nvarchar(200) NULL,
    OverriddenAt datetime2(0) NULL,
    CONSTRAINT UQ_LeaveApproval_Request_Level UNIQUE(LeaveRequestId,LevelNo)
);

/* OT */
IF OBJECT_ID(N'ot.OTRequest','U') IS NULL
CREATE TABLE ot.OTRequest(
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_OTRequest PRIMARY KEY,
    RequestCode nvarchar(50) NOT NULL,
    EmployeeCode nvarchar(50) NOT NULL,
    WorkDate date NOT NULL,
    StartTime time(0) NOT NULL,
    EndTime time(0) NOT NULL,
    TotalHours decimal(10,2) NOT NULL,
    OTRateMultiplier decimal(10,2) NOT NULL,
    Reason nvarchar(1000) NULL,
    Status tinyint NOT NULL CONSTRAINT DF_OTRequest_Status DEFAULT(0),
    SubmittedAt datetime2(0) NULL,
    CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_OTRequest_CreatedAt DEFAULT(SYSDATETIME()),
    UpdatedAt datetime2(0) NULL,
    RowVersion rowversion NOT NULL,
    CONSTRAINT UQ_OTRequest_Code UNIQUE(RequestCode),
    CONSTRAINT CK_OTRequest_Hours CHECK(TotalHours>=0)
);
IF OBJECT_ID(N'ot.Approval','U') IS NULL
CREATE TABLE ot.Approval(
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_OTApproval PRIMARY KEY,
    OTRequestId int NOT NULL,
    LevelNo int NOT NULL,
    ApproverCode nvarchar(50) NULL,
    ApproverName nvarchar(200) NULL,
    Decision tinyint NOT NULL CONSTRAINT DF_OTApproval_Decision DEFAULT(0),
    ApproveTime datetime2(0) NULL,
    Comment nvarchar(1000) NULL,
    IsRequired bit NOT NULL CONSTRAINT DF_OTApproval_IsRequired DEFAULT(1),
    CONSTRAINT UQ_OTApproval_Request_Level UNIQUE(OTRequestId,LevelNo)
);

/* Trip */
IF OBJECT_ID(N'trip.TripRequest','U') IS NULL
CREATE TABLE trip.TripRequest(
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_TripRequest PRIMARY KEY,
    TripCode nvarchar(50) NOT NULL,
    EmployeeCode nvarchar(50) NOT NULL,
    FromDate date NOT NULL,
    ToDate date NOT NULL,
    Destination nvarchar(500) NULL,
    Purpose nvarchar(1000) NULL,
    Status tinyint NOT NULL CONSTRAINT DF_TripRequest_Status DEFAULT(0),
    SubmittedAt datetime2(0) NULL,
    CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_TripRequest_CreatedAt DEFAULT(SYSDATETIME()),
    UpdatedAt datetime2(0) NULL,
    RowVersion rowversion NOT NULL,
    CONSTRAINT UQ_TripRequest_Code UNIQUE(TripCode),
    CONSTRAINT CK_TripRequest_Date CHECK(FromDate<=ToDate)
);
IF OBJECT_ID(N'trip.Approval','U') IS NULL
CREATE TABLE trip.Approval(
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_TripApproval PRIMARY KEY,
    TripRequestId int NOT NULL,
    LevelNo int NOT NULL,
    ApproverCode nvarchar(50) NULL,
    Decision tinyint NOT NULL CONSTRAINT DF_TripApproval_Decision DEFAULT(0),
    ApproveTime datetime2(0) NULL,
    Comment nvarchar(1000) NULL,
    IsRequired bit NOT NULL CONSTRAINT DF_TripApproval_IsRequired DEFAULT(1),
    CONSTRAINT UQ_TripApproval_Request_Level UNIQUE(TripRequestId,LevelNo)
);

/* Equipment */
IF OBJECT_ID(N'equipment.Request','U') IS NULL
CREATE TABLE equipment.Request(
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_EquipmentRequest PRIMARY KEY,
    RequestCode nvarchar(50) NOT NULL,
    EmployeeCode nvarchar(50) NOT NULL,
    ItemName nvarchar(300) NOT NULL,
    Quantity decimal(18,2) NOT NULL,
    Reason nvarchar(1000) NULL,
    Status tinyint NOT NULL CONSTRAINT DF_EquipmentRequest_Status DEFAULT(0),
    SubmittedAt datetime2(0) NULL,
    CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_EquipmentRequest_CreatedAt DEFAULT(SYSDATETIME()),
    CONSTRAINT UQ_EquipmentRequest_Code UNIQUE(RequestCode),
    CONSTRAINT CK_EquipmentRequest_Quantity CHECK(Quantity>0)
);

/* Notifications / email */
IF OBJECT_ID(N'notify.Notification','U') IS NULL
CREATE TABLE notify.Notification(
    Id bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_Notification PRIMARY KEY,
    EmployeeCode nvarchar(50) NOT NULL,
    Title nvarchar(300) NOT NULL,
    Message nvarchar(2000) NOT NULL,
    IsRead bit NOT NULL CONSTRAINT DF_Notification_IsRead DEFAULT(0),
    CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_Notification_CreatedAt DEFAULT(SYSDATETIME()),
    ReadAt datetime2(0) NULL
);
IF OBJECT_ID(N'notify.EmailTemplate','U') IS NULL
CREATE TABLE notify.EmailTemplate(
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_EmailTemplate PRIMARY KEY,
    Code nvarchar(100) NOT NULL,
    Name nvarchar(200) NOT NULL,
    SubjectTemplate nvarchar(500) NOT NULL,
    BodyTemplate nvarchar(max) NOT NULL,
    IsActive bit NOT NULL CONSTRAINT DF_EmailTemplate_IsActive DEFAULT(1),
    CONSTRAINT UQ_EmailTemplate_Code UNIQUE(Code)
);
IF OBJECT_ID(N'notify.EmailQueue','U') IS NULL
CREATE TABLE notify.EmailQueue(
    Id bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_EmailQueue PRIMARY KEY,
    TemplateCode nvarchar(100) NULL,
    Recipient nvarchar(255) NOT NULL,
    Subject nvarchar(500) NOT NULL,
    Body nvarchar(max) NOT NULL,
    Status tinyint NOT NULL CONSTRAINT DF_EmailQueue_Status DEFAULT(0),
    RetryCount int NOT NULL CONSTRAINT DF_EmailQueue_RetryCount DEFAULT(0),
    LastError nvarchar(2000) NULL,
    CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_EmailQueue_CreatedAt DEFAULT(SYSDATETIME()),
    SentAt datetime2(0) NULL
);
GO
