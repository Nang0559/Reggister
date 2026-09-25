# Security Function Registry

## Mục tiêu

Function Registry bổ sung một lớp quản lý vòng đời Security Function mà không thay đổi logic nghiệp vụ hiện tại.

- `FunctionCode` vẫn được giữ để tương thích permission cũ.
- `FunctionKey` là định danh ổn định của capability.
- Source code là nguồn để discovery.
- Discovery không tự cấp quyền, không tự thu hồi RoleFunction và không tự xóa Function.
- Chức năng mới đi vào `PendingRegistration`.
- Chức năng đã có trong DB nhưng không còn trong source đi vào `PendingRetirement`.
- Chức năng thay thế được lưu bằng `ReplacementFunctionKey`.
- Function/Role đã từng được sử dụng không bị hard-delete; thao tác Delete chuyển sang Retired/Inactive.

## Nguồn discovery hiện tại

1. `SecurityFunctionCodes` — tất cả capability numeric hiện hữu của hệ thống.
2. `[SecurityFunctionDefinition]` — dùng cho capability mới cần một `FunctionKey` tường minh.

Ví dụ:

```csharp
[SecurityFunctionDefinition("Equipment.Asset.Repair", "Sửa chữa tài sản", ModuleCode = "Equipment", ActionCode = "Repair", ScopeCode = "Own")]
public const int EquipmentRepair = 2304;
```

Hoặc đặt attribute trên endpoint/class nếu capability đã được quản lý bằng một cơ chế khác.

## Luồng lifecycle

```text
Source Code
   |
   v
Function Discovery
   |
   v
Reconciliation
   +--> Matched ------------------> Active
   +--> New -----------------------> PendingRegistration
   +--> Missing from source -------> PendingRetirement
   +--> Reappeared retired --------> Conflict
```

Admin xử lý trong `/admin/security/function-registry`:

- Register
- Ignore
- Retire
- Replace
- Manual Create/Edit
- Delete/Retire
- Role Create/Edit/Delete/Deactivate

## Migration

Chạy `SQL/46_SecurityFunctionRegistry.sql` sau các script Security hiện hữu. Script:

1. bổ sung metadata vào `F03Functions`;
2. backfill `FunctionKey` từ `SecurityFunctionCodes`;
3. tạo unique index trên `FunctionKey`;
4. tạo `F03SecurityFunctionRegistry`;
5. seed registry từ catalog hiện hữu.

Không thay đổi `F03RoleFunctions` hoặc `F03UserFunctions`.

## Background discovery

API chạy `SecurityFunctionDiscoveryHostedService` sau startup 15 giây và sau đó mỗi 6 giờ. Nếu database chưa sẵn sàng, worker retry sau 5 phút. Nút `Quét chức năng` trên Function Registry cho phép chạy on-demand.

## Nguyên tắc an toàn

Discovery chỉ phát hiện và phân loại. Việc cấp quyền vẫn do Role/User Function hiện tại quyết định. Vì vậy việc thêm endpoint/capability mới không làm phát sinh quyền cho user hiện hữu.
