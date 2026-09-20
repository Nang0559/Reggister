# FVN REGISTER — QUICK GUIDE

## 1. Approver — 60 giây

```mermaid
flowchart LR
    N[Notification] --> A[Action]
    A --> D[Open detail]
    D --> C[Check employee / date / content]
    C -->|OK| AP[Approve]
    C -->|Không phù hợp| RJ[Reject + Reason]
    AP --> N2[Next approver notification]
    RJ --> U[Requester notified]
```

### Trước khi Approve
- Đúng người.
- Đúng ngày/giờ.
- Đúng nội dung.
- Đúng scope.
- Đúng policy.

### Nhớ
**Approve = quyết định nghiệp vụ.**

Không phải "đã xem".

## 2. Employee — 60 giây

```mermaid
flowchart LR
    CREATE[Create] --> CHECK[Check]
    CHECK --> PRE[Preview]
    PRE --> SUB[Submit]
    SUB --> TRACK[Track]
    TRACK --> RESULT[Approved / Rejected]
```

Không Submit khi thông tin chưa chính xác.

## 3. HR — 60 giây

```mermaid
flowchart LR
    Q[HR Queue] --> R[Review]
    R --> OK[OK]
    R --> NG[NG + Reason]
    OK --> CORR[Correction if required]
    CORR --> CALC[Recalculate]
    CALC --> PAY[Payroll Input]
    NG --> USER[Notify User]
```

HR không sửa trực tiếp attendance result.

## 4. Equipment — 60 giây

```mermaid
flowchart TD
    REGISTER[Register] --> QR[QR]
    QR --> APPROVAL[Approval]
    APPROVAL --> ACTIVE[QR Active]
    ACTIVE --> SCAN[Scan]
    SCAN --> REPAIR[Repair Request]
    REPAIR --> RAP[Repair Approval]
    RAP --> HISTORY[Repair History]
```

## 5. Trạng thái cần nhớ

| Khái niệm | Ý nghĩa |
|---|---|
| Draft | Chưa gửi |
| Pending | Đang chờ xử lý/duyệt |
| Approved | Business đã được duyệt theo workflow |
| Rejected | Bị từ chối |
| Escalated | Hệ thống chuyển cấp do timeout; không đồng nghĩa Rejected |
| Action Open | Có việc cần xử lý |
| Action Completed | Công việc đã đóng |
| Mismatch | Planned và Actual khác nhau |
| Resolved | Reconciliation đã được giải quyết |

## 6. 8 nguyên tắc

1. Notification ≠ Approval.
2. Action ≠ Business Result.
3. Calendar ≠ Source of Truth.
4. Approved ≠ Actual.
5. Mismatch ≠ Rejected.
6. HR OK ≠ sửa trực tiếp database.
7. View ≠ Export.
8. Scope ≠ Role name.

## 7. Khi có lỗi

```mermaid
flowchart TD
    ERR[Lỗi / Không thấy chức năng] --> AUTH{Đăng nhập?}
    AUTH -->|No| LOGIN[Đăng nhập lại]
    AUTH -->|Yes| FUNC{Có capability?}
    FUNC -->|No| ADMIN[Liên hệ Admin]
    FUNC -->|Yes| SCOPE{Đúng Data Scope?}
    SCOPE -->|No| OWNER[Liên hệ quản lý / HR]
    SCOPE -->|Yes| DATA{Dữ liệu hợp lệ?}
    DATA -->|No| FIX[Sửa dữ liệu]
    DATA -->|Yes| SUPPORT[Kiểm tra notification / workflow / hỗ trợ kỹ thuật]
```
