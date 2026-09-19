# FVN REGISTER — Hướng dẫn sử dụng chi tiết

## 1. Workspace
1. Đăng nhập.
2. Kiểm tra Action Center.
3. Mở thông báo hoặc Approval Inbox.
4. Chọn module cá nhân: Nghỉ phép, OT, Công tác, Thiết bị.
5. Dashboard chỉ hiển thị contribution phù hợp capability.
Ví dụ: người có LeaveView nhưng không có LeaveApprove không có thao tác duyệt.

## 2. Notification / Approval Inbox
Luồng: PENDING → SCOPE CHECK → REVIEW → APPROVE/REJECT → AUDIT.
1. Mở Trung tâm công việc.
2. Xem số thông báo chưa đọc và việc chờ duyệt.
3. Mở nhóm theo module/cấp.
4. Kiểm tra nhân viên, phòng ban, thời gian và trạng thái.
5. Approve hoặc Reject.
6. Nếu batch có một item sai scope hoặc không còn pending, toàn batch bị từ chối.

## 3. Equipment Workspace
Luồng: SCHEMA → EXCEL → STAGING → VALIDATE → PREVIEW → COMMIT → AUDIT.
### Schema
1. Nhập DeptCode.
2. Tải schema.
3. Xem field hiện hành.
4. Thêm FieldKey, tên hiển thị, kiểu dữ liệu và cờ bắt buộc.
5. Lưu field.
### Excel
1. Chuẩn bị .xlsx với header theo schema.
2. Chọn file.
3. Upload & Preview.
4. Đọc tổng số dòng, dòng hợp lệ và lỗi.
5. Sửa file nguồn nếu có lỗi.
6. Chỉ Commit khi InvalidRows = 0.
Ví dụ IT: Mã thiết bị | Tên | IP Address | MAC Address.
Ví dụ Hành chính: Mã thiết bị | Tên | Vị trí | Nhà cung cấp.

## 4. CMS
Luồng: Draft → Preview → Published → Public Read → Archived.
1. Mở CMS thông tin công khai.
2. Tạo bản nháp.
3. Chọn loại: Announcement/Regulation/Guide/Policy.
4. Nhập nội dung, thời gian hiệu lực và đánh dấu quan trọng nếu cần.
5. Lưu.
6. Preview.
7. Publish.
8. Archive khi không còn hiệu lực.
Published không sửa trực tiếp; tạo phiên bản mới để giữ lịch sử.

## 5. Security Center
Luồng: USER → ROLE → FUNCTION/ACTION → SCOPE → AUDIT.
1. Xem effective permission.
2. Kiểm tra role và function/action.
3. Kiểm tra ScopeCode.
4. Sau thay đổi, kiểm tra audit.
Quy tắc: Approve không đồng nghĩa All; IsAdmin không bypass data scope.

## 6. Trợ giúp động
Nút ? trên thanh ứng dụng tự nhận diện route hiện tại.
Trợ giúp hiển thị sơ đồ, mục đích, các bước, ví dụ và quyền.
Chọn Mở tài liệu chi tiết để xem /huong-dan theo FeatureCode.

## 7. Xử lý sự cố
- 403: kiểm tra capability.
- Có capability nhưng không thấy dữ liệu: kiểm tra data scope.
- Import có lỗi: xem validation theo batch, sửa Excel rồi staging lại.
- Approval không thực hiện được: kiểm tra trạng thái pending và scope.
- Nội dung public không xuất hiện: kiểm tra Status và EffectiveFrom/EffectiveTo.

## 8. OT và hạn mức giờ

Luồng: **NHẬP OT → NGÀY → TUẦN → THÁNG → NĂM → SUBMIT → APPROVAL → THEO DÕI**.

1. Chọn ngày, loại OT và số giờ.
2. Hệ thống kiểm tra giới hạn ngày.
3. Kiểm tra tổng OT của tuần từ Thứ 2 đến Chủ nhật nếu doanh nghiệp đã cấu hình Weekly rule.
4. Kiểm tra hạn mức tháng; mặc định hiện tại 40 giờ/tháng nếu chưa có rule riêng.
5. Kiểm tra hạn mức năm; mặc định 200 giờ/năm và ngưỡng tối đa hiện tại 300 giờ/năm.
6. Nếu vượt ngưỡng năm tiêu chuẩn, hệ thống yêu cầu xử lý theo policy đặc biệt; không được vượt ngưỡng tối đa.
7. Submit và theo dõi tiến trình Approval.

