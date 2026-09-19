# Work Calendar & Annual Leave Entitlement

## 1. Mục tiêu

`WorkCalendar` không chứa danh sách ngày làm/nghỉ hard-code và không đọc dữ liệu từ file cấu hình ngày.

Nguồn dữ liệu nghiệp vụ là:
- `dbo.F03WorkYear`: xác định năm làm việc và khoảng hiệu lực.
- `dbo.F03CompanyHoliday`: xác định ngày nghỉ của công ty.
- `dbo.F03Employees.FirstWorkingDate`: mốc bắt đầu tính thâm niên phép.
- `dbo.F03LeaveBalances`: snapshot entitlement theo từng nhân viên và từng năm.

JavaScript chỉ chịu trách nhiệm render FullCalendar; không quyết định ngày làm việc/ngày nghỉ.

## 2. Quy tắc Work Calendar

Một ngày được xác định là ngày làm việc khi:
1. Ngày nằm trong một `F03WorkYear` đang `IsActive = 1`.
2. Không phải Thứ 7 hoặc Chủ nhật.
3. Không tồn tại trong `F03CompanyHoliday`.

Nếu ngày không thuộc `F03WorkYear`, calendar đánh dấu là chưa cấu hình năm làm việc thay vì tự suy đoán.

`F03CompanyHoliday.TinhPhep` chỉ biểu thị quy tắc tính vào phép; bản thân ngày lễ vẫn là ngày nghỉ công ty.

## 3. Quy tắc phép năm

Rule hiện tại:

```text
Phép cơ bản = 12 ngày/năm
Phép thâm niên = floor(Số năm làm việc đủ / 5)
Tổng phép = 12 + Phép thâm niên
```

Số năm làm việc là số năm tròn tính từ `FirstWorkingDate` đến ngày tính entitlement.

| FirstWorkingDate | Ngày tính | Năm đủ | Tổng phép |
|---|---|---:|---:|
| 2021-01-01 | 2026-12-31 | 5 | 13 |
| 2016-01-01 | 2026-12-31 | 10 | 14 |
| 2018-09-20 | 2026-12-31 | 8 | 13 |
| 2022-09-20 | 2026-12-31 | 4 | 12 |

Không áp dụng prorating năm đầu trong rule này.

Nếu nhân viên có `EndWorkingDate` trước ngày kết thúc năm, ngày tính entitlement là `EndWorkingDate`. Nếu nhân viên nghỉ trước khi năm bắt đầu hoặc bắt đầu sau khi năm kết thúc, entitlement của năm đó là 0.

## 4. F03LeaveBalances

`F03LeaveBalances` lưu kết quả tính:
- `EmployeeCode`
- `WorkYear`
- `BaseLeaveDays`
- `SeniorityLeaveDays`
- `TotalDays`
- `YearsOfService`
- `CalculatedAt`

`F03Employees.TotalLeaveDays` chỉ giữ entitlement của năm hiện tại để tương thích với master employee. Không dùng cột này làm lịch sử entitlement.

Lịch sử theo năm phải đọc từ `F03LeaveBalances`.

## 5. Application flow

```text
F03WorkYear ───────────────┐
                           │
F03CompanyHoliday ─────────┼──> IWorkCalendarService
                           │          │
                           │          ▼
                           │    WorkCalendarDto
                           │          │
                           │          ▼
                           │    WorkCalendar.razor
                           │          │
                           │          ▼
                           │    FullCalendar JS
                           │
F03Employees.FirstWorkingDate ──> ILeaveEntitlementService
                                      │
                                      ├── 12 ngày cơ bản
                                      ├── đủ mỗi 5 năm +1
                                      ▼
                               F03LeaveBalances
```

## 6. API

### Work Calendar

```text
GET /api/calendar?from=yyyy-MM-dd&to=yyyy-MM-dd
GET /api/calendar/availability?date=yyyy-MM-dd
```

### Annual Leave

```text
GET /api/leave-entitlements/me?year=2026
```

Endpoint entitlement tính lại snapshot trước khi trả kết quả, nên `F03LeaveBalances` được đồng bộ khi nhân viên truy vấn số phép.

## 7. Shared UI JavaScript

`WorkCalendar.razor` gọi:

```csharp
JS.InvokeVoidAsync("workCalendar.init", ...)
```

Global JS contract:

```javascript
window.workCalendar = {
    init,
    updateEvents,
    destroy
};
```

File nằm tại:

```text
FVN_REGISTER/FVN_REGISTER.Shared/wwwroot/js/workCalendar.js
```

Host `App.razor` load bằng:

```html
<script src="_content/FVN_REGISTER.Shared/js/workCalendar.js"></script>
```

FullCalendar phải được load trước file này và trước `blazor.web.js`.

## 8. SQL deployment

Các script liên quan:

```text
03_Tables.sql
    ↓
06_Seed.sql
    ↓
19_WorkCalendar.sql
    ↓
99_Verify.sql
```

`19_WorkCalendar.sql` hỗ trợ migration tên legacy:

```text
F03WorkYears        -> F03WorkYear
F03CompanyHolidays  -> F03CompanyHoliday
IsPaidLeave         -> TinhPhep
```

Không tạo thêm bảng calendar ngày-ngày. Ngày làm việc được tính từ năm làm việc + weekend + ngày nghỉ công ty.

## 9. Không nằm trong rule hiện tại

Chưa tự suy diễn:
- prorating phép năm đầu tiên;
- chuyển phép năm cũ;
- ngày làm bù Thứ 7/Chủ nhật;
- đổi ngày làm/nghỉ đặc biệt;
- cộng phép theo điều kiện sức khỏe/thâm niên đặc thù.

Nếu cần các nghiệp vụ này, nên bổ sung bảng override/rule riêng thay vì hard-code vào `WorkCalendar`.