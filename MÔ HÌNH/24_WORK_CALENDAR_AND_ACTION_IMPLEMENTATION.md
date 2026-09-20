# 24 — WORK CALENDAR + ACTION / TASK IMPLEMENTATION BLUEPRINT

> Đây là tài liệu implementation source-of-truth cho Shared Work Calendar, Việc cần làm, Notification và Dashboard/Login. Business modules vẫn là source of truth.

## 1. Mục tiêu
Xây một framework Calendar dùng chung cho OT, Leave, Trip và module tương lai; đồng thời mọi trạng thái cần người dùng xử lý phải xuất hiện đồng thời ở Calendar, Task List, Notification và Dashboard.

## 2. Kiến trúc
Business Module → Calendar Provider → Registry → Policy → Aggregator → Calendar Projection.
CalendarItem.RequiresAction = true → ActionItem → Task List + Notification + Dashboard.
Action hoàn tất → điều hướng/đồng bộ với business module owner.

## 3. Ranh giới trách nhiệm
| Thành phần | Trách nhiệm | Không làm |
|---|---|---|
| Business module | Source of truth, validation, approval, confirmation, mutation | Không phụ thuộc Calendar UI |
| Calendar | Projection, marker, summary, navigation | Không thay business status |
| Action | Việc cần làm, assignment, due date, lifecycle, idempotency | Không thay business workflow |
| Notification | Delivery, unread/read, realtime | Không thay Action/business status |
| Dashboard | Aggregate counts/list để người dùng thấy ngay | Không quyết định authorization |

## 4. Calendar contract
CalendarModuleDefinition: ModuleCode, ModuleName, SupportsCalendar, DefaultEnabled.
ICalendarModuleProvider.GetItemsAsync(CalendarContext, CancellationToken).
CalendarItemDto tối thiểu: WorkDate, ModuleCode, StatusCode, Marker, Summary, Severity, RequiresAction, DetailRoute, SourceId.

## 5. Provider rule
OT, Leave, Trip và module tương lai đăng ký provider qua DI collection. Aggregator không được có if/else theo ModuleCode.
Module mới phải thêm provider + definition + DI registration; không sửa CalendarAggregator chỉ để biết module mới.

## 6. Calendar policy
CalendarModulePolicy: ModuleCode, IsEnabled, DisplayMode, NoteMode, ConfirmationMode, ReconciliationMode, Priority, SummaryTemplate, DetailTemplate.
DisplayMode: None, Marker, Summary, MarkerAndSummary.
NoteMode: None, AlertsOnly, AllImportant.
ConfirmationMode: None, Employee, EmployeeThenHr, Admin.
ReconciliationMode: None, PlannedVsActual, ApprovedVsExecution, Custom.
Policy chỉ điều khiển projection; không bypass authorization.

## 7. SQL — Calendar
F03CalendarModuleDefinitions: Id, ModuleCode UNIQUE, ModuleName, SupportsCalendar, DefaultEnabled, IsActive, CreatedAt, UpdatedAt.
F03CalendarModulePolicies: Id, ModuleCode, IsEnabled, DisplayMode, NoteMode, ConfirmationMode, ReconciliationMode, Priority, SummaryTemplate, DetailTemplate, CreatedAt, UpdatedAt, UpdatedBy.
F03CalendarConfirmations chỉ tạo nếu hệ thống thực sự cần confirmation dùng chung; nếu module đã có workflow confirmation thì Calendar phải route về module owner.
F03CalendarProjection là read projection tùy chọn, có thể rebuild từ source; không phải source of truth.
Index khuyến nghị: EmployeeId + WorkDate; EmployeeId + WorkDate + ModuleCode; RequiresAction; UNIQUE nếu materialized theo EmployeeId + WorkDate + ModuleCode + SourceId.

## 8. Shared Action / Task
ActionItem là thực thể dùng chung cho mọi việc cần người dùng xử lý.
Fields: ActionId UNIQUE, ModuleCode, SourceId, EmployeeId, AssignedToUserId, AssignedToEmployeeId, WorkDate, ActionType, Title, Summary, Severity, Priority, Status, DueAt, DetailRoute, ReferenceNo, PayloadJson, CreatedAt, UpdatedAt, CompletedAt, DismissedAt.
ActionItemStatus: Open, InProgress, Completed, Dismissed, Expired, Cancelled.
Action status không thay thế business status.

## 9. ActionType
Không hard-code OT-only. Ví dụ: OT.ATTENDANCE_CONFIRMATION, OT.REVIEW_MISMATCH, LEAVE.CONFIRMATION, LEAVE.REVIEW, TRIP.CONFIRMATION, TRIP.REVIEW, FUTURE_MODULE.ACTION.
Module mới tự đăng ký action type/definition.

## 10. Action creation rule
RequiresAction = false → Calendar marker/summary, không tạo Task.
RequiresAction = true → tạo hoặc update ActionItem + Notification + Task projection.
Một business issue chỉ có một Open/InProgress action theo logical key: ModuleCode + SourceId + ActionType + AssignedToEmployeeId.
Worker chạy lại phải update action hiện hữu, không tạo duplicate.

