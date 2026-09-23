# 24 — KIẾN TRÚC HỆ THỐNG & KHẢ NĂNG PHẦN MỀM — SINGLE SOURCE OF TRUTH

> Tài liệu kiến trúc duy nhất của FVN_REGISTER. Mọi thay đổi kiến trúc phải được cập nhật trực tiếp tại tài liệu này trước khi triển khai code/SQL/UI.

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

## 4A. Ranh giới Business Module và Workspace

### RequestModule chỉ biểu diễn nghiệp vụ đăng ký

`RequestModule` là định danh của **business request module**, không phải Role, Workspace hay phòng ban.

Danh sách chuẩn:

| RequestModule | Ý nghĩa |
|---|---|
| `Leave` | Đăng ký/nghiệp vụ nghỉ phép |
| `Overtime` | Đăng ký/nghiệp vụ OT |
| `Trip` | Đăng ký/nghiệp vụ công tác |
| `Equipment` | Nghiệp vụ đăng ký/quản lý thiết bị |
| `Attendance` | Nghiệp vụ/read model liên quan công |

Không thêm các giá trị `HR`, `IT`, `Execution`, `Administration`, `Manager` vào `RequestModule`.

### Workspace / capability là lớp khác

- **HR**: Role + capability + scope + HR workspace.
- **IT**: Role + capability + scope + IT workspace.
- **Administration**: capability/administration workspace.
- **Execution**: execution/reconciliation capability và queue dùng chung.
- **Manager**: layout/workspace được kích hoạt bởi `ManagedScope`, không phải Role `Manager`.
- **Approver**: không phải Role nguồn của approval; quyền approve được resolve từ `F03ApprovalPolicies`.

Nguyên tắc:

`Business Module ≠ Role ≠ Capability ≠ ManagedScope ≠ Workspace ≠ ApprovalPolicy`

Không dùng `RequestModule` để mô hình hóa tổ chức hoặc quyền.

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



## 11A. Mapping với schema/runtime hiện hữu — không tạo entity song song

Các thành phần Action/Notification/Execution dưới đây **đã có trong branch** và là phần mở rộng của kiến trúc hiện hữu, không phải các entity mới độc lập phục vụ riêng cho Calendar:

| Concern | Runtime entity / table | Vai trò |
|---|---|---|
| Calendar module | `F03CalendarModuleDefinitions` | Definition/provider capability |
| Calendar policy | `F03CalendarModulePolicies` | Cấu hình hiển thị/reconciliation |
| Calendar projection | `F03CalendarProjection` | Read projection, rebuild được |
| Shared Action | `F03ActionItems` / `F03ActionItem` | Một work item dùng chung Calendar/Task |
| Action policy | `F03ActionPolicies` / `F03ActionPolicy` | Priority, deadline, notification policy |
| Notification | `F03AppNotifications` | **Bảng notification hiện hữu**, được bổ sung `ActionId` + `NotificationType` |
| Execution policy | `F03ExecutionPolicies` | Planned/Actual reconciliation policy |
| Reconciliation | `F03ExecutionReconciliations` | Source-of-truth của mismatch lifecycle |
| Confirmation | `F03ExecutionConfirmations` | Employee confirmation |
| Evidence | `F03ExecutionConfirmationEvidence` | Evidence của confirmation |
| HR Resolution | `F03ExecutionResolutions` | HR quyết định xử lý |

Không tạo `F03Notifications` thứ hai và không tạo một Action entity khác cho Calendar. Calendar chỉ **discover/link/navigate** tới Action; Execution tạo/duy trì Action khi policy yêu cầu.

## 12. Action

Action là việc cần một người xử lý, không phải business result.

Lifecycle chuẩn: Open → InProgress → Completed.

Action.Completed không đồng nghĩa Business Approved/Matched. Employee không thể đóng Action execution chỉ để làm mất dấu ?.

Action phải được deduplicate ở **hai lớp**:
1. Application `IActionItemWriter.EnsureOpenAsync` để reuse Action đang mở.
2. Database unique index `UX_F03ActionItems_OpenLogicalKey` để chặn race condition giữa nhiều worker instance.

Logical key:

`ModuleCode + SourceType + SourceId + ParticipantId + ActionType + AssignedToEmployeeId`

Database unique index chỉ khóa Action đang reusable trực tiếp (`Open`, `InProgress`, `IsActive=1`). `Expired` vẫn được application coi là reusable để có thể reopen và cấp lại `DueAt`; không đưa `Expired` vào unique index để tránh các bản ghi lịch sử Expired trên DB cũ chặn migration hoặc reopen. Action đã `Completed/Dismissed/Cancelled` không khóa việc tạo lại khi business issue phát sinh lại.


## 13. Notification

Reconciliation → ActionId → Notification → User → Deep link → Execution detail.

