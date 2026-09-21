# FVN_REGISTER — Public Information và Public Registration Forms

## 1. Hai module khác nhau
Public Information chỉ công bố nội dung để người dùng đọc: Thông báo, Tin tức, Nội dung HR, Thông tin công ty, Nội dung được công khai. Entity: F03PublicInformation.

Public Registration Forms là module nghiệp vụ riêng để HR tự tạo biểu mẫu động, cấu hình câu hỏi, đối tượng được phép xem/gửi và thu thập submission. Không dùng PublicInformation để lưu answer.

## 2. Chuẩn kiến trúc
Core: Entities/PublicForms.
Contract/Models: Dtos/PublicForms và Requests/PublicForms.
Application: Interfaces/PublicForms.
Infrastructure: Services/PublicForms.
API: PublicFormsController kế thừa BaseApiController.
Shared: employee pages và HR/Admin pages.

Controller dùng UserInfo, HandleResult, LogActionAsync và IAuthorizationService.HasAsync theo chuẩn controller hiện hữu.

## 3. Public Information UI
/admin/public-information: HR/Admin tạo, sửa, Publish, Archive.
/public: employee xem nội dung đã Publish.
Loại chuẩn: Announcement, News, HRContent, CompanyInfo, PublicContent.

## 4. Public Registration Forms
/admin/public-forms: thiết kế form, question, option, audience, Publish, Close.
/public-forms: employee xem form phù hợp.
/public-forms/{id}: employee nhập và submit.
Lifecycle: Draft -> Published -> Closed -> Archived.

## 5. Audience và Security
RBAC xác định ai được quản trị module. Audience xác định ai được truy cập từng form.
FunctionCode 2807 = Public Registration Form - Manage.
Không có 2807 thì không được quản trị. Employee không cần 2807 để submit form phù hợp Audience.
Audience v1: AllCompany, Department, Position, Employee. Nhiều rows dùng OR semantics.
Backend phải kiểm tra Audience cả khi lấy form và khi submit; không dựa vào hidden UI.

## 6. Question và Answer
Question v1: Text, Textarea, Number, Date, Time, DateTime, SingleChoice, MultiChoice, YesNo, Department, Employee, File.
Choice phải có Options.
Answer tách typed values: TextValue, NumberValue, DateValue, BoolValue, JsonValue.

## 7. Submission validation
Backend kiểm tra Published, StartAt/EndAt, Audience, duplicate submission, MaxSubmissions, required questions, active questions và option hợp lệ.

## 8. Approval
RequireApproval chỉ là điểm tích hợp với Approval Engine hiện tại. Không tạo approval framework thứ hai. Khi tích hợp: Form -> Submission -> ApprovalRoute/ApprovalEngine -> Approved/Rejected.

## 9. SQL
SQL/37_PublicRegistrationForms.sql tạo schema và seed permission 2807; SQL/00_Deploy_All.sql gọi script này.
Audit fields phải đồng bộ BaseAuditEntity: Id, IsActive, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt.

## 10. Acceptance
Authorization: không có 2807 không quản trị; có 2807 quản trị; employee không có 2807 vẫn submit được form hợp Audience.
Audience: AllCompany, Department, Position, Employee và OR nhiều rows.
Submission: required, duplicate, max, closed, expired, invalid option, unauthorized audience.
Lifecycle: Draft không submit; Published nhận submit; Closed/Archived không nhận.

## 11. Không được làm
Không tạo form riêng cho từng chương trình; không nhúng vào Leave/OT/Trip; không tạo RBAC riêng; không tạo approval engine riêng; không dùng PublicInformation để lưu submission.