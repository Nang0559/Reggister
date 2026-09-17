# Notification — Sơ đồ triển khai

> Tài liệu bổ sung cho `03_NOTIFICATION.md`, tập trung riêng vào sequence và dependency của notification pipeline.

## 1. End-to-end

```mermaid
flowchart LR
    A[ApprovalWorkflowOrchestrator] --> B[IApprovalNotificationService]
    B --> C[ApprovalNotificationService]
    C --> F[INotificationFactory]
    C --> N[INotificationService]
    N --> DB[(F03AppNotification)]
    N --> H[NotificationHub]
    H <--> CL[NotificationClientService]
    CL --> UI[Blazor Notification UI]
```

## 2. Runtime sequence

```mermaid
sequenceDiagram
    participant W as Approval Workflow
    participant A as ApprovalNotificationService
    participant F as NotificationFactory
    participant N as NotificationService
    participant DB as UnitOfWork / DB
    participant H as SignalR
    participant C as Blazor Client

    W->>A: NotifyNewRequest / NotifyStepCompleted
    A->>F: Build DTO content
    F-->>A: Title / Body / ActionUrl
    A->>N: CreateAsync(dto)
    N->>DB: Save F03AppNotification
    DB-->>N: Saved
    N->>H: Clients.User(userId)
    H-->>C: Realtime event
    N-->>A: NotificationDto
```

## 3. Approver identity mapping

```mermaid
flowchart LR
    AC[ApproverCode] --> U[IUserService]
    U --> ID[UserId]
    ID --> SC[SignalR Clients.User]
    SC --> C[NotificationClientService]
```

`ApproverCode → UserId` là boundary cần được xác nhận với implementation thực tế.

## 4. Dependency rule

```mermaid
flowchart BT
    APP[Application contracts]
    INF[Infrastructure implementations]
    DB[(Database)]
    SIG[SignalR]
    SH[Shared Client]

    APP --> INF
    INF --> DB
    INF --> SIG
    SIG --> SH
```

- Factory không I/O.
- Persistence và SignalR nằm ở Infrastructure.
- Shared chỉ nhận notification qua client contract/realtime channel.
