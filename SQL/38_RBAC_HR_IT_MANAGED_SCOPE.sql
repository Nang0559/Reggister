/* 38_RBAC_HR_IT_MANAGED_SCOPE.sql
   Canonical RBAC hardening for §22-25.
   - Adds HR/IT role catalog while retiring legacy Approver role from effective RBAC.
   - Adds missing Equipment capabilities.
   - Adds employee ManagedScope assignments; approval remains F03ApprovalPolicies-driven.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.F03Permissions',N'U') IS NOT NULL
BEGIN
    INSERT dbo.F03Permissions(IsActive,CreatedBy,PermissionCode,PermissionName,Detail)
    SELECT 1,0,v.PermissionCode,v.PermissionName,v.Detail
    FROM (VALUES
        (7,N'HR',N'Human resources operations'),
        (8,N'IT',N'IT, security and integration operations')
    ) v(PermissionCode,PermissionName,Detail)
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dbo.F03Permissions p WHERE p.PermissionCode=v.PermissionCode
    );
END;

INSERT dbo.F03Roles(RoleCode,RoleName,Detail,IsSystem,IsActive,CreatedBy)
SELECT v.RoleCode,v.RoleName,v.Detail,1,1,0
FROM (VALUES
    (7,N'HR',N'Human resources operations'),
    (8,N'IT',N'IT, security and integration operations')
) v(RoleCode,RoleName,Detail)
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.F03Roles r WHERE r.RoleCode=v.RoleCode
);

UPDATE dbo.F03Roles
SET IsActive=0,
    RoleName=N'Legacy Approver',
    Detail=N'Legacy role retired. Approval is resolved by F03ApprovalPolicies + ApprovalRouteService.',
    LastModifiedSource=N'RBAC_APPROVAL_POLICY_MIGRATION',
    ModifiedAt=GETDATE()
WHERE RoleCode=4;

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id=OBJECT_ID(N'dbo.F03ManagedScopes') AND type=N'U')
BEGIN
    CREATE TABLE dbo.F03ManagedScopes
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03ManagedScopes PRIMARY KEY,
        IsActive bit NOT NULL CONSTRAINT DF_F03ManagedScopes_IsActive DEFAULT(1),
        CreatedBy int NOT NULL CONSTRAINT DF_F03ManagedScopes_CreatedBy DEFAULT(0),
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_F03ManagedScopes_CreatedAt DEFAULT(GETDATE()),
        ModifiedBy int NULL,
        ModifiedAt datetime2(0) NULL,
        LastModifiedSource nvarchar(50) NULL,

        EmployeeCode nvarchar(50) NOT NULL,
        NodeType nvarchar(30) NOT NULL,
        NodeCode nvarchar(50) NULL,
        FactoryCode nvarchar(50) NULL,
        DeptCode nvarchar(20) NULL,
        SubDepartmentCode nvarchar(20) NULL,
        IncludeChildren bit NOT NULL CONSTRAINT DF_F03ManagedScopes_IncludeChildren DEFAULT(1),
        Remark nvarchar(500) NULL,

        CONSTRAINT CK_F03ManagedScopes_NodeType
            CHECK(NodeType IN(N'Company',N'Factory',N'Department',N'SubDepartment'))
    );

    CREATE INDEX IX_F03ManagedScopes_Employee
        ON dbo.F03ManagedScopes(EmployeeCode,IsActive);

    CREATE INDEX IX_F03ManagedScopes_Node
        ON dbo.F03ManagedScopes(NodeType,NodeCode,DeptCode,SubDepartmentCode,IsActive);

    CREATE UNIQUE INDEX UX_F03ManagedScopes_Assignment
        ON dbo.F03ManagedScopes(
            EmployeeCode,NodeType,
            ISNULL(NodeCode,N''),
            ISNULL(FactoryCode,N''),
            ISNULL(DeptCode,N''),
            ISNULL(SubDepartmentCode,N''));
END;

DECLARE @Equipment TABLE
(
    FunctionCode int,
    FunctionName nvarchar(100),
    Detail nvarchar(500),
    ActionCode nvarchar(50),
    ScopeCode nvarchar(30),
    DisplayOrder int
);

INSERT @Equipment VALUES
(2309,N'Equipment.Assign',N'Gán thiết bị',N'Assign',N'Department',375),
(2310,N'Equipment.Transfer',N'Điều chuyển thiết bị',N'Transfer',N'Department',376),
(2311,N'Equipment.Return',N'Thu hồi/trả thiết bị',N'Return',N'Department',377),
(2312,N'Equipment.Liquidate',N'Thanh lý thiết bị',N'Liquidate',N'Department',378),
(2313,N'Equipment.QR',N'Tra cứu/scan QR thiết bị',N'QR',N'Department',379),
(2314,N'Equipment.History',N'Xem lịch sử thiết bị',N'History',N'Department',380);

INSERT dbo.F03Functions
(
    IsActive,CreatedBy,FunctionCode,FunctionName,Detail,
    ModuleCode,ActionCode,ScopeCode,DisplayOrder
)
SELECT 1,0,e.FunctionCode,e.FunctionName,e.Detail,
       N'Equipment',e.ActionCode,e.ScopeCode,e.DisplayOrder
FROM @Equipment e
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.F03Functions f WHERE f.FunctionCode=e.FunctionCode
);

DECLARE @RoleIds TABLE(RoleCode int,Id int);
INSERT @RoleIds(RoleCode,Id)
SELECT r.RoleCode,r.Id
FROM dbo.F03Roles r
WHERE r.RoleCode IN(1,2,3,5,6,7,8) AND r.IsActive=1;

-- SuperAdmin/Admin: all Equipment actions.
INSERT dbo.F03RoleFunctions(IdRole,IdFunction)
SELECT r.Id,f.Id
FROM @RoleIds r
JOIN dbo.F03Functions f ON f.FunctionCode IN(2309,2310,2311,2312,2313,2314)
WHERE r.RoleCode IN(1,2)
AND NOT EXISTS(SELECT 1 FROM dbo.F03RoleFunctions x WHERE x.IdRole=r.Id AND x.IdFunction=f.Id);

-- HR: asset assignment/return/transfer/history.
INSERT dbo.F03RoleFunctions(IdRole,IdFunction)
SELECT r.Id,f.Id
FROM @RoleIds r
JOIN dbo.F03Functions f ON f.FunctionCode IN(2309,2310,2311,2314)
WHERE r.RoleCode=7
AND NOT EXISTS(SELECT 1 FROM dbo.F03RoleFunctions x WHERE x.IdRole=r.Id AND x.IdFunction=f.Id);

-- IT: technical equipment operations and history/transfer; no automatic business approval.
INSERT dbo.F03RoleFunctions(IdRole,IdFunction)
SELECT r.Id,f.Id
FROM @RoleIds r
JOIN dbo.F03Functions f ON f.FunctionCode IN(2301,2303,2304,2310,2313,2314)
WHERE r.RoleCode=8
AND NOT EXISTS(SELECT 1 FROM dbo.F03RoleFunctions x WHERE x.IdRole=r.Id AND x.IdFunction=f.Id);

-- Editor keeps business approval capability, but approval is valid only when
-- F03ApprovalPolicies resolves the current employee/position/node.
INSERT dbo.F03RoleFunctions(IdRole,IdFunction)
SELECT r.Id,f.Id
FROM @RoleIds r
JOIN dbo.F03Functions f ON f.FunctionCode IN(2005,2105,2205,2305)
WHERE r.RoleCode IN(1,2,3,7)
AND NOT EXISTS(SELECT 1 FROM dbo.F03RoleFunctions x WHERE x.IdRole=r.Id AND x.IdFunction=f.Id);

-- Normal User/Guest must not inherit approval capabilities from the old
-- "all module functions" seed.
DELETE rf
FROM dbo.F03RoleFunctions rf
JOIN dbo.F03Roles r ON r.Id=rf.IdRole
JOIN dbo.F03Functions f ON f.Id=rf.IdFunction
WHERE r.RoleCode IN(5,6)
  AND f.FunctionCode IN(2005,2105,2205,2305);

PRINT N'38_RBAC_HR_IT_MANAGED_SCOPE ready.';
