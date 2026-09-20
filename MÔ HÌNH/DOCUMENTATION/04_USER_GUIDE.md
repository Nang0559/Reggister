# FVN REGISTER — HƯỚNG DẪN SỬ DỤNG

## 1. Luồng sử dụng tổng quát

```mermaid
flowchart TD
    LOGIN[Đăng nhập] --> DASH[Dashboard]
    DASH --> ACT[Action Center]
    DASH --> CAL[Work Calendar]
    DASH --> MOD[Module]
    MOD --> CREATE[Tạo / chỉnh sửa request]
    CREATE --> PREVIEW[Preview / Validate]
    PREVIEW --> SUBMIT[Submit]
    SUBMIT --> SNAP[Approval Snapshot]
    SNAP --> AP[Approval]
    AP -->|Approved| EXEC[Execution]
    AP -->|Rejected| END[End]
    EXEC --> REC[Reconciliation]
    REC -->|Matched| END
    REC -->|Mismatch| CONF[Confirmation]
    CONF --> EVID[Evidence / Review]
    EVID --> HR[HR Resolution]
    HR --> END
```

## 2. Đăng nhập

1. Đăng nhập bằng tài khoản được cấp.
2. Kiểm tra Dashboard sau khi đăng nhập.
3. Nếu không thấy module, kiểm tra quyền với Admin/HR.
4. Không chia sẻ tài khoản.

## 3. Dashboard

Dashboard tập trung:
- Pending approvals.
- Module widgets.
- Action.
- Notification.
- Calendar.
- Thống kê được cấp quyền.

Dashboard là màn hình tổng hợp, không phải nơi thay thế business workflow.

## 4. Employee — tạo request

### Quy trình chung

```mermaid
sequenceDiagram
    participant E as Employee
    participant UI as Module UI
    participant V as Server Validation
    participant W as Approval Workflow
    participant N as Notification

    E->>UI: Nhập thông tin
    UI->>V: Preview / Validate
    V-->>UI: Valid / Error
    E->>UI: Submit
    UI->>W: Submit request
    W->>W: Create immutable snapshot
    W->>N: Notify approver
    N-->>E: Status / notification
```

### Trước khi Submit

Kiểm tra:
- Ngày giờ.
- Nội dung.
- Bộ phận.
- Người tham gia nếu có.
- File/evidence nếu yêu cầu.
- Hạn mức/policy.

Không Submit khi dữ liệu chưa chính xác.

## 5. Approver — quy trình phê duyệt

### Luồng chuẩn

```mermaid
flowchart TD
    N[Notification] --> I[Approval Inbox / Action]
    I --> D[Open Detail]
    D --> C{Kiểm tra}
    C -->|Đủ / đúng| A[Approve]
    C -->|Không phù hợp| R[Reject + Reason]
    A --> NEXT{Còn cấp?}
    NEXT -->|Yes| I
    NEXT -->|No| OK[Business Approved]
    R --> USER[Notify Requester]
```

### Checklist trước khi Approve

- Đúng nhân viên?
- Đúng bộ phận?
- Đúng ngày/giờ?
- Nội dung hợp lý?
- Có đủ thông tin?
- Có vi phạm policy/hạn mức?
- Request có đúng phạm vi mình được duyệt?

### Khi Reject

Luôn nhập lý do rõ ràng để người tạo biết cần sửa gì.

### Approval Snapshot

Sau Submit, workflow đã snapshot hierarchy. Thay đổi cấu hình approver sau đó không tự thay đổi lịch sử của request đã Submit.

## 6. Leave

### Employee
1. Mở Leave.
2. Chọn loại phép.
3. Chọn ngày.
4. Kiểm tra balance.
5. Preview.
6. Submit.
7. Theo dõi Approval.

### Approver
1. Mở Action/Approval.
2. Kiểm tra ngày và người đăng ký.
3. Approve hoặc Reject + Reason.

Chỉ Leave Approved được coi là lịch nghỉ chắc chắn.

## 7. Overtime

### Employee

```mermaid
flowchart LR
    INPUT[Nhập ngày + giờ OT] --> LIMIT[Kiểm tra limit]
    LIMIT -->|Valid| PREVIEW[Preview]
    LIMIT -->|Invalid| FIX[Sửa request]
    PREVIEW --> SUBMIT[Submit]
```

Hạn mức hiện hành: ngày, tuần nếu được cấu hình, tháng 40h, năm 200h và ngưỡng tối đa 300h theo policy. Hệ thống phải kiểm tra server-side.

### Approver

Không coi "đã đăng ký" là "đã thực hiện". Approval xác nhận kế hoạch; actual được đối soát sau đó.

## 8. Trip

