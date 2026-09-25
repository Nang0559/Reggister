USE [FVN_REGISTER];
GO
SET NOCOUNT ON;

/*
  Verification for the legacy SuperAdmin 2FA compatibility fix.
  E0001 must have the 2407 direct grant even when no F03UserRoles row exists.
*/

DECLARE @UserId int;
DECLARE @FunctionId int;
DECLARE @PermissionId int;

SELECT TOP (1) @UserId = Id
FROM dbo.F03Users
WHERE EmployeeCode = N'E0001'
ORDER BY Id;

SELECT TOP (1) @FunctionId = Id
FROM dbo.F03Functions
WHERE FunctionCode = 2407
ORDER BY Id;

SELECT TOP (1) @PermissionId = Id
FROM dbo.F03Permissions
WHERE PermissionCode = 1
ORDER BY Id;

IF @UserId IS NULL
    THROW 51490, N'FAIL: E0001 does not exist.', 1;

IF @FunctionId IS NULL
    THROW 51491, N'FAIL: Function 2407 does not exist.', 1;

IF @PermissionId IS NULL
    THROW 51492, N'FAIL: PermissionCode=1 does not exist.', 1;

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.F03UserFunctions uf
    WHERE uf.IdUser = @UserId
      AND uf.IdFunction = @FunctionId
      AND uf.IdPermission = @PermissionId
      AND ISNULL(uf.IsActive,1) = 1
)
    THROW 51493, N'FAIL: E0001 is missing direct grant for FunctionCode 2407.', 1;

PRINT N'PASS: E0001 has direct SuperAdmin grant for UserManagement.ManageTwoFactor (2407).';
GO
