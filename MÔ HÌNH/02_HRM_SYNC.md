# HRM Sync & OT Attendance Reconciliation

## 1. Hai pipeline độc lập

### Pipeline A — HRM Master Data Sync

Đối tượng: `LeaveType`, `OTType`, `Department`, `Position`, `Employee`.

Đặc điểm: dữ liệu tương đối ít thay đổi, cần theo dõi Insert/Update/Delete và nguồn thay đổi.

```text
HRM DB
 ├─ Trigger (real-time, khi bảng cho phép)
 │       └─ F03StagingEmployee / F03StagingXxx
 │
 └─ IHrmStagingImporter (poll/backfill)
         └─ HrmImportWorker
               └─ F03StagingXxx

F03StagingXxx
   → HrmSyncJob<TStaging,TEntity>
   → F03* entity
```

`HrmSyncJob` không cần biết staging đến từ trigger hay importer.

### Pipeline B — OT Attendance Reconciliation

Pipeline này **không sử dụng** `IHrmStagingEntity/HrmChangeAction` của Pipeline A.

```text
HRM attendance
  → usp_SyncAttendanceStaging
  → F03AttendanceStaging
  → IOTAttendanceStagingService
  → IOTAttendanceReconciliationService
  → usp_SyncOTActualHours
  → F03OTEmployee.ActualHours / ActualStartTime / ActualEndTime
```

Mỗi lần reconciliation là một thao tác ghi dữ liệu; vì vậy đây là command-with-result, không phải query thuần.

## 2. HRM staging importer

`IHrmSourceReader<TSourceRow>` đọc dữ liệu HRM. Implementation SQL dùng Dapper và có thể truy vấn cross-database bằng three-part name như `[HRM].[dbo].[tblXxx]`.

`HrmStagingImporterBase<TSourceRow,TStaging,TEntity>` chịu trách nhiệm logic chung:

- `EntityType` xác định domain.
- `ImportAsync(asOfDate, ct)` là entry point.
- dọn stale pending staging cùng loại trước khi ghi batch mới;
- tạo staging `Update` cho source rows;
- so sánh source keys với existing keys để sinh `Delete` cho key bị mất;
- nếu số key mất vượt ngưỡng an toàn (tài liệu nguồn nêu `> 50%`) thì không phát Delete để tránh sự cố nguồn làm xóa hàng loạt.

Các hook:

```text
GetSourceKey(row)
GetEntityKeySelector()
MapToStaging(row)
BuildDeleteStaging(key)
```

## 3. Staging contract

Mọi `F03StagingXxx` dùng contract chung:

```text
IHrmStagingEntity
 ├─ Id
 ├─ EntityKey
 ├─ Action : HrmChangeAction
 ├─ IsProcessed
 ├─ ErrorMessage
 ├─ CreatedAt
 └─ CreatedBy
```

`Id` được đề xuất dùng làm tiebreaker khi `CreatedAt` trùng; trạng thái này vẫn cần xác nhận với model/database thực tế.

## 4. Generic HrmSyncJob

```text
IHrmSyncJob
  ├─ EntityType
  ├─ SyncOrder
  ├─ IsBlockingDependency
  └─ RunAsync(ct)

HrmSyncJob<TStaging,TEntity>
```

### Algorithm

1. Lấy staging chưa xử lý, order theo `CreatedAt`.
2. Group theo `EntityKey`.
3. Chọn bản mới nhất bằng `CreatedAt`, sau đó `Id` giảm dần.
4. Các bản cũ hơn được đánh dấu `IsProcessed=true` và `Superseded by newer change`.
5. Load entity hiện tại thành dictionary theo key.
6. Xử lý từng change:
   - `Delete` → `ApplyDelete` nếu entity tồn tại;
   - `Insert/Update` → tạo mới bằng `MapToNewEntity` hoặc cập nhật bằng `ApplyUpdate`;
   - lỗi từng dòng được ghi vào staging và `HrmSyncResult.Errors`.
7. Chạy `AfterBatchAsync`.
8. `SaveChangesAsync` phải được bọc xử lý lỗi; nếu transaction/batch save thất bại thì kết quả phải chuyển sang trạng thái lỗi, không để exception phá vỡ worker loop.

Hooks:

```text
LoadExistingEntitiesAsync(keys, ct)
MapToNewEntity(staging)
ApplyUpdate(entity, staging) : bool
ApplyDelete(entity, staging) : bool
AfterBatchAsync(batchContext, ct)
```

## 5. Sync order và dependency

