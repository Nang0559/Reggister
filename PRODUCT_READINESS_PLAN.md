# PRODUCT READINESS PLAN & MATRIX

> Mục đích: làm tài liệu kiểm soát duy nhất để đưa FVN_REGISTER từ trạng thái phát triển sang Product/Production.
>
> Phạm vi: **tài liệu thiết kế + SQL + Entity/Configuration + Service/Orchestrator + API + UI + phân quyền + background jobs + test + hướng dẫn sử dụng + vận hành production**.
>
> Nguyên tắc: **không đánh dấu hoàn thành một phase chỉ vì code đã tồn tại**. Một chức năng chỉ được xem là Product Ready khi các lớp liên quan khớp nhau và có bằng chứng kiểm chứng.

## 1. Quy ước tiến độ

| Ký hiệu | Ý nghĩa |
|---|---|
| ⬜ | Chưa làm |
| 🔵 | Đang làm |
| 🟡 | Có implementation nhưng cần rà/đối soát |
| 🟢 | Đã hoàn thành và có bằng chứng |
| 🔴 | Có lỗi/mismatch/blocker |
| ⚪ | Không áp dụng |

### Điều kiện hoàn thành một phase

Một phase chỉ chuyển sang 🟢 khi:

1. Code đúng với thiết kế hiện hành.
2. SQL deploy/verify đúng với code và thiết kế.
3. API/UI sử dụng đúng contract.
4. Phân quyền/scope không tạo bypass.
5. Hướng dẫn sử dụng không mô tả hành vi chưa tồn tại.
6. Có test hoặc bằng chứng kiểm thử tương ứng.
7. Không còn P0/P1 blocker thuộc phase.
8. Có commit SHA làm mốc.

---

# 2. Tổng quan các Phase

| Phase | Nội dung | Trạng thái | Gate chính |
|---|---|---:|---|
| 0 | System Reconciliation | 🔵 | Design ↔ SQL ↔ Code ↔ UI ↔ Guide |
| 1 | Build & Static Integrity | ⬜ | Restore/Build/Test sạch |
| 2 | Database Integrity | ⬜ | Deploy + Verify sạch |
| 3 | HRM Integration Certification | ⬜ | Employee/Position/Department/Attendance khớp HRM |
| 4 | Security Certification | ⬜ | RBAC + Scope + API enforcement |
| 5 | Approval Certification | ⬜ | Route + snapshot + approval lifecycle |
| 6 | Business Modules UAT | ⬜ | Leave/OT/Trip/Equipment end-to-end |
| 7 | OT Certification | ⬜ | Employee/Department/Block limits |
| 8 | Attendance Certification | ⬜ | HRM ↔ FVN calculation reconciliation |
| 9 | Notification & Background Jobs | ⬜ | Worker/queue/SignalR/email |
| 10 | Dashboard & Reports | ⬜ | Permission → provider → scope → data |
| 11 | Documentation Certification | ⬜ | User guide khớp UI/code |
| 12 | Production Hardening | ⬜ | Security/config/backup/observability |
| 13 | Release Candidate | ⬜ | Freeze + regression + UAT sign-off |
| 14 | Production Go-Live | ⬜ | Deploy + smoke test + rollback |

---

# 3. Master Product Readiness Matrix

## 3.1 Architecture / Cross-cutting

| Domain | Design | SQL | Entity/Config | Service | API | UI | Guide | Test | Status |
|---|---|---|---|---|---|---|---|---|---|
| Auth | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | ⬜ | 🟡 |
| RBAC | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | ⬜ | 🟡 |
| Scope | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | ⬜ | 🟡 |
| Approval | 🟡 | 🔴 | 🟡 | 🟢 | 🟡 | 🟡 | 🟡 | ⬜ | 🔴 |
| HRM Sync | 🟡 | 🟡 | 🟢 | 🟢 | ⚪ | 🟡 | 🟡 | ⬜ | 🟡 |
| Attendance | 🟢 | 🔴 | 🟢 | 🟢 | 🟡 | 🟡 | 🟢 | ⬜ | 🔴 |
| Work Calendar | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | ⬜ | 🟡 |
| Notification | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | ⬜ | 🟡 |
| Dashboard | 🔴 | 🟡 | 🟡 | 🟢 | 🟡 | 🟡 | 🟡 | ⬜ | 🟡 |
| Reports | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | ⬜ | 🟡 |
| Public/CMS | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | ⬜ | 🟡 |

