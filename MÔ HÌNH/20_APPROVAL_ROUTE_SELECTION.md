# Approval Route v2 — Multi Approver Selection

## 1. Mục tiêu

Áp dụng thống nhất cho:

- Leave
- OT
- Trip
- Equipment / Requirement

Người đăng ký **không được chọn cấp phê duyệt**.

Hệ thống tự xác định các cấp phải duyệt từ `F03Employee.PositionCode`, là mã chức vụ HRM chuẩn (`0001`...`0016`). Sau đó hệ thống tải **tất cả approver hợp lệ của từng cấp** theo ` F03Approvers `.

Người đăng ký chỉ được chọn **một người cụ thể trong từng cấp**.

## 2. Luồng nghiệp vụ

```
Requester
   |
   | Department + Position
   v
F03ApprovalPolicies
   |
   | xác định Level/Sequence
   v
Approval Route
   |
   | tìm candidate
   v
F03Approvers
   |
   | nhiều người có thể cùng Level
   v
Level 1 -> [Trưởng ca 1, Trưởng ca 2, Trưởng ca 3]
Level 2 -> [Manager A, Manager B]
Level 3 -> [GM]
   |
   | requester chọn 1 người / level
   v
F03ApprovalSelections
   |
   | Submit
   v
F03ApprovalSnapshot
   |
   v
ApprovalEngine
```

## 3. Hai bảng cấu hình mới

### F03ApprovalPolicies

Xác định **cấp nào phải duyệt** theo chức vụ người đăng ký.

| Field | Ý nghĩa |
|---|---|
| RequestType | Leave / OT / Trip / Equipment |
| PositionCode | Mã chức vụ HRM của người đăng ký |
| Level | Cấp duyệt nghiệp vụ |
| Sequence | Thứ tự |
| LevelName | Tên hiển thị |
| RoleName | Role nghiệp vụ |
| Required | Có bắt buộc hay không |

### F03ApprovalSelections

Lưu lựa chọn của người đăng ký trong giai đoạn Draft/Submit.

| Field | Ý nghĩa |
|---|---|
| RequestType | Module |
| RequestId | Đơn |
| Level | Cấp |
| ApproverCode | Người được chọn |

Sau khi snapshot được tạo, ` F03ApprovalSnapshot ` là nguồn dữ liệu bất biến của quy trình duyệt.

## 4. Candidate resolution

Candidate được lấy từ ` F03Approvers `:

- cùng ` RequestType `
- cùng ` Level `
- ` IsActive = 1 `
- ưu tiên ` ApproveForDeptCode = Department ` của người đăng ký
- nếu không có cấu hình riêng thì dùng ` ApproveForDeptCode = ALL `
- một level có thể có **N người**

Không được dùng ` FirstOrDefault() ` để tự chọn approver.

## 5. UI rule

UI hiển thị:

```
Cấp 1 — Trưởng ca
[ Trưởng ca 1 v ]

Cấp 2 — Manager
[ Manager A v ]

Cấp 3 — GM
[ GM v ]
```

UI **không hiển thị dropdown chọn Level**.

Nếu một cấp chỉ có một candidate, UI hiển thị người đó và tự chọn.

Nếu cấp bắt buộc không có candidate hoặc chưa chọn candidate, không cho Submit.

## 6. Leave

Worker:

```
Worker
  -> Level 1 SubLeader/Leader
  -> Level 2 Manager
  -> Level 3 GM
```

Leader/SubLeader:

```
Leader
  -> Level 2 Manager
  -> Level 3 GM
```

Manager:

```
Manager
  -> Level 3 GM
```

## 7. OT

OT giữ các business level hiện tại:

```
Level 3 = SubLeader/Leader
Level 5 = Chief
Level 6 = Manager
Level 7 = GM
```

Worker: ` 3 -> 5 -> 6 -> 7 `

Leader: ` 5 -> 6 -> 7 `

Chief: ` 6 -> 7 `

Manager: ` 7 `

Không được suy diễn OT level bằng phép toán ` current.Level - 1 `.

## 8. Trip

Trip dùng cùng nguyên tắc Leave:

```
Worker       -> 1 -> 2 -> 3
Leader/Chief -> 2 -> 3
Manager      -> 3
```

## 9. Equipment / Requirement

Equipment dùng policy riêng:

```
Worker       -> Level 1 -> Level 2 -> GM
Leader/Chief -> Level 2 -> GM
Manager      -> GM
```

Nếu nghiệp vụ Requirement sau này có hierarchy khác, chỉ cần thêm policy; không sửa ApprovalEngine.

## 10. Snapshot invariant

Sau Submit:

- ` F03ApprovalSelections ` chỉ là staging.
- ` F03ApprovalSnapshot ` chứa approver đã chọn.
- Thay đổi Employee/Position/Approver configuration sau Submit **không làm thay đổi đơn đang chạy**.
- ApprovalEngine chỉ duyệt người nằm trong snapshot.

## 11. API

```
GET /api/approval-route/preview?requestType=Leave
GET /api/approval-route/preview?requestType=Overtime
GET /api/approval-route/preview?requestType=Trip
GET /api/approval-route/preview?requestType=Equipment
```

Response gồm:

- Level
- Sequence
- LevelName
- RoleName
- Required
- Candidates[]
- SelectedApproverCode

## 12. SQL triển khai

Chạy riêng:

` SQL/17_ApprovalRouteSelection.sql `

Script tạo:

- ` F03ApprovalPolicies ` với `PositionCode` trực tiếp từ HRM
- ` F03ApprovalSelections `
- unique indexes
- policy mặc định theo từng `F03Positions.PositionCode`

Không sử dụng và không tạo `F03ApprovalPositionGroups`. Các mã `EMP`, `SL`, `CHIEF`, `MGR`, `GM` là dữ liệu seed legacy và không phải HRM PositionCode.

Sau đó Admin phải bảo đảm ` F03Approvers ` có đủ candidate thực tế cho từng Level/Department.
