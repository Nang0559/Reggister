USE [FVN_REGISTER];
GO
SET NOCOUNT ON;

PRINT N'=== SECURITY FUNCTION REGISTRY VERIFY ===';

IF OBJECT_ID(N'dbo.F03Functions', N'U') IS NULL
    THROW 51470, N'F03Functions chưa tồn tại.', 1;
IF OBJECT_ID(N'dbo.F03SecurityFunctionRegistry', N'U') IS NULL
    THROW 51471, N'F03SecurityFunctionRegistry chưa tồn tại. Hãy chạy 46_SecurityFunctionRegistry.sql.', 1;
IF COL_LENGTH(N'dbo.F03Functions', N'FunctionKey') IS NULL
    THROW 51472, N'F03Functions.FunctionKey chưa tồn tại.', 1;

IF EXISTS (SELECT 1 FROM dbo.F03Functions WHERE FunctionKey IS NULL OR LTRIM(RTRIM(FunctionKey)) = N'')
    THROW 51473, N'F03Functions còn FunctionKey rỗng.', 1;
IF EXISTS (SELECT 1 FROM dbo.F03Functions GROUP BY FunctionKey HAVING COUNT(*) > 1)
    THROW 51474, N'F03Functions có FunctionKey trùng.', 1;
IF EXISTS (SELECT 1 FROM dbo.F03SecurityFunctionRegistry GROUP BY FunctionKey HAVING COUNT(*) > 1)
    THROW 51475, N'Function Registry có FunctionKey trùng.', 1;

SELECT
    LifecycleStatus,
    COUNT(*) AS FunctionCount
FROM dbo.F03SecurityFunctionRegistry
GROUP BY LifecycleStatus
ORDER BY LifecycleStatus;

SELECT TOP (100)
    r.FunctionKey,
    r.FunctionCode,
    r.DefinitionName,
    r.LifecycleStatus,
    r.SourceType,
    r.LastSeenAt,
    r.ReplacementFunctionKey,
    CASE WHEN f.Id IS NULL THEN 0 ELSE 1 END AS RegisteredInF03Functions
FROM dbo.F03SecurityFunctionRegistry r
LEFT JOIN dbo.F03Functions f ON f.FunctionKey = r.FunctionKey
ORDER BY CASE r.LifecycleStatus
    WHEN N'Conflict' THEN 1
    WHEN N'PendingRegistration' THEN 2
    WHEN N'PendingRetirement' THEN 3
    WHEN N'Active' THEN 4
    ELSE 5 END,
    r.FunctionKey;

PRINT N'=== SECURITY FUNCTION REGISTRY VERIFY OK ===';
GO
