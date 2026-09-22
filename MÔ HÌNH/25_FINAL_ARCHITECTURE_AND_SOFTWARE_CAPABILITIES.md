# 25 — KIẾN TRÚC CUỐI CÙNG & TOÀN BỘ KHẢ NĂNG PHẦN MỀM

> Source-of-truth tổng hợp sau các thay đổi của Shared Work Calendar + Execution Reconciliation + Action + OT actual-only.
> Branch: feature/work-calendar-action-implementation
> HEAD: 7fd45defd32fd19d676e56c4b59cc201e7c9b3ab

## 1. Bức tranh tổng thể

FVN_REGISTER là HRM Workspace có một trục thời gian chung. HRM là nguồn dữ liệu nhân sự/chấm công chính thức; FVN_REGISTER quản lý Leave / OT / Trip và approval; hệ thống tự đối chiếu Planned/Approved với Actual; khi lệch thì tạo Execution Action; Calendar là projection trung tâm; Action/Notification là lớp việc cần xử lý; Confirmation/Evidence/HR Resolution là workflow xử lý sai lệch; Dashboard/Reports là aggregation.

Luồng tổng quát:

HRM / Business Sources → Planned / Approved + Actual → Execution Reconciliation → Confirmation / Evidence / HR Resolution → Calendar / Action / Notification → Dashboard / Reports → Correction / Payroll Gate

Nguyên tắc: Business data ≠ Reconciliation ≠ Action ≠ Notification ≠ Calendar ≠ Dashboard.

## 2. Ba câu hỏi mà UI phải trả lời

### Calendar — Chuyện gì xảy ra vào ngày nào?
Calendar là màn hình thời gian trung tâm, hợp nhất attendance, Leave, OT, Trip, holiday, workday và execution exception.

### Dashboard / Overview — Tình hình của tôi thế nào?
Tổng hợp công, OT, leave entitlement/đã dùng/còn lại, trip, approval, execution mismatch và việc cần xử lý.

### Registration — Tôi muốn đăng ký gì?
Leave, OT và Trip giữ business workflow riêng: đăng ký, validation, approval, danh sách. Registration không sử dụng Execution Action làm command.

## 3. Navigation cuối cùng

WORKSPACE
- Trang chủ / Dashboard
- Lịch làm việc
- Công / Attendance
- Nghỉ phép: Tổng quan / Đăng ký / Danh sách
- OT: Tổng quan / Đăng ký / Danh sách
- Công tác: Tổng quan / Đăng ký / Danh sách
- Việc cần tôi xử lý: Execution / Action

Chỉ có một Calendar trung tâm. LeaveCreate, OTCreate và TripCreate không nhúng Calendar riêng; các trang này có nút Mở lịch chung → /calendar.

Calendar có thể mở registration opportunity và truyền ngày:
/calendar → /leave/create?date=yyyy-MM-dd
/calendar → /ot/create?date=yyyy-MM-dd
/calendar → /trip/create?date=yyyy-MM-dd

## 4. Central Work Calendar

Calendar là projection/navigation, không phải business source-of-truth.

Một ngày có thể có:
- Ca làm việc
- Attendance
- Leave
- OT
- Trip
- Company holiday / workday
- Execution mismatch
- Action cần xử lý

Interaction chuẩn:
| Interaction | Ý nghĩa |
|---|---|
| DETAIL | Dữ liệu nghiệp vụ → mở chi tiết |
| CONFIRMATION | Có mismatch/action → người dùng xử lý |
| OPEN_REGISTRATION | Ngày trống/được phép → mở đăng ký |
| INFO | Chỉ xem thông tin |

## 5. Attendance

Attendance chính thức đi theo luồng:
HRM → Attendance calculation → F03HrmAttendanceCalculated → Calendar / Dashboard / Reports.

FVN_REGISTER không tạo calculation engine thứ hai. Attendance calculation/backfill phải chạy tự động theo lịch để người dùng không phải tự bấm tính công trước khi mở Calendar.

Calendar đọc official result theo employee + WorkDate. Attendance provider không tạo reconciliation cho từng ngày attendance thông thường và không ghi CalendarProjection cho calculation result.

## 6. Leave

Leave workflow:
Leave Registration → Approval → F03LeaveDayDetails → Attendance → Reconciliation.

