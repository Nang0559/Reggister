# PRODUCT READINESS PLAN & TRACKING

> **Repository:** `Nang0559/Reggister`  
> **Branch:** `feature/work-calendar-action-implementation`  
> **Baseline:** `95cee16d7edca1f0610680db21b33a1c23709a41`  
> **Mục tiêu:** đưa hệ thống từ trạng thái development/integration sang Release Candidate và Production bằng một quy trình có kiểm soát, trong đó **thiết kế → SQL → Entity/Configuration → Service → API → UI → User Guide → Test → Production** phải cùng một contract.

---

## 1. Nguyên tắc quản lý tiến độ

### 1.1. Không đánh dấu hoàn thành chỉ vì code đã tồn tại

Một hạng mục chỉ được `DONE` khi đã kiểm tra được các lớp liên quan:

1. Design/architecture.
2. SQL schema/procedure/index/seed/verify.
3. Entity/configuration.
4. Application/service/orchestrator.
5. API/controller/authorization.
6. UI/validation/UX.
7. User Guide/help.
8. Test/runtime evidence.
9. Production concern nếu hạng mục ảnh hưởng vận hành.

### 1.2. Trạng thái

| Ký hiệu | Trạng thái | Ý nghĩa |
|---|---|---|
| ⬜ | TODO | Chưa thực hiện |
| 🔵 | IN PROGRESS | Đang thực hiện |
| 🟡 | BLOCKED | Có blocker hoặc cần business decision |
| 🟠 | NEED VERIFY | Có implementation nhưng chưa đủ evidence |
| 🟢 | DONE | Đã đối chiếu và có evidence |
| 🔴 | FAILED | Đã kiểm tra và phát hiện lỗi |

### 1.3. Gate

Không chuyển phase nếu còn P0 chưa xử lý.

- **P0:** sai contract, sai dữ liệu, sai security, sai approval, SQL verification sai, production blocker.
- **P1:** chức năng chính chưa hoàn chỉnh hoặc documentation/UI lệch.
- **P2:** cải thiện UX, tối ưu, hardening không chặn core flow.

---

# 2. Master Progress

| Phase | Tên phase | Mục tiêu | Tiến độ | Gate |
|---|---|---|---:|---|
| 0 | System Reconciliation | Đồng bộ design/SQL/code/UI/guide/test | 🔵 IN PROGRESS | P0 reconciliation |
| 1 | Build & Static Integrity | Full solution restore/build/test compile | ⬜ TODO | Build clean |
| 2 | Database Integrity | Deploy + verify canonical DB | ⬜ TODO | SQL verify PASS |
| 3 | HRM Integration | HRM master + user + approver + attendance | ⬜ TODO | HRM data certified |
| 4 | Security Certification | RBAC + data scope + API enforcement | ⬜ TODO | Security test PASS |
| 5 | Approval Certification | Route + selection + snapshot + engine | ⬜ TODO | Approval invariant PASS |
| 6 | Business UAT | Leave/OT/Trip/Equipment end-to-end | ⬜ TODO | UAT PASS |
| 7 | OT Certification | Employee/Department/Block limits | ⬜ TODO | Limit matrix PASS |
| 8 | Attendance Certification | HRM-compatible attendance/OT actual | ⬜ TODO | HRM comparison PASS |
| 9 | Notification & Jobs | Worker/queue/SignalR/email/escalation | ⬜ TODO | Operational test PASS |
| 10 | Dashboard & Reports | Permission/provider/query/scope/export | ⬜ TODO | Report/dashboard PASS |
| 11 | Documentation Certification | Guide/help/design match runtime | ⬜ TODO | Docs PASS |
| 12 | Production Hardening | Secrets/security/backup/logging/deployment | ⬜ TODO | Production checklist PASS |
| 13 | Release Candidate | Freeze + regression + final evidence | ⬜ TODO | RC GO |
| 14 | Production Go-Live | Deploy + smoke + rollback readiness | ⬜ TODO | GO-LIVE |

---

# 3. Phase 0 — System Reconciliation

## Mục tiêu

Tạo một contract duy nhất cho toàn hệ thống. Không sửa business tùy tiện; chỉ sửa những điểm đã xác nhận là lệch giữa tài liệu, SQL, code, UI hoặc guide.

### 0.1 Master matrix

