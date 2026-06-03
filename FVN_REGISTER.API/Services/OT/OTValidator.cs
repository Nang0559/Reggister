using FVN_REGISTER.Contract.Interfaces.OT;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Contract.ViewModels.OT;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;

namespace FVN_REGISTER.API.Services.OT
{
    public class OTValidator : IOTValidator
    {
        // ----------------------------------------------------------
        // Hằng số giới hạn giờ theo QĐ-HC-03
        // ----------------------------------------------------------
        private const decimal MAX_HOURS_NORMAL_DAY = 4m;
        private const decimal MAX_HOURS_HOLIDAY_DAY = 12m;   // Weekend | Holiday | Tet
        private const decimal MAX_HOURS_PER_MONTH = 40m;
        private const decimal MAX_HOURS_PER_YEAR = 200m;  // Ngưỡng cảnh báo thông thường
        private const decimal MAX_HOURS_YEAR_SPECIAL = 300m; // Ngưỡng tuyệt đối

        // DayType được coi là ngày nghỉ/lễ/Tết
        private static readonly HashSet<string> HolidayTypes =
            new(StringComparer.OrdinalIgnoreCase)
            { "Weekend", "Holiday", "Tet" };

        private readonly FVNWEBAPPContext _db;

        public OTValidator(FVNWEBAPPContext db) => _db = db;

        // ==============================================================
        // Entry point — chạy tuần tự 5 nhóm validate
        // ==============================================================
        public async Task<ServiceResult> ValidateAsync(
            CreateOTRequestModel model,
            CurrentUser user,
            CancellationToken ct = default)
        {
            try
            {
                // 1. Trường cơ bản
                var r = ValidateHeader(model, user);
                if (!r.IsSuccess) return r;

                // 2. Từng nhân viên trong ca OT (giờ, làm tròn 15', giới hạn ngày)
                r = ValidateEmployeeRows(model);
                if (!r.IsSuccess) return r;

                // 3. Tích lũy tháng / năm từ DB
                r = await ValidateAccumulatedAsync(model, user.EmployeeCode!, ct);
                if (!r.IsSuccess) return r;

                // 4. Chuỗi phê duyệt 4 cấp
                r = ValidateApproverChain(model, user);
                if (!r.IsSuccess) return r;

                // 5. Trùng lặp với đơn OT đã tồn tại
                r = await ValidateOverlapAsync(model, ct);
                if (!r.IsSuccess) return r;

                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail($"Lỗi hệ thống khi validate OT: {ex.Message}");
            }
        }

        // ==============================================================
        // 1. Header — trường bắt buộc
        // ==============================================================
        private static ServiceResult ValidateHeader(
            CreateOTRequestModel model, CurrentUser user)
        {
            if (string.IsNullOrWhiteSpace(user.EmployeeCode))
                return Fail("Không xác định được mã nhân viên.");

            if (string.IsNullOrWhiteSpace(user.Email))
                return Fail("Tài khoản chưa có địa chỉ email.");

            if (string.IsNullOrWhiteSpace(model.OTReason))
                return Fail("Lý do OT không được để trống.");

            if (model.OTDate == default || model.OTDate < DateTime.Today.AddDays(-30))
                return Fail("Ngày OT không hợp lệ (không được quá 30 ngày về trước).");

            if (model.OTDate > DateTime.Today.AddDays(30))
                return Fail("Ngày OT không được đặt quá 30 ngày tới.");

            if (model.Employees == null || model.Employees.Count == 0)
                return Fail("Phải có ít nhất 1 nhân viên trong ca OT.");

            if (model.WorkYear < DateTime.Now.Year - 1 || model.WorkYear > DateTime.Now.Year + 1)
                return Fail($"Năm làm việc không hợp lệ: {model.WorkYear}.");

            return ServiceResult.Ok();
        }

