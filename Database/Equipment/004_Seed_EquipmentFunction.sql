/*
  Equipment RBAC
  - 2301 is the module/view capability used by EquipmentController.
  - 2306 is deliberately separate so Excel import is not granted to every
    equipment user.
  - 900 is kept as a legacy module marker for existing deployments.
*/
IF NOT EXISTS (SELECT 1 FROM dbo.F03Functions WHERE FunctionCode = 2301)
BEGIN
    INSERT INTO dbo.F03Functions(FunctionCode, FunctionName, Detail, CreatedBy, CreatedAt, IsActive)
    VALUES(2301, N'Equipment.View', N'Quyền sử dụng Sổ quản lý thiết bị và tra cứu QR.', 1, GETDATE(), 1);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.F03Functions WHERE FunctionCode = 2302)
BEGIN
    INSERT INTO dbo.F03Functions(FunctionCode, FunctionName, Detail, CreatedBy, CreatedAt, IsActive)
    VALUES(2302, N'Equipment.Create', N'Đăng ký thiết bị mới.', 1, GETDATE(), 1);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.F03Functions WHERE FunctionCode = 2303)
BEGIN
    INSERT INTO dbo.F03Functions(FunctionCode, FunctionName, Detail, CreatedBy, CreatedAt, IsActive)
    VALUES(2303, N'Equipment.Edit', N'Chỉnh sửa/gửi đăng ký thiết bị.', 1, GETDATE(), 1);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.F03Functions WHERE FunctionCode = 2304)
BEGIN
    INSERT INTO dbo.F03Functions(FunctionCode, FunctionName, Detail, CreatedBy, CreatedAt, IsActive)
    VALUES(2304, N'Equipment.Repair', N'Tạo và gửi yêu cầu sửa chữa thiết bị.', 1, GETDATE(), 1);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.F03Functions WHERE FunctionCode = 2305)
BEGIN
    INSERT INTO dbo.F03Functions(FunctionCode, FunctionName, Detail, CreatedBy, CreatedAt, IsActive)
    VALUES(2305, N'Equipment.Approve', N'Phê duyệt yêu cầu thiết bị.', 1, GETDATE(), 1);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.F03Functions WHERE FunctionCode = 2306)
BEGIN
    INSERT INTO dbo.F03Functions(FunctionCode, FunctionName, Detail, CreatedBy, CreatedAt, IsActive)
    VALUES(2306, N'Equipment.Import', N'Import Excel thiết bị theo schema của từng phòng ban.', 1, GETDATE(), 1);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.F03Functions WHERE FunctionCode = 2307)
BEGIN
    INSERT INTO dbo.F03Functions(FunctionCode, FunctionName, Detail, CreatedBy, CreatedAt, IsActive)
    VALUES(2307, N'Equipment.Export', N'Xuất dữ liệu thiết bị.', 1, GETDATE(), 1);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.F03Functions WHERE FunctionCode = 2308)
BEGIN
    INSERT INTO dbo.F03Functions(FunctionCode, FunctionName, Detail, CreatedBy, CreatedAt, IsActive)
    VALUES(2308, N'Equipment.Cancel', N'Hủy yêu cầu thiết bị.', 1, GETDATE(), 1);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.F03Functions WHERE FunctionCode = 900)
BEGIN
    INSERT INTO dbo.F03Functions(FunctionCode, FunctionName, Detail, CreatedBy, CreatedAt, IsActive)
    VALUES(900, N'EquipmentModule', N'Legacy marker: quyền sử dụng Sổ quản lý thiết bị.', 1, GETDATE(), 1);
END;
GO

-- Admin gán 2301 cho user được phép dùng module Equipment.
-- Admin gán thêm 2306 CHỈ cho user được phép import Excel.
-- Scope của 2301/2306 vẫn được kiểm soát bởi RBAC hiện hành và EnsureScopeAsync.
-- RequestType Equipment = 3 cho cấu hình approver.
