IF NOT EXISTS (SELECT 1 FROM dbo.F03Functions WHERE FunctionCode = 900)
BEGIN
 INSERT INTO dbo.F03Functions(FunctionCode, FunctionName, Detail, CreatedBy, CreatedAt, IsActive)
 VALUES(900, N'EquipmentModule', N'Quyền sử dụng Sổ quản lý thiết bị: đăng ký, QR, repair request và tra cứu thiết bị.', 1, GETDATE(), 1);
END;
GO

-- Admin gán function 900 cho user bằng màn hình User Management hiện có.
-- Admin cấu hình approver Equipment bằng F03Approvers:
-- RequestType = 3 (Leave=0, Overtime=1, Trip=2, Equipment=3), Level=1..3, ApproveForDeptCode = dept hoặc ALL.