## 3.2 Business Modules

| Module | Create | Edit | Submit | Approval | Reject | Cancel | History | Notification | Dashboard | Scope | Status |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Leave | 🟡 | 🟡 | 🟡 | 🟡 | ⬜ | ⬜ | 🟡 | 🟡 | 🟢 | 🟡 |
| Overtime | 🟢 | 🟡 | 🟢 | 🟡 | ⬜ | ⬜ | 🟡 | 🟡 | 🟢 | 🟡 |
| Trip | 🟡 | 🟡 | 🟡 | 🟡 | ⬜ | ⬜ | 🟡 | 🟡 | 🟢 | 🟡 |
| Equipment | 🟡 | 🟡 | 🟡 | 🟡 | ⬜ | ⬜ | 🟡 | 🟡 | 🟢 | 🟡 |

---

# 4. PHASE 0 — SYSTEM RECONCILIATION

## Mục tiêu

Thiết lập một mô hình chuẩn duy nhất. Không để tình trạng:

- tài liệu nói một mô hình;
- SQL triển khai mô hình khác;
- Entity dùng mô hình thứ ba;
- Service đã sửa nhưng verify script vẫn kiểm tra mô hình cũ;
- UI có control nhưng không gửi đúng DTO;
- User Guide mô tả chức năng chưa tồn tại.

## 4.1 Việc phải làm

### P0-01 — Approval canonical model

Chuẩn duy nhất:

`F03Employee.PositionCode → F03Positions → F03ApprovalPolicies → F03Approvers`

Kiểm tra:

- [ ] Không còn dependency runtime vào `F03ApprovalGroup`.
- [ ] Không còn `GroupCode` trong `F03ApprovalPolicies`.
- [ ] `PositionCode` là khóa định tuyến theo HRM.
- [ ] Policy hỗ trợ nhiều level cho cùng RequestType + PositionCode.
- [ ] Unique policy theo RequestType + PositionCode + Level.
- [ ] Approver assignment lấy từ F03Approvers.
- [ ] Requester không tự duyệt request của chính mình.
- [ ] Department scope của approver được kiểm tra.
- [ ] Route preview lấy Employee/Position từ server, không tin PositionCode client.
- [ ] Approval selection/snapshot được kiểm tra xuyên suốt từ preview → submit → persistence → approval.

**Blocker hiện biết:** `SQL/12_Verify.sql` còn kiểm tra mô hình ApprovalGroup cũ.

### P0-02 — Attendance canonical model

Chuẩn:

`HRM → HrmSync → F03HrmAttendanceCalculated → F03HrmOTActual → F03OTEmployees.ActualHours`

Kiểm tra:

- [ ] Không còn coi `usp_SyncOTActualHours` là pipeline canonical.
- [ ] SQL verify phản ánh pipeline mới.
- [ ] Worker startup không chặn request đầu tiên.
- [ ] Batch/idempotency rõ ràng.
- [ ] Có đối soát HRM ↔ FVN.

**Blocker hiện biết:** `SQL/12_Verify.sql` còn yêu cầu `usp_SyncOTActualHours`.

### P0-03 — Dashboard documentation

Kiểm tra:

- [ ] Provider thực tế trong code.
- [ ] Provider được đăng ký DI.
- [ ] Capability/permission gating.
- [ ] Scope filter.
- [ ] UI widget.
- [ ] Guide.

**Mismatch hiện biết:** `MÔ HÌNH/13_DASHBOARD_CAPABILITIES.md` chưa phản ánh đầy đủ Trip/Equipment providers đang có trong code.

### P0-04 — Approval DTO/UI/documentation

Đối soát:

- `ApprovalRoutePreviewDto`
- `ApprovalRouteLevelDto`
- `ApprovalSelectionDto`
- `ApprovalRouteSelector`
- Leave/OT/Trip/Equipment submit DTO
- persistence/snapshot
- `MÔ HÌNH/20_APPROVAL_ROUTE_SELECTION.md`

Đặc biệt kiểm tra `SelectedApproverCode` và mọi field selection có thực sự được lưu hay chỉ hiển thị.

### P0-05 — Leave contract

Kiểm tra:

- Top-level `LeaveTypeCode`
- Detail `LeaveTypeCode`
- Half-day option
- Server calculation của DayValue/IsCountedAsLeave
- Submit DTO
- Edit/rebuild details
- Approval route

Không để UI chọn loại nghỉ nhưng top-level DTO gửi giá trị khác detail.

