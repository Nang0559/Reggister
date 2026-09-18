# 18 — WORK CALENDAR (LỊCH LÀM VIỆC CHUNG)

## 1. Mục tiêu

Work Calendar là lịch dùng chung cho toàn bộ nghiệp vụ đăng ký có liên quan đến thời gian:
- Nghỉ phép
- Tăng ca (OT)
- Công tác (Trip)
- Ngày nghỉ/lễ của công ty

Màn hình lịch cũ của Leave không còn được xem là một thành phần riêng của Leave. Leave, OT và Trip cùng sử dụng một lịch nền thống nhất.

## 2. Nguyên tắc kiến trúc

```text
                 COMPANY CALENDAR
                 F03CompanyHolidays
                        |
                        v
                 +-------------+
                 | WORK        |
                 | CALENDAR    |
                 +-------------+
                    /   |   \
                   /    |    \
                Leave   OT   Trip
                  |      |     |
             Leave DB  OT DB  Trip DB
```

Work Calendar là read projection. Nó không sở hữu trạng thái nghiệp vụ.

### Source of truth

| Dữ liệu | Nguồn |
|---|---|
| Nghỉ/lễ công ty | F03CompanyHolidays |
| Đăng ký nghỉ | VF03LeaveRequest |
| Đăng ký OT | VF03OTRequest |
| Đăng ký công tác | F03TripRequests |
| Quyền xem dữ liệu | IAuthorizationService |
| Quy tắc nghiệp vụ cuối cùng | Leave/OT/Trip service + validator |

## 3. Không được dùng Holiday như một global lock

Không áp dụng quy tắc CompanyHoliday => tất cả module bị khóa.

Thay vào đó mỗi module tự quyết định theo policy của mình. Một ngày lễ có thể không cho Leave thông thường nhưng vẫn có thể là ngày OT theo chính sách ngày lễ; Trip cũng có thể kéo dài qua ngày lễ.

Calendar chỉ hiển thị/cảnh báo; quyết định cuối cùng phải do server-side validator của module.

## 4. Contract

WorkCalendarDto gồm From, To, Days, Events.

WorkCalendarDayDto gồm Date, IsWeekend, IsWorkingDay, HolidayCode, HolidayName, CanRegisterLeave, CanRegisterOT, CanRegisterTrip, AvailabilityNote.

WorkCalendarEventDto gồm Id, ModuleCode, EventType, Title, Start, End, Status, RequestId, IsReadOnly.

ModuleCode chuẩn: COMPANY, LEAVE, OT, TRIP.

## 5. API

```http
GET /api/calendar?from=2026-09-01&to=2026-10-15
GET /api/calendar/availability?date=2026-09-19
```

API chỉ trả dữ liệu của nhân viên hiện tại. Client không được truyền EmployeeCode tùy ý để lấy dữ liệu của người khác.

API yêu cầu người dùng có ít nhất một quyền xem: LeaveView, OTView hoặc TripView.

## 6. UI

Các màn hình /leave/create, /ot/create và /trip/create đều nhúng cùng WorkCalendar.

Bộ lọc: Tất cả, Công ty, Nghỉ phép, OT, Công tác.

Click ngày được chuyển về host module: Leave mở đăng ký Leave; OT chọn ngày OT; Trip chọn ngày bắt đầu.

Click event: Leave mở chi tiết Leave; OT mở chi tiết OT; Trip mở chi tiết Trip; Company chỉ hiển thị thông tin.

## 7. Quy tắc hiển thị

- Company holiday: background/read-only.
- Leave: event theo khoảng ngày.
- OT: event theo ngày + giờ.
- Trip: event theo khoảng ngày.
- Weekend: style riêng.
- Status request được giữ nguyên.

Calendar không cho phép sửa trực tiếp event nghiệp vụ.

## 8. Range loading

Calendar tải theo vùng ngày đang hiển thị. API giới hạn tối đa 94 ngày cho mỗi lần tải.

Khi người dùng chuyển tháng, datesSet của FullCalendar gọi lại server để tải vùng mới.

## 9. Performance

Các index hỗ trợ: IX_F03CompanyHolidays_HolidayDate, IX_F03LeaveDays_Calendar, IX_F03OTRequests_Calendar, IX_F03TripRequests_Calendar.

Không tạo một bảng WorkCalendar vật lý để copy dữ liệu request; tránh duplicate state.

## 10. Authorization

Work Calendar không phải một quyền nghiệp vụ mới.

```text
Calendar View
   |
   +-- LeaveView
   +-- OTView
   +-- TripView
```

Nếu người dùng không có cả ba quyền trên thì API trả 403.

Quyền xem calendar không đồng nghĩa với Create, Approve, Edit hoặc Cancel.

## 11. Business rule boundary

```text
                 Work Calendar
                 /            \
             warning          navigation
                |
                v
        Module Service/Validator
                |
        final business decision
```

Client không được coi CanRegisterX là bằng chứng đủ để commit request.

## 12. SQL

SQL/19_WorkCalendar.sql chỉ tạo index phục vụ projection và ghi rõ policy boundary.

Không tạo foreign key chéo giữa Leave/OT/Trip chỉ để phục vụ calendar.

## 13. Migration từ Leave Calendar

Code mới dùng workCalendar.js, WorkCalendar.razor, IWorkCalendarService và IWorkCalendarClientService.

LeaveCreate chỉ giữ logic Leave; calendar lifecycle đã được tách khỏi Leave.

## 14. Kiểm thử bắt buộc

- Company holiday hiển thị đúng và read-only.
- Holiday không tự động khóa OT/Trip.
- Leave event đúng khoảng ngày và click mở đúng đơn.
- OT event đúng giờ và click mở đúng chi tiết.
- Trip event đúng khoảng ngày và click mở đúng chi tiết.
- Click ngày cập nhật đúng field của module host.
- User chỉ có một trong LeaveView/OTView/TripView chỉ thấy dữ liệu thuộc phạm vi được phép.
- User không có cả ba quyền nhận 403.
- Client không được dùng calendar để bypass validator hoặc authorization.