Notification không quyết định business state. `F03AppNotifications` là notification framework hiện hữu; branch chỉ bổ sung liên kết `ActionId` và `NotificationType`, không tạo bảng notification song song.

Notification failure không rollback business transaction; lỗi phải được log và có thể retry.

## 14. Confirmation

Mismatch mới tạo Confirmation khi policy yêu cầu.

Decision có thể gồm CONFIRMED, REJECTED, NEED_MORE_EVIDENCE. Confirmation là quyết định/xác nhận của người xử lý, không phải HR Resolution.

## 15. Evidence

Nếu policy yêu cầu: Confirmation → Evidence → HR Evidence Review.

Liên kết runtime chuẩn:

`CalendarProjection.ActionId → F03ActionItems.ActionId ← F03ExecutionReconciliations.ActionId → F03ExecutionConfirmations.ReconciliationId → F03ExecutionConfirmationEvidence.ConfirmationId`.

Vì vậy UI có thể đi từ Action/Calendar tới reconciliation, rồi từ reconciliation tới confirmation/evidence; không cần nhét Evidence vào Payload của Action và cũng không tạo foreign key trực tiếp `Action → Evidence`.

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

## 18A. Payroll / Kế toán và Execution Gate

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

### Điều kiện bắt buộc trước khi in/xuất bảng công - OT

**Không được in, prepare snapshot chính thức, lock hoặc export Payroll nếu trong kỳ 21→20 còn bất kỳ execution nào chưa được giải quyết.**

Hệ thống kiểm tra đồng thời:

- Không còn `F03ExecutionReconciliation` có trạng thái `Mismatch` hoặc `AwaitingConfirmation`.
- Không còn `F03ExecutionCorrection` ở `Pending` hoặc `Failed`.
- Resolution đã đóng phải có correction tương ứng ở `Applied` hoặc trường hợp `Cancelled` được audit hợp lệ.
- Không được có Resolution đã `Resolved` nhưng correction của nó vẫn `Failed`.
- Payroll Input phải được tạo lại sau lần correction cuối cùng; snapshot cũ không được dùng để khóa kỳ.

Do đó:

```text
Payroll Period 21 → 20
        │
        ▼
Kiểm tra toàn bộ Execution
        │
   ┌────┴────┐
   │         │
 CÒN LỖI   TẤT CẢ ĐÃ XỬ LÝ
   │         │
 BLOCK       ▼
   │     HRM Calculation hoàn tất
   │         │
   │         ▼
   │     F03PayrollInputs
   │         │
   │         ▼
   │       PRINT
   │         │
   │         ▼
   │       LOCK
   │         │
   │         ▼
   │      EXPORT
```

Kiểm tra được thực hiện **cả ở service và stored procedure** `dbo.usp_PreparePayrollPeriod`, để không thể bypass bằng cách gọi SQL trực tiếp.

Nếu kỳ đã khóa/xuất, correction bị chặn và phải đi qua Payroll Adjustment/Reopen.

API:
- `POST /api/payroll/periods/{periodId}/prepare`
- `POST /api/payroll/periods/{periodId}/lock`



## 19. Worker tự động làm gì?

1. HRM attendance calculation / backfill.
2. Execution reconciliation.
3. Action creation / dedup.
4. Action lifecycle.
5. Notification.
6. Calendar projection.

**Không trộn approval escalation với execution action lifecycle.** Approval timeout/escalation hiện hữu tiếp tục thuộc `ApprovalEscalationService` + `F03EscalationRules/F03EscalationLogs` và `EscalationBackgroundWorker`. `F03ExecutionPolicies.DueHours` và `F03ActionPolicies.DueHours` chỉ định deadline của execution confirmation/action; chúng không thay thế và không chạy song song một approval escalation engine.

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

Việc `/me` lấy EmployeeId từ authenticated claims/identity là một **security/non-functional invariant dùng chung của API**, không phải business rule riêng của Work Calendar.

Scope gồm Own, Employee, Department, All tùy policy.

Không đủ điều kiện chỉ vì có quyền ExecutionReview; user còn phải có scope tới employee đang review.


## 22. RBAC + Organization Scope + Approval Policy — mô hình quyền chuẩn

### 22.1 Nguyên tắc nền tảng

RBAC không được dùng để mô hình hóa toàn bộ cơ cấu tổ chức hoặc tuyến phê duyệt. Ba lớp có trách nhiệm độc lập:

| Lớp | Câu hỏi trả lời | Nguồn quyết định |
|---|---|---|
| Role | Người dùng thuộc nhóm quyền nào? | FVN RBAC |
| Capability / Function | Người dùng được thao tác gì? | FVN RBAC capability matrix |
| Organization Scope | Người dùng được xem/thao tác trên phạm vi tổ chức nào? | HRM organization + scope policy |
| Approval Policy | Người dùng có được phê duyệt hồ sơ này không và ở level nào? | F03ApprovalPolicies + ApprovalRouteService |
| Business State | Với trạng thái hiện tại, action có hợp lệ không? | Business workflow/state machine |