| Domain | Design | SQL | Entity/Config | Service | API | UI | Guide | Test | Status |
|---|---|---|---|---|---|---|---|---|---|
| Auth / Session | 🟡 | 🟡 | 🟡 | 🟡 | 🟢 | 🟡 | 🟡 | ⬜ | 🟡 |
| RBAC / Permission | 🟢 | 🟢 | 🟢 | 🟢 | 🟢 | 🟠 | 🟠 | ⬜ | 🟠 |
| HRM Employee/Dept/Position | 🟢 | 🟢 | 🟢 | 🟢 | 🟢 | 🟠 | 🟠 | ⬜ | 🟠 |
| Approval Policy | 🟢 | 🟢 | 🟢 | 🟢 | 🟢 | 🟠 | 🟢 | ⬜ | 🟡 |
| Approval Selection | 🟢 | 🟢 | 🟢 | 🟢 | 🟢 | 🟠 | 🟢 | ⬜ | 🟡 |
| Approval Snapshot/Engine | 🟢 | 🟢 | 🟢 | 🟢 | 🟢 | 🟠 | 🟢 | ⬜ | 🟡 |
| Leave | 🟢 | 🟢 | 🟢 | 🟢 | 🟢 | 🟠 | 🟠 | ⬜ | 🟠 |
| OT | 🟢 | 🟢 | 🟢 | 🟢 | 🟢 | 🟠 | 🟢 | ⬜ | 🟠 |
| Trip | 🟢 | 🟢 | 🟢 | 🟢 | 🟠 | 🟠 | 🟠 | ⬜ | 🟠 |
| Equipment | 🟢 | 🟢 | 🟢 | 🟢 | 🟠 | 🟠 | 🟠 | ⬜ | 🟠 |
| HRM Sync | 🟢 | 🟢 | 🟢 | 🟢 | 🟢 | 🟠 | 🟠 | ⬜ | 🟠 |
| Attendance | 🟢 | 🟢 | 🟢 | 🟢 | 🟢 | 🟠 | 🟢 | ⬜ | 🟠 |
| Work Calendar | 🟢 | 🟢 | 🟢 | 🟢 | 🟢 | 🟠 | 🟢 | ⬜ | 🟠 |
| Notification | 🟢 | 🟢 | 🟢 | 🟢 | 🟢 | 🟠 | 🟠 | ⬜ | 🟠 |
| Dashboard | 🟢 | 🟢 | 🟢 | 🟢 | 🟢 | 🟠 | 🟢 | ⬜ | 🟡 |
| Reports | 🟢 | 🟢 | 🟢 | 🟢 | 🟢 | 🟠 | 🟢 | ⬜ | 🟠 |
| CMS/Public Information | 🟢 | 🟢 | 🟢 | 🟢 | 🟢 | 🟠 | 🟢 | ⬜ | 🟠 |

> Các ô `🟢` ở trên là **baseline từ các artefact hiện có**, không thay thế runtime evidence. Các ô `🟠` bắt buộc được xác nhận trong Phase 0/1/2.

## 0.2 P0 reconciliation items

| ID | Hạng mục | File/Layer | Trạng thái |
|---|---|---|---|
| P0-001 | SQL verification còn kiểm tra legacy ApprovalGroup model | `SQL/12_Verify.sql` | 🟢 DONE |
| P0-002 | SQL verification còn yêu cầu `usp_SyncOTActualHours` | `SQL/12_Verify.sql` | 🟢 DONE |
| P0-003 | Reconcile dashboard documentation với provider thực tế | `MÔ HÌNH/13_DASHBOARD_CAPABILITIES.md` | 🟢 DONE |
| P0-004 | Reconcile Approval DTO/selection/snapshot với docs | `MÔ HÌNH/20_...`, Application, UI | 🟠 NEED VERIFY |
| P0-005 | Xác nhận Leave top-level `LeaveTypeCode` và detail cùng contract | Leave UI + DTO/service | 🟢 DONE |
| P0-006 | Xác nhận Trip/OT UI gọi route preview và render selector | UI | 🟠 NEED VERIFY |
| P0-007 | Xác nhận guide không mô tả chức năng chưa expose ở UI | `MÔ HÌNH/16_USER_GUIDE.md` | 🟠 NEED VERIFY |
| P0-008 | Xác nhận toàn bộ legacy approval objects/columns/service/data-source/DI đã bị loại bỏ hoặc không còn được runtime sử dụng | SQL + code + DI | 🟢 SQL DONE / 🟠 RUNTIME VERIFY |
| P0-009 | History/Cancel không được đọc hoặc ghi F03ApprovalSteps; phải dùng Snapshot + ApprovalHistory | BaseHistoryHandler + ApprovalEngine | 🟢 FIXED / 🟠 BUILD VERIFY |
| P0-010 | History detail Leave/OT không được NotImplementedException | LeaveHistoryHandler + OTHistoryHandler | 🟢 FIXED / 🟠 BUILD VERIFY |
| P0-011 | History phải có handler runtime cho Leave/OT/Trip/Equipment | HistoryDispatcher + handlers + DI | 🟢 FIXED / 🟠 BUILD VERIFY |