### P0-06 — Trip / OT approval UI

Xác nhận trực tiếp:

- Trip gọi `GetPreviewAsync(RequestModule.Trip)`.
- OT gọi `GetPreviewAsync(RequestModule.Overtime)`.
- UI render `ApprovalRouteSelector`.
- Empty route có thông báo rõ ràng.
- Submit gửi đúng selection.

### P0-07 — User Guide reconciliation

Rà `MÔ HÌNH/16_USER_GUIDE.md` theo từng chức năng:

- mục đích;
- người sử dụng;
- permission;
- đường dẫn mở màn hình;
- bước thao tác;
- điều kiện;
- validation;
- kết quả;
- lỗi thường gặp;
- ví dụ.

Không sửa nghiệp vụ chỉ để làm guide khớp; nếu guide sai thì sửa guide, nếu implementation thiếu thì đưa vào backlog.

### P0-08 — Master evidence

Tạo evidence cho từng domain:

- file/code path;
- SQL script;
- endpoint;
- UI screen;
- test;
- commit SHA;
- ngày kiểm tra;
- người xác nhận UAT.

## Phase 0 Gate

- [ ] Không còn P0 mismatch.
- [ ] SQL verify khớp canonical architecture.
- [ ] Docs khớp implementation.
- [ ] DTO/UI contracts khớp.
- [ ] Matrix được cập nhật bằng evidence.
- [ ] Commit SHA được ghi lại.

---

# 5. PHASE 1 — BUILD & STATIC INTEGRITY

## Việc phải làm

- [ ] `dotnet restore`.
- [ ] `dotnet build` toàn solution.
- [ ] Build API.
- [ ] Build Application.
- [ ] Build Infrastructure.
- [ ] Build Models/Shared.
- [ ] Build Web.
- [ ] Build MAUI nếu nằm trong release scope.
- [ ] Chạy test project.
- [ ] Không còn compiler error.
- [ ] Không còn warning quan trọng liên quan DI/nullability/API contract.
- [ ] Kiểm tra dead public methods bị UI/API bỏ quên.
- [ ] Kiểm tra DI registration cho tất cả service/provider/worker.
- [ ] Kiểm tra configuration binding.

## Exit criteria

**Build xanh + test baseline xanh + không có blocker compile/runtime được biết.**

---

# 6. PHASE 2 — DATABASE INTEGRITY

## Việc phải làm

- [ ] Backup database trước migration.
- [ ] Chạy `SQL/00_Deploy_All.sql`.
- [ ] Chạy `SQL/99_Verify.sql`.
- [ ] Chạy verify từng module.
- [ ] Kiểm tra FK.
- [ ] Kiểm tra unique/index.
- [ ] Kiểm tra nullable/precision.
- [ ] Kiểm tra enum/status mapping.
- [ ] Kiểm tra procedure/function/view.
- [ ] Kiểm tra seed.
- [ ] Kiểm tra không còn legacy ApprovalGroup objects.
- [ ] Kiểm tra attendance objects đúng canonical pipeline.
- [ ] Kiểm tra idempotency keys/unique constraints.
- [ ] Kiểm tra transaction boundaries.

## Exit criteria

Database deploy được trên DB sạch và DB nâng cấp, verify không còn blocker.

---

# 7. PHASE 3 — HRM INTEGRATION CERTIFICATION

## Chuỗi dữ liệu chuẩn

`Department → Position → Employee → User → PositionCode → Approval`

## Việc phải làm

- [ ] Department sync.
- [ ] Position sync.
- [ ] Employee sync.
- [ ] Employee active/inactive.
- [ ] Employee transfer department.
- [ ] Employee change position.
- [ ] User mapping.
- [ ] PositionCode bắt buộc cho approval route.
- [ ] Approval route không lấy identity từ client.
- [ ] Sync retry.
- [ ] Sync idempotency.
- [ ] Initial sync.
- [ ] Scheduled sync.
- [ ] Failure/recovery.
- [ ] Audit sync.

## Exit criteria

HRM master data và FVN authorization/approval identity không mâu thuẫn.

---

# 8. PHASE 4 — SECURITY CERTIFICATION

## Role matrix

Phải kiểm thử tối thiểu:

- Employee
- Leader
- Chief
- Manager
- GM
- HR
- Admin
- SuperAdmin

## Scope matrix

- Own
- Employee
- Department
- All

## Việc phải làm