Trên Workspace, OT cá nhân hiển thị **tuần / tháng / năm** dưới dạng đã dùng so với hạn mức. Hạn mức tuần chỉ hiển thị khi có Weekly rule.

## 9. Trung tâm thông tin cá nhân

Workspace hiển thị:
- OT đã dùng trong tuần, tháng, năm.
- Hạn mức và số giờ còn lại khi có cấu hình.
- Tổng phép được hưởng, đã nghỉ và còn lại.
- Ngày nghỉ đã duyệt sắp tới và tổng số ngày của các đợt nghỉ sắp tới.

Request Pending không được coi là lịch nghỉ chắc chắn; chỉ ngày nghỉ Approved mới xuất hiện trong “sắp nghỉ”.

## Lịch làm việc chung

Tại màn hình Nghỉ phép, OT và Công tác, người dùng dùng chung một lịch làm việc.

Lịch hiển thị:
- Ngày nghỉ/lễ của công ty.
- Các đơn Nghỉ phép.
- Các đơn OT.
- Các chuyến Công tác.

Bộ lọc gồm Tất cả, Công ty, Nghỉ phép, OT và Công tác.

Click vào một ngày sẽ chuyển ngày đó về nghiệp vụ đang mở:
- Nghỉ phép: ngày mặc định của đơn nghỉ.
- OT: ngày OT.
- Công tác: ngày bắt đầu công tác.

Click vào event đã có sẽ mở chi tiết nếu module hỗ trợ.

Lưu ý: lịch chỉ là màn hình tra cứu và cảnh báo. Việc có được phép đăng ký hay không vẫn do server kiểm tra quyền, phạm vi dữ liệu và validator của từng nghiệp vụ. Ngày nghỉ công ty không mặc nhiên khóa OT hoặc Công tác.

Chi tiết kiến trúc xem MÔ HÌNH/18_WORK_CALENDAR.md.

## Trung tâm báo cáo & thống kê


Mở `/reports` để xem các nhóm báo cáo mà tài khoản được cấp quyền.

### Nghỉ phép
- Số dư phép theo năm.
- Tổng hợp theo phòng ban/nhân viên.
- Chi tiết đơn.
- Trạng thái phê duyệt.

### OT
- Theo phòng ban.
- Theo nhân viên.
- Chi tiết phiếu.
- Trạng thái duyệt.
- Tích lũy hạn mức tuần/tháng/năm.

### Công tác
- Theo phòng ban.
- Theo nhân viên.
- Chi tiết chuyến.
- Trạng thái duyệt.

### Thiết bị
- Theo phòng ban.
- Chi tiết tài sản.
- Tổng hợp sửa chữa.

### Chấm công
- Tổng hợp theo phòng ban.
- Chi tiết vào/ra, ca, giờ công và OT.

### Bộ lọc và phạm vi
Báo cáo cho phép lọc khoảng thời gian, phòng ban và nhân viên tùy loại báo cáo. Hệ thống luôn áp dụng data scope ở server; việc nhập mã phòng hoặc mã nhân viên không thể mở rộng phạm vi.

### Xuất Excel
Nút Excel chỉ hoạt động khi tài khoản có capability Export tương ứng. Có quyền xem không mặc nhiên có quyền xuất.

### Lưu ý
Báo cáo là chức năng tra cứu. Không thể dùng báo cáo để sửa, duyệt hoặc hủy nghiệp vụ. Nếu số liệu cần xử lý, quay về module nghiệp vụ tương ứng.

Chi tiết kiến trúc và ma trận quyền xem `MÔ HÌNH/19_REPORTS_STATISTICS.md`.

## Chọn người phê duyệt khi đăng ký

Trong Leave / OT / Trip / Equipment, người dùng không chọn cấp approve. Hệ thống tự sinh các cấp theo chức vụ, tải danh sách approver của từng cấp và người dùng chọn một người ở mỗi cấp.

Chi tiết thao tác: [21_USER_GUIDE_APPROVAL_SELECTION.md](./21_USER_GUIDE_APPROVAL_SELECTION.md).