## 0.3 Deliverables

- [x] Tạo tài liệu này.
- [x] Canonical SQL verification.
- [x] Dashboard documentation reconciliation.
- [x] Approval contract reconciliation.
- [x] Leave/Trip/OT UI reconciliation.
- [x] User Guide reconciliation.
- [x] Cập nhật matrix sau mỗi fix.
- [x] Xác nhận `SQL/12_Verify.sql` hiện đã kiểm tra canonical Approval + Attendance contract.
- [x] Xác nhận `MÔ HÌNH/13_DASHBOARD_CAPABILITIES.md` đã mô tả Trip/Equipment là provider thực tế, còn runtime certification ở Phase 10.
- [x] Xác nhận ApprovalRouteController chỉ truyền EmployeeCode; PositionCode/DeptCode được resolve server-side.
- [ ] Runtime verify toàn bộ UI/submit/snapshot contract.

---

# 4. Phase 1 — Build & Static Integrity

### Tasks

- [ ] `dotnet restore`.
- [ ] `dotnet build` toàn solution.
- [ ] Build API.
- [ ] Build Application.
- [ ] Build Infrastructure.
- [ ] Build Models/Shared.
- [ ] Build Web.
- [ ] Build MAUI nếu nằm trong solution/release scope.
- [ ] Fix compile errors trước khi đổi business logic.
- [ ] Kiểm tra DI registrations.
- [ ] Kiểm tra nullable/reference mismatches.
- [ ] Kiểm tra DTO namespace/signature mismatch.
- [ ] Kiểm tra dead controller/service methods.
- [ ] Kiểm tra warnings có khả năng trở thành production bug.

### Exit criteria

- Full solution build PASS.
- Không còn compile blocker.
- Các warning quan trọng được phân loại.

---

# 5. Phase 2 — Database Integrity

### Deploy

- [ ] Backup database test/UAT.
- [ ] Chạy `SQL/00_Deploy_All.sql`.
- [ ] Chạy `SQL/99_Verify.sql`.
- [ ] Chạy các verify chuyên module.
- [ ] Kiểm tra thứ tự deployment.

### Schema

- [ ] Canonical PK/Id.
- [ ] FK.
- [ ] Unique indexes.
- [ ] Decimal precision.
- [ ] Active flags.
- [ ] Audit columns.
- [ ] Legacy tables/columns.
- [ ] Stored procedure contract.
- [ ] Views.
- [ ] Functions.
- [ ] Triggers.
- [ ] Seed.

### Exit criteria

- Deploy PASS.
- Verify PASS.
- Không còn verification script kiểm tra contract đã bị loại bỏ.

---

# 6. Phase 3 — HRM Integration Certification

### Master data

- [ ] Department sync.
- [ ] ParentDeptCode.
- [ ] Position sync.
- [ ] PositionCode HRM `000x`.
- [ ] Employee sync.
- [ ] EmployeeNo.
- [ ] EmployeeCode.
- [ ] Employee → Department.
- [ ] Employee → Position.
- [ ] Employee → User.

### Security/approval provisioning

- [ ] Employee → User reconciliation.
- [ ] Employee PositionCode → ApprovalPolicy.
- [ ] Employee PositionCode → Approver provisioning.
- [ ] Active/inactive employee behavior.

### Attendance

- [ ] Attendance staging.
- [ ] HRM-compatible calculation.
- [ ] Actual OT.
- [ ] Calculation batch.
- [ ] Re-run/idempotence.
- [ ] Review flags.

### Exit criteria

- Master data certified.
- Approval identity chain certified.
- Attendance comparison baseline established.

---

# 7. Phase 4 — Security Certification

### Authentication

- [ ] Login.
- [ ] JWT.
- [ ] Refresh/session.
- [ ] Logout/session termination.
- [ ] Expired session.
- [ ] Disabled user.

### Authorization

Test at least:

| Actor | Expected scope |
|---|---|
| Employee | Own |
| Leader | configured team/department |
| Chief | configured scope |
| Manager | configured scope |
| GM | configured scope |
| HR | HR scope |
| Admin | configured administrative scope |
| SuperAdmin | system-wide administrative scope |