        // ==============================================================
        // 2. Validate từng dòng OTEmployeeModel
        //    - PlannedFrom / PlannedTo hợp lệ
        //    - Làm tròn 15 phút
        //    - Giới hạn 4h (ngày thường) hoặc 12h (lễ/Tết/cuối tuần)
        //    - Tính lại PlannedHours (server không tin client)
        // ==============================================================
        private ServiceResult ValidateEmployeeRows(CreateOTRequestModel model)
        {
            bool isHolidayType = HolidayTypes.Contains(model.DayType);
            decimal limitPerDay = isHolidayType
                ? MAX_HOURS_HOLIDAY_DAY
                : MAX_HOURS_NORMAL_DAY;

            for (int i = 0; i < model.Employees.Count; i++)
            {
                var emp = model.Employees[i];
                int line = i + 1;

                // --- Mã nhân viên ---
                if (string.IsNullOrWhiteSpace(emp.EmployeeCode))
                    return Fail($"Nhân viên dòng {line}: Mã nhân viên không được để trống.");

                // --- Ngày phải khớp với header ---
                DateOnly otDate = DateOnly.FromDateTime(model.OTDate);

                if (DateOnly.FromDateTime(emp.PlannedFrom) != otDate)
                    return Fail($"Nhân viên [{emp.EmployeeCode}]: " +
                                $"Ngày giờ bắt đầu ({emp.PlannedFrom:dd/MM/yyyy}) " +
                                $"phải trùng với ngày OT ({model.OTDate:dd/MM/yyyy}).");

                // --- Thứ tự giờ ---
                if (emp.PlannedFrom >= emp.PlannedTo)
                    return Fail($"Nhân viên [{emp.EmployeeCode}]: " +
                                $"Giờ bắt đầu ({emp.PlannedFrom:HH:mm}) " +
                                $"phải nhỏ hơn giờ kết thúc ({emp.PlannedTo:HH:mm}).");

                // --- Bội số 15 phút ---
                if (!IsRounded15Min(emp.PlannedFrom) || !IsRounded15Min(emp.PlannedTo))
                    return Fail($"Nhân viên [{emp.EmployeeCode}]: " +
                                $"Giờ OT phải là bội số 15 phút (ví dụ: 17:00, 17:15, 17:30...).");

                // --- Tính lại PlannedHours phía server ---
                decimal computedHours = ComputeHours(emp.PlannedFrom, emp.PlannedTo);
                emp.PlannedHours = computedHours;   // Ghi đè giá trị client gửi lên

                if (computedHours <= 0)
                    return Fail($"Nhân viên [{emp.EmployeeCode}]: Số giờ OT phải lớn hơn 0.");

                // --- Giới hạn ngày ---
                if (computedHours > limitPerDay)
                {
                    string dayLabel = isHolidayType ? "nghỉ/lễ/Tết" : "thường";
                    return Fail($"Nhân viên [{emp.EmployeeCode}]: " +
                                $"Ngày {dayLabel} vượt giới hạn {limitPerDay}h/ngày. " +
                                $"Số giờ tính được: {computedHours}h.");
                }
            }

            // Tổng giờ theo header (lấy từ employee đầu tiên = người tạo đơn)
            var creator = model.Employees
                .FirstOrDefault(e => e.EmployeeCode == model.EmployeeCode);

            if (creator != null)
                model.PlannedHours = creator.PlannedHours;

            return ServiceResult.Ok();
        }

