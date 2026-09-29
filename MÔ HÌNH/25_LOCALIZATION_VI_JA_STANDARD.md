# 25 — Chuẩn hóa ngôn ngữ giao diện Việt / Nhật

## 1. Mục tiêu

FVN_REGISTER hỗ trợ đúng **2 ngôn ngữ ở Presentation/Web UI**:

- `vi-VN` — Tiếng Việt
- `ja-JP` — Tiếng Nhật

Người dùng chọn ngôn ngữ tại Login và có thể đổi lại sau khi đăng nhập.

## 2. Ranh giới kiến trúc — bắt buộc

### Backend giữ một ngôn ngữ duy nhất: TIẾNG VIỆT

Không triển khai localization cho:

- Core/Domain;
- Contract/DTO;
- Application;
- Infrastructure;
- API;
- SQL/Stored Procedure;
- audit/log kỹ thuật.

Backend tiếp tục trả message tiếng Việt như hiện tại để **không làm thay đổi nghiệp vụ/API hiện hữu**.

Nếu một lỗi nghiệp vụ cần hiển thị đúng tiếng Nhật, API có thể bổ sung `Code` ổn định; UI dùng `Code` để tra catalog. `Message` tiếng Việt vẫn được giữ làm fallback.

Ví dụ:

```json
{
  "code": "SECURITY.FORBIDDEN",
  "message": "Bạn không có quyền thực hiện thao tác này."
}
```

UI:

```text
vi-VN -> Bạn không có quyền thực hiện thao tác này.
ja-JP -> この操作を実行する権限がありません。
```

**Không đổi API hàng loạt chỉ để localization.**

## 3. Presentation/Web UI phải 100% localization

Mọi text người dùng nhìn thấy phải thuộc một trong hai nhóm:

1. **Localization key** → dịch VI/JA.
2. **Business/display data** → dữ liệu thực tế, không dịch nếu không có quy tắc nghiệp vụ.

Phải rà đủ:

- Login;
- Layout/Menu;
- Dashboard;
- Calendar;
- Leave;
- OT;
- Trip;
- Equipment;
- HRM Sync;
- Attendance Calculation;
- Execution/Confirmation;
- HR Review;
- Notification;
- Task/Action;
- Approval;
- Security Center;
- User/Employee/Department/Position;
- Reports;
- Payroll;
- Public Information;
- dialog;
- button;
- tab;
- table header;
- label;
- placeholder;
- tooltip;
- validation;
- Snackbar/Alert;
- loading/empty/error/success state;
- confirmation dialog;
- file/evidence UI;
- import/export/print UI;
- email UI/template nếu template được render tại Web.

Không được coi việc dịch Menu là hoàn thành localization.

## 4. Quy tắc viết UI

Không được:

```razor
<MudButton>Lưu / Save</MudButton>
<MudText>Danh sách OT</MudText>
<MudAlert>Load failed</MudAlert>
```

Phải dùng:

```razor
<MudButton>@Language.T("common.save")</MudButton>
<MudText>@Language.T("overtime.list.title")</MudText>
<MudAlert>@Language.T("common.loadFailed")</MudAlert>
```

Một label chỉ hiển thị **một ngôn ngữ tại một thời điểm**.

## 5. Business code không được localization

Không dịch và không gửi bản dịch ngược về API/database:

- EmployeeCode;
- DepartmentCode;
- PositionCode;
- RequestId;
- FunctionCode/SecurityFunctionCodes;
- ModuleCode;
- enum/code;
- database status/code;
- SQL identifier;
- URL/route;
- technical audit values.

Ví dụ database vẫn lưu `Approved`, UI mới hiển thị `Đã phê duyệt` hoặc `承認済み`.

## 6. Vocabulary trung tâm

`LanguageCatalog` là **single source of truth** cho UI vocabulary. Module không tự dịch lại một thuật ngữ đã có key chuẩn.

Nhóm key bắt buộc:

```text
common.*
nav.*
login.*
validation.*
message.*
status.*
leave.*
overtime.*
trip.*
equipment.*
attendance.*
execution.*
approval.*
security.*
hrm.*
notification.*
task.*
payroll.*
report.*
publicInfo.*
```

## 7. Thông báo và lỗi

UI không được tạo thông báo song ngữ bằng cách nối chuỗi.

Ví dụ không dùng:

```csharp
$"Lưu thành công / Save successfully"
```

Dùng key:

```text
message.saveSuccess
```

Nếu thông báo đến từ API:

1. ưu tiên `Code`;
2. tra `LanguageCatalog`;
3. nếu chưa có key thì hiển thị `Message` tiếng Việt làm fallback;
4. không làm mất thông báo chỉ vì thiếu bản dịch.

## 8. Tham số trong thông báo

