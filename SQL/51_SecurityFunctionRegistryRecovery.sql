USE [FVN_REGISTER];
GO
SET NOCOUNT ON;

/*
  Mục đích:
  - Bảo đảm danh mục phát hiện chức năng không phụ thuộc vào F03Functions.
  - Nếu F03Functions bị xóa toàn bộ, Function Registry vẫn giữ các mã đã phát hiện để SuperAdmin đăng ký lại từ giao diện.
  - Không tự cấp quyền cho Role/User.
  - Không tự tạo lại F03Functions; việc đăng ký lại phải do SuperAdmin xác nhận trên giao diện.
*/

IF OBJECT_ID(N'dbo.F03SecurityFunctionRegistry', N'U') IS NULL
    THROW 51480, N'Danh mục phát hiện chức năng chưa tồn tại. Hãy chạy 46_SecurityFunctionRegistry.sql trước.', 1;

IF COL_LENGTH(N'dbo.F03SecurityFunctionRegistry', N'IsIgnored') IS NULL
    ALTER TABLE dbo.F03SecurityFunctionRegistry ADD IsIgnored bit NOT NULL CONSTRAINT DF_F03SecurityFunctionRegistry_IsIgnored_Recovery DEFAULT 0;

/* Không cho phép thao tác xóa chức năng thông thường làm mất bản ghi phát hiện.
   Bản ghi Registry là bằng chứng để khôi phục chức năng từ mã nguồn. */

IF EXISTS
(
    SELECT 1
    FROM dbo.F03SecurityFunctionRegistry
    WHERE FunctionCode <= 0
)
    THROW 51481, N'Danh mục phát hiện chức năng có mã chức năng không hợp lệ.', 1;

IF EXISTS
(
    SELECT 1
    FROM dbo.F03SecurityFunctionRegistry
    GROUP BY FunctionKey
    HAVING COUNT(*) > 1
)
    THROW 51482, N'Danh mục phát hiện chức năng có mã định danh trùng.', 1;

PRINT N'=== SECURITY FUNCTION REGISTRY RECOVERY CONTRACT OK ===';
GO