### API security

- [ ] 401 unauthenticated.
- [ ] 403 unauthorized.
- [ ] Data scope enforced server-side.
- [ ] Export permission independent from View.
- [ ] Approve permission independent from View.
- [ ] No client-supplied DepartmentCode/PositionCode can widen scope.
- [ ] Audit permission/security changes.

### Exit criteria

- Direct API security tests PASS.
- No scope bypass.

---

# 8. Phase 5 — Approval Certification

## Route

- [ ] Requester identity comes from authenticated EmployeeCode.
- [ ] PositionCode comes from persisted HRM employee.
- [ ] PositionCode resolves to F03Positions.
- [ ] Policy resolves by RequestType + PositionCode.
- [ ] Multiple levels supported.
- [ ] Sequence preserved.
- [ ] Required enforced.
- [ ] Candidate resolution via F03Approvers.
- [ ] Department candidate before ALL fallback.
- [ ] Requester excluded.
- [ ] No FirstOrDefault auto-selection when multiple candidates.

## Selection

- [ ] One candidate per required level.
- [ ] Auto-select when exactly one candidate.
- [ ] Mandatory selection before Submit.
- [ ] Selection stored in F03ApprovalSelections.

## Snapshot

- [ ] Snapshot created at submit.
- [ ] Selected approver copied to snapshot.
- [ ] Existing request unaffected by later policy/employee/approver changes.
- [ ] ApprovalEngine reads snapshot, not live route.

## Modules

- [ ] Leave.
- [ ] OT.
- [ ] Trip.
- [ ] Equipment.

### Exit criteria

Approval route is deterministic and immutable after Submit.

---

# 9. Phase 6 — Business UAT

For each module:

**Create → Draft → Edit → Preview → Submit → Approval → Reject/Approve → Cancel → History → Notification → Dashboard → Report**

### Leave

- [ ] Full day.
- [ ] Half day AM.
- [ ] Half day PM.
- [ ] Leave type.
- [ ] Holiday.
- [ ] Balance.
- [ ] Upcoming approved leave.
- [ ] Invalid date range.
- [ ] Scope.

### OT

- [ ] Single employee.
- [ ] Multiple employees.
- [ ] Same department.
- [ ] Same block.
- [ ] Daily/weekly/monthly/yearly validation.
- [ ] Approval.
- [ ] Actual vs registered.

### Trip

- [ ] Create/edit/submit.
- [ ] Date validation.
- [ ] Approval.
- [ ] History.
- [ ] Scope.

### Equipment

- [ ] Schema.
- [ ] Excel.
- [ ] Staging.
- [ ] Validation.
- [ ] Preview.
- [ ] Commit.
- [ ] Approval.
- [ ] Audit.

---

# 10. Phase 7 — OT Certification

## Rule dimensions

- [ ] Employee.
- [ ] Department.
- [ ] Block.
- [ ] Active rule only.
- [ ] Rule precedence documented and approved.
- [ ] Daily.
- [ ] Weekly.
- [ ] Monthly.
- [ ] Yearly.
- [ ] Special/maximum policy.

## Required UAT

Example:

```text
Department MAY01
Current = 30h
Request = 12h
Limit = 40h
Projected = 42h
Expected = server rejects
```

Also test:

- [ ] Employee passes but department fails.
- [ ] Department passes but block fails.
- [ ] Multiple employees in same request.
- [ ] Multiple requests submitted concurrently.
- [ ] Pending request policy.
- [ ] Actual OT precedence.
- [ ] Boundary exactly at limit.
- [ ] Boundary above limit.

### Business signoff required

- [ ] Daily limit.
- [ ] Weekly limit.
- [ ] Monthly 40h baseline.
- [ ] Yearly 200h baseline.
- [ ] Special 300h policy.
- [ ] Pending request consumption.
- [ ] Actual vs registered precedence.
- [ ] Rule precedence.

---

# 11. Phase 8 — Attendance Certification

Compare FVN calculation against HRM for controlled samples:

- [ ] Normal day.
- [ ] Off day.
- [ ] Holiday.
- [ ] Night shift.
- [ ] Late.
- [ ] Early leave.
- [ ] Day OT.
- [ ] Night OT.
- [ ] Shift change.
- [ ] Missing check-in.
- [ ] Missing check-out.
- [ ] Leave.
- [ ] Holiday/leave overlap.

For each sample retain:

- EmployeeCode.
- Date.
- Shift.
- HRM source values.
- FVN calculated values.
- Difference.
- Expected reason.
- Resolution.

