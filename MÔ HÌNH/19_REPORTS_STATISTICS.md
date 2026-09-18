# 19 — BÁO CÁO & THỐNG KÊ THEO PHÂN QUYỀN

## 1. Mục tiêu

Trung tâm báo cáo phải trả lời ba câu hỏi: người dùng được xem loại nào, được xem dữ liệu của ai/phòng nào, và có được xuất Excel hay chỉ được xem.

Không dùng IsAdmin ở UI làm cơ chế bảo mật. Server là authority.

## 2. Danh mục báo cáo chuẩn

### Leave

| Báo cáo | Nội dung | Xem | Xuất |
|---|---|---|---|
| Số dư phép | Được cấp, đã dùng, còn lại | Leave.View | Leave.Export |
| Theo phòng ban | Số đơn, ngày nghỉ, nhân sự | Leave.View | Leave.Export |
| Theo nhân viên | Số đơn và ngày nghỉ | Leave.View | Leave.Export |
| Chi tiết đơn | Đơn + thời gian + trạng thái | Leave.View | Leave.Export |
| Trạng thái duyệt | Workflow | Leave.View | Leave.Export |

### OT

| Báo cáo | Nội dung | Xem | Xuất |
|---|---|---|---|
| Theo phòng ban | Nhân sự, phiếu, giờ OT | OT.View | OT.Export |
| Theo nhân viên | Kế hoạch, thực tế, đã duyệt | OT.View | OT.Export |
| Chi tiết | Phiếu OT | OT.View | OT.Export |
| Trạng thái duyệt | Workflow | OT.View | OT.Export |
| Tích lũy hạn mức | Tuần/tháng/năm | OT.View | OT.Export |

### Trip

| Báo cáo | Nội dung | Xem | Xuất |
|---|---|---|---|
| Theo phòng ban | Số chuyến, nhân sự, chi phí | Trip.View | Trip.Export |
| Theo nhân viên | Số chuyến, số ngày, chi phí | Trip.View | Trip.Export |
| Chi tiết | Địa điểm, mục đích, thời gian | Trip.View | Trip.Export |
| Trạng thái duyệt | Workflow | Trip.View | Trip.Export |

### Equipment

| Báo cáo | Nội dung | Xem | Xuất |
|---|---|---|---|
| Theo phòng ban | Số tài sản, nguyên giá, QR | Equipment.View | Equipment.Export |
| Chi tiết tài sản | Serial, mã tài sản, vị trí, nguyên giá | Equipment.View | Equipment.Export |
| Sửa chữa | Số lần sửa, chi phí, duyệt | Equipment.View | Equipment.Export |

### Attendance

| Báo cáo | Nội dung | Xem | Xuất |
|---|---|---|---|
| Tổng hợp | Ngày công, giờ công, OT | Attendance.View | Attendance.Export |
| Chi tiết | Vào/ra, ca, giờ công, OT | Attendance.View | Attendance.Export |

## 3. Data scope

Capability và data scope là hai lớp khác nhau.

```text
Report type
    ↓
Capability
    ↓
Data scope
    ↓
Query
```

Quy tắc: Own = nhân viên hiện tại; Department = phòng ban hiện tại; All = toàn hệ thống nếu role/function cho phép.

User chọn DeptCode/EmployeeCode không bao giờ được mở rộng scope vượt quyền.

## 4. Export là capability riêng

Có Leave.View không đồng nghĩa Leave.Export. Quy tắc tương tự cho OT, Trip, Equipment và Attendance.

POST /api/report/export/excel kiểm tra capability export trước khi tạo file.

## 5. API

```http
POST /api/report
POST /api/report/export/excel
GET  /api/report/lookup/departments
GET  /api/report/lookup/employees
```

ReportDispatcher định tuyến ReportType tới handler và kiểm tra capability trước khi handler chạy.

## 6. SQL reporting layer

SQL/20_Reports.sql tạo các read model: VF03Report_LeaveRequests, VF03Report_OTRequests, VF03Report_Trips, VF03Report_Equipment, VF03Report_Attendance.

View chỉ chuẩn hóa dữ liệu reporting; view không cấp quyền.

## 7. Không duplicate dữ liệu

Không tạo bảng tổng hợp chứa bản sao Leave/OT/Trip chỉ để báo cáo. Báo cáo đọc source/read view và tổng hợp tại query layer.

## 8. Pagination

Báo cáo chi tiết giới hạn page size tối đa 500 dòng/lần query.

## 9. Biểu đồ

Chart chỉ là presentation; không dùng chart để quyết định quyền hoặc business rule.

## 10. Sử dụng theo vai trò

Nhân viên: số dư phép, lịch sử nghỉ, OT tuần/tháng/năm, công tác cá nhân.

Trưởng phòng: nghỉ phép, OT, công tác, chấm công theo phòng.

HR: số dư phép, nghỉ, OT, chấm công, công tác và thiết bị khi được cấp quyền.

Admin: báo cáo module theo scope được cấp và export khi có capability tương ứng.

## 11. Quy tắc nghiệp vụ

Báo cáo chỉ là dữ liệu quan sát. Không cho báo cáo sửa, duyệt, hủy đơn, thay đổi số dư/hạn mức hoặc ghi vào HRM.

## 12. Kiểm thử bắt buộc

- Không có View => từ chối.
- Có View nhưng không có Export => xem được, export bị từ chối.
- Có Export nhưng không có View => export cũng bị từ chối.
- Own không thấy nhân viên khác.
- Department không thấy phòng khác.
- All chỉ hoạt động khi function có scope All.
- DeptCode/EmployeeCode không thể bypass scope.
- Leave/OT/Trip/Equipment/Attendance report phải khớp source dữ liệu chính thức.

## 13. UI

Trung tâm báo cáo: /reports.

Chỉ hiển thị nhóm khi user có capability View.

Nhóm: Nghỉ phép, Tăng ca, Công tác, Thiết bị, Chấm công.

Trang /report/operations dùng chung cho Trip/Equipment/Attendance.

## 14. Audit

Xem báo cáo không thay đổi dữ liệu nghiệp vụ. Export dữ liệu nhạy cảm hoặc số lượng lớn nên được audit.

## 15. Nguyên tắc cuối cùng

```text
Authentication
   ↓
Report Capability
   ↓
Data Scope
   ↓
Validated Query
   ↓
ReportResultDto
   ↓
UI / Excel
```

UI hide ≠ Security.
Controller Authorize ≠ Data Scope.
Report View ≠ Export.
Export ≠ quyền sửa.
SQL View ≠ Authorization.