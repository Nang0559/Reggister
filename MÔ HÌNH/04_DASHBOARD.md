# Dashboard Aggregation

## 1. Mục tiêu

Dashboard là read-only aggregation layer. Nó tổng hợp pending approvals và đóng góp dashboard từ từng module, sau đó trả một `DashboardResponse` cho Blazor client.

## 2. Luồng request

```text
DashboardPage.razor / Home.razor
        │ OnInitializedAsync
        ▼
IDashboardClientService
        │
        ▼
DashboardClientService
        │ HTTP GET api/dashboard
        ▼
DashboardController
        ▼
IDashboardOrchestrator
        ▼
DashboardOrchestrator
        ▼
DashboardResponse
        ▼
DashboardOverview.razor / UserDashboard.razor
```

## 3. DashboardOrchestrator

`DashboardOrchestrator` chịu trách nhiệm điều phối, không trực tiếp truy cập `DbContext`.

Các bước chính:

1. lấy pending approvals đa module từ approval infrastructure;
2. lặp qua collection `IModuleDashboardProvider`;
3. lấy contribution của từng module;
4. áp dụng `DashboardWidgetPolicy` để lọc/sắp xếp widget theo user;
5. dựng `DashboardResponse`.

## 4. Module Dashboard Provider

Contract:

```text
IModuleDashboardProvider
    GetContributionAsync(user, ct)
        → Task<ModuleDashboardContribution>
```

Mỗi module tự chịu trách nhiệm biết cách lấy dữ liệu dashboard của nó.

Các implementation được tài liệu hóa:

```text
LeaveDashboardProvider
  → ILeaveQueryService + IStatisticsService

OTDashboardProvider
  → IOTQueryService + ...

TripDashboardProvider
  → CHƯA BUILD; thêm khi Trip có Query service
```

Provider được đăng ký DI dạng collection.

### Nguyên tắc mở rộng

Thêm module mới:

```text
new XxxDashboardProvider
       +
DI registration
```

Không sửa `DashboardOrchestrator` chỉ để biết module mới.

## 5. Contribution model

```text
ModuleDashboardContribution
 ├─ Module : RequestModule
 ├─ Widgets : List<WidgetCounterDto>
 └─ Detail : object?
```

`Detail` có thể khác kiểu giữa các module. Tài liệu tổng hợp yêu cầu khi đọc contribution phải dùng kiểm tra kiểu an toàn (`is` pattern) và log warning nếu kiểu không đúng, thay vì dùng `as` khiến lỗi bị im lặng.

## 6. DashboardWidgetPolicy

`DashboardWidgetPolicy` thuộc `Application/Policies`.

Đây là pure logic:

- không I/O;
- không query DB;
- lọc widget theo quyền/context user;
- sắp xếp thứ tự widget.

Vai trò tương tự `NotificationFactory`: domain/application policy không phụ thuộc Infrastructure.

## 7. Approval aggregation

Dashboard cần pending approvals đa module nhưng không được query lại từng bảng Leave/OT.

Mô hình chuẩn là reuse approval aggregation đã có trong D2:

```text
Approval handlers / dispatcher
        ↓
GetAllPendingAsync()
        ↓
DashboardOrchestrator
```

Trong bản tài liệu tổng hợp cũ, component được gọi là `IApprovalInboxService`; bản chốt D1→D5 mô tả cơ chế inbox đa module được gộp vào các approval handlers/dispatcher. Khi code thực tế được cập nhật, cần giữ một nguồn aggregation duy nhất để tránh duplicate query logic.

## 8. Response boundary

```text
Provider contributions
        ↓
DashboardWidgetPolicy
        ↓
DashboardResponse
        ↓
API
        ↓
DashboardClientService
        ↓
Blazor UI
```

Không trả EF entities từ provider/API.

## 9. Namespace

| Thành phần | Vị trí |
|---|---|
| Orchestrator contract | `Application/Interfaces/Orchestrators` |
| Orchestrator | `Application/Orchestrators` |
| Provider contract | `Application/Interfaces/Dashboards` |
| Provider implementation | `Infrastructure/Services/Dashboards` |
| Policy | `Application/Policies` |
| DTO | `Application/Dtos/Dashboards` / `Contract` theo model thực tế |
| API controller | `API` |
| Client service | `Shared/Services/Dashboards` |
| Page | `Shared/Pages` / Blazor Pages |

## 10. Trạng thái

- Leave provider: đã được mô tả.
- OT provider: đã được mô tả.
- Trip provider: chưa build.
- Kiểu `Detail` và cơ chế runtime type-check cần được xác nhận trong implementation hiện tại.
