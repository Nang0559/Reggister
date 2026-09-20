---
marp: true
theme: default
paginate: true
size: 16:9
---

# FVN REGISTER
## Báo cáo tổng quan nền tảng

**Smart Employee Request & e-Approval Platform**

> Từ yêu cầu đến phê duyệt — từ thực tế đến đối soát

---

# 01 — Executive Summary

FVN REGISTER là nền tảng tập trung cho vòng đời nghiệp vụ nhân sự:

**Request → Approval → Execution → Reconciliation → HR Review → Reporting / Payroll**

### Mục tiêu
- Chuẩn hóa quy trình.
- Minh bạch trạng thái.
- Tập trung approval.
- Quản lý Planned vs Actual.
- Tăng khả năng truy vết.
- Kiểm soát quyền bằng **Capability + Data Scope**.

---

# 02 — Vì sao cần FVN REGISTER?

### Quy trình phân tán
```mermaid
flowchart LR
    OLD[Email / Excel / Chat / Giấy tờ] --> P1[Thông tin phân tán]
    OLD --> P2[Khó biết ai xử lý]
    OLD --> P3[Khó truy vết]
    OLD --> P4[Khó đối soát]
    P1 --> FVN[FVN REGISTER]
    P2 --> FVN
    P3 --> FVN
    P4 --> FVN
```

### Chuyển đổi
**Phân tán → Tập trung → Có workflow → Có audit → Có reconciliation**

---

# 03 — Giá trị cho doanh nghiệp

| Đối tượng | Giá trị |
|---|---|
| Employee | Đăng ký và theo dõi minh bạch |
| Approver | Tập trung việc cần duyệt |
| HR | Review, reconciliation, attendance |
| Manager | Dashboard, Calendar, Reports |
| Admin | Capability, Scope, Configuration |
| Leadership | Chuẩn hóa, kiểm soát, truy vết |

---

# 04 — Vòng đời nghiệp vụ chuẩn

```mermaid
flowchart LR
    R[Request] --> P[Preview / Validate]
    P --> S[Submit]
    S --> A[Approval Snapshot]
    A --> AP[Approve / Reject / Escalate]
    AP --> X[Execution]
    X --> RA[Planned vs Actual]
    RA -->|Matched| RES[Resolved]
    RA -->|Mismatch| CO[Confirmation]
    CO --> EV[Evidence / Review]
    EV --> HR[HR Resolution]
    HR --> RES
    RES --> REP[Reports / Payroll Input]
```

**Thông điệp:** Approved chưa đồng nghĩa Actual đã hoàn thành.

---

# 05 — Bản đồ nền tảng

```mermaid
mindmap
  root((FVN REGISTER))
    Employee
      Leave
      Overtime
      Trip
      Equipment
    Approval
      Multi-level
      Snapshot
      Escalation
    Execution
      Planned
      Actual
      Reconciliation
      Evidence
      HR Review
    Workspace
      Dashboard
      Action
      Calendar
      Notification
    HR
      Attendance
      Resolution
      Payroll
    Management
      Reports
      Export
    Security
      Capability
      Data Scope
      Audit
```

---

# 06 — Approval: trung tâm của quy trình

```mermaid
sequenceDiagram
    participant U as Employee
    participant M as Business Module
    participant W as Workflow
    participant S as Snapshot
    participant A as Approver
    U->>M: Create / Edit
    M->>M: Validate
    U->>M: Submit
    M->>W: Initialize
    W->>S: Capture hierarchy
    W->>A: Notify actor
    A->>W: Approve / Reject
    W->>W: Next step
```

### Nguyên tắc
- Approval Snapshot bảo toàn hierarchy tại thời điểm Submit.
- Escalated do timeout **không đồng nghĩa Rejected**.
- Approval là quyết định nghiệp vụ; Notification chỉ là delivery.

---

# 07 — Leave

### Employee
**Create → Check Balance → Preview → Submit → Track**

### Approver
**Open → Check → Approve / Reject + Reason**

