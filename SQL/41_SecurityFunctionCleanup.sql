SET NOCOUNT ON;
-- Attendance employee feedback is a real capability: employees may submit their own
-- reconciliation feedback while HR review remains protected by 2802 + operator assignment.
IF NOT EXISTS (SELECT 1 FROM dbo.F03Functions WHERE FunctionCode = 2912)
BEGIN
    INSERT INTO dbo.F03Functions
        (FunctionCode, FunctionName, Detail, ModuleCode, ActionCode, ScopeCode, DisplayOrder, IsActive, CreatedBy, CreatedAt)
    VALUES
        (2912, N'Attendance.Feedback', N'Gửi xác nhận, phản hồi và evidence cho ngày công của chính mình',
         N'Attendance', N'Feedback', N'Own', 2912, 1, 0, GETDATE());
END
ELSE
    UPDATE dbo.F03Functions SET IsActive = 1, FunctionName = N'Attendance.Feedback',
        Detail = N'Gửi xác nhận, phản hồi và evidence cho ngày công của chính mình',
        ModuleCode = N'Attendance', ActionCode = N'Feedback', ScopeCode = N'Own'
    WHERE FunctionCode = 2912;
GO

INSERT INTO dbo.F03RoleFunctions (IdRole, IdFunction, IsActive, CreatedBy, CreatedAt)
SELECT r.Id, f.Id, 1, 0, GETDATE()
FROM dbo.F03Roles r
CROSS JOIN dbo.F03Functions f
WHERE r.IsActive = 1 AND r.RoleCode > 0 AND f.FunctionCode = 2912
  AND NOT EXISTS (SELECT 1 FROM dbo.F03RoleFunctions rf WHERE rf.IdRole = r.Id AND rf.IdFunction = f.Id);
GO

-- These capabilities were declared but have no corresponding business action/service
-- on the current branch. Keep them out of the active matrix until the workflow exists.
UPDATE dbo.F03Functions SET IsActive = 0
WHERE FunctionCode IN (2106, 2308, 2309, 2310, 2311, 2312, 2602);
GO


-- Explicit per-user 2FA administration. Granted only to SuperAdmin (RoleCode=1).
IF NOT EXISTS (SELECT 1 FROM dbo.F03Functions WHERE FunctionCode = 2407)
BEGIN
    INSERT INTO dbo.F03Functions
        (FunctionCode, FunctionName, Detail, ModuleCode, ActionCode, ScopeCode, DisplayOrder, IsActive, CreatedBy, CreatedAt)
    VALUES
        (2407, N'UserManagement.ManageTwoFactor', N'Quản lý bắt buộc xác thực 2 lớp cho từng tài khoản', N'Security', N'ManageTwoFactor', N'Global', 2407, 1, 0, GETDATE());
END
ELSE
    UPDATE dbo.F03Functions SET IsActive=1, FunctionName=N'UserManagement.ManageTwoFactor', Detail=N'Quản lý bắt buộc xác thực 2 lớp cho từng tài khoản', ModuleCode=N'Security', ActionCode=N'ManageTwoFactor', ScopeCode=N'Global' WHERE FunctionCode=2407;
GO
INSERT INTO dbo.F03RoleFunctions (IdRole, IdFunction, IsActive, CreatedBy, CreatedAt)
SELECT r.Id, f.Id, 1, 0, GETDATE() FROM dbo.F03Roles r CROSS JOIN dbo.F03Functions f
WHERE r.IsActive=1 AND r.RoleCode=1 AND f.FunctionCode=2407
  AND NOT EXISTS (SELECT 1 FROM dbo.F03RoleFunctions rf WHERE rf.IdRole=r.Id AND rf.IdFunction=f.Id);
GO
