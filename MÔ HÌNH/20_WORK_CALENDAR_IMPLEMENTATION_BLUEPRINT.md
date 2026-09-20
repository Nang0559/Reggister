# 20 — WORK CALENDAR IMPLEMENTATION BLUEPRINT

> Tài liệu triển khai chuẩn duy nhất cho Work Calendar. Nội dung tổng hợp từ 01_FLOWS_TOTAL, 04_DASHBOARD, 08_APPROVAL_ESCALATION, 17_OT_LEAVE_LIMITS, 18_OT_ATTENDANCE_RECONCILIATION và 19_REPORTS_STATISTICS.

## 1. Kiến trúc

Work Calendar là framework dùng chung cho OT, Leave, Trip và module tương lai.

    OT Provider ───────┐
    Leave Provider ────┤
    Trip Provider ─────┤
    Future Provider ───┤
                       ↓
              Calendar Registry
                       ↓
              Calendar Policy Engine
                       ↓
              Calendar Aggregator
                 ┌─────┴─────┐
                 ↓           ↓
          Compact Calendar   Alerts / Confirmation

Quy tắc: không tạo Calendar service riêng cho từng module; module cung cấp provider; Admin quyết định module tham gia; Calendar chỉ là read projection + navigation/confirmation orchestration; business rule, approval, authorization và mutation thuộc module owner.

## 2. Contract

CalendarModuleDefinition: ModuleCode, ModuleName, SupportsCalendar, DefaultEnabled.

ICalendarModuleProvider: ModuleCode, GetItemsAsync(CalendarContext).

CalendarItemDto: WorkDate, ModuleCode, StatusCode, Marker, Summary, Severity, RequiresAction, DetailRoute, SourceId.

Provider không tự quyết định policy hiển thị cuối cùng.

## 3. Admin Policy

| Policy | Ý nghĩa |
|---|---|
| IsEnabled | module có xuất hiện |
| DisplayMode | None / Marker / Summary / MarkerAndSummary |
| NoteMode | None / AlertsOnly / AllImportant |
| ConfirmationMode | None / Employee / EmployeeThenHr / Admin |
| ReconciliationMode | None / PlannedVsActual / ApprovedVsExecution / Custom |
| Priority | ưu tiên khi nhiều module cùng ngày |
| SummaryTemplate | format summary |
| DetailTemplate | format detail |

Không cho chọn mode mà provider không hỗ trợ. Policy không bypass authorization.

## 4. SQL Design

### F03CalendarModuleDefinitions

Id, ModuleCode UNIQUE, ModuleName, SupportsCalendar, DefaultEnabled, IsActive, CreatedAt, UpdatedAt.

### F03CalendarModulePolicies

Id, ModuleCode, IsEnabled, DisplayMode, NoteMode, ConfirmationMode, ReconciliationMode, Priority, SummaryTemplate, DetailTemplate, CreatedAt, UpdatedAt, UpdatedBy.

### F03CalendarConfirmations

Chỉ dùng khi cần confirmation chung: Id, ModuleCode, SourceId, EmployeeId, WorkDate, Status, Claim, EvidenceFileId/EvidenceReference, SubmittedAt, ConfirmedBy, ConfirmedAt, HrNote, CreatedAt, UpdatedAt. Nếu module đã có confirmation workflow riêng thì Calendar chỉ route về module owner.

### F03CalendarProjection

Tùy chọn, chỉ dùng khi cần materialized/read projection: Id, EmployeeId, WorkDate, ModuleCode, SourceId, StatusCode, Marker, Summary, Severity, RequiresAction, DetailRoute, CalculatedAt. Projection phải rebuild được từ source và không phải source of truth.

Index đề xuất: UX ModuleCode; IX (EmployeeId, WorkDate); IX (EmployeeId, WorkDate, ModuleCode); IX RequiresAction. Nếu materialized: UNIQUE(EmployeeId, WorkDate, ModuleCode, SourceId).

## 5. Source of Truth

