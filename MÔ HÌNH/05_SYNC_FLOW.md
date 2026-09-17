# HRM Sync — Chi tiết tầng và flow chuẩn

## 1. Tổng quan

`SynFlow.txt` mô tả HRM synchronization theo 6 tầng. Mô hình này tách rõ nguồn dữ liệu, staging, generic sync job, resolver, background worker và management service.

```text
Tầng 1  Source
   ↓
Tầng 2  Staging
   ↓
Tầng 3  Sync Job
   ↓
Tầng 4  Resolver
   ↓
Tầng 5  Worker
   ↓
Tầng 6  Management Service
```

## 2. Tầng 1 — Source

Có hai đường vào staging:

### Pipeline A — Trigger

```text
HRM Trigger
  → F03StagingEmployee
```

Dùng cho bảng có quyền đặt trigger, ưu tiên realtime.

### Pipeline B — Poll / backfill

```text
IHrmSourceReader<TSourceRow>
  → HrmStagingImporterBase<TSourceRow,TStaging,TEntity>
  → F03StagingXxx
```

Importer đọc source, tạo Update staging và diff key để tạo Delete staging cho key biến mất.

Có guard chống xóa hàng loạt khi missing keys bất thường; tài liệu nguồn sử dụng ngưỡng `> 50% existingKeySet`.

## 3. Tầng 2 — Staging

Staging là **điểm hội tụ duy nhất**. `HrmSyncJob` không phân biệt trigger hay importer.

### Contract

```text
IHrmStagingEntity
 ├─ Id
 ├─ EntityKey : string
 ├─ Action : HrmChangeAction
 ├─ IsProcessed : bool
 ├─ ErrorMessage : string?
 ├─ CreatedAt : DateTime
 └─ CreatedBy : string
```

`Id` được dùng làm tiebreaker khi timestamp trùng; cần xác nhận schema thật.

## 4. Tầng 3 — Generic Sync Job

### Contract

```text
IHrmSyncJob
 ├─ EntityType : string
 ├─ SyncOrder : int
 ├─ IsBlockingDependency : bool
 └─ RunAsync(ct) : Task<HrmSyncResult>
```

### Result

```text
HrmSyncResult
 ├─ TotalSource
 ├─ Added
 ├─ Updated
 ├─ Deactivated
 ├─ Unchanged
 ├─ Superseded
 ├─ Errors : List<string>
 ├─ Success
 └─ Summary
```

`Superseded` là phần bổ sung cần có để phản ánh việc deduplicate staging. Logic `Success` vẫn cần xác nhận trong code thực tế.

### Processing algorithm

```text
pending = !IsProcessed
   ↓
OrderBy(CreatedAt)
   ↓
GroupBy(EntityKey)
   ↓
latest = CreatedAt DESC, Id DESC
   ↓
mark older rows Superseded
   ↓
LoadExistingEntitiesAsync(keys)
   ↓
process latest rows
   ├─ Delete → ApplyDelete
   └─ Insert/Update
        ├─ entity null → MapToNewEntity
        └─ entity exists → ApplyUpdate
   ↓
AfterBatchAsync
   ↓
SaveChangesAsync
```

Mỗi staging row cần được xử lý lỗi độc lập để ghi `ErrorMessage` và đưa lỗi vào result. `SaveChangesAsync` phải được bảo vệ; nếu persistence thất bại, result không được báo success giả.

## 5. Domain job

Ví dụ `PositionHrmSyncJob`:

- `EntityType = "Position"`;
- `IsBlockingDependency = true` khi domain là dependency của Employee;
- tạo mới từ HRM phải đánh dấu `LastModifiedSource = Hrm`;
- Update so sánh field trước khi ghi;
- phân biệt **đổi tên** và **reactivate**, tránh review message sai ngữ nghĩa;
- Delete HRM thực hiện soft-deactivate (`IsActive=false`), giữ source là HRM;
- nếu entity vẫn được tham chiếu thì ghi review flag.

## 6. Tầng 4 — Resolver

Có hai resolver:

