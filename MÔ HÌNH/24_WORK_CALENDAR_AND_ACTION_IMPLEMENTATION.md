# 24 — SHARED WORK CALENDAR + ACTION + EXECUTION RECONCILIATION

> Source-of-truth cho Calendar, Action, Notification và generic Planned-vs-Actual reconciliation.  
> Business Module vẫn là source-of-truth cho dữ liệu nghiệp vụ.

## 1. Kiến trúc runtime

`Business Module → Planned/Approved + Actual → Execution Reconciliation → Employee Confirmation/Evidence → HR Review → Resolution/Correction → Calendar/Action/Notification → Payroll/Reports`

- **Reconciliation** là trạng thái đối soát.
- **Confirmation** là quyết định của employee/assigned actor.
- **Evidence** là chứng từ, nằm ngoài Action payload.
- **HR Resolution** là quyết định xử lý của HR, lưu tại `F03ExecutionResolutions`.
- **Correction** là record audit/correction pipeline, lưu tại `F03ExecutionCorrections`.
- **Calendar/Action/Notification** chỉ là projection/orchestration, không thay thế business state.

## 2. Core entities

| Entity | Vai trò |
|---|---|
| `F03ExecutionPolicies` | Policy theo `ModuleCode`: ReconciliationMode, ConfirmationMode, EvidenceMode, ReviewMode, DueHours, AutoResolveMode |
| `F03ExecutionReconciliations` | Planned/Actual + lifecycle status + ActionId |
| `F03ExecutionConfirmations` | Decision/comment của employee và review state |
| `F03ExecutionConfirmationEvidence` | Evidence + HR evidence review |
| `F03ExecutionReconciliationHistory` | Audit lifecycle |
| `F03ExecutionResolutions` | Quyết định HR: OK/NG + Reason + CalendarAction |
| `F03ExecutionCorrections` | Correction/audit record sau HR resolution |

Logical identity của reconciliation:

`ModuleCode + SourceType + SourceId + ParticipantId + EmployeeId + WorkDate`

Database đã có unique index trên identity này để chống duplicate.

## 3. Calendar

Calendar là projection/navigation.

- `CalendarProjection.ActionId` phải trỏ tới cùng Action của reconciliation.
- HR quyết định trên Calendar được đánh dấu bằng `LastModifiedSource = HR_EXECUTION_REVIEW`.
- Worker không được ghi đè projection đã được HR resolution.
- Calendar không chứa approval history, evidence history hoặc workflow dài.

## 4. Action

Action là shared work item, không phải business result.

Lifecycle hợp lệ:

`Open → InProgress → Completed`

Action của execution:

- được tạo một lần theo logical identity;
- `DueAt` chỉ được tính khi Action mới được tạo;
- rerun reconciliation không được gia hạn `DueAt`;
- reconciliation chưa `Resolved` thì Action không được expire;
- Action cũ từng `Expired` nhưng reconciliation vẫn unresolved được repair về `InProgress`;
- employee không thể đóng Action execution để bypass reconciliation.
- Action overdue chỉ chuyển `Open → InProgress` một lần; không ghi `ModifiedAt` mỗi chu kỳ.

**Action.Completed ≠ business Approved/Matched.**

## 5. Notification

Notification là delivery/read state.

- Notification dùng `ActionId` để deep-link về work item.
- Notification failure không rollback business transaction.
- Evidence mới tạo notification cho HR có ReviewMode bật và không gửi cho chính employee.
- Failure phải được log để quan sát/retry; không dùng `catch { }` im lặng.
- Evidence mới tạo notification cho HR có scope phù hợp.
- Notification không được quyết định business state.

## 6. Policy

Policy là bắt buộc khi execution module được bật.

### ReconciliationMode

- `0`: module không chạy reconciliation worker.
- `1`: xử lý cửa sổ thời gian hiện tại.
- `2`: xử lý cửa sổ hiện tại **và** các reconciliation unresolved nằm ngoài cửa sổ.

Do đó một mismatch cũ không bị mất chỉ vì đã rời khỏi cửa sổ `today-2..today`.

### ConfirmationMode

Quyết định reconciliation có tạo employee confirmation/action hay không.

### EvidenceMode

Quyết định confirmation có yêu cầu evidence hay không.

