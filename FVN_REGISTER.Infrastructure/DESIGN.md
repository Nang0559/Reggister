# FVN_REGISTER.Infrastructure — Design & Architecture

## 1. Vai trò

`FVN_REGISTER.Infrastructure` là lớp triển khai các technical adapter của hệ thống.

Infrastructure sở hữu:

- EF Core `DbContext` và entity configuration;
- repository/unit-of-work implementation;
- database-specific query/write implementation;
- implementation của Application ports;
- email, file, notification và HRM integration;
- SignalR/transport adapter khi adapter đó cần technical framework;
- infrastructure-specific utilities.

Infrastructure **không chứa use-case orchestration** và không được trở thành nơi chứa business workflow thay cho Application.

## 2. Dependency rule

```text
API / Host
     ↓
Infrastructure ─────→ Application
     ↓                   ↓
 EF Core / DB         Contract
                         ↓
                        Core
```

Project references chuẩn:

```text
FVN_REGISTER.Infrastructure
 ├── FVN_REGISTER.Application
 ├── FVN_REGISTER.Models (Contract)
 └── FVN_REGISTER.Core
```

Infrastructure không reference:

- `FVN_REGISTER.API`;
- `FVN_REGISTER.Shared`;
- `FVN_REGISTER.Web`;
- `FVN_REGISTER.UI`.

Infrastructure được phép reference framework/package phục vụ implementation, ví dụ EF Core, SQL Server, Dapper, ClosedXML, ASP.NET Core SignalR.

## 3. Data boundary

### `Models/Data`

Chứa:

- `FVNWEBAPPContext`;
- EF Core model configuration discovery;
- database-specific persistence concerns.

`FVNWEBAPPContext` không được leak ra API/UI. API chỉ dùng Application service/orchestrator hoặc Application port.

### `Models/Configurations`

Mỗi entity mapping nên có `IEntityTypeConfiguration<TEntity>` riêng theo module/domain area.

`ApplyConfigurationsFromAssembly` là điểm đăng ký duy nhất của DbContext.

## 4. Repository boundary

`Repositories` triển khai repository abstraction từ Core/Application.

Repository được phép:

- `DbSet<T>`;
- `IQueryable<T>`;
- EF Core tracking/no-tracking;
- transaction và persistence operations.

Repository không được:

- trả DTO HTTP;
- gọi Controller;
- phụ thuộc UI;
- chứa workflow approval/leave/OT hoàn chỉnh.

## 5. Application port implementation

Infrastructure implement các interface được định nghĩa ở Application.

Ví dụ:

- approval providers/engine;
- history handlers/dispatcher;
- email service;
- notification service;
- user resolver;
- HRM synchronization;
- file service.

Implementation có thể dùng EF Core/entity trực tiếp, sau đó trả kết quả theo contract mà Application yêu cầu.

## 6. Result và DTO

Infrastructure có thể sử dụng:

```csharp
FVN_REGISTER.Contract.Utils.ServiceResult
FVN_REGISTER.Contract.Utils.PaginationResult<T>
```

Không tạo lại `ServiceResult`, `PaginationResult` hoặc DTO duplicate trong Infrastructure.

Không tạo namespace compatibility/alias để che lỗi namespace.

## 7. Mapping

Không sử dụng AutoMapper chỉ để thay thế vài mapping đơn giản.

Mapping entity → DTO được thực hiện explicit tại infrastructure/application boundary phù hợp.

Infrastructure không đưa EF entity trực tiếp sang UI/API nếu Application contract đã có DTO tương ứng.

## 8. Logging và configuration

Infrastructure có thể dùng:

- `ILogger<T>`;
- `IOptions<T>` / `IOptionsMonitor<T>`;
- configuration abstractions của Microsoft.Extensions.

Option thuộc Application thì đặt ở Application; Infrastructure-specific option phải nằm trong Infrastructure.

Không đưa configuration implementation xuống Core.

## 9. Transport adapters

Infrastructure có thể chứa technical adapters như SignalR khi adapter đó phục vụ notification/integration infrastructure.

Các type thuần transport không được đặt trong Core. Ví dụ `NotificationHubGroups` thuộc Infrastructure vì tên group SignalR là chi tiết transport.

Nếu một Hub trở thành HTTP endpoint boundary thuần túy, có thể chuyển Hub sang API ở bước hoàn thiện API mà không thay đổi Application ports.

## 10. Compatibility policy

Không giữ:

- `NamespaceCompatibility.cs`;
- duplicate entity namespace;
- duplicate enum namespace;
- legacy `Core.Configurations`/`Core.Logging` bridge;
- aliases để giữ code cũ compile.

Khi type đã được chuyển layer, mọi consumer phải dùng namespace canonical.

## 11. Quy tắc chất lượng

Infrastructure được coi là hoàn thiện khi:

- reference direction đúng;
- không reference API/UI/Web/Shared;
- không còn compatibility bridge;
- DbContext và configuration tập trung tại Infrastructure;
- Application ports được implementation đầy đủ;
- result types dùng Contract;
- không còn dependency legacy từ Core cho logging/configuration/result;
- không có AutoMapper/Newtonsoft dependency nếu không có consumer thực tế;
- build Infrastructure thành công cùng Core + Contract + Application.

## 12. Checklist trước khi chuyển sang API

```text
[ ] Core build OK
[ ] Contract build OK
[ ] Application build OK
[ ] Infrastructure project references đúng
[ ] DbContext canonical
[ ] EF configurations canonical
[ ] Repository/UoW canonical
[ ] Approval providers canonical
[ ] History handlers canonical
[ ] Notification/email/file adapters canonical
[ ] Không compatibility aliases
[ ] Không API/UI dependency
[ ] Infrastructure build OK
```


## 13. HRM attendance calculation adapter

Attendance calculation is a dedicated Infrastructure integration, separate from generic HRM master-data synchronization.

```text
API
 ↓
Application.Interfaces.HrmSync.IHrmAttendanceCalculationService
 ↓
Infrastructure.Services.HrmSync.HrmAttendanceCalculationService
 ↓
dbo.usp_CalculateHrmAttendance
 ↓
HRM.dbo.*  (READ ONLY)
 ↓
FVN_REGISTER.dbo.F03HrmAttendanceCalculated
FVN_REGISTER.dbo.F03HrmOTActual
```

Rules:

- HRM and FVN_REGISTER may reside on the same SQL Server; three-part names are used for direct cross-database reads.
- FVN does not execute the HRM UI.
- FVN does not write to `HRM.dbo.tblBaoCao`, `HRM.dbo.tblBaoCaoK` or other HRM result tables.
- The calculation procedure uses an FVN-local temporary calculation context and returns the calculated HRM-compatible result.
- HRM attendance rules/configuration remain in HRM source tables/procedure logic; FVN owns only the persisted calculation result.
- The calculation request is scoped by department and date range and receives a `CalculationBatchId` for traceability.
- A long SQL command timeout is intentional for batch calculation; the UnitOfWork timeout is restored after execution.
- Export services read FVN calculation results, never HRM report tables.

This adapter is the canonical runtime path for the **Tính giờ** use case.