Counted leave dựa trên IsCountedAsLeave, IsHalfDay, DayValue và LeaveTypeCode. Vì vậy thứ 7, chủ nhật, ngày lễ, half-day hoặc ngày không counted không tự động trở thành ngày phép.

## 7. OT

OT có hai trường hợp cần phân biệt.

### 7.1 OT có đăng ký
OT Registration → Approval → Approved OT → Actual Attendance → Reconciliation.

### 7.2 OT thực tế nhưng không có đơn
Official attendance calculation cung cấp OTMinutesDay, OTMinutesNight, OTMinutesDayTC, OTMinutesNightTC, OTRecognizedMinutesDay và OTRecognizedMinutesNight.

Rule:
Actual OT > 0 AND không có Approved OT cùng EmployeeCode + WorkDate
→ F03ExecutionReconciliations với ModuleCode=OT, SourceType=OT_ACTUAL_ONLY, ReconciliationStatus=Mismatch, RequiresConfirmation=true.

Calendar projection của case này:
- Marker = ?
- Severity = 3
- RequiresAction = true
- ActionId trỏ tới shared Execution Action

Dấu ? nghĩa là: hệ thống đã phát hiện OT thực tế nhưng chưa tìm thấy đơn OT được duyệt tương ứng; cần xác nhận/đối soát. Nó không có nghĩa là người dùng đang đứng trước một registration opportunity.

## 8. State machine OT_ACTUAL_ONLY

Trạng thái phát hiện:
Actual OT > 0 + No Approved OT → OT_ACTUAL_ONLY / Mismatch → Action → Calendar ? → Employee confirmation.

Khi Approved OT xuất hiện:
OT_ACTUAL_ONLY / Mismatch → Approved OT exists → Auto Resolve → Resolved → dọn projection stale.

Khi attendance được recalculation và OT biến mất:
OT_ACTUAL_ONLY / Mismatch → Actual OT = 0 → Auto Resolve → dọn projection stale.

## 9. Phân biệt Resolved và Cancellation

Đây là thay đổi kiến trúc quan trọng vừa được hoàn thiện.

Resolved có hai semantics:

1. IsCancellation = true → LastModifiedSource = EXECUTION_CANCELLED → hủy projection/action tương ứng.
2. IsCancellation = false → auto-resolved reconciliation → không ghi EXECUTION_CANCELLED, giữ semantics đối soát.

ExecutionReconciliationUpsertRequest hiện có bool IsCancellation = true. Các auto-resolve của OT_ACTUAL_ONLY truyền false.

Do đó hệ thống không còn biến trường hợp 'OT đã được đối soát' thành text 'Đã hủy OT.'.

## 10. Execution Reconciliation

Reconciliation là tầng chung cho các module có Planned/Approved và Actual.

OT / Leave / Trip / future modules → Execution Reconciliation → Calendar / Action / Notification.

Logical identity:
ModuleCode + SourceType + SourceId + ParticipantId + EmployeeId + WorkDate.

Mục tiêu:
- Không duplicate.
- Worker rerun idempotent.
- Một mismatch có một Action.
- Calendar và Notification dùng cùng ActionId.

## 11. Planned và Actual

| Thành phần | Source-of-truth |
|---|---|
| Employee | HRM/FVN employee sync |
| Department/Position | HRM |
| Attendance calculation | Official HRM calculation/read model |
| Leave registration | Leave business module |
| OT registration | OT business module |
| Trip registration | Trip business module |
| Approval | Approval engine |
| Reconciliation | Execution layer |
| Confirmation | Execution layer |
| Evidence | Execution layer |
| HR Resolution | Execution layer |
| Correction | Correction pipeline |
| Calendar | Projection |
| Action | Shared work item |
| Notification | Delivery/read state |
| Dashboard | Aggregation |

## 12. Action

Action là việc cần một người xử lý, không phải business result.

Lifecycle chuẩn: Open → InProgress → Completed.

Action.Completed không đồng nghĩa Business Approved/Matched. Employee không thể đóng Action execution chỉ để làm mất dấu ?.

Action phải được deduplicate theo logical reconciliation identity/action type/assigned employee; worker rerun không tạo Action mới.

## 13. Notification

Reconciliation → ActionId → Notification → User → Deep link → Execution detail.

Notification không quyết định business state. Notification failure không rollback business transaction; lỗi phải được log và có thể retry.

## 14. Confirmation

Mismatch mới tạo Confirmation khi policy yêu cầu.