| Dữ liệu | Source |
|---|---|
| OT Approved / Revision | OT module |
| OT Actual | Attendance/OT actual |
| OT Confirmation | OT/Attendance confirmation |
| Leave Approved / Used | Leave module / attendance |
| Trip Approved / Execution | Trip source |
| Calendar policy | Calendar |
| Calendar marker/summary | Calendar projection |
| Approval history | Common Approval |
| Evidence | Module/evidence storage |

Không copy toàn bộ OT/Leave/Trip sang Calendar.

## 6. OT Mapping

EffectiveApprovedRevision → Actual attendance/actual OT → Reconciliation → CalendarItem.

Status: None, Approved, Matched, Mismatch, ActualWithoutApproval, ApprovedWithoutActual, CancelledByNoAttendance.

Approved OT nhưng không có attendance: PendingConfirmation → ? → Employee worked + evidence + HR confirmation, hoặc ConfirmedNotWorked. Hết kỳ công mà vẫn Pending → AutoConfirmedNotWorked → Final OT = 0. Không xóa Approved OT, approval history hoặc audit.

## 7. OT Multi-Employee Revision

Một OT master có nhiều participant. Khi A sửa: không tạo master mới; chỉ tạo revision cho A; ApprovalScope=EmployeeOnly; B/C không đổi; approve R2 chỉ A đổi effective revision; reject R2 A giữ R1; recalculate chỉ A.

Không update theo OTMasterId đơn độc. Mutation phải scope OTMasterId + ParticipantId/EmployeeId + RevisionId.

## 8. Leave Mapping

Leave provider cung cấp Approved Leave → marker → summary. Nếu policy bật reconciliation thì Approved Leave → Actual/Used → Reconciliation. Calendar không tự tính quota; quota thuộc Leave service/validator.

## 9. Trip Mapping

Trip provider có thể cung cấp Approved, Start/End, Execution, Confirmation. ApprovedVsExecution thì reconciliation; None thì chỉ summary.

## 10. Calendar Aggregator

Không viết if/else riêng cho OT, Leave, Trip. Dùng Enabled Providers → GetItemsAsync → Policy Engine → Aggregate. Module mới chỉ cần provider + definition + DI registration.

## 11. Calendar Cell

Một ngày có thể có OT 🔴, Leave 🟦, Trip 🟪. Ô chỉ có Date + Primary status + Marker(s) + Short summary. Không đưa approval history, evidence, revision, participant, actual in/out hoặc note dài vào ô. Quá nhiều marker thì dùng +N; click ngày để xem đầy đủ.

## 12. Alerts / Confirmation dưới lịch

CalendarAlertItem: WorkDate, ModuleCode, Severity, Summary, RequiresAction, DetailRoute.

Ví dụ: 23/08 OT 🔴 lệch giờ; 24/08 OT ? thiếu chấm công; 25/08 Trip 🟪 công tác.

Ưu tiên: RequiresAction → Severity → Policy.Priority → WorkDate. Không thay đổi thứ tự ngày trong calendar.

## 13. API

GET /api/calendar/me?period=YYYY-MM
GET /api/calendar/me/{workDate}
GET /api/calendar/me/alerts
POST /api/calendar/me/{workDate}/confirmations

GET calendar trả projection gọn. EmployeeId lấy từ authenticated identity; không nhận EmployeeId/UserId tùy ý. Confirmation route về module owner.

## 14. Application / Infrastructure

Application/Calendar: ICalendarModuleProvider, ICalendarRegistry, ICalendarPolicyService, ICalendarAggregator, CalendarContext, CalendarItemDto, CalendarDaySummaryDto, CalendarAlertItemDto.

Core: Entities/Calendar và Enums/Calendar.

Infrastructure/Services/Calendar: Registry, PolicyService, Aggregator và Providers/OtCalendarProvider, LeaveCalendarProvider, TripCalendarProvider.

API: CalendarController.

## 15. Authorization

Authenticated User → UserIdentity → EmployeeId → Calendar scope. Admin policy chỉ điều khiển projection, không cấp quyền business.

## 16. Idempotency

Cùng EmployeeId + WorkDate + ModuleCode + SourceId không tạo duplicate projection/alert. Rebuild không làm mất business history.

## 17. Background Processing

Không chạy calculation engine nặng khi mở calendar. Ưu tiên Source change → event/scheduled refresh → calendar projection refresh → GET /me. Nếu chưa materialize thì Aggregator đọc provider trực tiếp.