### Kết quả
- Request được quản lý theo workflow.
- Lịch nghỉ được phản ánh sau khi đủ điều kiện.
- Calendar chỉ là projection; module Leave là nguồn nghiệp vụ.

---

# 08 — Overtime

```mermaid
flowchart LR
    INPUT[OT Request] --> V[Server Validation]
    V --> PRE[Preview]
    PRE --> SUB[Submit]
    SUB --> AP[Approval]
    AP --> ACT[Official Actual]
    ACT --> REC[Reconciliation]
```

### Điểm kiểm soát
- Hạn mức được kiểm tra **server-side**.
- Policy có thể cấu hình theo ngày/tuần/tháng/năm.
- Approved OT là **Planned**, không phải Actual.
- Actual được đối soát trước khi đi vào các bước HR/Payroll.

---

# 09 — Trip

```mermaid
flowchart LR
    D[Draft] --> P[Pending Approval]
    P --> A[Approved]
    A --> X[Execution / Actual]
    X --> R[Reconciliation]
    R --> RES[Resolved]
```

- Một approval engine dùng chung.
- Không tự chọn cấp approval từ client.
- Actual được xử lý riêng với Planned.

---

# 10 — Equipment & QR

```mermaid
flowchart TD
    U[Authorized User] --> R[Equipment Request]
    R --> QR[Generate QR]
    QR --> IN[QR chưa active]
    R --> AP[Approval]
    AP -->|Approved| ASSET[Asset]
    ASSET --> ACTIVE[QR Active]
    ACTIVE --> SCAN[Scan QR]
    SCAN --> REP[Repair Request]
    REP --> RAP[Repair Approval]
    RAP --> HIST[Official Repair History]
```

### Điểm kiểm soát
- **EquipmentModule ≠ Approval authority**.
- QR có thể tồn tại trước approval nhưng chưa có hiệu lực nghiệp vụ.
- Repair chỉ trở thành lịch sử chính thức sau approval.

---

# 11 — Planned vs Actual

```mermaid
flowchart LR
    PLAN[Approved Planned] --> R[Reconciliation]
    ACT[Official Actual] --> R
    R -->|Matched| OK[Resolved]
    R -->|Mismatch| C[Confirmation]
    C --> E[Evidence]
    E --> HR[HR Review]
    HR --> OK
```

### Ý nghĩa
**Approved = kế hoạch được duyệt**

**Actual = thực tế**

**Reconciliation = kiểm tra sự phù hợp giữa hai nguồn**

---

# 12 — HR Review & Resolution

```mermaid
flowchart TD
    M[Mismatch] --> Q[HR Review Queue]
    Q --> D{Decision}
    D -->|OK| CORR[Correction if required]
    D -->|NG| NG[Reason + Audit]
    CORR --> CALC[Calculation Pipeline]
    CALC --> PAY[Payroll Input]
```

- Evidence có lifecycle riêng.
- HR OK không có nghĩa sửa trực tiếp database.
- NG phải có Reason.
- Correction cần audit và đi qua calculation pipeline.

---

# 13 — Attendance

### Nguồn và kết quả

```mermaid
flowchart LR
    HRM[HRM Source] --> CALC[HRM-compatible Calculation]
    CALC --> ATT[Attendance Snapshot]
    CALC --> OT[OT Actual]
    ATT --> PAY[Payroll Input]
    OT --> PAY
```

### Nguyên tắc
- HRM là nguồn dữ liệu / luật tính.
- FVN cung cấp UI và snapshot kết quả theo thiết kế.
- Correction không bypass calculation pipeline.
- Calculation batch cần truy vết.

---

# 14 — Payroll

```mermaid
stateDiagram-v2
    [*] --> Open
    Open --> Calculated
    Calculated --> Locked
    Locked --> Exported
    Open --> Open: Correction / Recalculation
```

### Readiness
Payroll chỉ đi tiếp khi các điều kiện của kỳ được đáp ứng.

**Kỳ hiện hành: ngày 21 → ngày 20**