Không tạo Role chỉ để biểu diễn chức vụ như Manager/Leader hoặc chỉ để biểu diễn khả năng Approve.

### 22.2 Bộ Role chuẩn

Bộ Role chuẩn của FVN_REGISTER:

1. SuperAdmin — quản trị tối cao, Security/RBAC và cấu hình cấp cao.
2. Admin — quản trị hệ thống/nghiệp vụ được giao theo scope.
3. HR — nghiệp vụ nhân sự, attendance/leave/OT reconciliation, HR case, báo cáo và HR resolution theo capability/scope.
4. IT — tài khoản, kỹ thuật, security/audit, integration/diagnostics, Equipment kỹ thuật và IT case theo capability/scope.
5. Editor — tạo/chỉnh sửa dữ liệu nghiệp vụ được cấp.
6. User — người dùng nghiệp vụ thông thường, chủ yếu trong Own scope.
7. Guest — quyền tối thiểu, chủ yếu thông tin công khai hoặc capability được cấp.

Không sử dụng các Role Approver, Manager, DepartmentManager, EquipmentAdmin, HRApprover, ITApprover để thay thế các lớp policy ở trên.

### 22.3 Approval không phụ thuộc Role Approver

Một người có thể cùng Role với người khác nhưng có hoặc không có quyền phê duyệt khác nhau. Approval phải được resolve từ tổ chức + chức vụ + policy:

Factory → Department? → SubDepartment? → Position → ApprovalGroup → ApprovalLevel

Điều này cho phép cùng một Role HR, Editor hoặc Role nghiệp vụ khác nhưng chỉ những người có policy phù hợp mới được approve.

### 22.4 Cây tổ chức phải hỗ trợ ba dạng

Hệ thống phải hỗ trợ đồng thời:

Company → Factory → Department → SubDepartment → Employee

Company → Factory → Department → Employee

Company → Factory → SubDepartment → Employee

Department và SubDepartment là optional theo từng node thực tế; không được ép mọi Factory phải có Department.

F03ApprovalPolicies và Organization Scope phải resolve được đúng node tổ chức trước khi tính Approval Level.

### 22.5 Managed Scope của quản lý

“Quản lý” là thuộc tính/chức vụ và phạm vi tổ chức, không phải Role mới.

Ví dụ:
- Tổ trưởng Đúc: ManagedScope = Factory/Production/Đúc.
- Trưởng phòng Production: ManagedScope = Factory/Production và bao phủ các SubDepartment con.
- Quản lý một Factory: ManagedScope = Factory.

Managed Scope cho phép xem dashboard, attendance, leave, OT, trip, calendar, execution và report của phạm vi được giao nếu capability tương ứng được cấp.

Managed Scope không tự động cấp Approve. Approve vẫn phải qua F03ApprovalPolicies.

### 22.6 Quyền xem báo cáo của quản lý

Một quản lý không phải Approver vẫn có thể có:
- Dashboard.View trong ManagedScope.
- Attendance.View trong ManagedScope.
- Leave.View trong ManagedScope.
- OT.View trong ManagedScope.
- Trip.View trong ManagedScope.
- Calendar.View trong ManagedScope.
- Execution.View trong ManagedScope.
- Reports.View/các capability báo cáo tương ứng trong ManagedScope.

Đây là quyền xem/quản lý dữ liệu theo scope, không phải quyền phê duyệt.

### 22.7 Công thức authorization cuối cùng

Một action chỉ hợp lệ khi thỏa cả bốn điều kiện:

Role/Capability + Organization Scope + Business State + Approval Policy (nếu action là Approve)

API phải kiểm tra lại các điều kiện này; UI chỉ dùng chúng để hiển thị/ẩn menu và action phù hợp.

## 23. Màn hình chính theo từng loại Role

Không tạo bảy bộ ứng dụng khác nhau. Dùng một Workspace chung, nhưng nội dung Dashboard, Action Center, navigation và quick actions được compose theo capability + scope của người đăng nhập.

### 23.1 User Workspace

Màn hình chính ưu tiên:
- Lịch làm việc cá nhân.
- Công/attendance của bản thân.
- Số dư và lịch sử Leave.
- OT của bản thân.
- Trip của bản thân.
- Đơn đang chờ approval.
- Execution mismatch/action của bản thân.
- Notification/Inbox.
- Quick actions: Tạo Leave / OT / Trip.

### 23.2 Manager Workspace

Không phải Role Manager; đây là layout được bật khi người dùng có ManagedScope.

