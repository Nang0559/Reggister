# 18 — EXECUTION RECONCILIATION: PLANNED → ACTUAL → CONFIRMATION → EVIDENCE

## 1. Phạm vi

Đây không phải workflow riêng của OT. Đây là rule dùng chung cho mọi module có Planned/Approved và Actual có thể đối chiếu: OT, Leave, Trip và module tương lai.

## 2. Rule chuẩn

Planned/Approved → Actual → Reconciliation → Confirmation → Evidence → Review → Resolved.

Không phải mọi case đều đi qua toàn bộ bước. Planned và Actual khớp thì có thể tự Resolved. Lệch thì mới yêu cầu Confirmation. Evidence/Review do policy module quyết định.

Ví dụ:
- OT: Approved OT nhưng thiếu attendance → Confirmation; xác nhận đã làm → Evidence/Review nếu policy yêu cầu.
- Leave: Approved Leave nhưng attendance cho thấy đi làm → PlannedButActualDifferent → Confirmation.
- Trip: Approved Trip nhưng actual không có/khác → Confirmation.

## 3. Source of truth

| Dữ liệu | Owner |
|---|---|
| Planned / Approved | Business module |
| Actual execution | Business module hoặc official HRM/read model |
| Reconciliation | Shared execution reconciliation |
| Confirmation | Shared execution confirmation |
| Evidence | Shared evidence capability |
| Approval | Approval Engine / business module |
| Calendar | Projection/navigation |
| Action | Shared work item |
| Notification | Delivery/read state |
| Dashboard | Aggregation |

Calendar, Action và Notification không thay thế business state.

## 4. Generic Reconciliation

Entity: F03ExecutionReconciliations.

Khóa logic: ModuleCode + SourceId + EmployeeId + WorkDate.

Trạng thái chuẩn: None, Planned, Matched, PlannedWithoutActual, ActualWithoutPlanned, PlannedButActualDifferent, ConfirmationRequired, ConfirmationSubmitted, EvidencePendingReview, Resolved, Rejected, Cancelled.

Mỗi employee/participant có reconciliation độc lập; không dùng master ID làm khóa duy nhất.

## 5. Confirmation

Entity: F03ExecutionConfirmations.

Một reconciliation có tối đa một confirmation active/current. Confirmation ghi Employee, WorkDate, Decision, Comment, EvidenceRequired, Status, SubmittedAt và Review.

Decision có thể là Worked, NotWorked, Executed, NotExecuted, Confirmed, Rejected; module có thể định nghĩa decision riêng.

## 6. Evidence

Evidence không lưu trong ActionItem.PayloadJson.

Entity: F03ExecutionConfirmationEvidence.

Có thể tham chiếu FileId, ReferenceNo, ExternalUrl, Description. ReviewStatus gồm Pending, Approved, Rejected, NeedMoreEvidence.

Evidence bị Reject/NeedMoreEvidence thì Reconciliation chưa Resolved và Action không được tự động Completed.

## 7. Execution Policy

Entity: F03ExecutionPolicies.

Policy quyết định ReconciliationMode, ConfirmationMode, EvidenceMode, ReviewMode, DueHours và AutoResolveMode.

Không hard-code Evidence theo ModuleCode.

## 8. Action / Calendar integration

RequiresConfirmation = true → tạo/cập nhật một Action; Calendar, Task, Notification và Dashboard cùng tham chiếu ActionId.

Logical dedup: ModuleCode + Reconciliation/SourceId + ActionType + AssignedToEmployeeId.

Action chỉ biểu diễn việc cần làm, không quyết định business result.

## 9. Participant isolation

OT: OT Master → Participant → Approved Revision → Actual → Reconciliation. A thay đổi không làm B/C thay đổi. Leave/Trip nhiều participant cũng áp dụng nguyên tắc này.

## 10. Attendance

Không tạo calculation engine thứ hai. Attendance company-wide được worker/scheduler tính và backfill; Calendar/Dashboard chỉ đọc official read model. Payroll period hiện hành: ngày 21 → ngày 20.

Calendar module `ATTENDANCE` đọc trực tiếp `F03HrmAttendanceCalculated` theo employee + WorkDate. Module này không ghi `F03CalendarProjection`, không tạo reconciliation cho từng ngày attendance thường và không thay đổi calculation result. Quyền truy cập endpoint Calendar được kiểm tra bằng `SecurityFunctionCodes.AttendanceView`; `F03CalendarModulePolicies` phải có row `ATTENDANCE` enabled.

## 11. Invariants

1. Business module là source of truth.
2. Planned/Actual đối chiếu ở cấp employee + work date + source.
3. Mismatch mới tạo Confirmation.
4. Confirmation không đồng nghĩa Resolved.
5. Evidence không nằm trong ActionItem.PayloadJson.
6. Evidence bị Reject/NeedMoreEvidence không đóng Action.
7. Action Completed không đồng nghĩa Approved/Matched.
8. Notification không thay Action.
9. Calendar không thay Reconciliation.
10. Worker rerun idempotent.
11. /me dùng authenticated identity.
12. Module mới chỉ cần provider + policy + mapping Planned/Actual + action types.