        // ==============================================================
        // 3. Tích lũy tháng / năm — query DB
        //    Chỉ tính đơn IsActive=true, không Rejected/Cancelled
        // ==============================================================
        private async Task<ServiceResult> ValidateAccumulatedAsync(
            CreateOTRequestModel model,
            string employeeCode,
            CancellationToken ct)
        {
            int month = model.OTDate.Month;
            int year = model.OTDate.Year;

            // Tổng giờ mới đăng ký trong ca này (tất cả NV không tính — chỉ tính cá nhân)
            decimal newHours = model.Employees
                .Where(e => e.EmployeeCode == employeeCode)
                .Select(e => e.PlannedHours)
                .FirstOrDefault();

            if (newHours <= 0) return ServiceResult.Ok(); // Không có dòng của người tạo

            // --- Tháng ---
            decimal usedMonth = await _db.VF03OTRequests
                .AsNoTracking()
                .Where(x => x.EmployeeCode == employeeCode
                         && x.IsActive == true
                         && x.OTMonth == month
                         && x.OTYear == year
                         && x.RequestStatus != OTStatus.Rejected
                         && x.RequestStatus != OTStatus.Cancelled)
                .SumAsync(x => (decimal?)x.PlannedHours ?? 0m, ct);

            if (usedMonth + newHours > MAX_HOURS_PER_MONTH)
                return Fail(
                    $"Tháng {month}/{year}: Vượt giới hạn {MAX_HOURS_PER_MONTH}h/tháng. " +
                    $"Đã đăng ký: {usedMonth}h · Thêm: {newHours}h · " +
                    $"Tổng sẽ là: {usedMonth + newHours}h.");

            // --- Năm ---
            decimal usedYear = await _db.F03OTRequests
                .AsNoTracking()
                .Where(x => x.EmployeeCode == employeeCode
                         && x.IsActive == true
                         && x.OTYear == year
                         && x.RequestStatus != OTStatus.Rejected
                         && x.RequestStatus != OTStatus.Cancelled)
                .SumAsync(x => (decimal?)x.PlannedHours ?? 0m, ct);

            if (usedYear + newHours > MAX_HOURS_YEAR_SPECIAL)
                return Fail(
                    $"Năm {year}: Vượt giới hạn tối đa {MAX_HOURS_YEAR_SPECIAL}h/năm. " +
                    $"Đã đăng ký: {usedYear}h · Thêm: {newHours}h.");

            // Cảnh báo mềm vượt 200h (không block, chỉ ghi vào Message)
            // Thực tế có thể trả riêng Warning flag nếu cần UX cảnh báo
            // Hiện tại: im lặng nếu 200 < total <= 300

            return ServiceResult.Ok();
        }

        // ==============================================================
        // 4. Chuỗi phê duyệt — theo đúng QĐ-HC-03
        //
        //   Lv3  Sub.Leader/Leader   → bắt buộc nếu KHÔNG phải VP
        //   Lv4  BCH Công đoàn       → bắt buộc tất cả
        //   Lv5  Ast.Chief/Chief     → bắt buộc tất cả
        //   Lv6  A.MG/MG             → bắt buộc tất cả
        //   Lv7  GM                  → bắt buộc khi:
        //          · DayType = Weekend | Holiday | Tet  (OT ngày nghỉ/lễ/Tết)
        //          · CreatedByLevel = Ast.Chief+       (người đăng ký cấp cao)
        //          · LevelApprove >= 5 trong CurrentUser
        // ==============================================================
        private static ServiceResult ValidateApproverChain(
            CreateOTRequestModel model,
            CurrentUser user)
        {
            // ── Bước 3: Sub.Leader (chỉ công nhân — CvCode "0003") ──
            bool isOfficeStaff = !string.Equals(
                user.CvCode, "0003", StringComparison.OrdinalIgnoreCase);

            model.Lv3Skip = isOfficeStaff;   // Ghi lại vào model để Service dùng

            if (!isOfficeStaff && string.IsNullOrWhiteSpace(model.Lv3ApproveEmail))
                return Fail("Công nhân phải chọn Sub.Leader / Leader ký xác nhận (Bước 3).");

            // ── Bước 4: BCH Công đoàn ──
            if (string.IsNullOrWhiteSpace(model.Lv4ApproveEmail))
                return Fail("Phải chọn đại diện BCH Công đoàn (Bước 4).");

            // ── Bước 5: Ast.Chief / Chief ──
            if (string.IsNullOrWhiteSpace(model.Lv5ApproveEmail))
                return Fail("Phải chọn Ast.Chief / Chief (Bước 5).");

            // ── Bước 6: A.MG / MG ──
            if (string.IsNullOrWhiteSpace(model.Lv6ApproveEmail))
                return Fail("Phải chọn A.MG / MG (Bước 6).");

            // ── Bước 7: GM — tính RequiresGM ──
            bool hasHolidayOT = HolidayTypes.Contains(model.DayType);
            bool isHighLevel = user.LevelApprove >= 5;
            bool requiresGM = hasHolidayOT || isHighLevel;

            // Server tính lại, không tin giá trị client gửi lên
            model.RequiresGM = requiresGM;

            if (requiresGM && string.IsNullOrWhiteSpace(model.Lv7ApproveEmail))
            {
                string reason = hasHolidayOT
                    ? $"OT ngày {model.DayType} (nghỉ/lễ/Tết)"
                    : "người đăng ký từ cấp Ast.Chief trở lên";
                return Fail($"Phải chọn GM ký phê duyệt (Bước 7) vì {reason}.");
            }

            // ── Không được tự duyệt đơn của mình ──
            var approverEmails = new[]
            {
                model.Lv3ApproveEmail, model.Lv4ApproveEmail,
                model.Lv5ApproveEmail, model.Lv6ApproveEmail,
                model.Lv7ApproveEmail
            }
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .ToList();

            if (approverEmails.Any(e =>
                string.Equals(e, user.Email, StringComparison.OrdinalIgnoreCase)))
                return Fail("Không được chọn chính mình làm người ký duyệt.");

            // ── Không được trùng email giữa các bước ──
            var dupes = approverEmails
                .GroupBy(e => e!.ToLowerInvariant())
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (dupes.Any())
                return Fail(
                    $"Email '{dupes.First()}' xuất hiện ở nhiều bước. " +
                    "Mỗi bước phải là người ký riêng biệt.");

            return ServiceResult.Ok();
        }