Không ghép câu tiếng Việt rồi cố dịch cả câu.

Dùng template:

```text
employee.notFound = Không tìm thấy nhân viên {0}.
```

và:

```text
employee.notFound = 社員 {0} が見つかりません。
```

Placeholder/giá trị phải giữ nguyên.

## 9. Notification / Task / Action

Notification/Task/Action nghiệp vụ vẫn lưu code và dữ liệu tham số. UI dịch khi render.

Ví dụ:

```text
EXECUTION.CONFIRM_REQUIRED
WorkDate=2026-09-15
```

Không lưu trực tiếp câu tiếng Nhật vào dữ liệu nghiệp vụ.

## 10. Trạng thái

Database/API giữ code:

```text
Pending
Approved
Rejected
Resolved
Mismatch
AwaitingConfirmation
Cancelled
Expired
```

UI dịch:

```text
Pending       -> Chờ xử lý / 処理待ち
Approved      -> Đã phê duyệt / 承認済み
Rejected      -> Từ chối / 却下
Resolved      -> Đã giải quyết / 解決済み
Mismatch      -> Không khớp / 不一致
```

## 11. LanguageService

Web dùng:

```text
ILanguageService
LanguageService
LanguageCatalog
LanguageCode
LanguageSelector
```

Lựa chọn lưu tại browser:

```text
fvn.ui.language
```

và cập nhật `<html lang="...">`.

Mặc định an toàn: `vi-VN`.

## 12. Không được bỏ sót chức năng

Rà localization phải dựa trên **source inventory**, không dựa trên danh sách màn hình bằng tay.

Các thư mục Razor phải được kiểm tra định kỳ:

```text
FVN_REGISTER/FVN_REGISTER.Shared/Pages
FVN_REGISTER/FVN_REGISTER.Shared/Components
FVN_REGISTER/FVN_REGISTER.Shared/Dialogs
FVN_REGISTER/FVN_REGISTER.Shared/Layout
```

Ngoài `.razor`, phải rà `.cs` thuộc Shared có tạo text UI, Snackbar, Dialog, Notification hoặc validation.

## 13. UI-language audit

Repository phải có script kiểm tra các dấu hiệu chưa localization:

- literal text trong Razor;
- button text;
- MudText/MudAlert/MudTooltip;
- label/placeholder/helper text;
- dialog text;
- validation text;
- Snackbar/notification text;
- title/page title;
- table header;
- các chuỗi tiếng Anh còn sót trong UI.

Audit **không được coi business data/code là UI literal**.

CI có thể chạy audit và tạo danh sách file cần xử lý. Không tự động thay chuỗi bằng máy nếu có nguy cơ đổi nghiệp vụ.

## 14. Definition of Done

### Nền tảng

- [x] `LanguageCode` VI/JA.
- [x] `ILanguageService`/`LanguageService`.
- [x] `LanguageCatalog` tập trung.
- [x] Chọn ngôn ngữ tại Login.
- [x] Lưu lựa chọn ở browser.
- [x] Đổi ngôn ngữ sau Login.
- [x] MainLayout/NavMenu dùng vocabulary chuẩn ở phần đã chuyển.

### Toàn bộ UI

- [ ] Không còn UI literal Việt/Anh chưa được đánh giá.
- [ ] Tất cả button/action được chuyển key.
- [ ] Tất cả menu/tab/title/table header được chuyển key.
- [ ] Tất cả dialog/confirm/tooltip/placeholder được chuyển key.
- [ ] Tất cả validation được chuyển key.
- [ ] Tất cả Snackbar/Alert/error/success/warning/loading/empty state được chuyển key.
- [ ] Calendar/Notification/Task/Action được chuyển key.
- [ ] Leave/OT/Trip/Equipment được chuyển key.
- [ ] Attendance/Execution/HR Review được chuyển key.
- [ ] Approval/Security/HRM được chuyển key.
- [ ] Payroll/Report/Public Information được chuyển key.
- [ ] Không còn text Anh lẫn trong giao diện VI.
- [ ] Không có tiếng Nhật viết trực tiếp trong Razor/C# UI.
- [ ] Mọi key đều có cả VI và JA.
- [ ] Thiếu key phải fallback an toàn về tiếng Việt.
- [ ] Có automated UI-language audit.
- [ ] Build và test UI cho cả `vi-VN` và `ja-JP`.

## 15. Không thay đổi nghiệp vụ

Localization chỉ thuộc Presentation/Web UI. Không thay đổi:

- approval engine;
- calculation engine;
- attendance calculation;
- OT/Leave/Trip rules;
- execution reconciliation;
- payroll gate;
- security capability;
- SQL data model;
- API contract hiện hữu, trừ khi bổ sung error code không phá compatibility.
