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
        private readonly FVNWEBAPPContext _db;

        // ===== Giới hạn giờ theo QĐ-HC-03 =====
        private const decimal MAX_DAILY_NORMAL = 4m;    // Ngày thường: 4h/ngày
        private const decimal MAX_DAILY_HOLIDAY = 12m;   // Ngày nghỉ/lễ/Tết: 12h/ngày
        private const decimal MAX_MONTHLY = 40m;   // Tháng: 40h
        private const decimal MAX_YEARLY = 200m;  // Năm: 200h (300h khi có thủ tục đặc biệt)
        private const decimal MAX_YEARLY_SPECIAL = 300m;

        public OTValidator(FVNWEBAPPContext db)
        {
            _db = db;
        }

        public async Task<ServiceResult> ValidateAsync(
            CreateOTRequestModel model,
            CurrentUser user,
            CancellationToken ct = default)
        {
            // ===== 1. VALIDATE CƠ BẢN =====
            if (model.Employees == null || model.Employees.Count == 0)
                return Fail("Phải có ít nhất 1 nhân viên OT.");

            if (string.IsNullOrWhiteSpace(model.OTReason))
                return Fail("Vui lòng nhập lý do OT.");

            if (model.PlannedFrom >= model.PlannedTo)
                return Fail("Giờ bắt đầu phải nhỏ hơn giờ kết thúc.");

            // ===== 2. VALIDATE GIỜ OT & LÀM TRÒN 15 PHÚT =====
            var roundedFrom = RoundToQuarter(model.PlannedFrom);
            var roundedTo = RoundToQuarter(model.PlannedTo);
            model.PlannedFrom = roundedFrom;
            model.PlannedTo = roundedTo;

            var rawHours = (decimal)(roundedTo - roundedFrom).TotalHours;
            if (rawHours <= 0)
                return Fail("Thời gian OT sau khi làm tròn không hợp lệ.");

            model.PlannedHours = Math.Round(rawHours, 2);

            // ===== 3. GIỚI HẠN THEO LOẠI NGÀY =====
            var maxDaily = OTDayType.MaxHours(model.DayType);
            if (model.PlannedHours > maxDaily)
                return Fail($"Vượt quá giới hạn {maxDaily}h/ngày " +
                            $"({model.DayType switch
                            {
                                "Normal" => "ngày thường",
                                "Weekend" => "cuối tuần",
                                "Holiday" => "ngày lễ",
                                "Tet" => "Tết",
                                _ => model.DayType
                            }}).");

            // ===== 4. KIỂM TRA CÀI ĐẶT RequiresGM =====
            bool dayRequiresGM = OTDayType.RequiresGM(model.DayType);
            bool levelRequiresGM = OTCreatorLevel.RequiresGM(model.CreatedByLevel ?? "");
            model.RequiresGM = dayRequiresGM || levelRequiresGM;

            // ===== 5. KIỂM TRA Lv3Skip =====
            model.Lv3Skip = OTCreatorLevel.SkipLv3(model.CreatedByLevel ?? "");

            // ===== 6. VALIDATE NGƯỜI KÝ =====
            // Bước 3 (chỉ cần nếu không skip)
            if (!model.Lv3Skip && string.IsNullOrWhiteSpace(model.Lv3ApproveEmail))
                return Fail("Phải chọn Sub.Leader / Leader (Bước 3).");

            // Bước 4: BCH CĐ - luôn bắt buộc
            if (string.IsNullOrWhiteSpace(model.Lv4ApproveEmail))
                return Fail("Phải chọn đại diện BCH Công đoàn (Bước 4).");

            // Bước 5: Ast.Chief/Chief - luôn bắt buộc
            if (string.IsNullOrWhiteSpace(model.Lv5ApproveEmail))
                return Fail("Phải chọn Ast.Chief / Chief (Bước 5).");

            // Bước 6: A.MG/MG - luôn bắt buộc
            if (string.IsNullOrWhiteSpace(model.Lv6ApproveEmail))
                return Fail("Phải chọn A.MG / MG (Bước 6).");

            // Bước 7: GM - chỉ bắt buộc khi RequiresGM
            if (model.RequiresGM && string.IsNullOrWhiteSpace(model.Lv7ApproveEmail))
                return Fail("OT ngày nghỉ/lễ/Tết hoặc cấp bậc Ast.Chief trở lên phải có GM ký (Bước 7).");

            // Không tự ký tên mình
            var allApprovers = new[]
            {
            model.Lv3ApproveEmail, model.Lv4ApproveEmail,
            model.Lv5ApproveEmail, model.Lv6ApproveEmail,
            model.Lv7ApproveEmail
        };
            if (allApprovers.Any(x => !string.IsNullOrWhiteSpace(x) && x == user.Email))
                return Fail("Không thể chọn chính mình làm người ký duyệt.");

            // ===== 7. VALIDATE GIỜ THÁNG/NĂM CHO TỪNG NHÂN VIÊN =====
            var year = model.OTDate.Year;
            var month = model.OTDate.Month;
            var otDate = DateOnly.FromDateTime(model.OTDate);

            foreach (var emp in model.Employees)
            {
                // Giờ OT từng nhân viên (có thể riêng hoặc dùng giờ đơn)
                var empHours = emp.ActualHours ?? model.PlannedHours;

                // Lấy tổng giờ hiện tại
                var summary = await _db.Set<F03OTSummary>()
                    .FirstOrDefaultAsync(x =>
                        x.EmployeeCode == emp.EmployeeCode &&
                        x.WorkYear == year &&
                        x.WorkMonth == month, ct);

                var monthUsed = summary?.TotalHoursMonth ?? 0;
                var yearUsed = summary?.TotalHoursYear ?? 0;

                emp.ActualFrom = monthUsed;
                emp.CurrentYearHours = yearUsed;

                // Kiểm tra trùng ngày
                var hasDuplicate = await _db.Set<F03OTRow>()
                    .AnyAsync(r =>
                        r.EmployeeCode == emp.EmployeeCode &&
                        r.IsActive == true &&
                        r.Request.IsActive == true &&
                        r.Request.RequestStatus != OTStatus.Rejected &&
                        r.Request.RequestStatus != OTStatus.Cancelled &&
                        DateOnly.FromDateTime(r.PlannedFrom.Date) == otDate, ct);

                if (hasDuplicate)
                {
                    emp.IsValid = false;
                    emp.ValidationMessage = $"[{emp.EmployeeCode}] Đã có đơn OT ngày {otDate:dd/MM/yyyy}.";
                    return Fail(emp.ValidationMessage);
                }

                // Giới hạn tháng
                if (monthUsed + empHours > MAX_MONTHLY)
                    return Fail($"[{emp.EmployeeCode}] Vượt giới hạn 40h/tháng " +
                                $"(đã dùng {monthUsed}h, thêm {empHours}h).");

                // Giới hạn năm (dùng MAX_YEARLY = 200h; nếu cần 300h phải có thủ tục riêng)
                if (yearUsed + empHours > MAX_YEARLY)
                    return Fail($"[{emp.EmployeeCode}] Vượt giới hạn 200h/năm " +
                                $"(đã dùng {yearUsed}h, thêm {empHours}h). " +
                                "Liên hệ HR để xin phép vượt 200h.");
            }

            return ServiceResult.Ok();
        }

        // ===== Làm tròn đến 15 phút gần nhất =====
        private static DateTime RoundToQuarter(DateTime dt)
        {
            var totalMinutes = (int)dt.TimeOfDay.TotalMinutes;
            var rounded = (int)Math.Round(totalMinutes / 15.0) * 15;
            return dt.Date.AddMinutes(rounded);
        }

        private static ServiceResult Fail(string msg) => ServiceResult.Fail(msg);
    }
}