Không dùng Payroll để che giấu execution mismatch hoặc correction chưa xử lý.

---

# 15 — Action Center

### Action = công việc cần xử lý

```mermaid
flowchart LR
    EVENT[Business Event] --> ACTION[Action]
    ACTION --> OPEN[Open]
    OPEN --> PROG[InProgress]
    PROG --> DONE[Completed]
```

> **Action.Completed ≠ Business Approved**

Action là work item; business module mới là nguồn trạng thái nghiệp vụ.

---

# 16 — Work Calendar

```mermaid
flowchart LR
    LEAVE[Leave] --> CAL[Calendar Projection]
    OT[OT] --> CAL
    TRIP[Trip] --> CAL
    CAL --> UI[Calendar UI]
    UI --> DETAIL[Open Source Detail]
```

### Nguyên tắc
- Calendar giúp nhìn kế hoạch.
- Calendar không thay thế approval state.
- Khi cần quyết định nghiệp vụ → mở source module.

---

# 17 — Notification

### Notification phục vụ
- Submit.
- Approval.
- Reject.
- Next approver.
- Escalation.
- Confirmation.
- HR Review.

### Cần nhớ

**Notification ≠ Approval**

Đã đọc notification không có nghĩa request đã được duyệt.

---

# 18 — Dashboard & Reports

### Dashboard
- Pending approvals.
- Action.
- Notification.
- Calendar.
- Module widgets.
- Statistics theo scope.

### Reports
- Leave.
- OT.
- Trip.
- Equipment.
- Attendance.

**View và Export là hai capability riêng.**

---

# 19 — Security: Capability + Data Scope

```mermaid
flowchart LR
    USER[Authenticated User] --> CAP[Capability]
    CAP --> SCOPE[Effective Data Scope]
    SCOPE --> QUERY[Server Query]
    QUERY --> RESULT[DTO / Export]
    CLIENT[UI Filter] -. không cấp quyền .-> QUERY
```

### 2 lớp kiểm soát

**Capability:** được làm gì?

**Data Scope:** được xem / xử lý dữ liệu nào?

> Có quyền Approve không đồng nghĩa có Scope All.

---

# 20 — Vai trò trong hệ thống

| Vai trò | Trách nhiệm chính |
|---|---|
| Employee | Create / Track Request |
| Approver | Review / Approve / Reject |
| HR | Reconciliation / Resolution / Attendance |
| Manager | Dashboard / Calendar / Reports |
| Admin | Security / Configuration |
| Payroll | Payroll Input theo kỳ |

---

# 21 — Những nguyên tắc cần nhớ

### 8 nguyên tắc vận hành

1. **Notification ≠ Approval**
2. **Action ≠ Business Result**
3. **Calendar ≠ Source of Truth**
4. **Approved ≠ Actual**
5. **Mismatch ≠ Rejected**
6. **HR OK ≠ Direct DB Edit**
7. **View ≠ Export**
8. **Scope ≠ Role Name**

---

# 22 — Employee Journey

```mermaid
flowchart LR
    LOGIN[Login] --> MODULE[Module]
    MODULE --> CREATE[Create]
    CREATE --> PRE[Preview]
    PRE --> SUB[Submit]
    SUB --> TRACK[Track]
    TRACK --> RESULT[Approved / Rejected]
    RESULT --> EXEC[Execution]
```

### Người dùng cần tập trung vào
**Dữ liệu đúng → Submit đúng → Theo dõi đúng trạng thái**

---

# 23 — Approver Journey

```mermaid
flowchart LR
    NOTI[Notification] --> ACTION[Action]
    ACTION --> DETAIL[Detail]
    DETAIL --> CHECK[Check]
    CHECK --> AP[Approve]
    CHECK --> RJ[Reject + Reason]
    AP --> NEXT[Next Level]
```

### Checklist
- Đúng người.
- Đúng ngày/giờ.
- Đúng nội dung.
- Đúng scope.
- Đúng policy.

---

# 24 — HR Journey

