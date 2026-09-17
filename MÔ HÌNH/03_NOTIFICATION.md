# Notification Pipeline

## 1. Mục tiêu

Notification là pipeline cross-cutting nối Approval workflow với lưu trữ notification, realtime SignalR và UI Blazor.

## 2. Luồng chuẩn

```text
ApprovalWorkflowOrchestrator<TSubject>
        │
        ▼
IApprovalNotificationService
        │
        ▼
ApprovalNotificationService
        │
        ├─ INotificationFactory
        │      └─ build Title / Body / ActionUrl
        │
        └─ INotificationService
               │
               ├─ F03AppNotification via IUnitOfWork
               │
               └─ NotificationHub via SignalR
                         │
                         ▼
                NotificationClientService
                         │
                         ▼
                NotificationActionUIExtensions
```

## 3. Application layer

### `IApprovalNotificationService`

Là application contract được Approval workflow gọi khi có step mới hoặc một bước duyệt hoàn tất.

### `INotificationFactory`

Factory thuần logic, không I/O. Nhiệm vụ là chuyển ngữ cảnh nghiệp vụ thành:

- `Title`;
- `Body`;
- `ActionUrl`;
- các thông tin cần thiết cho notification DTO.

Factory không lưu database và không truy cập SignalR.

### `INotificationService`

Contract nghiệp vụ cho notification. Các API được tài liệu hóa gồm:

```text
CreateAsync(CreateNotificationDto, ct)
    → Task<NotificationDto>

GetByUserAsync(userId, page, pageSize, ct)
    → List<NotificationDto>

GetUnreadCountAsync(userId, ct)
    → int

MarkReadAsync(notificationId, userId, ct)
    → ServiceResult

MarkAllReadAsync(userId, ct)
    → ServiceResult
```

Contract trả DTO, không trả EF entity.

## 4. Infrastructure

### `ApprovalNotificationService`

Implementation thuộc `Infrastructure/Services/Approvals`:

1. gọi `INotificationFactory` để tạo nội dung;
2. gọi `INotificationService` để persistence và realtime.

### `NotificationService`

Implementation thuộc `Infrastructure/Services/Notifications`:

- lưu `F03AppNotification` thông qua `IUnitOfWork`;
- map entity → `NotificationDto`;
- dùng `IHubContext<NotificationHub>` để gửi realtime tới user.

### `NotificationHub`

SignalR hub thuộc:

```text
Infrastructure/Hubs
```

Không đặt hub trong API controller layer.

## 5. Client

`NotificationClientService` thuộc:

```text
Shared/Services/Notifications
```

Nó quản lý kết nối/nhận notification từ SignalR và cung cấp dữ liệu cho UI.

`NotificationActionUIExtensions` thuộc `Shared/Utils`, chịu trách nhiệm map enum/action sang biểu diễn UI như icon hoặc màu.

## 6. Điểm tích hợp Approval

Approval workflow gọi notification ở hai thời điểm chính:

```text
Bước 2 — Submit / có step mới
    → NotifyNewRequestAsync

Bước 3 — Approve/Reject hoàn tất
    → NotifyStepCompletedAsync
```

Notification là side effect của workflow; không đưa logic dựng nội dung notification vào Approval Engine.

## 7. Quy tắc dependency

```text
Application
 ├─ IApprovalNotificationService
 ├─ INotificationFactory
 └─ INotificationService

Infrastructure
 ├─ ApprovalNotificationService
 ├─ NotificationService
 └─ NotificationHub

Shared
 └─ NotificationClientService
```

Application chỉ biết contract. Infrastructure chứa EF/SignalR implementation. Shared không tham chiếu ngược Infrastructure.

## 8. TODO / điểm cần xác nhận

Approval step hiện có `ApproverCode:string`, trong khi kênh in-app cần `approverUserId:int` cho `Clients.User(...)`.

Đề xuất trong tài liệu nguồn:

```text
IUserService.GetUserIdByEmployeeCodeAsync(employeeCode, ct)
```

để resolve:

```text
ApproverCode
   → UserId
   → SignalR Clients.User(UserId)
```

Phần này chưa được coi là đã code hóa hoàn chỉnh cho đến khi implementation thực tế xác nhận.

## 9. Invariants

- Factory không I/O.
- Notification service không trả entity ra ngoài contract.
- SignalR hub thuộc Infrastructure.
- Approval workflow là nơi quyết định **khi nào** notification được phát; notification service quyết định **cách lưu/gửi**.
- Notification client thuộc Shared, không đặt logic UI notification trong domain Leave/OT.