- [ ] UI ẩn/hiện đúng permission.
- [ ] API vẫn chặn khi gọi trực tiếp.
- [ ] Không trust EmployeeCode từ client.
- [ ] Không trust DepartmentCode từ client.
- [ ] Không trust PositionCode từ client.
- [ ] Export có permission riêng nếu thiết kế yêu cầu.
- [ ] Detail endpoint áp scope.
- [ ] List endpoint áp scope.
- [ ] Dashboard provider áp scope.
- [ ] Report áp scope.
- [ ] Approval endpoint kiểm tra quyền + route.
- [ ] Admin/SuperAdmin boundary.
- [ ] 401/403 behavior.
- [ ] Audit security-sensitive operations.

## Exit criteria

Không có privilege escalation hoặc scope bypass trong test matrix.

---

# 9. PHASE 5 — APPROVAL CERTIFICATION

## Route

`Employee.PositionCode → Policy → Level → Approver`

## Việc phải làm

- [ ] 1 level.
- [ ] Nhiều level.
- [ ] Nhiều policy cùng PositionCode nhưng khác Level.
- [ ] Department-specific approver.
- [ ] All-department approver.
- [ ] Không có approver.
- [ ] Approver inactive.
- [ ] Requester cũng là approver.
- [ ] Position change.
- [ ] Department change.
- [ ] Policy change.
- [ ] Approver change.
- [ ] Approval selection.
- [ ] Approval snapshot.
- [ ] Reject.
- [ ] Approve.
- [ ] Cancel.
- [ ] Duplicate approval action/idempotency.
- [ ] Notification.
- [ ] History/audit.

### Snapshot invariant

Sau khi request đã submit:

> Thay đổi Employee.PositionCode, Department, Policy hoặc Approver không được tự động thay đổi route của request cũ.

## Exit criteria

Approval route deterministic, auditable, idempotent và snapshot đúng.

---

# 10. PHASE 6 — BUSINESS MODULE UAT

## Leave

- [ ] Create.
- [ ] Edit.
- [ ] Draft.
- [ ] Submit.
- [ ] Full day.
- [ ] Half-day AM.
- [ ] Half-day PM.
- [ ] Leave type.
- [ ] Holiday.
- [ ] Balance.
- [ ] Approval.
- [ ] Reject.
- [ ] Cancel.
- [ ] History.
- [ ] Notification.
- [ ] Dashboard.

## Overtime

- [ ] Create.
- [ ] Multiple employees.
- [ ] Edit.
- [ ] Submit.
- [ ] Approval.
- [ ] Reject.
- [ ] Cancel.
- [ ] Actual hours.
- [ ] Weekly/monthly/yearly balance.
- [ ] Department/block limits.

## Trip

- [ ] Create.
- [ ] Edit.
- [ ] Submit.
- [ ] Approval.
- [ ] Reject.
- [ ] Cancel.
- [ ] History.
- [ ] Notification.
- [ ] Dashboard.

## Equipment

- [ ] Create.
- [ ] Edit.
- [ ] Submit.
- [ ] Approval.
- [ ] Reject.
- [ ] Cancel.
- [ ] History.
- [ ] Notification.
- [ ] Dashboard.

---

# 11. PHASE 7 — OT CERTIFICATION

## Rule scopes

- Employee.
- Department.
- Block.

## Periods

- Daily.
- Weekly.
- Monthly.
- Yearly.
- Special policy nếu doanh nghiệp áp dụng.

## Việc phải làm

- [ ] Current hours.
- [ ] Requested hours.
- [ ] Projected hours.
- [ ] Remaining.
- [ ] Percent used.
- [ ] Near-limit warning.
- [ ] Employee enforcement.
- [ ] Department aggregate enforcement.
- [ ] Block aggregate enforcement.
- [ ] Server-side enforcement.
- [ ] UI preview không phải enforcement duy nhất.
- [ ] Multi-employee request.
- [ ] Cross-department request.
- [ ] Active/inactive rule.
- [ ] Rule precedence được business sign-off.
- [ ] Pending request policy được business sign-off.
- [ ] Actual vs registered hours precedence được business sign-off.

## Baseline hiện có cần UAT xác nhận

- Monthly 40h.
- Yearly 200h.
- Special 300h nếu áp dụng.
- Weekly chưa seed nếu chưa có policy doanh nghiệp.

---

# 12. PHASE 8 — ATTENDANCE CERTIFICATION

## Test matrix