Màn hình chính ưu tiên:
- Dashboard của Factory/Department/SubDepartment được phụ trách.
- Headcount và attendance summary.
- Leave/OT/Trip summary của scope.
- Calendar theo scope.
- Execution exceptions của scope.
- Báo cáo/statistics của scope.
- Equipment thuộc scope nếu có capability.
- Action cần quản lý xử lý.
- Approval chỉ xuất hiện nếu F03ApprovalPolicies xác định người đó là approver.

### 23.3 HR Workspace

Màn hình chính ưu tiên:
- Attendance/HRM sync status.
- Leave/OT/Trip overview toàn phạm vi được cấp.
- Execution reconciliation queue.
- HR Case / phản hồi thắc mắc người dùng.
- Confirmation/Evidence/HR Resolution.
- Correction queue.
- Báo cáo nhân sự.
- Work Calendar administration nếu có WorkCalendar.Manage.
- Các approval action chỉ xuất hiện khi Approval Policy cho phép.

### 23.4 IT Workspace

Màn hình chính ưu tiên:
- System/Integration health.
- User account/security issues.
- Security audit.
- HRM sync diagnostics/retry.
- Equipment workspace: asset, QR, repair, transfer và lịch sử theo capability/scope.
- IT Case / hỗ trợ kỹ thuật người dùng.
- Notification/integration failures.

IT không tự động có Leave/OT/Trip approval, HR resolution hoặc payroll authority.

### 23.5 Admin Workspace

Màn hình chính ưu tiên:
- System administration.
- User/role/function management.
- Organization/scope configuration được cấp.
- Work calendar configuration.
- Approval policy configuration nếu capability được cấp.
- Email/notification configuration.
- Reports và operational status.
- Audit/security overview theo quyền.

Admin không mặc nhiên trở thành Approver; Approval Policy vẫn quyết định.

### 23.6 SuperAdmin Workspace

Màn hình chính có toàn bộ vùng quản trị:
- Security Center/RBAC.
- User and role administration.
- Organization/scope administration.
- Approval policy administration.
- Integration/HRM synchronization.
- Execution/reconciliation monitoring.
- Equipment administration.
- Work Calendar administration.
- Audit.
- System health.

SuperAdmin là quyền hệ thống cao nhất nhưng mọi thao tác nhạy cảm vẫn phải được audit.

### 23.7 Editor Workspace

Màn hình chính ưu tiên:
- Business modules được cấp.
- Create/Edit queues.
- Registration/list/history.
- Dashboard và report trong scope.
- Action Center nếu có capability.

Editor không mặc nhiên có Approval Policy.

### 23.8 Guest Workspace

Chỉ hiển thị các vùng được phép, ưu tiên Public Information và các capability read-only đã cấp. Không hiển thị action/approval/configuration nếu không có capability.

## 24. Security Center / màn hình phân quyền chuẩn

Security Center phải biểu diễn quyền theo ma trận thay vì chỉ danh sách Role.

Màn hình nên có bốn vùng:

1. Role Matrix — 7 Role chuẩn và capability/function được cấp.
2. Organization Scope — Factory → Department → SubDepartment, cho phép cấu hình phạm vi dữ liệu.
3. Approval Policy — chọn Factory, Department (nếu có), SubDepartment (nếu có), Position, ApprovalGroup và ApprovalLevel; hiển thị preview tuyến phê duyệt.
4. Effective Permission Preview — chọn một user để xem quyền thực tế sau khi merge Role + Capability + Scope + Approval Policy.

Effective Permission Preview phải trả lời được:
- User thấy menu nào?
- User xem được dữ liệu nào?
- User được Create/Edit/Export/Repair/Resolve action nào?
- User có phải Approver không?
- Nếu là Approver, đang ở Level nào và cho node tổ chức nào?
- User có bị giới hạn bởi Business State không?

Security Center không được biến Approval Policy thành Role assignment.

## 25. Equipment authorization model

Equipment dùng cùng mô hình RBAC + Scope + State.

Capability tối thiểu:
- Equipment.View
- Equipment.Create
- Equipment.Edit
- Equipment.Assign
- Equipment.Transfer
- Equipment.Repair
- Equipment.Return
- Equipment.Liquidate
- Equipment.Approve nếu nghiệp vụ cần approval riêng
- Equipment.Import
- Equipment.Export
- Equipment.QR
- Equipment.History

Equipment.Edit không có nghĩa được sửa mọi field hoặc mọi state. Business state và field-level rule phải tiếp tục giới hạn action.

Ví dụ: IT có thể Repair/QR/Technical Edit; HR có thể Assign/Return/Transfer; Manager có thể View/History trong ManagedScope. Không tạo Role EquipmentAdmin chỉ để đạt các quyền này.

## 26. Work Calendar authorization

Tách rõ:
- Calendar.View: xem Calendar chung theo Own/ManagedScope/Company tùy capability.
- WorkCalendar.Manage: quản lý WorkYear, khóa WorkYear, holiday/workday và import Excel theo scope quản trị.

