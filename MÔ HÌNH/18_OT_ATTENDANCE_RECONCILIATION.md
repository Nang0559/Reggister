# 18 — OT ↔ ATTENDANCE RECONCILIATION & SHARED WORK CALENDAR

## 1. Mục tiêu
OT Approved, attendance actual và OT reconciliation phải có một pipeline thống nhất. Work Calendar chỉ là projection dùng chung; không trở thành nguồn dữ liệu nghiệp vụ thứ hai.

## 2. Source of truth
| Dữ liệu | Source |
|---|---|
| OT request / revision / approval | OT module |
| Attendance actual | HRM Attendance Calculation / F03 read model |
| OT actual | Attendance/OT calculation |
| Reconciliation | OT/Attendance reconciliation service |
| Calendar policy | Shared Calendar |
| Calendar marker/summary | Calendar projection |
| User action | Shared Action |
| Notification | Notification service |
Không copy toàn bộ OT/Attendance entity vào Calendar.

## 3. Reconciliation pipeline
EffectiveApprovedRevision → Attendance Actual → Reconciliation → CalendarItem
Trạng thái chuẩn: None, Approved, Matched, Mismatch, ActualWithoutApproval, ApprovedWithoutActual, CancelledByNoAttendance.

## 4. Attendance confirmation
Khi OT Approved nhưng không có attendance/actual phù hợp: Calendar có thể hiển thị ?; RequiresAction = true; tạo ActionItem với ActionType = OT.ATTENDANCE_CONFIRMATION; Notification và Task List cùng tham chiếu ActionId.
Người dùng xác nhận đã làm hoặc không làm; evidence thuộc confirmation/module workflow. Business transition vẫn do OT/Attendance owner xử lý.

## 5. Payroll period
Chu kỳ nghiệp vụ hiện hành: 21 của tháng hiện tại → 20 của tháng kế tiếp. Worker/reconciliation phải xử lý đầy đủ các ngày đã qua; không phụ thuộc việc user có mở Dashboard hay Calendar hay không.

## 6. Calculation worker
Không tính attendance riêng khi user login. Tính giờ là background/company-wide pipeline. Dashboard/Calendar chỉ đọc kết quả đã tính.
Calculation phải idempotent theo employee/date/batch rule và có khả năng backfill các ngày còn thiếu.

## 7. Shared Calendar projection
Calendar provider của OT trả CalendarItemDto gồm tối thiểu: WorkDate, ModuleCode, StatusCode, Marker, Summary, Severity, RequiresAction, DetailRoute, SourceId.
Calendar không tự quyết định OT Approved/Rejected/Actual; chỉ đọc kết quả từ module.

## 8. Action / Task rule
RequiresAction = false → marker/summary, không tạo task.
RequiresAction = true → tạo hoặc update một ActionItem.
Logical deduplication: ModuleCode + SourceId + ActionType + AssignedToEmployeeId. Worker chạy lặp không được tạo duplicate Open/InProgress action.

## 9. Dashboard / Login
Dashboard phải hiển thị ngay số việc cần làm, danh sách action quan trọng, unread notification count và Calendar summary. User không cần mở Calendar để phát hiện OT thiếu chấm công.

## 10. Invariants
1. Calendar không sở hữu OT business state.
2. Action status không thay thế OT/Attendance status.
3. Notification status không thay thế Action status.
4. Recalculation không tạo duplicate action.
5. Approval revision của một participant không làm thay đổi participant khác.
6. User login không phải trigger duy nhất của attendance calculation.
7. /me API dùng authenticated identity, không nhận arbitrary EmployeeId/UserId.

## 11. Implementation reference
Chi tiết implementation chung nằm tại 24_WORK_CALENDAR_AND_ACTION_IMPLEMENTATION.md.