### ReviewMode

- `0`: không đưa module vào HR Review queue.
- `!= 0`: Mismatch/AwaitingConfirmation được đưa vào HR Review và Resolve phải đi qua policy này.
- Queue HR áp dụng đúng capability + data scope: Own / Employee / Department / All.

### AutoResolveMode

Chỉ được auto-resolve khi policy tồn tại và bật AutoResolve.  
**Không có policy ⇒ không auto-resolve.**

## 7. Module mapping hiện tại

| Module | Planned | Actual | Mismatch |
|---|---|---|---|
| OT | Approved OT / planned hours | ActualHours + ActualStart/End | `NO_ACTUAL`, chênh lệch hours |
| Leave | `F03LeaveDayDetails` với `IsCountedAsLeave=true` | Attendance | Full-day leave nhưng có attendance |
| Trip | Approved Start/End | `F03TripActual` | Missing, incomplete hoặc ActualStart/End khác planned |
| Attendance | — | `F03HrmAttendanceCalculated` | Calendar hiển thị official attendance và các exception; reconciliation không tạo calculation engine thứ hai |
| Future | Module provider định nghĩa | Module provider định nghĩa | Module provider định nghĩa |

### Attendance

Attendance trong Calendar là read-only projection từ official HRM calculation result `F03HrmAttendanceCalculated`. Provider `ATTENDANCE` đọc theo employee + WorkDate; không ghi `F03CalendarProjection` và không chạy calculation riêng. `AttendanceViewOwn` (2912, scope `Own`, cấp cho mọi role nghiệp vụ 1–5) là capability để endpoint `/api/calendar/me` trả module này; `AttendanceView` (2901) là quyền báo cáo scope `Department`, không cấp cho role User và chỉ được chấp nhận thêm như quyền tương thích.

### Leave

Worker **không tự sinh toàn bộ ngày StartDate → EndDate**.

Nguồn counted leave là `F03LeaveDayDetails`:

- `IsCountedAsLeave`
- `IsHalfDay`
- `DayValue`
- `LeaveTypeCode`

Vì vậy thứ 7, chủ nhật, ngày lễ và ngày không counted không tự động trở thành Leave reconciliation.

### Trip

Worker so sánh:

- Planned Start/End
- Actual Start/End
- Actual Status

Actual Completed nhưng ngày khác planned ⇒ `Mismatch`.

## 8. Confirmation / Evidence / HR Resolution

Flow:

`Mismatch → Action Open/InProgress → Employee Confirmation → Evidence (nếu policy yêu cầu) → HR Evidence Review → HR Resolution → Resolved`

Evidence:

- `Approved`: có thể tiếp tục Resolve OK.
- `Rejected` hoặc `NeedMoreEvidence`: giữ confirmation/reconciliation mở.
- HR review evidence phải kiểm tra capability **và data scope của employee đích**.

HR không ghi đè `F03ExecutionConfirmation.Decision`.

Employee decision:

- `CONFIRMED`
- `REJECTED`
- `NEED_MORE_EVIDENCE`

HR decision:

- `OK`
- `NG`

Hai khái niệm này được lưu độc lập.

## 9. Correction

Correction được quyết định bởi `ExecutionPolicy.CorrectionMode`, không hard-code theo ModuleCode.

`F03ExecutionCorrections` không còn bị hard-code chỉ cho `ATTENDANCE`.

- Mọi HR Resolution OK có thể tạo correction/audit record theo ModuleCode.
- `None`: không tạo correction.
- `AttendanceRecalculate`: chạy attendance recalculation + payroll-period gate.
- OT/Leave/Trip mặc định không bị chặn bởi payroll period nếu policy không bật correction.
- Correction có `Pending/Applied/Failed/Cancelled`.

Không được coi việc tạo correction là thay thế business module update.

## 10. Participant isolation

Mọi query/reconciliation/action phải giữ:

- EmployeeId
- ParticipantId khi module có participant
- SourceType
- SourceId

OT hiện tại dùng `OTRequestId + EmployeeCode` làm SourceId logic.  
**OTMaster + EffectiveRevision chưa tồn tại trong code hiện tại**; không được mô tả đây là capability đã triển khai cho tới khi có entity/schema/provider tương ứng.

## 11. ActualState và mismatch type