Employee phụ thuộc Department/Position. Do đó job phải có thứ tự xác định thay vì phụ thuộc thứ tự DI registration.

```text
Department / Position
        ↓
     Employee
```

`IHrmSyncJobResolver.GetAll()` phải `OrderBy(j => j.SyncOrder)`.

Nếu một job có `IsBlockingDependency=true` và thất bại, `HrmSyncWorker` dừng chuỗi job còn lại; lỗi của từng job vẫn phải được catch/log riêng để worker không chết.

## 6. Source tracking

```csharp
// Giá trị minh họa theo tài liệu nguồn
enum SyncSourceTags
{
    Hrm,
    Manual
}
```

Quy tắc:

- HrmSyncJob ghi entity → `LastModifiedSource = Hrm`.
- Admin Create/Update/Toggle → `LastModifiedSource = Manual`.
- Delete thủ công bị chặn khi entity đang mang nguồn HRM.
- `ApplyUpdate` phải xác định entity từng bị chỉnh tay trước khi ghi đè.
- Khi HRM ghi đè thay đổi thủ công, `AfterBatchAsync` tạo `F03SyncReviewFlag`.

Tài liệu nguồn cảnh báo rằng nếu `Hrm=0` là default enum và có nơi quên set field, entity mới có thể bị hiểu nhầm là HRM. Cần xác nhận enum/model thực tế trước khi đổi giá trị.

## 7. Management Service

`CodeKeyedManagementService<TEntity,TKey,TDto,TUpsertDto>` cung cấp CRUD dùng chung cho domain có code key.

### Create

```text
check duplicate code
→ ToNewEntity
→ LastModifiedSource = Manual
→ Add
→ SaveChanges
```

### Update

```text
check duplicate code (trừ chính entity)
→ IsInUse guard nếu đổi code
→ ApplyUpsert
→ LastModifiedSource = Manual
→ SaveChanges
```

### Toggle

```text
SetIsActive(!current)
→ TouchModified
→ LastModifiedSource = Manual
```

Việc set `LastModifiedSource=Manual` khi Toggle là điểm cần giữ để HRM sync nhận biết thay đổi Admin.

### Delete

Chặn nếu:

- `LastModifiedSource == Hrm`;
- entity đang được tham chiếu (`IsInUseAsync`).

### SyncNow

Admin có thể yêu cầu sync một domain qua:

```text
IHrmSyncJobResolver.Resolve(EntityType)
→ job.RunAsync(ct)
```

Không tạo một engine sync thứ hai.

## 8. Review flags

```text
F03SyncReviewFlag
 ├─ EntityType
 ├─ EntityKey
 ├─ FlagType
 └─ Message
```

Các flag được nêu trong tài liệu:

- `ManualEditOverwrittenByHrm`
- `ManualDeactivationOverwrittenByHrm`
- `DeactivatedButStillReferenced`

`IsResolved/ResolvedAt/ResolvedBy` vẫn là mục cần xác nhận.

## 9. OT attendance staging

`F03AttendanceStaging` là staging riêng của Attendance, không dùng chung với `F03StagingEmployee`, `F03StagingPosition`, v.v.

Services:

```text
IOTAttendanceStagingService
  SyncAttendanceStagingAsync(workDate, ct)
  IsStagingReadyAsync(workDate, ct)

IOTAttendanceReconciliationService
  ReconcileActualHoursAsync(date, deptCode, ct)
```

Reconciliation service có thể tự kiểm tra staging và self-heal bằng cách sync staging trước khi gọi `usp_SyncOTActualHours`.

## 10. Worker

```text
HrmImportWorker (~1h)
  → IHrmStagingImporterResolver
  → ImportAsync từng importer

HrmSyncWorker (~30s)
  → IHrmSyncJobResolver
  → RunAsync theo SyncOrder
```

Một importer/job lỗi không được làm worker chết. Tuy nhiên failure của job blocking dependency có thể dừng phần còn lại của chuỗi để bảo vệ FK/dependency.

## 11. OT staging worker

`OTAttendanceStagingWorker` chạy theo lịch (tài liệu nêu 00:30) và **chỉ** gọi staging service.

Nó không tự động chạy reconciliation. Reconciliation chỉ được kích hoạt khi Admin chủ động yêu cầu.

## 12. Shared service

`IDepartmentLookupService.GetActiveDepartmentsAsync()` là cross-cutting service dùng chung cho Leave, OT, Trip và Dashboard; không đặt trong `OTAttendanceReconciliationService`.