Decision có thể gồm CONFIRMED, REJECTED, NEED_MORE_EVIDENCE. Confirmation là quyết định/xác nhận của người xử lý, không phải HR Resolution.

## 15. Evidence

Nếu policy yêu cầu: Confirmation → Evidence → HR Evidence Review.

Evidence có thể tham chiếu FileId, ReferenceNo, ExternalUrl, Description.

Review status: Pending, Approved, Rejected, NeedMoreEvidence.

Rejected hoặc NeedMoreEvidence không được tự đóng reconciliation/action.

## 16. HR Resolution

HR Review → OK / NG → F03ExecutionResolutions.

Employee Decision và HR Resolution độc lập. Ví dụ employee có thể CONFIRMED nhưng HR vẫn quyết định NG nếu không đủ căn cứ.

## 17. Correction

HR Resolution → Correction policy → F03ExecutionCorrections → business correction/recalculation.

Correction là audit/correction pipeline, không thay thế business module.

## 18. Payroll Gate

Payroll chỉ nên đi tiếp khi không còn unresolved reconciliation, correction pending/failed đã được xử lý và snapshot không stale.

Không thể đóng Action để giả tạo trạng thái hoàn tất rồi bypass Payroll gate.

## 19. Worker tự động làm gì?

1. HRM attendance calculation / backfill.
2. Execution reconciliation.
3. Action creation / dedup.
4. Action lifecycle.
5. Notification.
6. Calendar projection.

Execution worker phải idempotent.

ReconciliationMode:
- 0: không chạy reconciliation.
- 1: xử lý cửa sổ hiện tại.
- 2: xử lý cửa sổ hiện tại và unresolved records ngoài cửa sổ.

Mode 2 ngăn mismatch cũ bị bỏ quên chỉ vì ngày đó đã ra khỏi worker window.

## 20. Leave / OT / Trip comparison

| Module | Planned | Actual | Ví dụ mismatch |
|---|---|---|---|
| Leave | Counted leave day | Attendance | Có attendance trong ngày nghỉ |
| OT | Approved OT | Actual OT | Actual OT thiếu request hoặc lệch plan |
| Trip | Approved Start/End | Actual Start/End/Status | Missing/incomplete/different |
| Attendance | — | F03HrmAttendanceCalculated | Official read-only projection |

## 21. Security

Execution workflow phải kiểm tra cả capability/function và data scope.

Scope gồm Own, Employee, Department, All tùy policy.

Không đủ điều kiện chỉ vì có quyền ExecutionReview; user còn phải có scope tới employee đang review.

## 22. Audit

Các lớp audit quan trọng:
- F03ExecutionReconciliationHistory
- F03ExecutionConfirmations
- F03ExecutionConfirmationEvidence
- F03ExecutionResolutions
- F03ExecutionCorrections
- Action lifecycle
- Notification lifecycle

Do đó có thể truy ngược: hệ thống phát hiện gì → ai xác nhận → evidence nào → HR quyết định gì → correction nào được tạo.

## 23. Một ví dụ hoàn chỉnh

Ngày 22/09, E0001 có ca làm việc và attendance cho thấy 2 giờ OT.

Không có Approved OT.

Hệ thống chạy:
HRM calculation → F03HrmAttendanceCalculated → Execution Worker → OT_ACTUAL_ONLY → Mismatch.

Calendar hiển thị ngày 22/09 với dấu ? đỏ, Severity 3.

Action được tạo cho người có trách nhiệm.

Employee click ? → Execution detail → xem Actual OT 2 giờ → xác nhận.

Nếu policy yêu cầu evidence → bổ sung evidence → HR review.

HR quyết định OK/NG.

Nếu sau đó Approved OT xuất hiện trước khi HR xử lý, worker có thể auto-resolve synthetic mismatch với IsCancellation=false; Calendar không hiển thị 'Đã hủy OT.'.

## 24. Toàn bộ khả năng người dùng có thể hình dung

Nhân viên có thể:
- xem toàn bộ ngày làm việc trên một Calendar;
- xem attendance chính thức;
- đăng ký Leave/OT/Trip;
- xem approval progression;
- nhận biết OT thực tế nhưng thiếu đơn;
- click trực tiếp mismatch để xử lý;
- xác nhận/reject/need more evidence;
- cung cấp evidence;
- theo dõi kết quả HR resolution;
- xem việc cần xử lý;
- xem dashboard tổng hợp công/OT/leave/trip;
- xem các trạng thái cần chú ý mà không cần tự chạy calculation thủ công.