```mermaid
flowchart LR
    QUEUE[Review Queue] --> REVIEW[Review]
    REVIEW --> OK[OK]
    REVIEW --> NG[NG + Reason]
    OK --> CORR[Correction]
    CORR --> CALC[Recalculate]
    CALC --> PAY[Payroll Input]
```

### Mục tiêu
**Không chỉ xử lý lỗi — phải tạo được resolution có thể truy vết.**

---

# 25 — FAQ: các tình huống quan trọng

### Tôi có notification nhưng không thấy Approve?
Request có thể đã được xử lý, chuyển cấp hoặc bạn không còn là actor hiện tại.

### Tôi có quyền Approve nhưng không thấy request?
Kiểm tra required step, Pending status, Data Scope và actor hiện tại.

### Escalated có phải Rejected?
**Không.** Escalated là system decision do timeout để chuyển cấp.

### Action Completed có phải Approved?
**Không nhất thiết.** Kiểm tra business state ở module nguồn.

---

# 26 — FAQ: Planned / Actual / Reconciliation

```mermaid
flowchart TD
    P[Planned] --> R[Reconciliation]
    A[Actual] --> R
    R -->|Matched| OK[Resolved]
    R -->|Mismatch| C[Confirmation]
    C --> E[Evidence]
    E --> H[HR Review]
    H --> OK
```

### Quy tắc
- Không tự sửa Actual để làm cho dữ liệu khớp.
- Không đóng Action để bypass mismatch.
- Evidence chưa đạt → case chưa Resolved.

---

# 27 — Triển khai & Adoption

```mermaid
flowchart LR
    A[Awareness] --> T[Training]
    T --> P[Pilot]
    P --> F[Feedback]
    F --> G[Go-live]
    G --> M[Monitor]
    M --> I[Improve]
    I --> M
```

### Đo lường
- Tỷ lệ request trên hệ thống.
- Tỷ lệ approval xử lý trên hệ thống.
- Request tồn.
- Thời gian xử lý.
- Reconciliation chưa giải quyết.
- Mức sử dụng Dashboard / Reports.

---

# 28 — Thông điệp truyền thông

## FVN REGISTER

### **Một nền tảng. Một quy trình. Một nơi để theo dõi.**

Nghỉ phép • OT • Công tác • Thiết bị  
Approval • Calendar • Action • HR Review • Reports

**Request → Approval → Execution → Reconciliation → Resolution**

---

# 29 — Góc nhìn quản trị

```mermaid
flowchart TB
    REQUEST[Request Data] --> APPROVAL[Approval Control]
    APPROVAL --> EXEC[Execution Data]
    EXEC --> RECON[Reconciliation]
    RECON --> HR[HR Resolution]
    HR --> REPORT[Management Reporting]
    REPORT --> DECISION[Management Visibility]
```

### Kết quả mong muốn
- Một nguồn dữ liệu nghiệp vụ rõ ràng.
- Một workflow approval có kiểm soát.
- Một cơ chế reconciliation thống nhất.
- Một lớp báo cáo phục vụ quản trị.

---

# 30 — Kết luận

## FVN REGISTER

**Từ Request đến Approval**

**Từ Approval đến Execution**

**Từ Execution đến Reconciliation**

**Từ Reconciliation đến HR Resolution**

**Từ dữ liệu đến Management Insight**

### FVN REGISTER
**Từ yêu cầu đến phê duyệt — từ thực tế đến đối soát.**

---

<!--
## Speaker Notes — định hướng trình bày

Mục tiêu của deck:
1. Nêu vấn đề quản trị trước khi đi vào chức năng.
2. Cho lãnh đạo thấy toàn bộ lifecycle, không chỉ từng màn hình.
3. Nhấn mạnh 4 giá trị: chuẩn hóa, minh bạch, truy vết, đối soát.
4. Khi demo, đi theo một request xuyên suốt: Employee → Approver → Execution → HR → Report.
5. Không đi sâu code/kiến trúc kỹ thuật trong phần trình bày quản trị.
-->