Người dùng bình thường không cần quyền cấu hình WorkYear chỉ vì họ được xem Calendar.

## 27. Cập nhật mô hình UI tổng thể

Navigation và Dashboard không hard-code theo Role name. UI nên compose từ effective permissions:

EffectivePermission = RoleCapabilities + ExplicitCapabilities + OrganizationScope + ApprovalPolicy + BusinessState

Menu/action chỉ xuất hiện khi capability và scope phù hợp; Approval menu/action chỉ xuất hiện khi có route/policy thực tế.

Dashboard vẫn giữ một Workspace chung, nhưng các card/queue/quick actions được thêm theo effective permission:

My Overview + Managed Scope Overview + Approval Queue (nếu có) + Execution Queue + HR Case (nếu có) + IT Case (nếu có) + Equipment (nếu có).

## 28. Production rule cho Organization/Approval

Trước khi coi RBAC/Approval hoàn thiện production phải kiểm thử tối thiểu:
- Factory có Department và SubDepartment.
- Factory có Department nhưng không có SubDepartment.
- Factory chỉ có SubDepartment.
- Một Department có nhiều SubDepartment.
- Manager chỉ xem được ManagedScope của mình.
- Manager không có Approval Policy thì không được approve.
- Cùng Position nhưng khác Factory/Department có Approval Level khác nhau.
- Approval route fallback/không tìm thấy policy phải trả lỗi cấu hình rõ ràng, không tự cấp quyền.
- User đổi Department/Position sau HRM sync phải re-resolve scope và approval.
- UI và API cho cùng một effective permission result.

## 29. Audit

Các lớp audit quan trọng:
- F03ExecutionReconciliationHistory
- F03ExecutionConfirmations
- F03ExecutionConfirmationEvidence
- F03ExecutionResolutions
- F03ExecutionCorrections
- Action lifecycle
- Notification lifecycle

Do đó có thể truy ngược: hệ thống phát hiện gì → ai xác nhận → evidence nào → HR quyết định gì → correction nào được tạo.

## 30. Một ví dụ hoàn chỉnh

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

## 31. Toàn bộ khả năng người dùng có thể hình dung

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

## 32. Kiến trúc 5 tầng

5. EXPERIENCE / WORKSPACE: Calendar · Dashboard · Lists · Notifications

4. WORK / EXECUTION: Action · Confirmation · Evidence · HR Resolution · Correction

3. RECONCILIATION: Planned ↔ Actual · Policy · State machine

2. BUSINESS MODULES: Leave · OT · Trip · Approval

1. SOURCE / ACTUAL: HRM Employee · Attendance · Company data

Không tầng nào được phép biến projection thành source-of-truth của tầng dưới.

## 33. Khả năng mở rộng module

Reconciliation engine phải pluggable giống Calendar provider.

Worker không được chứa method riêng cho từng module. Worker chỉ:
1. đọc ExecutionPolicy;
2. resolve danh sách `IExecutionReconciliationModuleProvider`;
3. truyền execution window + reconciliation mode;
4. chạy từng provider độc lập và cô lập lỗi theo ModuleCode.

Contract mở rộng hiện có:
- `IExecutionReconciliationModuleProvider` — module entry point.
- `IPlannedProvider` — extension contract cho nguồn Planned/Approved.
- `IActualProvider` — extension contract cho nguồn Actual.
- `IReconciliationMapper` — extension contract cho mapping Planned ↔ Actual → reconciliation request.

Các module OT / Leave / Trip / Attendance hiện được tách thành provider riêng; shared base chỉ giữ helper hạ tầng (unresolved-source lookup, employee resolution và worker row DTO), không chứa business branch theo ModuleCode.

Module mới chỉ cần thêm provider + các thành phần Planned/Actual/Mapper phù hợp và đăng ký DI. Không sửa `ExecutionReconciliationBackgroundWorker`.

Module tiếp tục dùng chung:
Calendar → Action → Notification → Confirmation → Evidence → HR Resolution → Correction.

Không tạo workflow execution riêng chỉ vì thêm module.

## 34. Trạng thái hiện tại

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

## 35. Production hardening còn nên hoàn thiện

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

## 36. Kết luận

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
## 37. Implementation status — §22–§35 hardening