| Case | HRM | FVN | Expected |
|---|---|---|---|
| Normal day | ⬜ | ⬜ | Khớp |
| Off day | ⬜ | ⬜ | Khớp |
| Holiday | ⬜ | ⬜ | Khớp |
| Night shift | ⬜ | ⬜ | Khớp |
| Late | ⬜ | ⬜ | Khớp |
| Early leave | ⬜ | ⬜ | Khớp |
| Day OT | ⬜ | ⬜ | Khớp |
| Night OT | ⬜ | ⬜ | Khớp |
| Shift change | ⬜ | ⬜ | Khớp |
| Missing check-in/out | ⬜ | ⬜ | Có rule rõ |
| Leave | ⬜ | ⬜ | Khớp |
| Holiday + OT | ⬜ | ⬜ | Khớp |

## Việc phải làm

- [ ] CalculationBatchId.
- [ ] FromDate/ToDate.
- [ ] DeptCode.
- [ ] EmployeeCount.
- [ ] Calculated rows.
- [ ] Work hours.
- [ ] OT hours.
- [ ] Late/early.
- [ ] Retry/idempotency.
- [ ] Recalculation policy.
- [ ] Export/reconciliation report.

---

# 13. PHASE 9 — NOTIFICATION & BACKGROUND JOBS

## Workers

- [ ] HRM sync worker.
- [ ] Attendance calculation worker.
- [ ] Approval notification.
- [ ] Email queue.
- [ ] SignalR.
- [ ] Escalation nếu có.

## Production concerns

- [ ] Startup does not block API readiness.
- [ ] CancellationToken.
- [ ] Retry.
- [ ] Backoff.
- [ ] Idempotency.
- [ ] Error logging.
- [ ] Dead-letter/failure handling nếu cần.
- [ ] Health/readiness visibility.
- [ ] Duplicate notification prevention.

---

# 14. PHASE 10 — DASHBOARD & REPORTS

## Dashboard pipeline

`Permission → Capability → Provider → Query → Scope → Widget`

## Việc phải làm

- [ ] Provider registration.
- [ ] Permission gating.
- [ ] Scope.
- [ ] Query performance.
- [ ] Empty state.
- [ ] Error state.
- [ ] Loading state.
- [ ] Leave widget.
- [ ] OT widget.
- [ ] Trip widget.
- [ ] Equipment widget.
- [ ] Other enabled providers.
- [ ] Admin dashboard.
- [ ] Employee dashboard.
- [ ] Report filters.
- [ ] Export permission.
- [ ] Pagination.
- [ ] Date range.
- [ ] Department filter.

---

# 15. PHASE 11 — DOCUMENTATION CERTIFICATION

## Tài liệu phải rà

- [ ] Architecture docs.
- [ ] Approval route docs.
- [ ] Dashboard docs.
- [ ] Attendance docs.
- [ ] OT docs.
- [ ] User Guide.
- [ ] SQL deployment docs.
- [ ] SQL verification docs.
- [ ] Troubleshooting/runbook.

## Mỗi chức năng phải có

1. Mục đích.
2. Đối tượng sử dụng.
3. Permission.
4. Cách mở.
5. Các bước thao tác.
6. Điều kiện.
7. Validation.
8. Kết quả.
9. Lỗi thường gặp.
10. Ví dụ thực tế.

---

# 16. PHASE 12 — PRODUCTION HARDENING

## Security

- [ ] Secrets không nằm trong source.
- [ ] Production connection string.
- [ ] JWT.
- [ ] Refresh token.
- [ ] HTTPS.
- [ ] CORS.
- [ ] Cookie/token policy.
- [ ] Rate limit.
- [ ] Upload validation.
- [ ] Excel validation.
- [ ] SQL injection review.
- [ ] Authorization review.

## Operations

- [ ] Structured logging.
- [ ] Correlation ID.
- [ ] Audit.
- [ ] Health checks.
- [ ] Readiness/liveness.
- [ ] Metrics.
- [ ] Alerting.
- [ ] Error handling.
- [ ] Backup.
- [ ] Restore test.
- [ ] Rollback procedure.

---

# 17. PHASE 13 — RELEASE CANDIDATE

## Freeze policy

Sau khi bắt đầu RC:

Chỉ cho phép:

- P0/P1.
- Security.
- Data integrity.
- Production blocker.
- Regression fix.

Không thêm feature mới.

## RC checklist

