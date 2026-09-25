USE [FVN_REGISTER];
GO
SET NOCOUNT ON;

PRINT N'=== SECURITY FUNCTION RECOVERY VERIFY ===';

IF OBJECT_ID(N'dbo.F03SecurityFunctionRegistry', N'U') IS NULL
    THROW 51490, N'Danh mục phát hiện chức năng chưa tồn tại.', 1;

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.F03SecurityFunctionRegistry
    WHERE FunctionKey = N'Security.ManageFunctions'
      AND FunctionCode = 2603
)
    THROW 51491, N'Không tìm thấy định nghĩa Security.ManageFunctions trong Function Registry.', 1;

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.F03SecurityFunctionRegistry
    WHERE FunctionKey = N'Security.ManageRoles'
      AND FunctionCode = 2602
)
    THROW 51492, N'Không tìm thấy định nghĩa Security.ManageRoles trong Function Registry.', 1;

/* F03Functions có thể tạm thời không chứa các dòng này sau thao tác xóa.
   Đây không phải lỗi của Registry; SuperAdmin sẽ khôi phục từ danh mục phát hiện. */
SELECT
    r.FunctionKey,
    r.FunctionCode,
    r.DefinitionName,
    r.LifecycleStatus,
    CASE WHEN f.Id IS NULL THEN N'Chờ đăng ký' ELSE N'Đã đăng ký' END AS RegistrationState
FROM dbo.F03SecurityFunctionRegistry r
LEFT JOIN dbo.F03Functions f ON f.FunctionKey = r.FunctionKey
WHERE r.FunctionKey IN (N'Security.ManageFunctions', N'Security.ManageRoles')
ORDER BY r.FunctionCode;

PRINT N'=== SECURITY FUNCTION RECOVERY VERIFY OK ===';
GO
