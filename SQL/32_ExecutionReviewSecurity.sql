USE [FVN_REGISTER];
GO
SET NOCOUNT ON;

IF EXISTS (SELECT 1 FROM dbo.F03Functions WHERE FunctionCode = 2107)
BEGIN
    UPDATE dbo.F03Functions
    SET FunctionName = N'OT.Export',
        Detail = N'Xuất báo cáo OT.',
        ModuleCode = N'OT',
        ActionCode = N'EXPORT',
        ScopeCode = N'Department',
        DisplayOrder = 2107
    WHERE FunctionCode = 2107;
END;
GO

IF EXISTS (SELECT 1 FROM dbo.F03Functions WHERE FunctionCode = 2802)
BEGIN
    UPDATE dbo.F03Functions
    SET FunctionName = N'Execution.Review',
        Detail = N'Xem và giải quyết phản hồi đối soát thực tế của nhân viên.',
        ModuleCode = N'EXECUTION',
        ActionCode = N'REVIEW',
        ScopeCode = N'All',
        DisplayOrder = 2802,
        IsActive = 1
    WHERE FunctionCode = 2802;
END
ELSE
BEGIN
    INSERT dbo.F03Functions
        (CreatedBy, FunctionCode, FunctionName, Detail, ModuleCode, ActionCode, ScopeCode, DisplayOrder, IsActive)
    VALUES
        (0, 2802, N'Execution.Review',
         N'Xem và giải quyết phản hồi đối soát thực tế của nhân viên.',
         N'EXECUTION', N'REVIEW', N'All', 2802, 1);
END;
GO
GO
/* Payroll security: view/prepare/lock/export are separate capabilities. */
MERGE dbo.F03Functions AS T
USING (VALUES
 (2803,N'Payroll.View',N'Xem bảng công theo kỳ lương.',N'PAYROLL',N'VIEW',N'All',2803),
 (2804,N'Payroll.Prepare',N'Chuẩn bị snapshot Payroll Input.',N'PAYROLL',N'PREPARE',N'All',2804),
 (2805,N'Payroll.Lock',N'Khóa kỳ lương.',N'PAYROLL',N'LOCK',N'All',2805),
 (2806,N'Payroll.Export',N'Xuất bảng công theo kỳ lương.',N'PAYROLL',N'EXPORT',N'All',2806)
) AS S(FunctionCode,FunctionName,Detail,ModuleCode,ActionCode,ScopeCode,DisplayOrder)
ON T.FunctionCode=S.FunctionCode
WHEN MATCHED THEN UPDATE SET FunctionName=S.FunctionName,Detail=S.Detail,ModuleCode=S.ModuleCode,ActionCode=S.ActionCode,ScopeCode=S.ScopeCode,DisplayOrder=S.DisplayOrder,IsActive=1
WHEN NOT MATCHED THEN INSERT(CreatedBy,FunctionCode,FunctionName,Detail,ModuleCode,ActionCode,ScopeCode,DisplayOrder,IsActive)
VALUES(0,S.FunctionCode,S.FunctionName,S.Detail,S.ModuleCode,S.ActionCode,S.ScopeCode,S.DisplayOrder,1);
GO

/* P0 RBAC: calculation is a write action and must not use Attendance.View. */
INSERT dbo.F03Functions(IsActive,CreatedBy,FunctionCode,FunctionName,Detail,ModuleCode,ActionCode,ScopeCode,DisplayOrder)
SELECT 1,0,2911,N'Attendance.Calculate',N'Tính giờ HRM và ghi kết quả chấm công',N'Attendance',N'Calculate',N'All',830
WHERE NOT EXISTS (SELECT 1 FROM dbo.F03Functions WHERE FunctionCode=2911);
GO
INSERT dbo.F03RoleFunctions(IdRole,IdFunction)
SELECT r.Id,f.Id FROM dbo.F03Roles r CROSS JOIN dbo.F03Functions f
WHERE r.RoleCode IN (1,2) AND f.FunctionCode=2911
  AND NOT EXISTS(SELECT 1 FROM dbo.F03RoleFunctions rf WHERE rf.IdRole=r.Id AND rf.IdFunction=f.Id);
GO
/* Functions 2801-2806 are created by scripts 15/32; grant only after creation. */
INSERT dbo.F03RoleFunctions(IdRole,IdFunction)
SELECT r.Id,f.Id FROM dbo.F03Roles r CROSS JOIN dbo.F03Functions f
WHERE r.RoleCode IN (1,2) AND f.FunctionCode IN (2801,2802,2803,2804,2805,2806)
  AND NOT EXISTS(SELECT 1 FROM dbo.F03RoleFunctions rf WHERE rf.IdRole=r.Id AND rf.IdFunction=f.Id);
GO