## 11. DueAt / expiry
Action có thể có DueAt, Severity và Priority. Việc quá hạn/auto-close phải do Action worker hoặc business module worker theo rule.
Ví dụ OT thiếu attendance: action mở khi phát hiện thiếu; DueAt theo payroll/work-period close; hết hạn thì module owner quyết định AutoConfirmedNotWorked hoặc transition tương ứng.

## 12. Notification
Notification là delivery/read state của Action hoặc event nghiệp vụ. Quan hệ khuyến nghị: ActionItem 1 → N Notification.
F03Notifications: Id, ActionId, RecipientUserId, NotificationType, Title, Message, IsRead, ReadAt, CreatedAt.
Notification không được giữ business workflow state.

## 13. Dashboard / Login
DashboardOrchestrator aggregate: Calendar summary + Action count/items + Notification unread count + các dashboard blocks khác.
User phải thấy việc cần làm ngay sau authentication; không cần mở Calendar.
Identity phải lấy từ claims/session server-side. Không nhận arbitrary UserId/EmployeeId từ /me endpoint.

## 14. API boundary
Calendar: GET /api/calendar/me?period=YYYY-MM; GET /api/calendar/me/{workDate}; GET /api/calendar/me/alerts.
Action: GET /api/actions/me; GET /api/actions/me/count; GET /api/actions/me/{actionId}; POST /api/actions/me/{actionId}/complete; POST /api/actions/me/{actionId}/dismiss.
Notification: GET /api/notifications/me; GET /api/notifications/me/unread-count; POST /api/notifications/me/{notificationId}/read.
API chỉ trả DTO, không trả EF entity.

## 15. Database indexes
Action: UX_ActionItems_ActionId; IX_ActionItems_AssignedToUser_Status; IX_ActionItems_Employee_Status; IX_ActionItems_DueAt; IX_ActionItems_Module_Source.
Notification: index RecipientUserId + IsRead + CreatedAt; index ActionId.

## 16. Attendance / OT integration
Không tạo attendance calculation engine thứ hai. Reuse calculation pipeline hiện hành và các read model như F03HrmAttendanceCalculated/F03HrmOTActual khi đó là source chính thức.
Calculation phải chạy company-wide bằng worker/scheduler và có backfill; login chỉ đọc kết quả.
Payroll period hiện hành: 21 → 20.

## 17. OT multi-employee revision
Mutation/recalculation phải scope OTMasterId + ParticipantId/EmployeeId + RevisionId. Không xử lý chỉ theo OTMasterId.
Action cho participant A không được tạo hoặc đóng action của participant B/C.

## 18. Approval integration
Approval pending có thể tạo ActionItem cho approver. Approval Engine vẫn sở hữu ApprovalStep/Decision/History.
Action Completed không có nghĩa Approval Approved.

## 19. Calendar UI
Cell chỉ hiển thị date + marker + short summary. Không nhồi approval history, evidence, revision history, participant list hoặc workflow dài.
Nếu nhiều marker: dùng compact representation như marker count; chi tiết ở panel/list.

## 20. Security
Calendar/Action/Notification đều phải kiểm tra authenticated identity và data scope. UI hide không phải security.
DetailRoute chỉ là navigation hint; endpoint đích vẫn phải authorize.

## 21. Background jobs
1. Attendance calculation/backfill.
2. Calendar projection refresh nếu materialized.
3. Action creation/deduplication.
4. DueAt/expiry/escalation.
5. Notification delivery/retry.
Jobs phải idempotent và không tạo duplicate business action.

## 22. Implementation order
1. Freeze business contracts.
2. Calendar enums/DTO/interfaces.
3. SQL Calendar definitions/policies.
4. EF entities/configurations.
5. Calendar Registry.
6. Calendar Policy Service.
7. OT Provider.
8. Leave Provider.
9. Trip Provider.
10. Calendar Aggregator.
11. Calendar /me API.
12. Action contract/entity/repository/service.
13. Action deduplication + lifecycle.
14. Notification integration.
15. Dashboard aggregation.
16. UI Calendar + Task List + Notification.
17. Admin policy UI.
18. Workers/projection if needed.
19. Reports.
20. Integration tests.

## 23. Definition of Done
- Calendar supports multiple module providers without module-specific branches in aggregator.
- RequiresAction creates one shared ActionItem.
- Same ActionId is referenced by Calendar, Task List, Notification and Dashboard.
- Worker rerun does not create duplicate Open/InProgress action.
- Action status never replaces business status.
- Notification is delivery/read state.
- User sees action count/list immediately after login/dashboard.
- DueAt/expiry is deterministic and idempotent.
- New module can add provider and action types without changing shared aggregator.
- /me endpoints derive identity from authenticated claims.
- Full solution build and integration tests pass in the actual project environment.

## 24. Final architecture
Business Modules → Calendar Engine → Calendar UI.
Calendar Engine → RequiresAction → ActionItem.
ActionItem → Task List + Notification + Dashboard.
Task click → Module Detail → Business Workflow.

**Calendar = projection/navigation. Action = shared Việc cần làm. Notification = delivery/read. Business module = source of truth.**