### Exit criteria

No unexplained difference in agreed certification set.

---

# 12. Phase 9 — Notification & Background Jobs

- [ ] HRM sync worker.
- [ ] Attendance calculation worker.
- [ ] Email queue.
- [ ] Retry.
- [ ] Dead/failure state.
- [ ] Approval notification.
- [ ] SignalR notification.
- [ ] Escalation.
- [ ] Duplicate prevention.
- [ ] Idempotence.
- [ ] Startup behavior.
- [ ] Graceful shutdown.

### Startup requirement

HRM sync must not block first dashboard/request startup.

---

# 13. Phase 10 — Dashboard & Reports

## Dashboard

- [ ] DashboardView.
- [ ] Provider capability.
- [ ] Provider query scope.
- [ ] Leave.
- [ ] OT.
- [ ] Trip.
- [ ] Equipment.
- [ ] Notification.
- [ ] Approval.
- [ ] Department statistics.
- [ ] Administration/system health.

## Reports

- [ ] Leave.
- [ ] OT.
- [ ] Trip.
- [ ] Equipment.
- [ ] Attendance.
- [ ] Date filter.
- [ ] Department filter.
- [ ] Employee filter.
- [ ] Server-side scope.
- [ ] Pagination.
- [ ] Export permission.
- [ ] Export data scope.

---

# 14. Phase 11 — Documentation Certification

Mỗi chức năng phải có:

1. Purpose.
2. Who can use.
3. Permission.
4. Navigation.
5. Input.
6. Validation.
7. Workflow.
8. Approval.
9. Result.
10. Error handling.
11. Example.
12. Scope/security notes.

### Documents

- [ ] Architecture docs.
- [ ] SQL docs.
- [ ] Approval docs.
- [ ] OT docs.
- [ ] Attendance docs.
- [ ] Dashboard docs.
- [ ] Reports docs.
- [ ] User Guide.
- [ ] Approval Selection Guide.
- [ ] HRM Attendance Guide.
- [ ] Work Calendar Guide.
- [ ] CMS/Help.

### Rule

Không ghi trong guide một behavior mà UI/API runtime chưa chứng minh.

---

# 15. Phase 12 — Production Hardening

- [ ] Production connection string/secrets.
- [ ] JWT secret/key rotation policy.
- [ ] HTTPS.
- [ ] CORS.
- [ ] Secure cookies/token storage.
- [ ] File upload validation.
- [ ] Excel upload limits.
- [ ] Rate limiting nếu cần.
- [ ] Exception handling.
- [ ] Structured logging.
- [ ] Audit retention.
- [ ] PII/log redaction.
- [ ] Email configuration.
- [ ] SignalR production configuration.
- [ ] Worker configuration.
- [ ] Database backup.
- [ ] Database restore drill.
- [ ] Monitoring.
- [ ] Health checks.
- [ ] Alerting.
- [ ] Rollback package.

---

# 16. Phase 13 — Release Candidate

Feature freeze.

Chỉ cho phép:

- P0 bug.
- P1 production blocker.
- Security fix.
- Data integrity fix.
- Deployment blocker.

### RC checklist

- [ ] Full build.
- [ ] SQL deploy.
- [ ] SQL verify.
- [ ] HRM sync.
- [ ] Auth.
- [ ] RBAC.
- [ ] Scope.
- [ ] Approval.
- [ ] Leave.
- [ ] OT.
- [ ] Trip.
- [ ] Equipment.
- [ ] Attendance.
- [ ] Notification.
- [ ] Dashboard.
- [ ] Reports.
- [ ] Export.
- [ ] User Guide.
- [ ] Backup/restore.
- [ ] Security test.
- [ ] UAT signoff.

---

# 17. Phase 14 — Production Go-Live

## Pre-deploy

- [ ] Production backup.
- [ ] Verify backup restore point.
- [ ] Freeze source/version.
- [ ] Confirm deployment package.
- [ ] Confirm SQL version.
- [ ] Confirm rollback plan.
- [ ] Confirm support owner.

## Deployment

1. Database backup.
2. SQL deployment.
3. SQL verification.
4. API deployment.
5. Web deployment.
6. Worker deployment.
7. Configuration validation.
8. Health check.

## Smoke test

- [ ] Login.
- [ ] Profile.
- [ ] Dashboard.
- [ ] Create Leave.
- [ ] Create OT.
- [ ] Approval.
- [ ] Notification.
- [ ] Report.
- [ ] Export.
- [ ] HRM sync.
- [ ] Attendance.

