# HRM Sync — Chi tiết tầng và flow chuẩn

## 1. Tổng quan — từ kiến trúc đến implementation

`SynFlow.txt` mô tả HRM synchronization theo 6 tầng. Đây là tài liệu chi tiết nhất của pipeline sync.

```mermaid
flowchart TB
    S1[Tầng 1 — Source] --> S2[Tầng 2 — Staging]
    S2 --> S3[Tầng 3 — Generic Sync Job]
    S3 --> S4[Tầng 4 — Resolver]
    S4 --> S5[Tầng 5 — Worker]
    S5 --> S6[Tầng 6 — Management Service]
    S6 --> DB[(Master Data)]
    S6 -. review .-> RF[F03SyncReviewFlag]
```

## 2. Tầng 1 — Source

### 2.1 Trigger / importer

```mermaid
flowchart LR
    HRM[(HRM DB)] --> TR[HRM Trigger]
    TR --> ES[F03StagingEmployee]
    HRM --> R[IHrmSourceReader<TSourceRow>]
    R --> I[HrmStagingImporterBase<TSourceRow,TStaging,TEntity>]
    I --> SX[F03StagingXxx]
```

Có hai đường vào staging:

- Trigger cho bảng có quyền đặt trigger, ưu tiên realtime.
- Poll/backfill qua `IHrmSourceReader` + `HrmStagingImporterBase`.

Importer tạo Update staging và diff key để tạo Delete staging cho key biến mất. Có guard chống xóa hàng loạt khi missing keys bất thường; ngưỡng nguồn nêu `> 50% existingKeySet`.

## 3. Tầng 2 — Staging

Staging là **điểm hội tụ duy nhất**. `HrmSyncJob` không phân biệt trigger hay importer.

```mermaid
flowchart LR
    TR[Trigger] --> ST[IHrmStagingEntity]
    IMP[Importer] --> ST
    ST --> P[Pending queue]
    P --> JOB[Generic Sync Job]
```

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

### 4.1 Contract

```mermaid
classDiagram
    class IHrmSyncJob {
        +string EntityType
        +int SyncOrder
        +bool IsBlockingDependency
        +RunAsync(ct)
    }
    class HrmSyncJob~TStaging,TEntity~
    IHrmSyncJob <|.. HrmSyncJob~TStaging,TEntity~
```

### 4.2 Processing algorithm

```mermaid
flowchart TD
    P[Pending = !IsProcessed] --> O[OrderBy CreatedAt]
    O --> G[GroupBy EntityKey]
    G --> L[Latest: CreatedAt DESC, Id DESC]
    L --> SP[Mark older rows Superseded]
    SP --> E[LoadExistingEntitiesAsync]
    E --> C{Action}
    C -->|Delete| D[ApplyDelete]
    C -->|Insert / Update| X{Entity exists?}
    X -->|No| N[MapToNewEntity]
    X -->|Yes| U[ApplyUpdate]
    D --> AB[AfterBatchAsync]
    N --> AB
    U --> AB
    AB --> SAVE[SaveChangesAsync]
    SAVE --> RESULT[HrmSyncResult]
```

Mỗi staging row cần xử lý lỗi độc lập để ghi `ErrorMessage` và đưa lỗi vào result. `SaveChangesAsync` phải được bảo vệ; nếu persistence thất bại, result không được báo success giả.

### 4.3 Result

```text
HrmSyncResult
 ├─ TotalSource
 ├─ Added
 ├─ Updated
 ├─ Deactivated
 ├─ Unchanged
 ├─ Superseded
 ├─ Errors
 ├─ Success
 └─ Summary
```

Logic `Success` vẫn cần xác nhận trong code thực tế.

## 5. Domain job

Ví dụ `PositionHrmSyncJob`:

```mermaid
flowchart LR
    ST[Position staging] --> J[PositionHrmSyncJob]
    J --> MAP[Map / ApplyUpdate]
    MAP --> SRC[LastModifiedSource = Hrm]
    J --> ACT[Soft deactivate]
    ACT --> REF{Still referenced?}
    REF -->|Yes| FLAG[F03SyncReviewFlag]
    REF -->|No| DONE[Completed]
```

Các quy tắc domain:

- `EntityType = "Position"`;
- dependency của Employee phải có `SyncOrder` phù hợp;
- tạo mới từ HRM đánh dấu `LastModifiedSource = Hrm`;
- Update so sánh field trước khi ghi;
- phân biệt đổi tên và reactivate;
- Delete HRM thực hiện soft-deactivate (`IsActive=false`);
- nếu entity vẫn được tham chiếu thì ghi review flag.

## 6. Tầng 4 — Resolver

```mermaid
flowchart LR
    DI[DI IEnumerable<IHrmSyncJob>] --> R[IHrmSyncJobResolver]
    R --> MAP[Dictionary<EntityType, Job>]
    MAP --> ONE[Resolve(entityType)]
    MAP --> ALL[GetAll()]
    ALL --> SORT[OrderBy SyncOrder]
```

Có hai resolver:

```text
IHrmSyncJobResolver
IHrmStagingImporterResolver
```

