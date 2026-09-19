# Hướng dẫn — Tính giờ theo HRM

1. Đăng nhập FVN_REGISTER.
2. Mở HRM → Tính giờ.
3. Chọn Bộ phận. Mã phải tương ứng HRM.dbo.tblNhanVien.NVMaBP.
4. Chọn Từ ngày và Đến ngày.
5. Nhấn Tính giờ.
6. Ghi lại CalculationBatchId nếu cần đối soát.
7. Báo cáo chấm công/OT đọc snapshot FVN, không đọc HRM.tblBaoCao.

Chạy lại sau khi HRM nhận đủ quẹt thẻ hoặc cấu hình ca/nghỉ/ưu đãi thay đổi. Batch cũ không bị sửa.

Nếu không có kết quả: kiểm tra RecordDataNew, tblCapThe, NVMaCa, tblCa và quyền đọc database HRM.


## OT integration

Không cần chạy một màn hình đồng bộ OT riêng. Sau khi HRM có đủ dữ liệu chấm công, chạy **Tính giờ theo HRM**. CalculationBatch mới sẽ tạo snapshot chấm công/OT và cập nhật ActualHours cho các dòng OT đã Approved trong cùng pipeline. OT UI chỉ phục vụ đăng ký, danh sách, chi tiết và theo dõi tiến trình phê duyệt.
