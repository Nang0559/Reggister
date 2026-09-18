# FVN_REGISTER — Mô hình hệ thống

## Bản đồ tài liệu

```mermaid
flowchart TD
 A[00_INDEX] --> B[01_FLOWS_TOTAL]
 B --> C[02_HRM_SYNC]
 B --> D[03_NOTIFICATION]
 B --> E[04_DASHBOARD]
 B --> F[08_APPROVAL_ESCALATION]
 B --> G[09_TRIP]
 B --> H[10_EQUIPMENT_REGISTER]
 C --> I[05_SYNC_FLOW]
 D --> J[06_NOTIFICATION_DIAGRAMS]
 F --> D
 G --> F
 H --> F
 H --> E
```

`08_APPROVAL_ESCALATION.md` là chuẩn approval chung. `09_TRIP.md` là chuẩn Trip. `10_EQUIPMENT_REGISTER.md` là chuẩn Sổ quản lý thiết bị, QR lifecycle và repair approval.

> **HRM / Attendance:** `02_HRM_SYNC.md` và `05_SYNC_FLOW.md` là chuẩn mô tả hai pipeline: đồng bộ master/ca/lịch từ HRM vào F03 cục bộ và attendance/OT từ HRM → `F03AttendanceStaging` → `F03OTEmployees`.

## Kiến trúc tổng thể

```mermaid
flowchart TB
 subgraph Client[Presentation]
  UI[Blazor Pages]
  QR[QR Scanner]
  DS[Dashboard Client]
  NS[Notification Client]
 end
 subgraph API[API]
  CTRL[Controllers]
  HUB[SignalR]
 end
 subgraph APP[Application]
  ORC[Approval Orchestrators]
  CONTRACT[Interfaces + DTO + Subjects]
 end
 subgraph INFRA[Infrastructure]
  SVC[Domain Services]
  EF[EF Core / UnitOfWork]
  QRGEN[QR Adapter]
 end
 DB[(SQL Server)]
 UI --> CTRL
 QR --> CTRL
 DS --> CTRL
 NS --> HUB
 CTRL --> CONTRACT
 CONTRACT --> ORC
 ORC --> SVC
 SVC --> EF
 SVC --> QRGEN
 EF --> DB
```

## Equipment

```mermaid
flowchart LR
 ADMIN[Admin] --> FUNC[F03Functions / F03UserFunctions]
 ADMIN --> APPROVER[F03Approvers: Equipment]
 USER[Authorized User] --> REG[Registration]
 USER --> SCAN[Scan QR]
 REG --> REQ[F03EquipmentRequest]
 REQ --> APPROVAL[Common Approval Engine]
 APPROVAL --> ASSET[F03EquipmentAsset]
 SCAN --> ASSET
 ASSET --> HISTORY[F03EquipmentRepairHistory]
 HISTORY -. only after approval .-> APPROVAL
```

QR token được sinh khi Draft để in/dán, nhưng chỉ request Approved mới tạo/activate asset. Repair history chính thức chỉ chứa record Approved.

## Dependency direction

```mermaid
flowchart BT
 CORE[Core] --> CONTRACT[Contract]
 CONTRACT --> APP[Application]
 APP --> INFRA[Infrastructure]
 APP --> API[API]
 CONTRACT --> SHARED[Shared]
 APP --> SHARED
```

Controller không query EF; Application không phụ thuộc Infrastructure. Module mới plug-in qua contract/service/provider và DI.

## Namespace convention

| Thành phần | Vị trí |
|---|---|
| Core entity | `FVN_REGISTER.Core/Entities/{Domain}` |
| Enum | `FVN_REGISTER.Core/Enums` |
| Contract DTO | `FVN_REGISTER.Models/Requests` + `Responses` |
| Application interface | `Application/Interfaces/{Domain}` |
| Subject | `Application/Models/Subjects` |
| Infrastructure service | `FVN_REGISTER.Infrastructure/Services/{Domain}` |
| EF configuration | `FVN_REGISTER.Infrastructure/Models/Data/Configurations/{Domain}` |
| API controller | `FVN_REGISTER.API/Controllers` |
| Shared client/page | `FVN_REGISTER/FVN_REGISTER.Shared/Services/{Domain}`, `Pages` |
| Database script | `Database/{Domain}` |

- [11 — Code Runtime Audit: Core → Application → Infrastructure → API → Web](./11_CODE_RUNTIME_AUDIT.md)
