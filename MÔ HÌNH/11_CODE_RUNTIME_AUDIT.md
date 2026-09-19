# CODE RUNTIME AUDIT — Core → Application → Infrastructure → API → Web

> Audit theo code hiện tại trên `main`, đối chiếu với SQL runtime đã chuẩn hóa trong `SQL/`.
> Mục tiêu: xác nhận luồng HRM → FVN_REGISTER và API → Web không chỉ tồn tại trong tài liệu mà có implementation thực tế.

## 1. Kiến trúc runtime đã có

```text
HRM
 │ READ ONLY
 ├── master/config + attendance sources
 │
 └── HRM calculation rules
        │
        ▼
FVN_REGISTER.Infrastructure
 ├── HrmSyncService
 │    └── master/staging synchronization
 │
 └── HrmAttendanceCalculationService
      └── dbo.usp_CalculateHrmAttendance
             │
             ├── HRM.dbo.* READ ONLY
             │
             └── FVN local result tables
                  ├── F03HrmAttendanceCalculated
                  └── F03HrmOTActual
             │
             ▼
FVN_REGISTER.API
             ↓
FVN_REGISTER.Shared client services
             ↓
FVN_REGISTER.Web / Blazor Server
```

## 2. Core

**Đã xử lý.**

- `FVN_REGISTER.Core` giữ entities, enums, repositories và các abstraction nền tảng.
- Core không tham chiếu Infrastructure/API/Web.
- Các contract/interface nghiệp vụ được đặt ở Application thay vì kéo SQL/EF implementation vào Core.

## 3. Application

**Đã xử lý.**

- Có abstraction cho Auth, HRM Sync, OT attendance, Dashboard.
- `DashboardOrchestrator` là application-level aggregator, không truy cập EF/SQL trực tiếp.
- `IOTAttendanceStagingService` và `IOTAttendanceReconciliationService` phân biệt rõ staging command và reconciliation command.
- Đã hiệu chỉnh comment của staging interface để phản ánh đúng runtime dependency: attendance staging tách khỏi generic HrmSyncJob nhưng `usp_SyncAttendanceStaging` vẫn phụ thuộc `usp_SyncHrmShiftMaster`.

## 4A. HRM attendance calculation — current canonical runtime

The attendance calculation path is now separate from generic HRM synchronization:

```text
Web/UI
  ↓ POST /api/hrm-attendance-calculation/calculate
HrmAttendanceCalculationController
  ↓
IHrmAttendanceCalculationService
  ↓
HrmAttendanceCalculationService
  ↓
EXEC dbo.usp_CalculateHrmAttendance
  ↓
HRM source tables + HRM-compatible calculation rules
  ↓
F03HrmAttendanceCalculated / F03HrmOTActual
```

`DeptCode`, `FromDate`, and `ToDate` are the calculation scope. The SQL procedure creates a `CalculationBatchId` and returns a calculation summary. The service uses an extended SQL timeout for this batch operation and restores the normal timeout afterward.

The calculation engine does not depend on the HRM UI and does not persist to HRM report/result tables. HRM is read-only from FVN's perspective. The temporary calculation context is FVN-local, while the source rules/data remain in HRM.

For OT, `ActualHours` is derived from the HRM-compatible calculated result; there is no separate OT reconciliation pipeline for this purpose.

## 4. Infrastructure

**Đã xử lý.**

### HRM master

`HrmSyncService` thực thi:

1. `usp_SyncHrmShiftMaster` cho shift master.
2. Generic staging importers.
3. Sync jobs theo `SyncOrder`.

### Attendance / OT

`HrmAttendanceCalculationService` gọi `dbo.usp_CalculateHrmAttendance` for the explicit Tính giờ use case.

Generic HRM master synchronization remains separate.

`OTAttendanceStagingService` gọi:

`usp_SyncAttendanceStaging @WorkDate`

Procedure SQL tự refresh shift master trước khi resolve attendance.

`OTAttendanceReconciliationService` gọi:

`usp_SyncOTActualHours @OTDate, @DeptCode`

Đây là command vì procedure cập nhật `ActualHours` trước khi trả result.

### Background worker

`OTAttendanceStagingWorker` chỉ chạy staging tự động. Reconcile không chạy ngầm; admin chủ động thực hiện qua API/UI.

## 5. API

**Đã xử lý cho các luồng chính.**

### Auth

- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `GET /api/auth/profile`
- `PUT /api/auth/profile-update`
- `POST /api/auth/change-password`
- `POST /api/auth/logout`
- session endpoints

Web client đã có `GetProfileAsync`, `UpdateProfileAsync`, `ChangePassword` và logout tương ứng.

### Dashboard

- `GET /api/dashboard`
- yêu cầu JWT
- lấy `UserInfo`
- gọi `IDashboardOrchestrator.BuildAsync`
- trả `ApiResponse<DashboardResponse>`

### HRM Sync

- `GET /api/hrm-sync/status`
- `POST /api/hrm-sync/run`
- `POST /api/hrm-sync/run/{entityType}`

### OT Sync

- staging sync
- worker status
- manual reconciliation
- department lookup

## 6. Web

**Đã xử lý cho authentication + dashboard.**

Luồng hiện tại:

```text
Login.razor
   ↓
IAuthClientService
   ↓
POST api/auth/login
   ↓
JWT
   ↓
ITokenStorage / LocalStorageTokenService
   ↓
CustomAuthStateProvider
   ↓
AuthorizeRouteView
   ↓
Home.razor
   ↓
IDashboardClientService
   ↓
GET api/dashboard
   ↓
DashboardOverview
```

`AuthorizedHttpClient` chịu trách nhiệm gắn Bearer token cho các request authenticated.

## 7. Các điểm cần lưu ý

### A. API không được gọi trực tiếp từ Core/Application

Không đặt `HttpClient`, SQL connection hoặc ASP.NET Controller vào Core/Application.

### B. SQL ownership

HRM là source of truth. FVN_REGISTER chỉ đọc HRM; dữ liệu staging/master/reconciliation thuộc FVN_REGISTER.

### C. Attendance không phải generic HrmSyncJob

Attendance là transaction pipeline riêng. Không nên ép `F03AttendanceStaging` vào generic master-data importer.

### D. Reconciliation là command

Không gọi `ReconcileActualHoursAsync` trong `OnInitializedAsync` hoặc auto-refresh của Dashboard/UI.

## 8. Kết luận

Tại thời điểm audit, chuỗi **Core → Application → Infrastructure → API → Shared/Web** đã có implementation tương ứng với kiến trúc đã thống nhất.

Một thiếu sót runtime được phát hiện và đã sửa trực tiếp trên `main`:

- Web đã có client gọi `api/auth/change-password`.
- AuthService/Application đã có `ChangePassword`.
- API trước đó thiếu endpoint tương ứng.
- Đã bổ sung `POST /api/auth/change-password` và contract `ChangePasswordRequestDto`.

SQL calculation/runtime đã được cập nhật cùng với code để phản ánh boundary mới: `22_00` → `22_01` → `22_02` → `22_03` → `22_04`. Các bảng kết quả attendance/OT thuộc FVN; HRM result tables nằm ngoài boundary và không bị ghi.