`ActualState` hiện là state string phục vụ reconciliation, ví dụ:

- `NO_ACTUAL`
- `WORKED`
- `CANCELLED`
- `Actual=yyyy-MM-dd..yyyy-MM-dd;Status=Completed`

Các tên như `ApprovedWithoutActual` hoặc `Different` là **business interpretation**, không phải enum/column riêng trong schema hiện tại.

## 12. Background jobs

1. HRM attendance calculation/backfill.
2. Execution reconciliation.
3. Action creation/dedup.
4. Action lifecycle.
5. Notification delivery.
6. Calendar projection.

Các worker phải idempotent.

Execution worker hiện:

- xử lý cửa sổ gần hiện tại;
- đồng thời quét unresolved ngoài cửa sổ khi `ReconciliationMode=2`;
- xử lý approved/cancelled source; cancelled chỉ đóng reconciliation đã tồn tại, không tạo row Resolved mới.
- không bỏ lại reconciliation chỉ vì actual về muộn.

## 13. HR UI

Đã có:

- `/execution/hr`
- filter Module/Status;
- Resolve OK/NG;
- xem detail;
- review Evidence Approved/Rejected/NeedMoreEvidence;
- deep-link từ notification qua `?reconciliationId=`.

Employee `/execution` hỗ trợ:

- deep-link `?reconciliationId=`;
- xem Confirmation;
- xem Evidence + FileId/Reference;
- xem HR Resolution;
- bổ sung evidence.

## 14. Dashboard

Dashboard tổng sử dụng shared Action service:

- ActionCount;
- Open/InProgress Actions;
- ActionId được giữ trong Action/Calendar/Notification.

Module Dashboard Provider không được coi Action là business source-of-truth.

## 15. Payroll gate

Payroll chỉ được chuẩn bị/khóa/xuất khi:

- không còn reconciliation unresolved;
- correction pending/failed đã được xử lý;
- snapshot không stale so với execution reconciliation.

Do đó Action không được đóng để bypass Payroll gate.

## 16. SQL deployment

Deployment hiện dùng thứ tự duy nhất:

- `01..30`: foundation/features;
- `31..35`: hardening/post-foundation;
- `99`: final verification.

Không còn duplicate numeric prefixes trong SQL deployment chain.

## 17. Definition of Done

- [x] OT/Leave/Trip dùng shared reconciliation contract.
- [x] Reconciliation identity có unique database constraint.
- [x] Rerun không gia hạn DueAt.
- [x] Unresolved ngoài worker window không bị bỏ rơi khi policy mode = 2.
- [x] Leave dùng F03LeaveDayDetails.
- [x] Trip phát hiện Actual khác Planned.
- [x] HR scope kiểm theo employee đích.
- [x] Employee Decision không bị HR Resolution ghi đè.
- [x] Auto-resolve yêu cầu policy.
- [x] Action không expire unresolved.
- [x] HR UI + employee detail deep-link.
- [x] Evidence review giữ workflow mở khi cần.
- [x] Notification failure được log.
- [x] SQL deployment numbering được chuẩn hóa.
- [ ] Integration tests phải chứng minh idempotent rerun, dedup, participant isolation và evidence-review giữ Action mở trong môi trường DB test.

## 18. Nguyên tắc mở rộng module mới

Module mới **không sửa switch trung tâm để giả mạo Leave/OT/Trip**.

Cần cung cấp:

1. ModuleCode.
2. ExecutionPolicy.
3. Planned provider.
4. Actual provider.
5. Reconciliation mapping.
6. Confirmation/Evidence policy.
7. Action policy.
8. Notification mapping.
9. Calendar projection provider.
10. Correction producer nếu module cần cập nhật business source.



### Calendar interaction model

Calendar distinguishes four interactions: CONFIRMATION for reconciliation work items with ActionId; DETAIL for existing business data with a DetailRoute; OPEN_REGISTRATION for empty dates where registration is allowed; and INFO for read-only information. Registration opportunities are exposed separately from existing calendar items and are resolved from the existing WorkCalendar availability rules. Existing data, including Attendance mismatch, takes precedence over creating a new registration on the same date. Blank dates on the standalone Calendar can open a registration chooser for Leave/OT/Trip, and the selected date is carried into the corresponding create page.