```text
IHrmSyncJobResolver
  Resolve(entityType)
  GetAll()

IHrmStagingImporterResolver
  Resolve(entityType)
  GetAll()
```

Implementation build dictionary từ `IEnumerable<T>` đã đăng ký DI.

`GetAll()` của sync job phải sort theo:

```csharp
OrderBy(j => j.SyncOrder)
```

Không phụ thuộc thứ tự registration của DI.

`Resolve()` phải báo lỗi rõ ràng khi không tồn tại `EntityType`.

## 7. Tầng 5 — Workers

### `HrmImportWorker`

```text
resolver.GetAll()
  → từng importer
  → ImportAsync(asOfDate, ct)
```

Lỗi của một importer không chặn importer khác.

### `HrmSyncWorker`

```text
resolver.GetAll()  // đã sort SyncOrder
  → RunAsync(ct)
  → failure + IsBlockingDependency ? break : continue
```

Worker phải catch/log riêng từng job và không để một exception làm chết vòng polling.

## 8. Tầng 6 — Management Service

`CodeKeyedManagementService<TEntity,TKey,TDto,TUpsertDto>` cung cấp CRUD generic.

### Hooks

```text
GetCode
GetIsActive
SetIsActive
TouchModified
ToDto
ToNewEntity
ApplyUpsert
KeyEqualsExpr
KeyNotEqualsExpr
CodeEqualsExpr
ApplyActiveFilter
ApplyDefaultOrder
IsInUseAsync
```

### Source tracking trong CRUD

```text
CreateAsync
  → LastModifiedSource = Manual

UpdateAsync
  → ApplyUpsert
  → LastModifiedSource = Manual

ToggleActiveAsync
  → SetIsActive
  → TouchModified
  → LastModifiedSource = Manual
```

Điểm `ToggleActiveAsync` phải set `Manual` là quan trọng: nếu thiếu, HRM sync không biết Admin vừa can thiệp.

### Delete

```text
if LastModifiedSource == Hrm → block
if IsInUseAsync → block
else → delete
```

### SyncNow

Admin sync một domain bằng chính resolver/job:

```text
IHrmSyncJobResolver.Resolve(EntityType)
  → RunAsync(ct)
```

Không tạo một `IHrmSyncEngine` riêng cho Admin.

## 9. Source precedence

```text
                 ┌─ HRM ──→ LastModifiedSource = Hrm
Entity change ───┤
                 └─ Admin ─→ LastModifiedSource = Manual
```

Nếu HRM ghi đè thay đổi Admin, cần review flag thay vì silently overwrite.

## 10. Review queue

```text
F03SyncReviewFlag
 ├─ EntityType
 ├─ EntityKey
 ├─ FlagType
 └─ Message
```

Các loại flag:

```text
ManualEditOverwrittenByHrm
ManualDeactivationOverwrittenByHrm
DeactivatedButStillReferenced
```

Query contract:

```text
IHrmSyncReviewQueryService
  GetUnresolvedAsync
  CountUnresolvedAsync
  ResolveAsync
  ResolveByEntityAsync
```

Các field resolution (`IsResolved`, `ResolvedAt`, `ResolvedBy`) vẫn cần xác nhận.

## 11. Dependency ordering

Employee có FK tới Department/Position, vì vậy thứ tự phải được xác định bằng `SyncOrder`.

```text
Department ─┐
            ├─→ Employee
Position ───┘
```

Nếu Department/Position là blocking job và thất bại, worker không tiếp tục Employee trong cùng chu kỳ.

## 12. Invariants cần bảo vệ

1. Staging là boundary giữa source và sync engine.
2. Generic job không biết source đến từ đâu.
3. Job không phụ thuộc DI registration order.
4. Admin CRUD và HRM sync không dùng chung mutation path.
5. Mọi Admin mutation phải ghi `Manual`.
6. Mọi HRM mutation phải ghi `Hrm`.
7. HRM overwrite manual change phải tạo review flag.
8. Delete không được phép xóa entity đang thuộc source HRM.
9. Blocking dependency failure phải ngăn job phụ thuộc chạy tiếp.
10. Worker exception phải được cô lập, không làm worker chết.