## Rollback

- [ ] DB rollback/restore strategy.
- [ ] Application rollback package.
- [ ] Configuration rollback.
- [ ] Communication plan.

---

# 18. Definition of Product Ready

Hệ thống chỉ được đánh dấu **PRODUCT READY** khi:

- [ ] Phase 0 không còn P0/P1 reconciliation blocker.
- [ ] Phase 1 full build PASS.
- [ ] Phase 2 SQL deploy/verify PASS.
- [ ] Phase 3 HRM certification PASS.
- [ ] Phase 4 security certification PASS.
- [ ] Phase 5 approval certification PASS.
- [ ] Phase 6 business UAT PASS.
- [ ] Phase 7 OT certification PASS.
- [ ] Phase 8 attendance certification PASS.
- [ ] Phase 9 workers/notifications PASS.
- [ ] Phase 10 dashboard/reports PASS.
- [ ] Phase 11 documentation PASS.
- [ ] Phase 12 production hardening PASS.
- [ ] Phase 13 RC PASS.
- [ ] Phase 14 go-live smoke test PASS.

---

# 19. Change Log

| Date | Phase | Change | Commit |
|---|---|---|---|
| 2026-09-20 | 0 | Created Product Readiness master plan | `555d54d79b553f53ec34ade68ba53381893beda3` |
| 2026-09-20 | 0 | Reconciled SQL verification contract | `5f5c39286697c97f0c41c03a556908523c6bd058` |
| 2026-09-20 | 0 | Reconciled dashboard capability documentation | `e978d2aae5ce8d30ecadad2360203eb09fb282f4` |
| 2026-09-20 | 0 | Fixed Leave type contract in create UI | `c00d6d8580f932b7d94a85ea471f58d85b9c8b7a` |
| 2026-09-20 | 0 | Fixed Trip approval route loading and missing-route UX | `761e2ebf4ebc2a53a9e894dc7740158710b1d1b0`, `11367c22bc50117c7877d6bcfe7f3dc3b22a3548` |


### Phase 1 Evidence Update — 2026-09-25

#### Static review fixes applied on the current work-calendar branch

1. Retired RequestModule.AccessChange references removed.
   - RequestModule no longer contains AccessChange.
   - Removed the stale enum reference from ApprovalPolicyService.
   - Removed stale AccessChange mappings from RequestModuleExtensions.
   - This was a concrete compile blocker introduced by the earlier enum cleanup.

2. OT join endpoint scope hardened.
   - OTService.JoinAsync now requires OTCreate data-scope authorization for the current employee before adding participation.
   - Knowing an OT request id is no longer sufficient to mutate that request.

3. OT employee-add deduplication fixed.
   - AddEmployeesAsync now normalizes the submitted employee list case-insensitively and ignores employees already active in the request.
   - TotalOTHours is calculated from the deduplicated candidate set, preventing duplicate submitted employee codes from inflating the total.

4. Leave detail mutation hardened.
   - LeaveService.CancelDetailAsync now blocks removal of a leave detail once the request has entered the approval route.
   - This prevents changing the request detail while an existing approval snapshot/history can still represent the previous request content.

#### Current evidence status

- Verified by source inspection: the above static defects are corrected on the current branch.
- NEED BUILD: full solution compile is still required after the retired enum cleanup.
- NEED RUNTIME: approval snapshot immutability, cancellation/action reconciliation, notification idempotence, and IDOR/scope tests still require runtime evidence.
- NEED SQL VERIFY: deploy/verify must be run against the target database; source inspection alone is not a deployment certificate.

### Phase 1 Evidence Update — 2026-09-20

Đã bắt đầu Phase 1 trên branch `feature/security-rbac-dashboard`.

#### Static integrity fixes đã áp dụng

1. **Anonymous auth debug endpoint**
   - Xóa `GET api/auth/test-auth` khỏi `AuthController`.
   - Endpoint cũ trả claims identity trong khi gắn `[AllowAnonymous]`; đây không phải runtime contract cần thiết của sản phẩm.

2. **Nullable identity guards**
   - Đã loại bỏ các `UserInfo!` dereference ở các mutation endpoint sau:
     - `UserManagementController`: Create, Update, Delete, ToggleLock, ResetPassword.
     - `ApprovalPoliciesController`: Create, Update.
     - `HrmUserRoleRulesController`: Create, Update.
     - `EmailTemplateController`: Save, Toggle.
   - Các action mutation hiện phải có authenticated `UserInfo` trước khi truyền `UserId` xuống service.