HR/Approver có thể:
- xem execution review queue;
- lọc theo module/status;
- kiểm tra employee + work date + planned + actual;
- review evidence;
- Resolve OK/NG;
- theo dõi history;
- thực hiện correction theo policy;
- bảo đảm payroll không đi qua khi execution còn unresolved.

Hệ thống có thể tự động:
- tính/backfill attendance theo lịch;
- phát hiện mismatch;
- phát hiện OT actual-only;
- dedup reconciliation/action;
- revisit unresolved cũ;
- auto-resolve các case đủ điều kiện;
- cập nhật Calendar projection;
- tạo notification;
- duy trì audit trail.

## 25. Kiến trúc 5 tầng

5. EXPERIENCE / WORKSPACE: Calendar · Dashboard · Lists · Notifications

4. WORK / EXECUTION: Action · Confirmation · Evidence · HR Resolution · Correction

3. RECONCILIATION: Planned ↔ Actual · Policy · State machine

2. BUSINESS MODULES: Leave · OT · Trip · Approval

1. SOURCE / ACTUAL: HRM Employee · Attendance · Company data

Không tầng nào được phép biến projection thành source-of-truth của tầng dưới.

## 26. Khả năng mở rộng module

Module mới chỉ cần cung cấp:
1. ModuleCode.
2. Planned provider.
3. Actual provider.
4. Reconciliation mapping.
5. Execution policy.
6. Confirmation/Evidence policy.
7. Action mapping.
8. Notification mapping.
9. Calendar projection.
10. Correction producer nếu cần.

Nhờ vậy module mới dùng chung Calendar, Action, Notification, Confirmation và HR Resolution thay vì tạo workflow riêng.

## 27. Trạng thái hiện tại

Đã có trong kiến trúc/code của branch:
- Shared Work Calendar và Calendar là entry point trung tâm.
- Leave/OT/Trip Create không nhúng Calendar riêng.
- Registration từ Calendar truyền ngày sang form.
- Planned/Actual reconciliation dùng chung.
- OT actual-only detection.
- OT_ACTUAL_ONLY.
- Marker ? và Severity 3.
- ActionId liên kết reconciliation/projection.
- Revisit unresolved ngoài worker window theo policy.
- Auto-resolve khi Approved OT xuất hiện.
- Auto-resolve khi Actual OT biến mất sau recalculation.
- Phân biệt cancellation và auto-resolution bằng IsCancellation.
- Test state-machine cơ bản cho OT actual-only.
- Confirmation/Evidence/HR Resolution architecture.
- HR scope/security architecture.
- Calendar/Action/Notification separation.

## 28. Production hardening còn nên hoàn thiện

- Integration tests trên DB thật/test container cho toàn bộ lifecycle.
- Test worker idempotent nhiều lần.
- Test dedup Action + Notification.
- Test participant isolation với nhiều employee.
- Test evidence rejection/need-more-evidence giữ Action mở.
- Test HR resolution không ghi đè employee confirmation.
- Test payroll gate end-to-end.
- Test background attendance calculation/backfill company-wide.
- Performance/load test reconciliation toàn công ty.
- Monitoring/alerting cho failed background jobs.
- Retry policy đầy đủ cho integration/notification failures.

## 29. Kết luận

FVN_REGISTER hiện được định hình không còn chỉ là phần mềm đăng ký nghỉ/OT/công tác có thêm Calendar.

Nó là một HRM execution workspace lấy thời gian làm trục, có registration + approval + official attendance + planned-vs-actual reconciliation + human confirmation + evidence + HR resolution + action + notification + calendar projection + correction/payroll gate.

Calendar trả lời: điều gì xảy ra?
Registration trả lời: tôi muốn đăng ký gì?
Reconciliation trả lời: thực tế có khớp kế hoạch không?
Action trả lời: ai cần làm gì?
Confirmation trả lời: người thực hiện xác nhận thế nào?
Evidence trả lời: căn cứ là gì?
HR Resolution trả lời: HR quyết định ra sao?
Correction trả lời: có cần sửa dữ liệu nghiệp vụ không?
Dashboard trả lời: tình hình tổng thể thế nào?

Đây là ranh giới trách nhiệm cuối cùng cần giữ khi tiếp tục mở rộng hệ thống.