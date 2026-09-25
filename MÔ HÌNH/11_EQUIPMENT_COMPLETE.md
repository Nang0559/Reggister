# Quản lý thiết bị — kiến trúc và vận hành chuẩn

## 1. Phạm vi

Module quản lý thiết bị gồm đăng ký/import Excel theo schema, giao cá nhân, trách nhiệm vận hành, bàn giao qua Approval + IT, checklist version, yêu cầu sửa chữa, Action Center, notification/email và báo cáo Excel.

## 2. Giao thiết bị và HRM validation

Khi chọn **Giao cho nhân viên** khi import:

1. Excel bắt buộc có cột EmployeeCode (có thể đặt tên Mã nhân viên).
2. Backend kiểm tra từng dòng với HRM.
3. Nhân viên phải đang hoạt động và thuộc đúng DeptCode của file.
4. Chỉ khi tất cả dòng hợp lệ batch mới ở trạng thái Ready.
5. Commit mới tạo Asset và gắn ResponsibleEmployeeCode.
6. Commit revalidate lại EmployeeCode + IsActive + EndWorkingDate + DeptCode; nếu HRM đã chuyển nhân viên sang bộ phận khác sau staging thì dòng bị chặn, không được import sai phạm vi.

Khi nhập mới trên UI, danh sách nhân viên lấy từ HRM; không cho nhập mã tự do.

## 3. Checklist Excel theo schema

Bắt buộc: TemplateCode, TemplateName, DeptCode, Frequency, ItemCode, ItemLabel, InputType, DisplayOrder.

Tùy chọn: Description, IsRequired, RequireImage, MinImages, MaxImages, MinValue, MaxValue, Unit, OptionsJson.

InputType: PassFail | YesNo | Number | Text | Select.

Toàn bộ file được validate trước; có lỗi thì không tạo template. File hợp lệ tạo Draft, sau đó người có quyền mới publish Active.

~~~mermaid
flowchart LR
    X[Excel Schema] --> V[Validate toàn file]
    V -->|Lỗi| E[Hiển thị lỗi từng dòng]
    V -->|OK| D[Template Draft]
    D --> P[Review]
    P --> A[Active Version]
    A --> T[Scheduler]
    T --> K[Inspection Task]
    K --> C[Submit]
    C --> AP[Approve/Reject]
    AP --> R[History + Report]
~~~

## 4. Trách nhiệm vận hành

Asset có thể có OperatingResponsibleDeptCode hoặc OperatingResponsibleEmployeeCode.

Yêu cầu sửa chữa bắt buộc chọn đúng một kiểu:

- **Bộ phận**: DeptCode phải tồn tại và active. Sau khi đủ approval, tất cả nhân viên active của bộ phận nhận Action + notification + email.
- **Cá nhân**: EmployeeCode phải tồn tại, active và hệ thống tự xác định DeptCode.

~~~mermaid
flowchart TD
    A[Người phụ trách thiết bị] --> B[Tạo Repair Request]
    B --> C{Trách nhiệm sửa chữa}
    C -->|Bộ phận| D[Validate Dept HRM]
    C -->|Cá nhân| E[Validate Employee HRM]
    D --> F[Draft + Approval Selection]
    E --> F
    F --> G[Approval Level 1..N]
    G -->|Rejected| H[Kết thúc]
    G -->|Approved đủ cấp| I[Sinh Repair Actions]
    I --> J{Department?}
    J -->|Yes| K[Action cho tất cả thành viên active]
    J -->|No| L[Action cho người được giao]
    K --> M[Người đầu tiên xử lý]
    L --> M
    M --> N[Complete Repair]
    N --> O[Đóng Action hiện tại]
    O --> P[Cancel Action song song]
    P --> Q[Ghi RepairHistory]
    Q --> R[Notify + Email requester]
    R --> S[Notify + Email toàn bộ approver]
~~~

## 5. Bàn giao

Người tiếp quản là người lập phiếu. Server xác thực người lập chính là NewEmployeeCode theo tài khoản đăng nhập.

~~~mermaid
sequenceDiagram
    participant N as Người tiếp quản
    participant S as System
    participant H as HRM
    participant A as Approval
    participant IT as IT

    N->>S: Chọn nhân sự cũ và tài sản
    S->>H: Validate HR data
    S->>S: Capture PreChangeSnapshot
    S->>A: Tạo approval snapshot
    A-->>S: Pending / Approved / Rejected
    A->>IT: Phiếu Approved đủ cấp
    IT->>S: Execute
    S->>S: Cập nhật trách nhiệm/quyền
    S->>S: Capture PostChangeResult
    S-->>N: Hoàn tất
~~~

Đối với thiết bị, PreChangeSnapshot chứa thông tin thiết bị, người phụ trách/approver, lịch sử sửa chữa, lịch sử bàn giao và checklist/result/evidence tham chiếu tại thời điểm lập phiếu. Approver xem snapshot này thay vì phụ thuộc dữ liệu asset đã thay đổi sau đó.

## 6. Dashboard cá nhân

Thiết bị xuất hiện trên Dashboard nếu người dùng là ResponsibleEmployeeCode hoặc OperatingResponsibleEmployeeCode.

Người được giao thiết bị được phép tạo repair request cho chính asset được giao. Server vẫn kiểm tra quan hệ asset; không cấp quyền sửa chữa cho toàn bộ người dùng.

## 7. Báo cáo

Excel quản lý thiết bị gồm:

- **TaiSan**: tài sản, người phụ trách, vận hành, số lần sửa/bàn giao/checklist.
- **LichSuSuaChua**: request, ngày, bộ phận/người sửa, nội dung, chi phí, kết quả, phản hồi.
- **LichSuBanGiao**: người cũ/mới, approver cũ/mới, lý do, thời điểm, người thực hiện.
- **Checklist**: task, template/version, ngày, trạng thái, kết quả, inspector, approver.

Có bộ lọc bộ phận, người phụ trách, bộ phận sửa chữa và thời gian.

## 8. Security / audit

- HRM là nguồn chuẩn cho EmployeeCode, DeptCode, PositionCode và trạng thái làm việc.
- Validate lại tại server, không tin dữ liệu UI.
- Approval dùng snapshot.
- IT là điểm thực thi bàn giao.
- Repair completion idempotent; request đã hoàn thành không được hoàn thành lần hai.
- Khi một thành viên xử lý Department repair, các Action song song còn lại được cancel.
- Lịch sử sửa chữa/bàn giao giữ nguyên sau khi thay đổi trách nhiệm.
