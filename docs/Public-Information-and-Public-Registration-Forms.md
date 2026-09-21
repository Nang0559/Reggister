# FVN_REGISTER — Public Information và Public Registration Forms

## 1. Hai module khác nhau
Public Information dùng để công bố nội dung cho người dùng đọc: thông báo, tin tức, nội dung HR, thông tin công ty và nội dung công khai. Entity hiện tại là F03PublicInformation và không được dùng làm nơi lưu câu trả lời đăng ký.

Public Registration Forms là module nghiệp vụ riêng: HR tạo biểu mẫu động, cấu hình câu hỏi, lựa chọn đối tượng được xem/gửi, mở/đóng thời gian và thu thập submission.

## 2. Public Information UI
Menu quản trị: Public → Quản lý nội dung công khai.
Các loại chuẩn: Announcement, News, HRContent, CompanyInfo, PublicContent.
User portal: /public.

## 3. Public Registration Forms
Lifecycle: Draft → Published → Closed → Archived.
Entities: F03PublicForms, F03PublicFormQuestions, F03PublicFormQuestionOptions, F03PublicFormAudiences, F03PublicFormSubmissions, F03PublicFormAnswers.
Audience v1: AllCompany, Department, Position, Employee; nhiều rows có OR semantics.
Server phải kiểm tra audience khi lấy form và khi submit; không chỉ ẩn UI.
Question v1: Text, Textarea, Number, Date, Time, DateTime, SingleChoice, MultiChoice, YesNo, Department, Employee, File.
Submission/Answer lưu tách biệt, có typed values.

## 4. Security
RBAC xác định user có quyền sử dụng module; Audience xác định user có quyền truy cập một form cụ thể.

## 5. SQL deployment
Chạy SQL/37_PublicRegistrationForms.sql sau schema nền. Script idempotent cho create table/index và không xóa dữ liệu hiện hữu.

## 6. Approval
RequireApproval được lưu để mở rộng. Phase 1 không ép mọi form qua Approval Engine.
