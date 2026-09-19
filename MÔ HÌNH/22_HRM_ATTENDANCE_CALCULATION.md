# 22 — HRM-COMPATIBLE ATTENDANCE CALCULATION

## 1. Mục tiêu

FVN_REGISTER cho phép người dùng chọn **Bộ phận + Từ ngày + Đến ngày** và bấm **Tính giờ** mà không cần mở giao diện HRM.

Kết quả phải bám theo luật/chains thực tế của HRM và được lưu độc lập trong database FVN_REGISTER.

## 2. Nguyên tắc nguồn dữ liệu

HRM là nguồn dữ liệu và luật tính chấm công. FVN chỉ READ các bảng nguồn HRM.

FVN không mở HRM UI, không ghi HRM.dbo.tblBaoCao hoặc HRM.dbo.tblBaoCaoK, không dùng trigger để đồng bộ và không tạo một bộ công thức Work/OT độc lập với HRM.

## 3. SQL Server boundary

HRM và FVN_REGISTER cùng SQL Server nên không cần linked server.

```text
FVN_REGISTER
   │ READ
   └──────> HRM.dbo.*
   │
   └ WRITE -> FVN_REGISTER.dbo.F03Hrm*
```

Ba-part name HRM.dbo.<table> là boundary chính thức giữa hai database.

## 4. Runtime chain

```text
FVN Web
   ↓ POST /api/hrm-attendance-calculation/calculate
HrmAttendanceCalculationController
   ↓
IHrmAttendanceCalculationService
   ↓
HrmAttendanceCalculationService
   ↓
dbo.usp_CalculateHrmAttendance
   ↓
HRM source data + HRM-compatible calculation rules
   ↓
FVN local calculation context (#tblBaoCao)
   ↓
F03HrmAttendanceCalculated
   ↓
F03HrmOTActual
   ↓
F03OTEmployees.ActualHours
```

## 5. HRM calculation compatibility

Phần tính Work/OT phải bám theo chain thực tế của HRM, đặc biệt logic của sphrmvn_TimeKeepingForStaff_K và sphrmvn_FindShift_New / FindShiftOfStaff.

FVN chỉ thay persistence/context boundary:

```text
HRM: calculation → tblBaoCao / tblBaoCaoK
FVN: calculation-compatible logic → #tblBaoCao → F03HrmAttendanceCalculated → F03HrmOTActual
```

Không thay đổi business formula chỉ vì FVN có schema khác.

## 6. FVN result tables

F03HrmAttendanceCalculated là snapshot kết quả của một lần tính: CalculationBatchId, WorkDate, HrmEmployeeId, EmployeeCode, DeptCode, shift, CheckIn/CheckOut, Work minutes, OT minutes, Late/Early, Required minutes và các trường HRM leave/holiday.

F03HrmOTActual là projection OT thực tế phục vụ request OT: thời gian vào/ra, tổng phút, OT ngày/đêm, OT được ghi nhận, source attendance row và CalculationBatchId.

F03OTEmployees.ActualHours được cập nhật từ calculation result trong cùng SQL orchestration.

## 7. Calculation batch

Mỗi lần người dùng bấm Tính giờ tạo một CalculationBatchId. Batch dùng để truy vết, phân biệt các lần chạy, export đúng snapshot và đối chiếu với HRM.

Summary trả về: CalculationBatchId, DeptCode, FromDate, ToDate, EmployeeCount, CalculatedRows, StartedAt, FinishedAt, CalculationVersion.

## 8. API

```http
POST /api/hrm-attendance-calculation/calculate
GET  /api/hrm-attendance-calculation/export/attendance/{batchId}
GET  /api/hrm-attendance-calculation/export/ot/{batchId}
```

Request gồm DeptCode, FromDate và ToDate.

## 9. Application / Infrastructure responsibility

Application định nghĩa contract IHrmAttendanceCalculationService và các DTO request/result; không biết bảng HRM cụ thể.

Infrastructure HrmAttendanceCalculationService validate date range, gọi dbo.usp_CalculateHrmAttendance, dùng timeout dài cho batch, trả summary, log lỗi kỹ thuật và khôi phục timeout mặc định.

API yêu cầu authentication, nhận department/date range, truyền EmployeeCode người chạy vào TriggeredBy và không tự tính Work/OT.

## 10. Background calculation

HrmAttendanceCalculationWorker dùng cùng IHrmAttendanceCalculationService. Manual và background phải đi qua cùng dbo.usp_CalculateHrmAttendance.

## 11. Export

Excel chỉ đọc F03HrmAttendanceCalculated và F03HrmOTActual theo CalculationBatchId, không export trực tiếp từ HRM.dbo.tblBaoCao.

## 12. Kiểm thử đối chiếu

Baseline thực tế đã xác nhận:

```text
Employee: FCC1331
HrmEmployeeId: 207
Date: 2026-09-18
BCMaCa = 1
BCTGDen = 07:50:01
BCTGVe = 20:02:25
BCTGLamNgay = 480
BCTGQuaGioNgay = 197
BCTGThemNgay = 197
BCTGLamToi = 0
BCTGQuaGioToi = 0
BCTGThemToi = 0
BCTGDiMuonNgay = 0
BCTGVeSomNgay = 0
BCTGQuyDinh = 480
BCTinhLamThem = 1
```

FVN phải đối chiếu các trường tương ứng trong F03HrmAttendanceCalculated.

## 13. Không quay lại kiến trúc cũ

Không tạo lại công thức Work/OT C# riêng, pipeline reconciliation thay thế calculation engine, attendance staging làm nguồn tính chính, trigger trên HRM result table, ghi ngược vào HRM hoặc phụ thuộc HRM UI.

## 14. SQL deployment

```text
22_00_HrmAttendanceTables.sql
22_01_InterSectionTime3.sql
22_02_HrmCompatibleTimeKeepingForStaff.sql
22_03_CalculateHrmAttendance.sql
22_04_GetHrmAttendanceCalculation.sql
```

00_Deploy_All.sql là entry point deploy.

## 15. Nguyên tắc cuối

> **HRM sở hữu luật và dữ liệu chấm công. FVN sở hữu lần chạy, snapshot kết quả và giao diện sử dụng.**

FVN phải chạy độc lập về UI nhưng không được tạo một HRM thứ hai về business formula.