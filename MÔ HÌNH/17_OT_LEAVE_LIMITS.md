# 17 — QUY ĐỊNH OT & THÔNG TIN NGHỈ PHÉP TRÊN WORKSPACE

## 1. Mục tiêu

FVN_REGISTER phải tính và hiển thị hạn mức OT nhất quán ở ba tầng:

- **Ngày**: kiểm tra giờ OT của ngày đăng ký.
- **Tuần**: tính từ **Thứ 2 → Chủ nhật**; chỉ chặn khi doanh nghiệp đã cấu hình OTLimitType.Weekly.
- **Tháng**: mặc định nghiệp vụ hiện tại là **40 giờ/tháng** nếu chưa có rule cấu hình.
- **Năm**: mặc định nghiệp vụ hiện tại là **200 giờ/năm**; ngưỡng đặc biệt **300 giờ/năm** là ngưỡng tối đa đang được dùng trong validator hiện tại.

> Giá trị tuần không được tự suy đoán hoặc hard-code. Nếu quy định nội bộ có hạn mức tuần, phải tạo F03OTLimitRules với LimitType = Weekly và LimitHours tương ứng.

## 2. Nguyên tắc tính

```mermaid
flowchart TD
    A[Đơn OT] --> B[Nhân viên + ngày OT]
    B --> C[Giờ OT hiện tại]
    C --> D[Ngày]
    C --> E[Tuần Thứ 2 → Chủ nhật]
    C --> F[Tháng]
    C --> G[Năm]
    D --> H{Vượt limit?}
    E --> I{Có Weekly rule và vượt?}
    F --> J{Vượt Monthly limit?}
    G --> K{Vượt 200h?}
    K --> L{Vượt 300h?}
    H --> M[Reject]
    I --> M
    J --> M
    K --> N[Policy đặc biệt]
    L --> M
```

## 3. Dữ liệu dùng để kiểm tra

Các đơn Rejected và Cancelled không được cộng vào hạn mức.

Khi dữ liệu OT thực tế đã được đối chiếu, validator ưu tiên ActualHours; nếu chưa có actual thì dùng OTHours đăng ký.

Dashboard cá nhân lấy OT Approved để hiển thị số giờ đã sử dụng. Vì vậy:

- **Giờ đã sử dụng** trên Dashboard = OT đã Approved.
- **Giờ dùng để chặn đăng ký** = request còn hiệu lực theo validator; request Pending vẫn chiếm hạn mức để tránh đăng ký chồng vượt giới hạn.

## 4. Hạn mức

| Chu kỳ | Rule | Giá trị mặc định hiện tại | Cách xử lý |
|---|---|---:|---|
| Ngày | Daily | 4h cho loại ngày thường; 12h cho loại ngày ngoài ngày thường nếu chưa có rule | Vượt → lỗi |
| Tuần | Weekly | Không tự đặt | Có rule thì vượt → lỗi |
| Tháng | Monthly | 40h | Vượt → lỗi |
| Năm | Yearly | 200h | Vượt 200h → policy đặc biệt |
| Năm tối đa | Special | 300h | Vượt → lỗi |

LimitHours là giá trị ưu tiên để tính hạn mức. LimitValue được giữ làm compatibility fallback.

## 5. Cảnh báo

Dashboard hiển thị:

- OT tuần: đã dùng / hạn mức nếu Weekly đã cấu hình.
- OT tháng: đã dùng / hạn mức.
- OT năm: đã dùng / hạn mức.
- Cảnh báo khi số dư còn lại gần ngưỡng.

Cảnh báo không thay thế authorization/validation ở server.

## 6. Nghỉ phép trên Workspace

Dashboard cá nhân hiển thị:

- Tổng ngày phép được hưởng.
- Ngày phép đã nghỉ.
- Ngày phép còn lại.
- Ngày nghỉ đã duyệt sắp tới.
- Ngày bắt đầu đợt nghỉ kế tiếp.
- Tổng số ngày của các đợt nghỉ đã duyệt sắp tới.

