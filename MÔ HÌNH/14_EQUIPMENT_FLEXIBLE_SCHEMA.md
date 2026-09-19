# 14 — Equipment Flexible Schema & Excel Import

## Mục tiêu

Các phòng ban không bị ép dùng cùng một bộ cột thiết bị. Hệ thống giữ một canonical core dùng chung và cho phép mỗi phòng ban khai báo thêm các trường nghiệp vụ riêng.

Luồng tổng thể:

Excel phòng ban -> Import Gateway -> Staging Batch/Rows -> Validate theo Schema -> Review lỗi -> Commit -> Equipment Assets.

## 1. Không dùng EAV thuần túy

Không biến toàn bộ thiết bị thành FieldId/Value vì như vậy các truy vấn chuẩn, index và báo cáo sẽ khó kiểm soát.

Thiết kế gồm:

- Core fields: EquipmentCode, EquipmentName, Specification, SerialNumber, AssetCode, PurchasePrice, PurchaseDate, ExpectedDepreciationDate, DeptCode, Location, QR, Note.
- Department fields: metadata tại F03EquipmentFieldDefinitions.
- Values của field riêng: JSON tại F03EquipmentAssets.CustomDataJson.

Như vậy một component có thể dùng cho mọi phòng ban mà không cần tạo bảng Equipment riêng cho từng phòng.

## 2. Ví dụ

IT có thể khai báo CPU, RAM, IP, MAC, OS.

QA/QC có thể khai báo ngày hiệu chuẩn, ngày kiểm định, tiêu chuẩn, kết quả hiệu chuẩn.

Sản xuất có thể khai báo công suất, Line, Machine No, chu kỳ bảo trì, nhà cung cấp.

Không cần tạo F03EquipmentIT, F03EquipmentQC hoặc F03EquipmentProduction.

## 3. Schema phòng ban

F03EquipmentFieldDefinitions quản lý:

| Field | Ý nghĩa |
|---|---|
| DeptCode | Schema thuộc phòng ban |
| FieldKey | Key ổn định, dùng trong JSON/API |
| FieldLabel | Nhãn hiển thị |
| DataType | Text / Number / Date / Boolean / Choice |
| IsRequired | Bắt buộc hay không |
| IsImportable | Có nhận từ Excel hay không |
| IsSearchable | Có dùng làm điều kiện tìm kiếm hay không |
| DisplayOrder | Thứ tự UI |
| OptionsJson | Danh sách Choice |

## 4. Excel không ghi thẳng vào Asset

Luồng import bắt buộc:

Upload -> Staging -> Header mapping -> Validation -> Review -> Commit.

F03EquipmentImportBatches và F03EquipmentImportRows giữ lịch sử import để biết ai upload, file nào, phòng ban nào, dòng nào lỗi và dòng nào đã import.

## 5. Header thông minh

Import không yêu cầu Excel phải dùng đúng tên tiếng Anh. Hệ thống chuẩn hóa header và nhận alias.

Ví dụ:

- Mã thiết bị / Mã TB / Mã tài sản -> EquipmentCode
- Tên thiết bị / Tên TB / Tên tài sản -> EquipmentName
- Số serial / Serial / S/N -> SerialNumber
- Nguyên giá / Giá mua -> PurchasePrice

Các cột không phải canonical field vẫn được giữ trong CustomDataJson để không mất dữ liệu Excel.

## 6. Authorization

Capability mới:

Equipment.Import = 2306
Scope = Department

Không dùng IsAdmin để bypass scope. User chỉ được import vào department mà AuthorizationService.CanAccessAsync cho phép.

## 7. UI tương lai

Equipment:
- Danh sách
- Quét QR
- Sửa chữa
- Import Excel
- Cấu hình trường phòng ban

Form thiết bị render từ Core fields cộng với F03EquipmentFieldDefinitions của DeptCode.

## 8. Quy tắc

Không cho Excel tự tạo schema. Schema phải được cấu hình trước bởi người có capability Equipment.Import. Excel chỉ cung cấp dữ liệu.

Điều này tránh typo header tạo ra hàng loạt field rác.