1. Tạo Draft.
2. Nhập thời gian, địa điểm, mục đích, chi phí và người đi cùng.
3. Preview.
4. Submit.
5. Theo dõi Approval.
6. Sau khi Approved, execution/actual được dùng cho reconciliation.

Trip dùng approval engine chung; không tự chọn cấp approval.

## 9. Equipment

### Đăng ký thiết bị

```mermaid
flowchart TD
    U[User có EquipmentModule] --> F[Nhập thông tin thiết bị]
    F --> QR[Server sinh QR token]
    QR --> DRAFT[Draft / QR chưa active]
    DRAFT --> SUB[Submit]
    SUB --> AP[Approval]
    AP -->|Approved| ASSET[Asset active]
    ASSET --> QRACTIVE[QR active]
```

### Sửa chữa

```mermaid
flowchart LR
    SCAN[Scan QR] --> DETAIL[Xem thiết bị]
    DETAIL --> REPAIR[Tạo Repair Request]
    REPAIR --> AP[Approval]
    AP -->|Approved| HISTORY[Official Repair History]
```

Repair chưa Approved không được coi là lịch sử sửa chữa chính thức.

## 10. Action Center

Action là việc cần xử lý, không phải kết quả nghiệp vụ.

Trạng thái thường gặp:
- Open.
- InProgress.
- Completed.
- Dismissed.

**Action.Completed không đồng nghĩa Business Approved/Matched.**

Nếu reconciliation còn Evidence bị Reject/NeedMoreEvidence, Action phải tiếp tục mở/in progress theo policy.

## 11. Notification

Notification có thể đến từ:
- Submit.
- Approval.
- Reject.
- Next approver.
- Escalation.
- Confirmation.
- HR Review.

Notification chỉ là kênh delivery/read state. Không dùng việc "đã đọc notification" để suy ra request đã được duyệt.

## 12. Work Calendar

Calendar là projection/navigation.

```mermaid
flowchart LR
    LEAVE[Leave] --> CAL[Calendar Projection]
    OT[OT] --> CAL
    TRIP[Trip] --> CAL
    CAL --> VIEW[Calendar UI]
    CAL --> DETAIL[Open source detail]
```

Calendar không phải source-of-truth cho approval.

## 13. Execution Reconciliation

```mermaid
flowchart TD
    PLAN[Approved Planned] --> R[Reconciliation]
    ACT[Official Actual] --> R
    R --> M{Match?}
    M -->|Yes| RES[Resolved]
    M -->|No| C[Employee Confirmation]
    C --> E[Evidence if required]
    E --> RV[Review]
    RV --> HR[HR Resolution]
    HR --> RES
```

Các module áp dụng: OT, Leave, Trip và module tương lai.

## 14. HR Review

### OK
- Xác nhận phản hồi hợp lệ.
- Tạo resolution.
- Với Attendance, tạo correction và chạy lại HRM-compatible calculation.
- Cập nhật CalendarAction.
- Đóng Action.
- Thông báo User.

### NG
- Bắt buộc Reason.
- Không tự sửa nguồn nghiệp vụ.
- Ghi audit.
- Thông báo User.

## 15. Attendance Calculation

```mermaid
flowchart LR
    HRM[HRM source] --> CALC[HRM-compatible calculation]
    CALC --> ATT[F03HrmAttendanceCalculated]
    CALC --> OTA[F03HrmOTActual]
    ATT --> PAY[F03PayrollInputs]
    OTA --> PAY
```

FVN đọc HRM; không ghi ngược HRM. Mỗi lần tính có CalculationBatchId để truy vết.

## 16. Payroll

Kỳ hiện hành: ngày 21 tháng hiện tại → ngày 20 tháng kế tiếp.

```mermaid
stateDiagram-v2
    [*] --> Open
    Open --> Calculated
    Calculated --> Locked
    Locked --> Exported
    Open --> Open: correction / recalculation
```

Không prepare/lock/export chính thức khi kỳ còn execution mismatch hoặc correction chưa xử lý theo policy.

## 17. Reports

1. Mở Reports.
2. Chọn nhóm.
3. Chọn bộ lọc trong phạm vi được phép.
4. Xem kết quả.
5. Export nếu có capability Export.

Có View không đồng nghĩa có Export.

## 18. Những điều không được làm

- Không dùng UI hide để coi là security.
- Không tự thay đổi approver bằng client.
- Không coi notification là approval.
- Không coi Action.Completed là business result.
- Không sửa trực tiếp attendance snapshot.
- Không bypass Data Scope bằng cách gửi DeptCode/EmployeeCode khác.
- Không coi Calendar là nguồn dữ liệu nghiệp vụ.
- Không đóng Action để bypass reconciliation.
