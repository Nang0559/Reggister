USE [FVN_REGISTER];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

/*
  Shared operator assignment / resource responsibility.
  This is intentionally separate from:
    - F03RoleFunctions / F03Functions: capability (what a user may do)
    - F03ManagedScopes: organizational data scope
    - F03ApprovalPolicies / F03Approvers: approval workflow
    - F03PublicFormAudiences: who may register a public form

  Semantics:
    1. RBAC capability is always required by the API.
    2. If no operator assignment exists for a function/resource, existing RBAC+scope behavior remains.
    3. If resource-specific assignments exist, only those employees may operate that resource.
    4. If no resource-specific assignment exists but global assignments (ResourceId IS NULL) exist,
       those employees may operate all resources of that ResourceType.
    5. HRM F03Employee is the source of truth for EmployeeCode; this table stores only the assignment.
*/

IF OBJECT_ID(N'dbo.F03FeatureOperatorAssignments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.F03FeatureOperatorAssignments
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03FeatureOperatorAssignments PRIMARY KEY,
        IsActive bit NULL CONSTRAINT DF_F03FeatureOperatorAssignments_IsActive DEFAULT(1),
        CreatedBy int NOT NULL CONSTRAINT DF_F03FeatureOperatorAssignments_CreatedBy DEFAULT(0),
        LastModifiedSource nvarchar(200) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03FeatureOperatorAssignments_CreatedAt DEFAULT(GETDATE()),
        ModifiedBy int NULL,
        ModifiedAt datetime2(0) NULL,

        EmployeeCode nvarchar(50) NOT NULL,
        FunctionCode int NOT NULL,
        ResourceType nvarchar(50) NOT NULL,
        ResourceId int NULL,
        Remark nvarchar(500) NULL,

        /* FunctionCode is validated by application against F03Functions. */
    );
END;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name=N'UX_F03FeatureOperatorAssignments_Active'
      AND object_id=OBJECT_ID(N'dbo.F03FeatureOperatorAssignments'))
BEGIN
    CREATE UNIQUE INDEX UX_F03FeatureOperatorAssignments_Active
    ON dbo.F03FeatureOperatorAssignments(FunctionCode,ResourceType,ResourceId,EmployeeCode)
    WHERE IsActive=1;
END;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name=N'IX_F03FeatureOperatorAssignments_Lookup'
      AND object_id=OBJECT_ID(N'dbo.F03FeatureOperatorAssignments'))
BEGIN
    CREATE INDEX IX_F03FeatureOperatorAssignments_Lookup
    ON dbo.F03FeatureOperatorAssignments(FunctionCode,ResourceType,ResourceId,IsActive,EmployeeCode);
END;
GO

/* Re-assert the four existing capabilities that use resource assignment. */
IF OBJECT_ID(N'dbo.F03Functions',N'U') IS NOT NULL
BEGIN
    INSERT dbo.F03Functions(IsActive,CreatedBy,FunctionCode,FunctionName,Detail,ModuleCode,ActionCode,ScopeCode,DisplayOrder)
    SELECT 1,0,v.FunctionCode,v.FunctionName,v.Detail,v.ModuleCode,v.ActionCode,v.ScopeCode,v.DisplayOrder
    FROM (VALUES
        (2801,N'PublicInformation.Manage',N'Quản lý thông tin công khai',N'PublicInformation',N'Manage',N'All',2801),
        (2802,N'Execution.Review',N'Xem và giải quyết phản hồi đối soát thực tế của nhân viên',N'EXECUTION',N'REVIEW',N'All',2802),
        (2807,N'PublicForm.Manage',N'Tạo, thiết kế, publish và đóng biểu mẫu đăng ký',N'PublicForm',N'Manage',N'All',2807),
        (2808,N'PublicForm.SubmissionView',N'Xem danh sách, chi tiết và tổng hợp đăng ký biểu mẫu',N'PublicForm',N'SubmissionView',N'All',2808),
        (2809,N'PublicForm.Export',N'Xuất Excel dữ liệu đăng ký biểu mẫu',N'PublicForm',N'Export',N'All',2809)
    ) v(FunctionCode,FunctionName,Detail,ModuleCode,ActionCode,ScopeCode,DisplayOrder)
    WHERE NOT EXISTS(SELECT 1 FROM dbo.F03Functions f WHERE f.FunctionCode=v.FunctionCode);
END;
GO

/* Public Information is an existing capability: explicitly keep it in Admin/SuperAdmin. */
IF OBJECT_ID(N'dbo.F03RoleFunctions',N'U') IS NOT NULL
BEGIN
    INSERT dbo.F03RoleFunctions(IdRole,IdFunction)
    SELECT r.Id,f.Id
    FROM dbo.F03Roles r
    JOIN dbo.F03Functions f ON f.FunctionCode=2801
    WHERE r.RoleCode IN(1,2)
      AND NOT EXISTS(
          SELECT 1 FROM dbo.F03RoleFunctions rf
          WHERE rf.IdRole=r.Id AND rf.IdFunction=f.Id);
END;
GO

PRINT N'FVN_REGISTER feature operator assignment security completed.';
GO
