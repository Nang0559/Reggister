# Hướng dẫn sử dụng — Chọn người phê duyệt

## Leave / OT / Trip / Equipment

Khi mở màn hình đăng ký, hệ thống tự đọc:

1. EmployeeCode
2. Department
3. PositionCode
4. Approval Policy

Sau đó hiển thị từng cấp phê duyệt.

### Người đăng ký phải làm gì?

Chỉ chọn người trong dropdown của từng cấp.

Ví dụ:

- Cấp 1 — Trưởng ca: chọn Trưởng ca 2
- Cấp 2 — Manager: chọn Manager A
- Cấp 3 — GM: chọn GM

Không chọn Level.

### Nếu chỉ có một người

Hệ thống tự chọn người duy nhất.

### Nếu không có người

Màn hình báo lỗi:

> Chưa có người phê duyệt phù hợp cho cấp này.

Không được gửi đơn.

### Khi Submit

Hệ thống kiểm tra lại candidate ở server.

Sau khi hợp lệ:

1. Lưu lựa chọn.
2. Tạo Approval Snapshot.
3. Gửi thông báo cho cấp đầu tiên.
4. ApprovalEngine xử lý tuần tự.

### Nếu approver configuration thay đổi sau khi Submit

Đơn đã Submit vẫn dùng approver trong snapshot cũ.

Cấu hình mới chỉ áp dụng cho đơn mới.