| Hạng mục | Triển khai |
|---|---|
| RBAC HR/IT | `UserRole.HR=7`, `UserRole.IT=8`; legacy Approver=4 được retire khỏi effective RBAC bởi SQL 38. |
| Approval | Approval Inbox/Policy/Route resolve theo `F03ApprovalPolicies`; Role Approver không được dùng làm source-of-truth cho Approve. |
| ManagedScope | `dbo.F03ManagedScopes` + `F03ManagedScope`; scope là data-scope bổ sung, không tự cấp capability. |
| Calendar authorization | `Calendar.View=3043` tách khỏi `WorkCalendar.Manage=3042`. |
| Equipment | Assign/Transfer/Return/Liquidate/QR/History có SecurityFunctionCode riêng và được enforce theo action. |
| Security Center | Có Organization Scope + Approval Policy + Effective Permission Preview. |
| Reconciliation | Worker orchestration-only; OT/Leave/Trip/Attendance là pluggable providers; Planned/Actual/Mapper contracts đã có. |
| Dashboard | Business module dashboards (Leave/OT/Trip/Equipment) được compose theo capability/scope. HR/IT/Administration/Execution là workspace/capability, không phải RequestModule. |
| Manager Workspace | Chỉ bật khi user có ManagedScope; Approval Inbox tách độc lập. |
| Overview | Leave/OT/Trip có trang Overview riêng; quick-create trên Home theo capability. |
| Guest | Thiếu Dashboard.View không còn hiện lỗi Dashboard; chuyển sang Public Information workspace. |
| Production hardening | Payroll gate, evidence/HR review invariants, notification dedup, attendance backfill/company-wide path và worker health tiếp tục giữ theo các implementation trước. |

### Production note

`F03ManagedScopes` là data-scope assignment, không phải Role. Có ManagedScope không tự cấp View/Create/Edit/Approve; capability vẫn phải tồn tại trong effective permission. Approval tiếp tục phụ thuộc `F03ApprovalPolicies` và business state.


## 38. Quy tắc quản lý tài liệu kiến trúc

Tài liệu này là **Single Source of Truth** cho kiến trúc FVN_REGISTER.

1. Không tạo tài liệu kiến trúc tổng hợp mới để thay thế tài liệu này.
2. Nếu phát hiện kiến trúc chưa đúng, sửa trực tiếp tại đây.
3. Code, SQL, API, UI và worker phải được đối chiếu với tài liệu này.
4. Tài liệu chuyên môn cũ chỉ được giữ nếu có mục đích riêng và không được trở thành một nguồn kiến trúc thay thế.
5. Không dùng số tài liệu mới như 25/26/... để tạo một phiên bản kiến trúc song song.
6. Khi một quyết định kiến trúc thay đổi, cập nhật tài liệu này và implementation trong cùng luồng commit.

### Trạng thái nguồn sự thật

`24_WORK_CALENDAR_AND_ACTION_IMPLEMENTATION.md` là file kiến trúc chính.

Các file kiến trúc tổng hợp trùng lặp phải được loại bỏ để tránh hai luồng thiết kế song song.

## 24A — CALENDAR DAY CONTRACT v1

The central Work Calendar read model is one `CalendarDayDto` per employee/date. The UI must not treat each business record as an independent visual event.

### 24A.1 Day composition

A day is assembled from five fact groups:

1. Company calendar:
   - `F03WorkYears`
   - `F03CompanyHolidays`
   - A date is a company working day when it is inside an active WorkYear and is not present in F03CompanyHolidays.
   - Saturday/Sunday is informational only. It does not block registration unless the company calendar marks the date as a holiday.

2. Shift:
   - `ShiftId`
   - `ShiftAbbr` (HC/C1/C2/C3/Kip/...)
   - `RequiredMinutes`

3. Attendance:
   - IN / OUT
   - WorkMinutes
   - RequiredMinutes
   - ActualHours / RequiredHours
   - Actual OT minutes / recognized OT minutes
   - Numeric variance flag
   - reconciliation ActionId / DetailRoute when available

4. Registration:
   - LEAVE
   - OT
   - TRIP
   - leave `SubTypeCode` is preserved so rules can distinguish `P` and `NB`
   - `IsHalfDay` and `DayValue` are preserved for leave details

5. Issues:
   - Generated only by CalendarDayRule implementations.
   - Issue is not a UI event.
   - Each Issue can expose multiple ActionOptions.

### 24A.2 Registration opening policy

Registration is available only when:

`WorkDate > Today` and the date belongs to an active WorkYear.

Rules:

| Date | Leave | OT | Trip |
|---|---:|---:|---:|
| Today | No | No | No |
| Past | No | No | No |
| Future working day | Yes | Yes | Yes |
| Future company holiday | No | Yes | No |
| Outside WorkYear | No | No | No |

The source of company holidays is F03CompanyHolidays. Never hard-code public/company holidays in the UI.

The base availability is calculated server-side. Existing registration of the same module on the date suppresses a new registration opportunity for that module.

### 24A.3 Issue contract

Issue is the stable business signal used by Calendar UI.

Current issue codes:

