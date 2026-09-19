# FVN REGISTER — Workspace / CMS / Inbox / Dashboard / Security / Dynamic Help

## 1. Kiến trúc thống nhất

```text
                     FVN REGISTER
                          |
                  +-------v-------+
                  | HOME WORKSPACE|
                  +-------+-------+
                          |
       +------------------+------------------+
       |                  |                  |
 ACTION CENTER          MY WORK          MANAGEMENT
       |                  |                  |
 Notifications       Leave / OT       Dashboard
 Approval Inbox       Trip / Equipment Security
       |                  |                  |
       +------------------+------------------+
                          |
                AUTHORIZATION BOUNDARY
              Capability + Data Scope + Audit
```

Home là workspace, không phải nơi cấp quyền.

## 2. CMS

```text
Draft -> Preview -> Published -> Public Read
   \___________________________> Archived
```

Nguồn dữ liệu: F03PublicInformation. Capability: PublicInformationManage (2801).
Published không sửa trực tiếp. Tạo phiên bản mới để giữ lịch sử.

## 3. Notification + Approval

```text
Notification ----+
                 +--> Action Center --> Scope Check --> User Action --> Audit
Approval --------+
```

Approval batch phải được kiểm tra toàn bộ trước workflow. Không silently skip item không hợp lệ.

## 4. Dashboard orchestration

```text
UserIdentity
    |
DashboardOrchestrator
    +-- LeaveProvider
    +-- OTProvider
    +-- TripProvider
    +-- EquipmentProvider
    +-- authorized contributions
```

Provider tự khai báo RequiredFunctionCode. Contribution không đủ capability không được trả về.

## 5. Equipment đa phòng ban

Không dùng EAV thuần túy.

```text
F03EquipmentAssets
  +-- canonical fields
  +-- CustomDataJson
          ^
          |
F03EquipmentFieldDefinitions
          ^
          |
Department Schema
```

Import:

```text
Excel -> Header Mapping -> Staging -> Validation -> Preview -> Commit -> Audit
```

Ví dụ:
- IT: Mã thiết bị, Tên, IP Address, MAC Address.
- Hành chính: Mã thiết bị, Tên, Vị trí, Nhà cung cấp.
Hai phòng ban không cần hai bảng thiết bị.

## 6. Authorization

```text
Authentication -> Capability (WHAT) -> Data Scope (WHICH DATA)
                                      -> Service/Query/Command -> Audit
```

Scope: All > Department > Employee > Own > None.
IsAdmin không phải data-scope bypass.

## 7. Dynamic Help

Thanh ứng dụng có nút ? Component xác định FeatureCode từ route hiện tại và hiển thị mục đích, sơ đồ luồng, bước thao tác, ví dụ, quyền/phạm vi và link tới /huong-dan?feature=....
FeatureCode ổn định giúp sau này thay catalog tĩnh bằng DB/CMS mà không đổi UI contract.

## 8. Chuẩn tài liệu

| Lớp | Nội dung |
|---|---|
| SQL | bảng, function, scope, seed, deployment |
| Domain/Code | entity, service, query, command, audit |
| UI | workspace, trạng thái, capability-aware actions |
| Help/Guide | sơ đồ, thao tác, ví dụ, security |

Khi một lớp thay đổi, cập nhật ba lớp còn lại trong cùng change set.

## 9. Kiểm thử

Không coi việc UI ẩn nút là security test.
1. Không có capability -> 403.
2. Có capability nhưng sai scope -> từ chối.
3. Đúng capability + scope -> thành công.
4. Batch có một item sai scope -> toàn batch bị từ chối.
5. Thay đổi quyền -> audit và session policy được áp dụng.