Implementation build dictionary từ `IEnumerable<T>` đã đăng ký DI. `GetAll()` phải sort theo `SyncOrder`; `Resolve()` phải báo lỗi rõ ràng khi không tồn tại `EntityType`.

## 7. Tầng 5 — Workers

```mermaid
sequenceDiagram
    participant W as HrmSyncWorker
    participant R as IHrmSyncJobResolver
    participant J as Sync Jobs

    W->>R: GetAll()
    R-->>W: Jobs ordered by SyncOrder
    loop Each job
        W->>J: RunAsync(ct)
        alt Success
            J-->>W: HrmSyncResult.Success
        else Non-blocking failure
            J-->>W: Error
            W->>W: Log + continue
        else Blocking failure
            J-->>W: Error + IsBlockingDependency
            W->>W: Log + break chain
        end
    end
```

### `HrmImportWorker`

`resolver.GetAll()` → từng importer → `ImportAsync(asOfDate, ct)`. Lỗi của một importer không chặn importer khác.

### `HrmSyncWorker`

`resolver.GetAll()` đã sort `SyncOrder` → `RunAsync(ct)` → failure + blocking ? break : continue. Worker phải catch/log riêng từng job.

## 8. Tầng 6 — Management Service

```mermaid
flowchart TD
    ADMIN[Admin] --> M[CodeKeyedManagementService<TEntity,...>]
    M --> V[Validation / IsInUse]
    V --> C{Operation}
    C -->|Create| CR[ToNewEntity + Manual]
    C -->|Update| UP[ApplyUpsert + Manual]
    C -->|Toggle| TO[SetIsActive + TouchModified + Manual]
    C -->|Delete| DE{Hrm source or in use?}
    DE -->|Yes| BLOCK[Reject]
    DE -->|No| DEL[Delete]
    CR --> DB[(Persistence)]
    UP --> DB
    TO --> DB
    DEL --> DB
```

Hooks: `GetCode`, `GetIsActive`, `SetIsActive`, `TouchModified`, `ToDto`, `ToNewEntity`, `ApplyUpsert`, `KeyEqualsExpr`, `KeyNotEqualsExpr`, `CodeEqualsExpr`, `ApplyActiveFilter`, `ApplyDefaultOrder`, `IsInUseAsync`.

Mọi Admin mutation phải set `LastModifiedSource = Manual`.

## 9. SyncNow — không tạo engine thứ hai

```mermaid
flowchart LR
    ADMIN[Admin Sync Now] --> R[IHrmSyncJobResolver.Resolve(EntityType)]
    R --> J[Existing HrmSyncJob]
    J --> RUN[RunAsync(ct)]
    RUN --> RESULT[HrmSyncResult]
```

Admin sync một domain bằng chính resolver/job hiện hữu; không tạo `IHrmSyncEngine` riêng.

## 10. Source precedence và review queue

```mermaid
flowchart TD
    M[Entity mutation] --> S{Source}
    S -->|HRM| H[LastModifiedSource = Hrm]
    S -->|Admin| A[LastModifiedSource = Manual]
    H --> O{Overwrites manual change?}
    O -->|Yes| F[F03SyncReviewFlag]
    O -->|No| OK[Normal sync]
    F --> Q[IHrmSyncReviewQueryService]
    Q --> UI[Admin Review]
```

Flag types:

```text
ManualEditOverwrittenByHrm
ManualDeactivationOverwrittenByHrm
DeactivatedButStillReferenced
```

Các field resolution (`IsResolved`, `ResolvedAt`, `ResolvedBy`) vẫn cần xác nhận.

## 11. Dependency ordering

```mermaid
flowchart LR
    D[Department] -->|FK dependency| E[Employee]
    P[Position] -->|FK dependency| E
    D --> DSO[SyncOrder]
    P --> PSO[SyncOrder]
    DSO --> E
    PSO --> E
```

Nếu Department/Position là blocking job và thất bại, worker không tiếp tục Employee trong cùng chu kỳ.

## 12. Invariants cần bảo vệ

```mermaid
flowchart TD
    A[Architecture invariants]
    A --> I1[Staging = source/sync boundary]
    A --> I2[Generic job source-agnostic]
    A --> I3[Resolver controls SyncOrder]
    A --> I4[Admin CRUD != HRM mutation path]
    A --> I5[Admin => Manual]
    A --> I6[HRM => Hrm]
    A --> I7[HRM overwrite => Review Flag]
    A --> I8[HRM-owned delete blocked]
    A --> I9[Blocking failure stops dependent jobs]
    A --> I10[Worker exceptions isolated]
```

1. Staging là boundary giữa source và sync engine.
2. Generic job không biết source đến từ đâu.
3. Job không phụ thuộc DI registration order.
4. Admin CRUD và HRM sync không dùng chung mutation path.
5. Mọi Admin mutation ghi `Manual`.
6. Mọi HRM mutation ghi `Hrm`.
7. HRM overwrite manual change tạo review flag.
8. Delete không được phép xóa entity đang thuộc source HRM.
9. Blocking dependency failure ngăn job phụ thuộc chạy tiếp.
10. Worker exception không làm worker chết.