Chỉ request nghỉ Approved được đưa vào phần “sắp nghỉ”; Pending không được coi là lịch nghỉ chắc chắn.

## 7. Database → Code → UI

```mermaid
flowchart LR
    SQL[F03OTLimitRules] --> V[OTValidator]
    SQL --> Q[OTQueryService]
    V --> API[Validate / Create]
    Q --> BAL[OTBalanceDto]
    BAL --> DASH[DashboardResponse]
    DASH --> UI[Home Workspace]
    LDB[F03LeaveBalances + Approved Leave] --> LQ[LeaveQueryService]
    LQ --> LB[LeaveBalanceDto]
    LB --> DASH
```

## 8. Kiểm tra triển khai

- OTLimitType phải có Daily, Weekly, Monthly, Yearly, Special.
- Validator không được dùng Weekly thay cho Monthly.
- Weekly phải tính đúng từ Thứ 2 đến Chủ nhật.
- Rule phải resolve theo độ ưu tiên Dept + Position > Dept > Position > toàn công ty.
- Dashboard và validator phải dùng cùng nguồn rule.
- Không hiển thị hạn mức tuần giả nếu doanh nghiệp chưa cấu hình.
- Help phải mô tả đúng cách tính và cảnh báo.

## Work Calendar integration

The OT and Leave registration screens use the shared Work Calendar. Company holidays are displayed as calendar information; OT and Leave quota validation remains the responsibility of their respective validators. Trip uses the same calendar projection and is not subject to OT/Leave quota rules.


## 8. OT revision và hạn mức theo Employee

Hạn mức OT phải tính theo effective OT của từng participant/employee, không tính toàn bộ OT Master như một đơn vị duy nhất sau khi có revision.

Khi A điều chỉnh OT trong đơn nhiều người: OT Master = 1; A Effective Revision = R2; B Effective Revision = R1; C Effective Revision = R1.

Validator/query hạn mức của A chỉ sử dụng effective revision của A. Revision Pending có thể được dùng để kiểm tra request mới theo rule chống đăng ký chồng, nhưng không thay thế approved effective revision cho dashboard/payroll cho đến khi được approve.

## 9. OT Approved nhưng không có attendance

Không đưa ngày đó ngay vào ActualHours = 0 để kết luận không đi làm. Phải tạo AttendanceConfirmationStatus = PendingConfirmation và hiển thị ? trên calendar.

- Employee chọn Đi làm → bắt buộc evidence → HC xác nhận.
- Employee chọn Không đi làm → ConfirmedNotWorked.
- Không phản hồi đến ngày khóa công 20 → AutoConfirmedNotWorked.

Sau ConfirmedNotWorked hoặc AutoConfirmedNotWorked, effective OT của employee cho ngày đó không được cộng vào OT đã sử dụng/payroll projection. Bản ghi approval ban đầu vẫn được giữ để audit.

## 10. Không ảnh hưởng participant khác

Mọi command/recalculation liên quan revision phải có phạm vi: OTMasterId + ParticipantId/EmployeeId + RevisionId. Không được dùng OTMasterId đơn độc để cập nhật ActualHours, effective OT hoặc calendar của tất cả nhân viên.


## 11. Shared Calendar policy

OT và Leave cùng tham gia Work Calendar nhưng không được hard-code presentation trong từng module.

- OT đăng ký Calendar Provider với reconciliation ApprovedVsActual/PlannedVsActual theo policy.
- Leave đăng ký Calendar Provider với trạng thái Approved/Used hoặc policy khác.
- Trip dùng cùng framework nếu được Admin bật.
- Module mới có thể tham gia bằng provider/definition mà không sửa calendar aggregator.

Admin quyết định IsEnabled, DisplayMode, NoteMode, ConfirmationMode, ReconciliationMode và Priority.

Calendar chỉ là read projection. Hạn mức OT/Leave vẫn do validator/service của module quyết định; Calendar policy không thay đổi quota.