3. **Auth refresh contract**
   - Backend hiện có `POST api/auth/refresh`.
   - Client vẫn chưa hoàn tất refresh pipeline: `ITokenStorage` hiện chỉ lưu access token, chưa có refresh-token storage; `AuthHeaderHandler` chưa retry 401 bằng refresh.
   - Vì vậy **không đánh dấu refresh là DONE**. Đây là P1 còn lại của Phase 1.

#### Phase 1 items cần tiếp tục

- [ ] Full `dotnet restore`.
- [ ] Full solution build.
- [ ] Build API/Application/Infrastructure/Models/Shared/Web.
- [ ] DI verification.
- [ ] DTO namespace/signature verification.
- [ ] Complete refresh-token client pipeline.
- [ ] Complete dead-interface/service cleanup only after repository-wide reference verification.
- [ ] Warning classification.
- [ ] Runtime verification after successful build.

#### Current Phase 1 blockers

- **P1:** refresh token chưa được nối từ client đến backend.
- **P1:** dead service/interface candidates cần repository-wide reference evidence trước khi xóa.
- **NEED VERIFY:** full solution build chưa có evidence trên branch.


---

## 20. Current execution order

**Không nhảy phase.**

```text
PHASE 0
  ↓
PHASE 1
  ↓
PHASE 2
  ↓
PHASE 3
  ↓
PHASE 4
  ↓
PHASE 5
  ↓
PHASE 6/7/8
  ↓
PHASE 9
  ↓
PHASE 10
  ↓
PHASE 11
  ↓
PHASE 12
  ↓
PHASE 13
  ↓
PHASE 14
```

### Current focus

> **PHASE 1 — Build & Static Integrity**

Việc tiếp theo sau khi cập nhật tài liệu là xử lý từng P0/P1 có evidence, sau đó mới chạy full build ở Phase 1.


---

# 21. Phase 0 Evidence Update — 2026-09-20

### Verified directly on `feature/security-rbac-dashboard`

1. **SQL/12_Verify.sql**
   - Đã là canonical verification.
   - Kiểm tra `F03ApprovalPolicies.PositionCode`.
   - Kiểm tra FK `FK_F03ApprovalPolicies_F03Positions`.
   - Kiểm tra unique `UX_F03ApprovalPolicies_Request_Position_Level`.
   - Chặn `GroupCode`, `ApprovalGroupCode`, `RequesterPositionCode`.
   - Chặn `F03ApprovalPositionGroups`.
   - Chặn `F03ApprovalGroups`.
   - Chặn `usp_SyncOTActualHours`.
   - Kiểm tra `usp_SyncAttendanceStaging`.

   **Kết luận:** hai blocker SQL trước đây không còn là blocker trên branch hiện tại.

2. **SQL/99_Verify.sql**
   - Đồng nhất với attendance canonical pipeline.
   - Tiếp tục chặn legacy `usp_SyncOTActualHours`.

3. **SQL/17_ApprovalRouteSelection.sql**
   - Canonical route là `F03Employees.PositionCode → F03Positions → F03ApprovalPolicies → F03Approvers`.
   - Có `F03ApprovalSelections`.
   - Không tạo `F03ApprovalPositionGroups`.
   - Policy unique theo RequestType + PositionCode + Level.

4. **MÔ HÌNH/13_DASHBOARD_CAPABILITIES.md**
   - Đã ghi nhận Leave, OT, Trip, Equipment providers.
   - Trip/Equipment vẫn cần runtime certification ở Phase 10.

5. **MÔ HÌNH/20_APPROVAL_ROUTE_SELECTION.md**
   - Đã mô tả Selection → Snapshot → ApprovalEngine.
   - Đã mô tả immutable snapshot invariant.
   - API response documentation vẫn cần đối soát với DTO runtime thực tế.

6. **ApprovalRouteController**
   - Chỉ nhận EmployeeCode từ authenticated user.
   - Không nhận PositionCode/DeptCode từ caller.
   - Authorization function được map theo RequestModule.

7. **ApprovalRouteService**
   - Resolve active F03Employee theo EmployeeCode.
   - Resolve PositionCode từ F03Employee.
   - Resolve policy theo RequestType + PositionCode.
   - Resolve candidates từ F03Approvers.
   - Loại requester khỏi candidates.
   - Ưu tiên candidate theo department, fallback ALL.
   - Không tự chọn approver bằng FirstOrDefault.

### Phase 0 conclusion

**Không còn evidence cho P0-001/P0-002 là blocker trên branch hiện tại.**

Các mục cần tiếp tục trước Phase 1:

