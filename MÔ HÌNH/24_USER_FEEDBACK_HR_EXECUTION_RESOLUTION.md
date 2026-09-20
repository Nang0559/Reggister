# 24. USER FEEDBACK ↔ HR EXECUTION RESOLUTION

## 24.1 Mục tiêu

Phản hồi của người dùng về công, OT, nghỉ phép, công tác và các execution khác phải được HR tiếp nhận, phân loại, kiểm tra, quyết định OK/NG và trả kết quả ngược lại cho người dùng.

## 24.2 Luồng hai chiều

```text
USER
  │ phản hồi
  ▼
F03ExecutionReconciliation
  │
  ├── Attendance
  ├── OT
  ├── Leave
  ├── Trip
  └── Future module
  │
  ▼
HR REVIEW QUEUE
  │
 ┌┴────────┐
 ▼         ▼
OK         NG
 │         │
 │         ├── bắt buộc Reason
 │         └── giữ dữ liệu nghiệp vụ
 │
 ├── Confirmation = Approved / USER_CORRECT
 ├── Resolution = OK
 ├── cập nhật Calendar Projection theo CalendarAction
 ├── đóng Action
 └── Notification → User
           │
           └── Resolution = NG + Reason → User
```

## 24.3 Quyết định HR

Reconciliation chỉ được HR giải quyết khi đang Mismatch hoặc AwaitingConfirmation.

- OK: xác nhận thông tin phản hồi của User là đúng.
- NG: không xác nhận phản hồi; bắt buộc có lý do.
- Resolution là trạng thái quyết định của HR; Action chỉ là trạng thái công việc.

## 24.4 API

'GET /api/execution/hr/reconciliations'

Filter: moduleCode, status, from, to. Chỉ trả reconciliation chưa Resolved.

'POST /api/execution/hr/reconciliations/{id}/resolve'

```json
{
  "decision": "OK",
  "reason": "Đã kiểm tra dữ liệu HRM và xác nhận nhân viên có đi làm.",
  "calendarAction": "REFRESH"
}
```

decision chỉ có OK hoặc NG. reason bắt buộc.

calendarAction:
- KEEP: giữ projection hiện tại và đóng yêu cầu xử lý.
- REFRESH: cập nhật projection thành trạng thái đã xác nhận.
- CANCEL: hủy projection/lịch tương ứng.

## 24.5 Khi HR chọn OK

1. Confirmation.Status = Approved.
2. Confirmation.Decision = USER_CORRECT.
3. Tạo đúng một F03ExecutionResolution.
4. Reconciliation.Status = Resolved.
5. Ghi ResolvedAt/ResolvedBy.
6. Đóng Action liên quan.
7. Áp dụng CalendarAction.
8. Ghi reconciliation history.
9. Tạo notification cho User.

## 24.6 Khi HR chọn NG

1. Confirmation.Status = Rejected.
2. Confirmation.Decision = USER_NOT_CORRECT.
3. Bắt buộc lưu Reason.
4. Không tự động thay đổi dữ liệu nguồn nghiệp vụ.
5. Đóng reconciliation sau khi quyết định đã được audit.
6. Đóng action.
7. Thông báo User kèm lý do NG.

## 24.7 Calendar

Calendar là projection, không phải source-of-truth.

```text
Resolution
   │
   └── CalendarAction
          ├── KEEP
          ├── REFRESH
          └── CANCEL
                  │
                  ▼
          F03CalendarProjection
```

Module nguồn vẫn chịu trách nhiệm tạo projection chính xác ở lần tính tiếp theo.

## 24.8 Authorization

Quyền HR review dùng FunctionCode 2107 - Execution Review.
Không dựa vào EmployeeId của người đăng nhập hoặc dữ liệu do client tự gửi.

## 24.9 Idempotence / concurrency

Mỗi reconciliation chỉ có một resolution nhờ unique constraint UX_F03ExecutionResolutions_Reconciliation. Resolution chạy trong transaction Serializable. Request thứ hai sau khi request đầu đã Resolved sẽ bị từ chối.

## 24.10 Audit

Resolution lưu ReconciliationId, Decision, Reason, CalendarAction, ResolvedByUserId, ResolvedByEmployeeId, ResolvedAt và AppliedChangeJson. Mọi chuyển trạng thái cũng ghi F03ExecutionReconciliationHistory.

## 24.11 Phân tách trách nhiệm

```text
Business module → source-of-truth
Execution Reconciliation → mismatch / confirmation / HR resolution
Calendar → projection
Action → work/inbox state
Notification → user communication
```

Không dùng Action hoặc Notification làm nguồn dữ liệu nghiệp vụ.

## 24.12 Payroll / Kế toán

HR Resolution không phải là dữ liệu đầu vào trực tiếp của kế toán.

Luồng chính thức:

    F03HrmAttendanceCalculated ─┐
                                ├──> F03PayrollInputs ──> Kế toán
    F03HrmOTActual ─────────────┘

Kỳ lương dùng chu kỳ **21 tháng hiện tại → 20 tháng kế tiếp** và được quản lý bằng `F03PayrollCalculationPeriods`.

Trạng thái: `Open` → `Calculated` → `Locked` → `Exported`.

Khi HR chọn `OK` cho Attendance:
1. Kiểm tra ngày phản hồi có thuộc kỳ lương đã `Locked/Exported` hay không.
2. Nếu kỳ còn mở, tạo `F03ExecutionCorrection` với `Pending`.
3. Gọi lại cùng `dbo.usp_CalculateHrmAttendance`; không UPDATE trực tiếp `F03HrmAttendanceCalculated`.
4. Calculation cập nhật `F03HrmAttendanceCalculated` và `F03HrmOTActual`.
5. Correction chuyển `Applied`.
6. Payroll Input được tạo bằng `dbo.usp_PreparePayrollPeriod`.
7. Kế toán đọc `F03PayrollInputs`, không đọc reconciliation/action/notification.

Nếu kỳ đã khóa/xuất, correction bị chặn và phải đi qua Payroll Adjustment/Reopen.

API:
- `POST /api/payroll/periods/{periodId}/prepare`
- `POST /api/payroll/periods/{periodId}/lock`

## 24.13 Definition of Done

- [x] HR queue dùng chung cho OT / Leave / Trip / future execution.
- [x] Filter theo Module / Status / Date.
- [x] Function 2107 bảo vệ HR review.
- [x] OK/NG rõ ràng; Reason bắt buộc.
- [x] OK approve user confirmation; NG reject user confirmation.
- [x] CalendarAction KEEP / REFRESH / CANCEL.
- [x] Action đóng sau khi reconciliation Resolved.
- [x] User nhận notification kết quả.
- [x] Unique resolution theo reconciliation.
- [x] History và transaction Serializable.
- [x] Attendance OK tạo ExecutionCorrection và chạy lại HRM-compatible calculation.
- [x] Không UPDATE trực tiếp bảng kết quả tính công.
- [x] Payroll Period 21→20 được quản lý và có trạng thái Lock.
- [x] Payroll Input snapshot lấy từ F03HrmAttendanceCalculated + F03HrmOTActual.
- [x] Có chặn HR correction đối với kỳ lương Locked/Exported.
- [x] Có API prepare/lock Payroll Input.
- [ ] Payroll Adjustment/Reopen cho kỳ đã Locked/Exported.
- [ ] Module-specific source correction handlers cho OT / Leave / Trip phải được nối vào resolution engine trước khi cho phép mutation dữ liệu nguồn.