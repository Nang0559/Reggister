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
        ScopeCode = N'All',
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