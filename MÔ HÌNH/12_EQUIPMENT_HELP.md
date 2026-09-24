# Hướng dẫn sử dụng — Quản lý thiết bị

## A. Import danh sách thiết bị

1. Mở **Equipment → Import Excel**.
2. Chọn DeptCode/schema tương ứng.
3. Nếu danh sách có giao người phụ trách, bật **Giao cho nhân viên**.
4. Excel phải có EmployeeCode cho mọi dòng.
5. Bấm staging/kiểm tra.
6. Sửa tất cả dòng báo lỗi. Các lỗi thường gặp: mã nhân viên không tồn tại, đã nghỉ việc, không thuộc bộ phận.
7. Chỉ khi số dòng lỗi bằng 0 mới được Commit.

> Không sửa database trực tiếp để bỏ qua bước validation.

## B. Import checklist

Mở **Equipment → Quản lý kiểm tra → Import Checklist từ Excel**.

Mỗi dòng là một hạng mục. Dùng TemplateCode/TemplateName/DeptCode/Frequency giống nhau trên toàn file.

Ví dụ cột:

| TemplateCode | TemplateName | DeptCode | Frequency | ItemCode | ItemLabel | InputType | DisplayOrder |
|---|---|---|---|---|---|---|---|
| PC-MAINT | Checklist PC | IT | Monthly | MON-01 | Kiểm tra nguồn | PassFail | 1 |
| PC-MAINT | Checklist PC | IT | Monthly | MON-02 | Nhiệt độ CPU | Number | 2 |
| PC-MAINT | Checklist PC | IT | Monthly | MON-03 | Tình trạng quạt | Select | 3 |

Với Select, điền OptionsJson, ví dụ: ["OK","NG","N/A"].

Import luôn tạo **Draft**. Hãy kiểm tra nội dung trước khi publish Active.

## C. Yêu cầu sửa chữa

Người được giao/vận hành thiết bị có thể mở thiết bị trên Dashboard hoặc quét QR.

Khi tạo repair:

1. Nhập hiện trạng cần sửa.
2. Chọn **Bộ phận** hoặc **Nhân viên** chịu trách nhiệm sửa chữa.
3. Nếu chọn bộ phận, hệ thống kiểm tra DeptCode tồn tại và lấy toàn bộ nhân viên active trong bộ phận.
4. Nếu chọn nhân viên, hệ thống kiểm tra EmployeeCode active và tự xác định bộ phận.
5. Chọn các cấp approver theo Approval Policy.
6. Gửi duyệt.

Sau khi đủ cấp, hệ thống tự sinh nhiệm vụ. Với bộ phận, tất cả thành viên nhận nhiệm vụ; người đầu tiên xử lý hoàn thành sẽ làm các nhiệm vụ song song còn lại biến mất khỏi danh sách xử lý.

## D. Phản hồi sửa chữa

Người xử lý mở Action/Nhiệm vụ sửa chữa, nhập kết quả và hoàn thành.

Hệ thống:

- ghi RepairHistory;
- lưu RepairFeedback và CompletedAt;
- đóng Action người xử lý;
- cancel Action song song;
- gửi notification và email cho người yêu cầu;
- gửi notification và email cho toàn bộ approver của chính nhánh yêu cầu.

## E. Bàn giao thiết bị

Người tiếp quản đăng nhập bằng tài khoản của mình và lập phiếu bàn giao.

Phiếu phải thể hiện thông tin trước bàn giao. Với thiết bị gồm:

- thông tin tài sản;
- người phụ trách/approver hiện tại;
- lịch sử sửa chữa;
- lịch sử bàn giao;
- checklist gần nhất và kết quả/evidence tham chiếu.

Các thông tin này được snapshot tại thời điểm lập phiếu để approver duyệt đúng hiện trạng.

Khi bàn giao, chọn rõ vai trò cần chuyển (trách nhiệm và/hoặc approver), sau đó chọn **từng tài sản** cần chuyển; không mặc định chuyển toàn bộ tài sản người cũ đang giữ.
Server kiểm tra lại từng AssetId với người cũ ở đúng vai trò tương ứng. Approver chỉ duyệt snapshot đã chụp tại thời điểm lập phiếu.
Sau khi đủ approval, phiếu vào hàng đợi IT. Chỉ IT thực hiện điều chuyển. Hệ thống lưu PostChangeResult và lịch sử bàn giao.

## F. Báo cáo

Vào **Equipment → Báo cáo thiết bị**.

Báo cáo có thể lọc:

- Bộ phận;
- người phụ trách;
- bộ phận sửa chữa;
- khoảng thời gian.

Xuất Excel gồm 4 sheet: TaiSan, LichSuSuaChua, LichSuBanGiao, Checklist.

## G. Quy tắc kiểm soát

Không được:

- gán EmployeeCode không tồn tại;
- gán nhân viên đã nghỉ việc;
- gán nhân viên ngoài phạm vi bộ phận khi import;
- tạo repair không có Department/Employee chịu trách nhiệm;
- hoàn thành cùng một repair hai lần;
- sửa trực tiếp Active checklist version;
- tự điều chuyển thiết bị bỏ qua Approval + IT.


## H. Lưu ý sau staging import

Nếu HRM thay đổi DeptCode của nhân viên sau khi staging nhưng trước Commit, hệ thống kiểm tra lại tại Commit và chặn dòng không còn thuộc đúng DeptCode của batch. Người dùng phải tạo/staging lại dữ liệu theo HRM hiện tại.
