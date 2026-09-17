# FVN_REGISTER — Kiến trúc tổng hợp D1 → D5

## 1. Mục tiêu

Tài liệu này là bản Markdown chuẩn hóa của `FolowsTotal.txt`, mô tả các domain/pipeline chính và các nguyên tắc kiến trúc xuyên suốt hệ thống.

## 2. Sơ đồ tổng thể

```text
[D1] HRM Master Data Sync
          │
          ▼
[D2] Leave / OT Request + Approval
          │
          ├──────────────► [D4] Notification Pipeline
          │
          ▼
[D3] OT Attendance Reconciliation
          │
          ▼
[D5] Dashboard Aggregation
```

- **D1** cung cấp Employee / Department / Position / LeaveType / OTType sạch cho hệ thống.
- **D2** quản lý Leave/OT Request và Approval.
- **D3** đối chiếu chấm công OT thực tế với dữ liệu OT đã Approved.
- **D4** phát notification cho approver/employee khi workflow có thay đổi.
- **D5** tổng hợp widgets và pending approvals để hiển thị dashboard.

## 3. Nguyên tắc kiến trúc xuyên suốt

### 3.1 Dependency direction

```text
Contract / Application
        ▲
        │ contracts, interfaces, DTOs
Infrastructure
```

Chiều phụ thuộc được kiểm soát: Application không phụ thuộc implementation của Infrastructure.

### 3.2 Không truy cập DbContext từ Orchestrator

Orchestrator chỉ điều phối use-case thông qua application interfaces/service contracts. EF Core, SQL, SignalR và các implementation cụ thể thuộc Infrastructure.

### 3.3 Entity không được lộ qua Application contract

Application/Interfaces trao đổi bằng DTO, response và Subject; EF entity không trở thành contract cho UI/client.

### 3.4 Open/Closed cho module

Thêm module mới (ví dụ Trip) phải ưu tiên:

1. thêm implementation mới;
2. đăng ký DI;
3. không sửa orchestrator/worker/dispatcher dùng chung.

## 4. D1 — HRM Master Data Sync

```text
HRM DB
  ├─ Trigger ───────────────┐
  └─ IHrmStagingImporter ───┤
                            ▼
                 F03StagingXxx
                            ▼
                 HrmSyncJob<TStaging,TEntity>
                            ▼
                 F03Department / Position /
                 LeaveType / OTType / Employee
```

Các domain sync dùng chung staging và generic sync job. Thứ tự dependency bắt buộc: **Department và Position trước Employee**.

## 5. D2 — Leave / OT Request + Approval

### Request flow

```text
CreateAsync
  → ValidateAsync
  → PersistNewRequestAsync
  → BuildContext
  → ApprovalWorkflow.InitApprovalAsync
  → MarkPendingAsync
```

`LeaveOrchestrator` và `OTOrchestrator` là domain facade mỏng trên generic request/approval infrastructure.

### Approval workflow — 5 bước bất biến

| Bước | Ý nghĩa | Persistence |
|---|---|---|
| 1. Preview | Dựng hierarchy cho form | Không persist |
| 2. Submit | Chụp snapshot vị trí approver | Persist một lần |
| 3. Approve/Reject | Ghi lịch sử quyết định | Append-only history |
| 4. Read back | Tính trạng thái từ snapshot + history | Không cache Calculated |
| 5. Reminder | Theo dõi nhắc duyệt | Reminder log riêng |

### Approve đa module

```text
ApprovalsController
  → IApprovalWorkflowDispatcher
  → IApprovalActionHandler
  → ApprovalWorkflowOrchestrator<TSubject>
```

Một endpoint chung xử lý nhiều module; dispatcher/handler route tới workflow tương ứng.

## 6. D3 — OT Attendance Reconciliation

```text
HRM Attendance
   → usp_SyncAttendanceStaging
   → F03AttendanceStaging
   → IOTAttendanceStagingService
   → IOTAttendanceReconciliationService
   → usp_SyncOTActualHours
   → F03OTEmployee.ActualHours
```

Đây là **command-with-result** vì reconciliation có ghi dữ liệu. UI chỉ thực hiện khi Admin chủ động bấm "Đối chiếu"; không gọi trong lifecycle render/timer.

## 7. D4 — Notification Pipeline

```text
ApprovalWorkflowOrchestrator
  → IApprovalNotificationService
  → ApprovalNotificationService
  → INotificationFactory
  → INotificationService
  → F03AppNotification + SignalR
  → NotificationClientService
```

Factory chỉ chứa logic dựng notification; persistence và realtime thuộc Infrastructure.

## 8. D5 — Dashboard Aggregation

```text
DashboardPage
  → DashboardClientService
  → DashboardController
  → DashboardOrchestrator
       ├─ Pending approvals
       ├─ IModuleDashboardProvider (collection)
       └─ DashboardWidgetPolicy
  → DashboardResponse
  → Blazor UI
```

Mỗi module đóng góp dữ liệu qua `IModuleDashboardProvider`; thêm module không yêu cầu sửa `DashboardOrchestrator`.

## 9. Namespace convention

| Thành phần | Vị trí |
|---|---|
| Interface nghiệp vụ | `Application/Interfaces/{Domain}` |
| Orchestrator | `Application/Orchestrators` |
| Dispatcher | `Application/Dispatchers` |
| Policy / Factory | `Application/Policies`, `Application/Factories` |
| Subject | `Application/Models/Subjects` |
| DTO | `Contract/Dtos/{Domain}` |
| Response | `Contract/Responses` |
| Entity | `Core/Entities/{Domain}` |
| Implementation | `Infrastructure/Services/{Domain}` |
| Hub | `Infrastructure/Hubs` |
| Worker / Job | `Infrastructure/Jobs` |
| Controller | `API/Controllers` |
| Blazor | `Shared/Pages`, `Shared/Services/{Domain}` |

## 10. Các điểm còn tồn đọng

- Xác nhận logic `HrmSyncResult.Success`.
- Xác nhận `IHrmStagingEntity.Id` dùng làm tiebreaker.
- Xác nhận thứ tự enum `SyncSourceTags` và giá trị mặc định an toàn.
- Migration cho `ApproverEmail` và `F03ApprovalReminderLog`.
- Hoàn thiện rule Admin override trong approval.
- Lộ trình loại bỏ `F03ApprovalStep` cũ.
- Resolve `ApproverCode → UserId` cho notification in-app.
- Hoàn thiện kiểu `Detail` của dashboard contribution.