| Code | Severity | Condition | Actions |
|---|---:|---|---|
| `ATTENDANCE_TIME_VARIANCE` | 2 / yellow | AttendanceDisplayValue is numeric because actual hours differ from required shift hours | OPEN_OT when actual > required; OPEN_HR_FEEDBACK |
| `OT_NO_ACTUAL` | 3 / red | Past date + approved OT registration + no actual OT | OPEN_HR_FEEDBACK; CANCEL_OT |
| `LEAVE_HAS_ATTENDANCE` | 3 / red | Full-day P/NB + actual attendance | OPEN_HR_FEEDBACK |
| `TRIP_HAS_ATTENDANCE` | 3 / red | Full-day Trip + actual attendance | OPEN_HR_FEEDBACK |
| `OT_ACTUAL_WITHOUT_REQUEST` | 3 / red | Actual OT exists + no OT registration | OPEN_HR_FEEDBACK; OPEN_OT |

Severity is a UI-neutral numeric contract:
- 0/1 = information/attention
- 2 = warning (yellow)
- 3 = critical (red)

The backend never hard-codes CSS. The UI maps severity to presentation.

### 24A.4 Attendance numeric variance

`ATTENDANCE_TIME_VARIANCE` deliberately follows the calculated HRM display state.

The source calculation in `dbo.usp_CalculateHrmAttendance` produces a numeric AttendanceDisplayValue when both IN/OUT exist, RequiredMinutes exists, and actual work time differs from the required shift time.

This issue is yellow and means “needs user/HR attention”, not automatically “OT approved”.

If actual hours exceed required hours, the day exposes:
- OPEN_OT
- OPEN_HR_FEEDBACK

If actual hours do not exceed required hours, only OPEN_HR_FEEDBACK is exposed.

### 24A.5 OT with no actual

`OT_NO_ACTUAL` is not evaluated on future dates because no actual attendance is expected yet.

For a past date, an approved OT request without actual OT shows a red `?`.

Actions:
- OPEN_HR_FEEDBACK: opens the reconciliation/confirmation path when available.
- CANCEL_OT: calls the OT business service and records the cancellation reason. Calendar must never directly UPDATE F03OTRequests.

### 24A.6 Leave / Trip conflicts

Only a full-day leave is considered a full-day attendance conflict.

For Leave:
- `P` and `NB` are currently treated as attendance-conflict candidates.
- `IsHalfDay = true` or `DayValue < 1` suppresses the full-day conflict.

For Trip:
- F03TripRequest currently represents a day-range request, therefore the calendar treats each covered date as a full-day registration.

Future leave types and half-day semantics must be added as Rule implementations, not as UI conditionals.

### 24A.7 Rule engine extension contract

New calendar checks must implement:

`ICalendarDayRule`

Each rule receives:
- `CalendarDayDto`
- Source `CalendarItemDto` records for that date

Rules return `CalendarIssueDto`.

Add a new rule by:
1. Creating a class under `FVN_REGISTER.Infrastructure/Services/Calendar/Rules`.
2. Assigning a deterministic `Order`.
3. Registering it as `ICalendarDayRule` in API DI.
4. Adding a unit test.
5. Updating this section with the new IssueCode and ActionOptions.

Do not put new issue logic in:
- `WorkCalendar.razor`
- `workCalendar.js`
- `SharedWorkCalendarService` beyond composition/orchestration.

### 24A.8 UI contract

The Work Calendar uses FullCalendar only for month navigation and date geometry.

One cell represents one `CalendarDayDto` and visually contains:

1. Date
2. Company holiday (if any)
3. Shift
4. Attendance IN/OUT and actual/required hours
5. P/NB/OT/Trip registrations
6. Issue marker `?`
7. “+ Đăng ký” when registration is available

Clicking a day opens `CalendarDayDialog`.

Clicking the `?` also opens the same dialog. The dialog lists every issue and its ActionOptions, so a day can safely contain multiple independent issues.

### 24A.9 Action semantics

`ActionOption` is a command/navigation option, not an ActionItem row.

Current commands:
- OPEN_OT → navigate to /ot/create
- OPEN_HR_FEEDBACK → open the existing execution reconciliation/confirmation route when the issue has a DetailRoute
- CANCEL_OT → invoke OTService.CancelAsync with user-entered reason

F03ActionItems remain the shared work item generated by execution reconciliation. Calendar ActionOptions do not replace F03ActionItems.

### 24A.10 HR feedback / evidence

HR feedback is deliberately routed through the existing execution reconciliation workflow whenever the issue has a reconciliation DetailRoute.

That workflow already supports:
- employee confirmation
- comment/reason
- evidence
- evidence file upload
- HR resolution

Therefore Calendar does not create a second attachment/evidence subsystem.

If a new issue does not yet have a reconciliation, its Rule should first define how the issue receives a source/reconciliation before introducing a new feedback store.

### 24A.11 Current source-of-truth boundary

`WorkCalendarService`:
- loads company calendar/registrations;
- does not own reconciliation;
- exposes registration availability.

`CalendarModuleProvider`:
- reads business/attendance projection facts.

`SharedWorkCalendarService`:
- composes facts into CalendarDay;
- invokes RuleEngine;
- exposes Alerts and RegistrationOpportunities.

