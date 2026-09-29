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

## 3. Presentation/Web UI phải 100% localization

Mọi text người dùng nhìn thấy phải thuộc một trong hai nhóm: localization key hoặc business/display data. Phải rà toàn bộ Pages, Components, Dialogs, Layout và `.cs` thuộc Shared có tạo text UI.

Phạm vi bắt buộc gồm Login, Dashboard, Calendar, Leave, OT, Trip, Equipment, HRM, Attendance, Execution/Confirmation, HR Review, Notification, Task/Action, Approval, Security, User/Employee/Department/Position, Reports, Payroll, Public Information, dialog, button, tab, table header, table data label, label, placeholder, tooltip, page title, aria-label, validation, Snackbar/Alert, loading/empty/error/success, confirmation, file/evidence, import/export/print.

Không được coi việc dịch Menu là hoàn thành localization.

## 4. Quy tắc UI

Không viết text song ngữ trực tiếp. Dùng `@Language.T("...")` hoặc `Language.T("...", parameters)`.

Một label chỉ hiển thị một ngôn ngữ tại một thời điểm.

## 5. Business code không localization

Không dịch EmployeeCode, DepartmentCode, PositionCode, RequestId, FunctionCode, ModuleCode, enum/code, database status/code, SQL identifier, URL/route và audit values.

## 6. Vocabulary

`LanguageCatalog` và các catalog module bổ sung là nguồn vocabulary UI. Không tự dịch lại một thuật ngữ đã có key chuẩn.

Nhóm key chính:

```text
common.* nav.* login.* validation.* message.* status.*
leave.* overtime.* trip.* equipment.* attendance.* execution.*
approval.* security.* hrm.* notification.* task.* payroll.* report.* publicInfo.*
```

## 7. Thông báo và lỗi

UI ưu tiên `Code` từ API nếu có, sau đó tra catalog. Nếu chưa có key thì hiển thị `Message` tiếng Việt làm fallback. Không làm mất thông báo chỉ vì thiếu bản dịch.

## 8. Template có tham số

Không ghép câu tiếng Việt rồi dịch cả câu. Dùng template có placeholder và giữ nguyên giá trị tham số.

## 9. Notification / Task / Action

Dữ liệu nghiệp vụ lưu code + tham số. UI dịch khi render; không lưu câu tiếng Nhật vào dữ liệu nghiệp vụ.

## 10. Trạng thái

Database/API giữ code như `Pending`, `Approved`, `Rejected`, `Resolved`, `Mismatch`, `AwaitingConfirmation`, `Cancelled`, `Expired`; UI dịch theo catalog.

## 11. LanguageService

Web dùng `ILanguageService`, `LanguageService`, `LanguageCatalog`, `LanguageCode`, `LanguageSelector`. Lựa chọn lưu tại `fvn.ui.language`, cập nhật `<html lang>`, mặc định `vi-VN`.

## 12. Source inventory bắt buộc

Rà định kỳ:

```text
FVN_REGISTER/FVN_REGISTER.Shared/Pages
FVN_REGISTER/FVN_REGISTER.Shared/Components
FVN_REGISTER/FVN_REGISTER.Shared/Dialogs
FVN_REGISTER/FVN_REGISTER.Shared/Layout
```

và `.cs` thuộc Shared tạo text UI, Snackbar, Dialog, Notification hoặc validation.

## 13. Automated UI audit

`scripts/audit-ui-localization.ps1` rà literal trong Razor/C# Presentation. Audit bao phủ PageTitle, Button, Text, Alert, table header/cell/DataLabel, Tab, Tooltip, Select/TextField/NumericField/DatePicker/TimePicker/CheckBox/Switch/Radio labels, Placeholder, Title, aria-label, alt, Snackbar, Dialog `ShowAsync` và message-box literals.

Audit không tự động thay chuỗi vì phải phân biệt UI text với business data/code. Mọi candidate phải được rà và hoặc chuyển thành key, hoặc ghi nhận rõ là business/display data được phép giữ nguyên.

Chạy:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\audit-ui-localization.ps1
```

Chế độ bắt buộc:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\audit-ui-localization.ps1 -Strict
```

`-Strict` trả exit code `2` khi còn candidate cần rà.

## 14. Definition of Done

### Nền tảng

- [x] `LanguageCode` VI/JA.
- [x] `ILanguageService`/`LanguageService`.
- [x] Catalog tập trung + catalog module.
- [x] Chọn ngôn ngữ tại Login.
- [x] Lưu lựa chọn ở browser.
- [x] Đổi ngôn ngữ sau Login.
- [x] Navigation dùng vocabulary chuẩn.
- [x] Home, Equipment, LeaveCreate, NotificationBell, NotificationBadge, ChangePassword, HRM Review, Session Management đã được chuyển sang key trong các commit hiện tại.

### Toàn bộ UI — chưa được đánh dấu hoàn thành cho đến khi audit sạch

- [ ] Không còn UI literal Việt/Anh chưa được đánh giá.
- [ ] Tất cả button/action được chuyển key.
- [ ] Tất cả menu/tab/title/table header/data label được chuyển key.
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
- [ ] Thiếu key fallback an toàn về tiếng Việt.
- [ ] Audit chạy Strict không còn candidate chưa được xử lý.
- [ ] Build và test UI cho cả `vi-VN` và `ja-JP`.

## 15. Không thay đổi nghiệp vụ

Localization chỉ thuộc Presentation/Web UI. Không thay đổi approval engine, calculation engine, attendance, OT/Leave/Trip rules, execution reconciliation, payroll gate, security capability, SQL model hoặc API contract hiện hữu.

## 16. Trạng thái triển khai hiện tại

Đã triển khai nền + nhiều màn hình thực tế trên branch `feature/i18n-vi-ja`. Audit script đã được mở rộng để không bỏ sót các loại component và dialog phổ biến.

Các module còn lại **không được coi là hoàn thành** cho đến khi source audit xử lý hết candidate và `-Strict` sạch.