- Runtime verification của ApprovalSelection → Snapshot → ApprovalEngine.
- Runtime verification Trip/OT UI.
- Build verification sau History/Cancel migration và 4-module History registration.
- Full User Guide certification.
- Legacy object/code/DI search ở toàn bộ runtime, đặc biệt pending-list legacy abstractions.
- Sau khi Phase 0 evidence đủ: chuyển Phase 1 và chạy full restore/build/test.


### Phase 0 Evidence Update — legacy pending-list runtime cleanup

- Đã xác nhận trên branch `feature/security-rbac-dashboard` rằng pending inbox runtime hiện dùng `ApprovalInboxService.GetPendingAsync` và các approval workflow/engine `GetPendingForApproverAsync`.
- Đã loại bỏ các legacy pending-list artifacts còn tồn tại trong runtime tree:
  - `Application/Interfaces/Approvals/IApprovalListDataSource.cs`
  - `FVN_REGISTER.Infrastructure/Services/Leaves/LeaveApprovalListDataSource.cs`
  - `FVN_REGISTER.Infrastructure/Services/OT/OTApprovalListDataSource.cs`
  - hai DI registrations tương ứng trong `FVN_REGISTER.API/Program.cs`
- `ApprovalListService` generic đã được xóa trước đó; sau cleanup này, tree search không còn file khớp `ApprovalListService` / `IApprovalListDataSource` / `*ApprovalListDataSource`.
- `IApprovalListClientService` và `ApprovalListController` **không được coi là legacy pending-list engine**: chúng là HTTP/UI façade hiện tại và controller gọi trực tiếp `IApprovalInboxService`.
- Cập nhật comment của `IApprovalInboxService` để không còn mô tả một `ApprovalListService` generic đã bị loại bỏ.

**Commits:** `5e94e8136b6ca86d367ac97ed2ec3d0775d1eab0`, `f2fb10de6efe8753d3b56cfdf24a2f3552f8750b`, `b14cd5c07553489bf97f6f1a3e800ade7ad4297a`, `f4213418af8dcc5a1b2b561e13bf7d66a8464184`, `c50eda061f40a8caec2780e19536cb89f00ab5ae`.


### Phase 0 Evidence Update — History / Cancel canonicalization

Đã xác nhận BaseHistoryHandler<TRequest> trước đây đọc trực tiếp Db.ApprovalSteps (F03ApprovalSteps) cho cả History timeline và Cancel. Đây là legacy branch, không còn phù hợp với pipeline Selection → Snapshot → ApprovalEngine.

Đã sửa:

- History timeline chuyển sang F03ApprovalSnapshots + F03ApprovalStepSnapshots + F03ApprovalHistories, dùng chung ApprovalStepMapper với ApprovalEngine.
- CancelAsync không còn mutate F03ApprovalSteps. Cancellation được biểu diễn bởi trạng thái request; snapshot approval steps giữ immutable contract.
- ApprovalEngine.GetPendingForApproverAsync bổ sung kiểm tra subject.OverallStatus, loại request đã Cancelled/Approved/Rejected khỏi pending inbox ngay cả khi snapshot còn step Pending.
- LeaveHistoryHandler.GetDetailAsync đã implement.
- OTHistoryHandler.GetDetailAsync đã implement.
- Bổ sung TripHistoryHandler và EquipmentHistoryHandler.
- Đăng ký đủ 4 handlers trong DI.
- HistoryController chấp nhận leave, ot, trip, equipment.

**Important:** chưa đánh dấu Phase 0 DONE. Cần Phase 1 build/runtime evidence để xác nhận các thay đổi compile và endpoint/UI contract hoạt động thực tế.


### Master-data display/transport convention — 2026-09-25

Đã chuẩn hóa nguyên tắc dùng chung cho Department/Position/Employee và các lookup tương tự:

- **Value/request/DB key luôn là Code**: `DeptCode`, `PositionCode`, `EmployeeCode`.
- **UI hiển thị phải có Mã + Tên**: `CODE — NAME`; không dùng Name làm value/key.
- DTO lookup phải mang đủ cặp `Code + Name` khi UI cần hiển thị; không suy ra identity từ tên.
- Approval route tiếp tục canonical theo `F03Employees.PositionCode → F03Positions`.
- Đã rà và sửa Employee Management, Employee/Approver picker, Approver management và Security Center 2FA; bổ sung `PositionCode` vào `EmployeeSelectDto` và lookup tên phòng ban/chức vụ cho danh sách quản trị 2FA.
