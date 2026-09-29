# 25 — Chuẩn hóa ngôn ngữ giao diện Việt / Nhật

## 1. Mục tiêu

FVN_REGISTER hỗ trợ đúng **2 ngôn ngữ giao diện**:

- `vi-VN` — Tiếng Việt
- `ja-JP` — Tiếng Nhật

Người dùng chọn ngôn ngữ **trước khi đăng nhập**. Lựa chọn được lưu ở trình duyệt và được giữ sau khi đăng nhập. Sau khi đăng nhập, người dùng vẫn có thể đổi ngôn ngữ tại thanh giao diện.

## 2. Nguyên tắc kiến trúc

### 2.1 Không dịch dữ liệu nghiệp vụ

Không dịch và không đổi:

- EmployeeCode
- DepartmentCode
- PositionCode
- RequestId
- FunctionCode / SecurityFunctionCodes
- ModuleCode
- API enum/code
- Database status/code
- SQL column/table/procedure names
- URL/route

Ví dụ `OT`, `Leave`, `Trip`, `Resolved`, `Mismatch` là code nghiệp vụ nếu đang được lưu/tra cứu; chỉ lớp UI mới dịch thành nhãn hiển thị.

### 2.2 Không nhúng song ngữ vào một label

Không dùng:

```text
Lưu / Save
Phê duyệt (Approve)
```

Thay bằng key:

```text
common.save
common.approve
```

và catalog trả về một ngôn ngữ duy nhất.

### 2.3 Một nguồn từ điển UI

`LanguageCatalog` là vocabulary trung tâm. Các module không tự tạo bản dịch riêng nếu từ đó đã có key chuẩn.

Nếu thêm màn hình mới:

1. Thêm key UI.
2. Có `vi-VN`.
3. Có `ja-JP`.
4. Razor chỉ gọi `Language.T(key)`.

## 3. Cơ chế chọn ngôn ngữ

```text
/login
  |
  +-- LanguageSelector
  |      +-- vi-VN
  |      +-- ja-JP
  |
  +-- Login
         |
         +-- LanguageService
                |
                +-- localStorage: fvn.ui.language
                +-- document.lang
```

Mặc định an toàn là `vi-VN`.

Không gửi language selection vào database nghiệp vụ và không để language thay đổi quyền hoặc logic nghiệp vụ.

## 4. Phạm vi chuẩn hóa

### UI phải chuẩn hóa

- Login
- Layout / Menu
- Dashboard
- Calendar
- Leave
- OT
- Trip
- Equipment
- HRM Sync
- Attendance Calculation
- Execution / Confirmation
- HR Review
- Notification
- Task / Action
- Approval
- Security Center
- User/Employee/Department management
- Reports
- Payroll
- Public Information
- Dialog / validation / snackbar / empty-state / error-state

### Không cần dịch

- Mã nhân viên
- Mã phòng ban/chức vụ
- Dữ liệu HRM nguyên bản
- Tên file chứng từ
- URL/API route
- SQL/database identifiers
- Audit technical values

## 5. Chuẩn thuật ngữ tiếng Việt

| Key | Chuẩn tiếng Việt |
|---|---|
| home | Trang chủ |
| calendar | Lịch công |
| leave | Nghỉ phép |
| overtime | Làm thêm giờ (OT) |
| trip | Công tác |
| equipment | Thiết bị |
| approval | Phê duyệt |
| employee | Nhân viên |
| department | Bộ phận |
| approver | Người phê duyệt |
| attendance | Chấm công |
| task | Việc cần làm |
| notification | Thông báo |
| payroll | Tính lương |
| save | Lưu |
| cancel | Hủy |
| submit | Gửi |
| approve | Phê duyệt |
| reject | Từ chối |
| pending | Chờ xử lý |
| resolved | Đã giải quyết |
| evidence | Bằng chứng |
| confirmation | Xác nhận |

Không dùng lẫn `Approver`, `Approval Policy`, `Security Center`, `Conflict HRM`, `Role HRM → User` làm text UI tiếng Việt.

## 6. Chuẩn tiếng Nhật

Bản tiếng Nhật tương ứng phải được đặt trong `LanguageCatalog`, không dịch tự phát trong từng Razor.

Các thuật ngữ nghiệp vụ quan trọng:

- Nghỉ phép → 休暇申請
- OT → 残業申請
- Công tác → 出張申請
- Chấm công → 勤怠
- Phê duyệt → 承認
- Nhân viên → 社員
- Bộ phận → 部門
- Người phê duyệt → 承認者
- Xác nhận công → 勤務実績の確認
- Bằng chứng làm việc → 勤務証拠
- Việc cần làm → タスク
- Thông báo → 通知
- Tính lương → 給与計算

## 7. Quy tắc rà soát code

Một UI literal phải được coi là chưa chuẩn nếu:

- chứa tiếng Anh trong màn hình tiếng Việt mà không phải business code;
- chứa cả Việt + Anh trong cùng label;
- có bản dịch tiếng Nhật viết trực tiếp trong một component thay vì catalog;
- snackbar/error/validation chỉ có một ngôn ngữ;
- title/menu/dialog/tooltip không dùng key;
- status/code nghiệp vụ bị dịch ngược rồi gửi trở lại API.

## 8. Definition of Done

- [x] Có `LanguageCode` cho `vi-VN` và `ja-JP`.
- [x] Có `ILanguageService`/`LanguageService`.
- [x] Có catalog tập trung.
- [x] Chọn ngôn ngữ ngay tại Login.
- [x] Lưu lựa chọn ở browser.
- [x] Có selector sau đăng nhập.
- [x] Login đã dùng vocabulary chuẩn.
- [x] MainLayout đã dùng vocabulary chuẩn ở các mục chính.
- [x] NavMenu đã loại bỏ các nhãn tiếng Anh lẫn trong tiếng Việt ở các mục chính.
- [ ] Rà và chuyển toàn bộ Razor component còn literal sang key.
- [ ] Rà dialog/validation/snackbar/error message.
- [ ] Rà email/template/public information có cần bản dịch hay không; nội dung nghiệp vụ phải được xác định language theo template.
- [ ] Thêm automated UI-language audit vào CI/build validation.
- [ ] Kiểm tra Japanese font rendering trên browser/Windows deployment.

## 9. Không làm thay đổi nghiệp vụ

Localization chỉ thay đổi presentation layer. Không thay đổi:

- approval engine;
- calculation engine;
- attendance calculation;
- OT/Leave/Trip rules;
- execution reconciliation;
- payroll gate;
- security capability;
- SQL data model.

Một lỗi dịch không được phép làm thay đổi code nghiệp vụ.