## 18. UI

Calendar gồm: Month Calendar → Alerts/Cần xác nhận → Detail Panel. Click đỏ mở nguyên nhân; click ? mở confirmation; click marker mở module detail.

## 19. Admin UI

Calendar Settings cần cho từng module: Enabled, Display, Note, Confirm, Reconcile, Priority. Chỉ hiển thị option module hỗ trợ.

## 20. Reporting

Report phân biệt Business source, Calendar projection, Confirmation và Final business result. Calendar projection không phải business transaction. OT report thêm OTMasterNo, Participant, RevisionNo, PreviousRevisionNo, ApprovedStart/End/Hours, ActualStart/End/Hours, FinalOTHours, EvidenceStatus.

## 21. Testing Matrix

Framework: disabled không xuất hiện; enabled gọi provider; module mới không sửa aggregator; nhiều module cùng ngày không overflow; duplicate idempotent; unauthorized không đọc employee khác; policy đổi thì projection đổi.

OT: Approved=Actual → Matched; khác → Mismatch; Actual không Approved → ActualWithoutApproval; Approved không Attendance → Pending/?; confirmation worked → evidence/HR; not worked → ConfirmedNotWorked; deadline → AutoConfirmedNotWorked; A revision không đổi B/C; reject A giữ R1.

Leave: Approved → marker; Used/Actual nếu policy bật; Calendar không đổi quota.

Trip: Approved → marker; Execution reconciliation nếu policy bật; Calendar không tự đổi approval.

## 22. Implementation Order

01 Freeze business contract → 02 enums/DTO/interfaces → 03 SQL definitions/policies → 04 EF entities/configurations → 05 Registry → 06 Policy Service → 07 OT Provider → 08 Leave Provider → 09 Trip Provider → 10 Aggregator → 11 API /me → 12 Alerts/confirmation routing → 13 UI → 14 Admin UI → 15 Projection/worker nếu cần → 16 Reports → 17 Integration tests.

Không bắt đầu bằng UI.

## 23. Definition of Done

- [ ] OT/Leave/Trip cùng Calendar Engine.
- [ ] Admin bật/tắt module.
- [ ] Admin cấu hình display/note/confirmation/reconciliation.
- [ ] Module mới plug-in không sửa aggregator.
- [ ] /me không spoof EmployeeId.
- [ ] Cell không over-information.
- [ ] Alert/confirmation nằm dưới lịch/detail.
- [ ] OT revision không ảnh hưởng participant khác.
- [ ] Attendance confirmation có audit.
- [ ] Projection không phải source of truth.
- [ ] Re-run không duplicate.
- [ ] Approval không bị Calendar bypass.
- [ ] Leave quota không bị Calendar thay đổi.
- [ ] Trip execution không bị Calendar tự quyết định.
- [ ] Reports phân biệt source/projection/final result.

## 24. Quy tắc Developer

Trước khi tạo table/class/service mới phải trả lời: (1) business data hay calendar projection; (2) source of truth ở module nào; (3) có dùng contract/provider hiện tại không; (4) Admin policy có quyết định behavior không; (5) có cần nằm trong ô lịch hay nên đưa xuống alert/detail.

Chỉ phản ánh → không tạo business entity mới. Business rule → module owner. Presentation → Calendar Policy. Dữ liệu tổng hợp đọc nhanh → Calendar Projection.

## 25. Tài liệu nguồn

01_FLOWS_TOTAL = flow tổng thể.
04_DASHBOARD = Calendar UX.
08_APPROVAL_ESCALATION = approval/scope.
17_OT_LEAVE_LIMITS = OT/Leave rules.
18_OT_ATTENDANCE_RECONCILIATION = OT reconciliation/confirmation.
19_REPORTS_STATISTICS = reporting.
20_WORK_CALENDAR_IMPLEMENTATION_BLUEPRINT = SQL + Code implementation source of truth.

Khi có mâu thuẫn, rà lại tài liệu nghiệp vụ gốc trước khi thay đổi contract. Không tự tạo behavior mới chỉ vì thuận tiện khi code.
