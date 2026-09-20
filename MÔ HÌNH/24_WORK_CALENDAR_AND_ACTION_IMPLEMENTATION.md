# 24 — SHARED WORK CALENDAR + ACTION + EXECUTION RECONCILIATION

> Implementation source-of-truth cho Calendar, Action/Task, Notification, Dashboard và generic Planned-vs-Actual reconciliation.

## 1. Kiến trúc

Business Module → Planned/Approved + Actual → Execution Reconciliation → Confirmation → Evidence/Review → Resolved → Action → Calendar/Task/Notification → Dashboard.

## 2. Shared rule

OT, Leave, Trip và module tương lai dùng cùng execution contract. Module chỉ cung cấp Planned/Approved source, Actual source, mapping trạng thái, execution policy, action types và detail route.

## 3. Core entities

- F03ExecutionPolicies: policy theo module.
- F03ExecutionReconciliations: Planned/Actual/status ở cấp ModuleCode + SourceId + EmployeeId + WorkDate.
- F03ExecutionConfirmations: quyết định và review của employee/assigned actor.
- F03ExecutionConfirmationEvidence: evidence và evidence review ngoài Action.
- F03ExecutionReconciliationHistory: audit lifecycle.

## 4. Calendar

Calendar chỉ projection/navigation. CalendarItem.RequiresAction = true thì dùng cùng ActionId với Task, Notification và Dashboard. Calendar cell không chứa approval history, evidence, revision history, participant list hoặc workflow dài.

## 5. Action

Action là shared work item, không phải business result. Action.Completed không có nghĩa business Approved/Matched.

Logical dedup: ModuleCode + SourceId/ReconciliationId + ActionType + AssignedToEmployeeId.

## 6. Notification

Notification là delivery/read state của Action hoặc business event; không lưu business workflow state.

## 7. Module mapping

| Module | Planned | Actual | Mismatch ví dụ |
|---|---|---|---|
| OT | Approved OT / effective revision | Attendance/OT actual | ApprovedWithoutActual |
| Leave | Approved Leave | Attendance | PlannedButActualDifferent |
| Trip | Approved Trip | Trip actual/check-in | PlannedWithoutActual / Different |
| Future | Module-defined | Module-defined | Module-defined |

## 8. Confirmation / Evidence

Không hard-code if ModuleCode == OT. Policy quyết định có confirmation, evidence, review, due date và auto-resolve hay không.

Nếu evidence bị Rejected hoặc NeedMoreEvidence thì Reconciliation chưa Resolved và Action phải Open/InProgress.

## 9. Participant isolation

Mọi reconciliation/action phải scoped theo employee. OT dùng OTMaster + Participant + EffectiveRevision + Employee. Leave/Trip nhiều người cũng phải độc lập theo participant nếu nghiệp vụ cho phép.

## 10. Background jobs

1. Attendance calculation/backfill.
2. Module Planned/Actual reconciliation.
3. Confirmation/action creation và dedup.
4. DueAt/expiry/escalation.
5. Notification delivery/retry.
6. Calendar projection refresh nếu materialized.

Tất cả job phải idempotent.

## 11. Implementation order

1. Generic execution SQL.
2. Core entities/configurations.
3. Application contracts/services.
4. OT mapping.
5. Leave mapping.
6. Trip mapping.
7. Shared Action integration.
8. Calendar integration.
9. Notification.
10. Dashboard.
11. Web.
12. Integration tests.

Không triển khai OT/Leave/Trip Web trước khi generic execution contract hoàn chỉnh.

## 12. Definition of Done

- OT/Leave/Trip dùng cùng reconciliation engine.
- Planned/Actual mismatch tạo đúng một reconciliation.
- Employee confirmation độc lập theo participant.
- Evidence nằm ngoài Action payload.
- Evidence review có thể giữ Action mở.
- Calendar/Task/Notification/Dashboard dùng cùng ActionId.
- Worker rerun không duplicate.
- Module mới tham gia bằng provider + policy + mapping.
- /me derive identity từ authenticated claims.
- Full solution build + integration tests pass.
