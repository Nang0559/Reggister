# FVN_REGISTER — Mô hình hệ thống

> Bộ tài liệu Markdown được dựng lại từ các tài liệu TXT hiện có trong thư mục `MÔ HÌNH`.
>
> **Nguồn:** `Dasboard.txt`, `FolowsTotal.txt`, `HRMSync.txt`, `Notification.txt`, `SynFlow.txt`.

## 1. Tài liệu

| Tài liệu | Nội dung |
|---|---|
| [01_FLOWS_TOTAL.md](./01_FLOWS_TOTAL.md) | Kiến trúc tổng hợp D1 → D5 và các nguyên tắc xuyên suốt |
| [02_HRM_SYNC.md](./02_HRM_SYNC.md) | HRM Master Data Sync và OT Attendance Reconciliation |
| [03_NOTIFICATION.md](./03_NOTIFICATION.md) | Notification Pipeline, Factory, Service, SignalR và Blazor client |
| [04_DASHBOARD.md](./04_DASHBOARD.md) | Dashboard aggregation và module providers |
| [05_SYNC_FLOW.md](./05_SYNC_FLOW.md) | Chi tiết tầng của pipeline HRM Sync, resolver, worker, management service và review flags |

## 2. Các pipeline chính

```text
D1  HRM Master Data Sync
 ↓
D2  Leave / OT Request + Approval
 ↓
D4  Notification Pipeline
 ↘
D3  OT Attendance Reconciliation
 ↓
D5  Dashboard Aggregation
```

## 3. Nguyên tắc kiến trúc

- Contract/Application định nghĩa interface và DTO; Infrastructure cung cấp EF/SQL/SignalR implementation.
- Không có chiều phụ thuộc ngược từ Application về Infrastructure.
- Orchestrator không truy cập trực tiếp `DbContext`.
- Entity không được lộ ra khỏi Application interface; giao tiếp qua DTO/Subject.
- Module mới phải mở rộng bằng implementation + DI registration, không sửa orchestrator/worker/dispatcher dùng chung.
- CRUD thủ công và HRM synchronization là hai luồng độc lập.
- OT attendance reconciliation là **command-with-result**, không phải query thuần vì thao tác reconciliation có ghi dữ liệu.

## 4. Namespace convention

| Thành phần | Namespace / vị trí |
|---|---|
| Interface nghiệp vụ | `Application/Interfaces/{Domain}` |
| Orchestrator | `Application/Orchestrators` |
| Dispatcher | `Application/Dispatchers` |
| Policy / Factory | `Application/Policies`, `Application/Factories` |
| Subject generic | `Application/Models/Subjects` |
| DTO | `Contract/Dtos/{Domain}` |
| Response | `Contract/Responses` |
| EF Entity | `Core/Entities/{Domain}` |
| Implementation | `Infrastructure/Services/{Domain}` |
| SignalR Hub | `Infrastructure/Hubs` |
| Background Worker / Job | `Infrastructure/Jobs` |
| Controller | `API/Controllers` |
| Blazor Page / Client Service | `Shared/Pages`, `Shared/Services/{Domain}` |

## 5. Project dependency

```text
Core
  ↑
Contract
  ↑
Application
  ↑
Infrastructure

Web / Blazor / Shared
  → Core, Contract, Application
  → không tham chiếu Infrastructure
```

## 6. Trạng thái tài liệu

- Nội dung được chuẩn hóa từ TXT, không chủ động thay đổi business rule.
- Các mục được TXT đánh dấu `TODO`, `CẦN XÁC NHẬN`, `CHƯA BUILD` vẫn được giữ lại và đánh dấu rõ.
- Khi code thực tế thay đổi, cập nhật Markdown cùng commit với thay đổi kiến trúc.