`CalendarDayRuleEngine`:
- detects issues;
- creates no business side effects.

Business services:
- execute registration/approval/cancellation commands.

ExecutionReconciliation:
- owns mismatch lifecycle, confirmation, evidence, HR resolution and ActionItem linkage.

### 24A.12 Mandatory regression checks

Before adding a new Calendar rule, verify at minimum:

- future normal day → Leave + OT + Trip registration options
- future company holiday → OT only
- today/past → no registration option
- half-day P → no full-day attendance conflict
- full-day P/NB + attendance → red ?
- full-day Trip + attendance → red ?
- past approved OT + no actual OT → red ? + feedback + cancel
- numeric attendance variance → yellow ? + HR feedback; OPEN_OT only when actual > required
- actual OT without request → red ?
- resolving/cancelling source business data removes the stale issue after reconciliation/projection refresh



## 24A.13. Integration với kiến trúc Work Calendar hiện hữu

Work Calendar đã có foundation trước khi thêm Action/Execution:

- `SQL/19_WorkCalendar.sql` là foundation/indexes của Work Calendar trong lịch sử schema.
- Branch hiện tại dùng `SQL/35_WorkCalendar.sql` để **chuẩn hóa/migrate** WorkYear + CompanyHoliday; file này không tạo một Calendar service thứ hai.
- Shared Calendar runtime vẫn đi qua `CalendarModuleRegistry` → providers → `SharedWorkCalendarService`; không tạo một `LeaveCalendarService`, `OtCalendarService` hoặc `TripCalendarService` song song.
- `F03CalendarProjection` là projection cho dữ liệu cần materialize/action-link; nó không thay thế các request tables. `WorkCalendarService` vẫn đọc business registration source để hiển thị đăng ký.
- `24_WORK_CALENDAR_AND_ACTION_IMPLEMENTATION.md` là source of truth hiện tại của branch. Các blueprint cũ chỉ là historical input/reference; không tạo thêm một architecture document cùng cấp.


### 24A.13B. Registration status trong Calendar Rules

Execution/reconciliation và Calendar conflict rules chỉ dùng request **Approved** làm planned business state.

- `OT_NO_ACTUAL`: chỉ Approved OT.
- `OT_ACTUAL_WITHOUT_REQUEST`: chỉ được coi là “chưa có đơn OT” khi không có Approved OT.
- `LEAVE_HAS_ATTENDANCE`: chỉ Approved Leave P/NB.
- `TRIP_HAS_ATTENDANCE`: chỉ Approved Trip.

Draft/Pending/InProgress/NeedsRevision không phải bằng chứng rằng business plan đã có hiệu lực; chúng vẫn có thể xuất hiện trong Calendar với status riêng nhưng không được làm tắt/bật issue reconciliation.

### 24A.13A. Source identity khi ghép Projection với Registration

Calendar không được lấy `ModuleCode` làm identity duy nhất khi gắn `ActionId`/`DetailRoute` vào một registration.

Identity chuẩn hiện tại:
- Leave: `LEAVE_DAY` + `{RequestId}:{yyyyMMdd}`
- OT: `OT_EMPLOYEE` + `{OTCode}:{EmployeeCode}`
- Trip: `TRIP_REQUEST` + `{TripCode}`

`SharedWorkCalendarService` ưu tiên match `ModuleCode + SourceId`. Chỉ fallback theo `ModuleCode` khi đúng module đó có **duy nhất một** projection trong ngày. Nhờ vậy một ngày có nhiều đơn Leave/OT/Trip không thể lấy nhầm Action của đơn khác.

### Approval escalation không bị thay thế

Leave/OT approval timeout đang có pipeline riêng:

`Approval request → Approval snapshot/history → ApprovalEscalationService → F03EscalationRules/F03EscalationLogs`.

Execution deadline là pipeline khác:

`ExecutionReconciliation → ActionItem → DueAt → Action lifecycle`.

Chỉ khi có quyết định kiến trúc riêng sau này mới refactor hai pipeline này về một engine chung. Không chạy hai engine cho cùng **approval timeout**.

### Action → Evidence

Action không mang Evidence payload. Luồng đọc là:

`ActionId → ReconciliationId → ConfirmationId → Evidence`.

Điều này giữ Action nhỏ, Evidence/audit đầy đủ và cho phép cùng một execution workflow được mở từ Calendar, Task List hoặc Notification.

### 24A.14 Example day display

```
┌─────────────────────────────┐
│ 18                      ?  │
│                             │
│ Ca C1                       │
│ 07:55 → 18:10               │
│ 10.17h / 8h                 │
│                             │
│ OT · 18:00-20:00            │
│                             │
│ 🟡 Chênh lệch giờ công      │
└─────────────────────────────┘
```

The visual marker `?` is the presentation of an Issue. It is not a calendar Event identity.