- [ ] Build.
- [ ] SQL deploy.
- [ ] SQL verify.
- [ ] HRM sync.
- [ ] Attendance.
- [ ] Auth/RBAC.
- [ ] Scope.
- [ ] Approval.
- [ ] Leave.
- [ ] OT.
- [ ] Trip.
- [ ] Equipment.
- [ ] Notification.
- [ ] Dashboard.
- [ ] Reports.
- [ ] Export.
- [ ] User Guide.
- [ ] Backup/restore.
- [ ] Security test.
- [ ] UAT sign-off.

---

# 18. PHASE 14 — PRODUCTION GO-LIVE

## Trước deploy

- [ ] Production backup.
- [ ] Verify rollback package.
- [ ] Freeze database changes.
- [ ] Verify configuration.
- [ ] Verify secrets.
- [ ] Verify migration order.

## Deploy

- [ ] SQL migration.
- [ ] SQL verify.
- [ ] API.
- [ ] Web.
- [ ] Worker.
- [ ] Configuration.
- [ ] Health checks.

## Smoke test

- [ ] Login.
- [ ] Profile.
- [ ] Dashboard.
- [ ] Create request.
- [ ] Submit.
- [ ] Approval.
- [ ] Notification.
- [ ] Report.
- [ ] Export.
- [ ] HRM sync.

## Rollback

- [ ] Application rollback.
- [ ] Database rollback strategy.
- [ ] Feature flag/disable strategy.
- [ ] Communication procedure.

---

# 19. P0/P1 Backlog hiện tại

| ID | Severity | Issue | Phase | Status |
|---|---|---|---|---|
| P0-001 | P0 | `SQL/12_Verify.sql` còn ApprovalGroup model cũ | 0/2 | 🔴 |
| P0-002 | P0 | `SQL/12_Verify.sql` còn yêu cầu `usp_SyncOTActualHours` | 0/2/8 | 🔴 |
| P0-003 | P1 | Dashboard capability docs chưa phản ánh Trip/Equipment providers | 0/10/11 | 🟡 |
| P0-004 | P1 | Approval DTO/selection/snapshot cần đối soát end-to-end | 0/5 | 🟡 |
| P0-005 | P1 | Leave top-level LeaveTypeCode cần xác nhận với detail/submit | 0/6 | 🟡 |
| P0-006 | P1 | Trip/OT approval UI cần xác nhận preview + selector + submit | 0/5/6 | 🟡 |
| P0-007 | P1 | Full solution build/test chưa có evidence hoàn chỉnh | 1 | ⬜ |
| P0-008 | P1 | User Guide cần certification line-by-line với UI/code | 0/11 | 🟡 |
| P0-009 | P1 | HRM attendance cần UAT đối soát dữ liệu thực | 3/8 | ⬜ |
| P0-010 | P1 | Security matrix chưa có evidence đầy đủ | 4 | ⬜ |

---

# 20. Evidence Log

| Date | Phase | Item | Evidence | Result | Commit |
|---|---|---|---|---|---|
| 2026-09-20 | 0 | Approval canonical model | Code + SQL review | SQL verify mismatch | — |
| 2026-09-20 | 0 | Attendance canonical model | Code + SQL review | SQL verify mismatch | — |
| 2026-09-20 | 0 | Dashboard | Code + docs review | Docs lag implementation | — |
| 2026-09-20 | 0 | Leave | UI/DTO review | Need final submit reconciliation | — |

---

# 21. Change Log

| Date | Change |
|---|---|
| 2026-09-20 | Tạo Product Readiness Plan & Master Matrix. |
| 2026-09-20 | Ghi nhận P0/P1 mismatches cần xử lý trước Phase 1. |

---

# 22. Execution Order

Không làm theo kiểu sửa ngẫu nhiên. Thứ tự bắt buộc:

```text
PHASE 0
  ↓
Canonical reconciliation
  ↓
SQL Verify + Documentation + DTO/UI fixes
  ↓
PHASE 1
  ↓
Full Build/Test
  ↓
PHASE 2
  ↓
Database Deploy/Verify
  ↓
PHASE 3–10
  ↓
Business/UAT/Integration Certification
  ↓
PHASE 11
  ↓
Documentation Certification
  ↓
PHASE 12
  ↓
Production Hardening
  ↓
PHASE 13
  ↓
Release Candidate
  ↓
PHASE 14
  ↓
Production
```

**Rule:** nếu Phase 0 còn P0 blocker thì không tuyên bố Product Ready và không nhảy thẳng sang Go-Live.