        // ==============================================================
        // 5. Kiểm tra overlap với đơn OT đã tồn tại trong ngày
        //    Áp dụng cho từng nhân viên trong danh sách Employees
        // ==============================================================
        private async Task<ServiceResult> ValidateOverlapAsync(
            CreateOTRequestModel model,
            CancellationToken ct)
        {
            DateOnly otDate = DateOnly.FromDateTime(model.OTDate);

            foreach (var emp in model.Employees)
            {
                // Lấy tất cả dòng OT đã tồn tại của nhân viên này trong ngày hôm đó
                var existingRows = await _db.F03OTRows
                    .AsNoTracking()
                    .Where(r => r.Request.EmployeeCode == emp.EmployeeCode
                             && r.OTDate == otDate
                             && r.Request.IsActive == true
                             && r.Request.RequestStatus != OTStatus.Rejected
                             && r.Request.RequestStatus != OTStatus.Cancelled)
                    .Select(r => new { r.PlannedFrom, r.PlannedTo })
                    .ToListAsync(ct);

                foreach (var existing in existingRows)
                {
                    // Overlap: [A,B) ∩ [C,D) ≠ ∅  ⟺  A < D && C < B
                    if (emp.PlannedFrom < existing.PlannedTo &&
                        existing.PlannedFrom < emp.PlannedTo)
                    {
                        return Fail(
                            $"Nhân viên [{emp.EmployeeCode}]: " +
                            $"Khoảng {emp.PlannedFrom:HH:mm}–{emp.PlannedTo:HH:mm} " +
                            $"bị trùng với đơn OT đã đăng ký " +
                            $"({existing.PlannedFrom:HH:mm}–{existing.PlannedTo:HH:mm}) " +
                            $"ngày {otDate:dd/MM/yyyy}.");
                    }
                }
            }

            return ServiceResult.Ok();
        }

        // ==============================================================
        // Helpers
        // ==============================================================

        /// <summary>Giờ phải là bội số 15 phút, giây = 0.</summary>
        private static bool IsRounded15Min(DateTime dt)
            => dt.Minute % 15 == 0 && dt.Second == 0 && dt.Millisecond == 0;

        /// <summary>Tính số giờ OT, làm tròn 2 chữ số thập phân.</summary>
        private static decimal ComputeHours(DateTime from, DateTime to)
        {
            double raw = (to - from).TotalHours;
            return raw > 0 ? (decimal)Math.Round(raw, 2) : 0m;
        }

        private static ServiceResult Fail(string msg)
            => ServiceResult.Fail(msg);
    }
}


