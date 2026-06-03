using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Contract.ViewModels.OT;
using System.Net.NetworkInformation;

namespace FVN_REGISTER.API.Services.OT
{
    // OTValidator.cs
    public class OTValidator : IOTValidator
    {
        private readonly FVNWEBAPPContext _db;

        public async Task<ServiceResult> ValidateAsync(
            CreateOTRequestModel model, CancellationToken ct)
        {
            // === GIỚI HẠN THEO LOẠI NGÀY ===
            decimal maxPerDay = model.OTType == "Normal" ? 4m : 12m;

            foreach (var emp in model.Employees)
            {
                var hours = CalcHours(emp.PlannedFrom, emp.PlannedTo);

                if (hours < 0.5m)
                    return ServiceResult.Fail(
                        $"{emp.EmployeeName}: Tối thiểu 30 phút OT.");

                if (hours > maxPerDay)
                    return ServiceResult.Fail(
                        $"{emp.EmployeeName}: Vượt {maxPerDay}h/ngày " +
                        $"({(model.OTType == "Normal" ? "ngày thường" : "ngày nghỉ/lễ")}).");

                // === GIỚI HẠN THÁNG: 40h ===
                var monthTotal = await _db.F03OTRows
                    .Where(r => r.EmployeeCode == emp.EmployeeCode
                             && r.OTRequest.OTDate.Year == model.OTDate.Year
                             && r.OTRequest.OTDate.Month == model.OTDate.Month
                             && r.OTRequest.RequestStatus != OTStatus.Rejected
                             && r.OTRequest.RequestStatus != OTStatus.Cancelled)
                    .SumAsync(r => (decimal)(r.PlannedTo.ToTimeSpan()
                                 - r.PlannedFrom.ToTimeSpan()).TotalHours, ct);

                if (monthTotal + hours > 40m)
                    return ServiceResult.Fail(
                        $"{emp.EmployeeName}: Vượt 40h/tháng " +
                        $"(hiện: {monthTotal:F1}h, thêm: {hours:F1}h).");

                // === GIỚI HẠN NĂM: 200h (cảnh báo ≥200, chặn ≥300) ===
                var yearTotal = await _db.F03OTRows
                    .Where(r => r.EmployeeCode == emp.EmployeeCode
                             && r.OTRequest.OTDate.Year == model.OTDate.Year
                             && r.OTRequest.RequestStatus != OTStatus.Rejected
                             && r.OTRequest.RequestStatus != OTStatus.Cancelled)
                    .SumAsync(r => (decimal)(r.PlannedTo.ToTimeSpan()
                                 - r.PlannedFrom.ToTimeSpan()).TotalHours, ct);

                if (yearTotal + hours > 300m)
                    return ServiceResult.Fail(
                        $"{emp.EmployeeName}: Vượt 300h/năm. " +
                        $"Cần thủ tục đặc biệt theo Điều 107 BLLĐ.");
            }

            // === VALIDATE GM BẮT BUỘC ===
            if (model.RequiresGM && string.IsNullOrEmpty(model.GMEmail))
                return ServiceResult.Fail(
                    "OT ngày nghỉ/lễ hoặc có Ast.Chief trở lên " +
                    "bắt buộc phải chọn GM duyệt.");

            // === VALIDATE ĐỦ 3 CẤP CƠ BẢN ===
            if (string.IsNullOrEmpty(model.UnionRepEmail))
                return ServiceResult.Fail("Thiếu đại diện BCH Công đoàn.");
            if (string.IsNullOrEmpty(model.ChiefEmail))
                return ServiceResult.Fail("Thiếu Ast.Chief/Chief.");
            if (string.IsNullOrEmpty(model.MGEmail))
                return ServiceResult.Fail("Thiếu A.MG/MG.");

            // === LÀM TRÒN GIỜ THEO QUY ĐỊNH (đơn vị 15 phút) ===
            foreach (var emp in model.Employees)
                emp.PlannedTo = RoundDownTo15Min(emp.PlannedTo);

            return ServiceResult.Ok();
        }

        // Làm tròn xuống đến bội số 15 phút
        // VD: 1h55 → 1h45 (theo quy định trang 3)
        private static TimeOnly RoundDownTo15Min(TimeOnly t)
        {
            var minutes = (t.Hour * 60 + t.Minute) / 15 * 15;
            return new TimeOnly(minutes / 60, minutes % 60);
        }

        private static decimal CalcHours(TimeOnly from, TimeOnly to)
            => to > from ? (decimal)(to - from).TotalHours : 0;
    }
}
