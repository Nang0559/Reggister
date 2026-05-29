using FVN_REGISTER.Contract.Interfaces.Leaves;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Contract.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.API.Services.Leaves
{
    public class LeaveValidator(FVNWEBAPPContext db) : ILeaveValidator
    {
        private readonly FVNWEBAPPContext _db = db;

        public async Task<ServiceResult> ValidateAsync(
            CreateLeaveRequestModel model,
            UserSessionDto user,
            CancellationToken ct = default)
        {
            try
            {
                int currYear = DateTime.Now.Year;

                // ================= 1. BASIC VALIDATION =================
                if (model.WorkYear < currYear || model.WorkYear > currYear + 1)
                    return Fail("Năm làm việc không hợp lệ.");

                if (string.IsNullOrWhiteSpace(user.Email))
                    return Fail("Tài khoản chưa có Email.");

                if (model.Details == null || model.Details.Count == 0)
                    return Fail("Phải chọn ít nhất 1 ngày nghỉ.");

                model.Details = model.Details
                    .OrderBy(x => x.LeaveDate)
                    .ToList();

                // ================= 2. VALIDATE LEAVE TYPE (1 query) =================
                var leaveTypeCodes = model.Details
                    .Select(x => x.LeaveTypeCode)
                    .Distinct()
                    .ToList();

                var leaveTypeMap = await _db.F03leaveTypes
                    .AsNoTracking()
                    .Where(x => leaveTypeCodes.Contains(x.LeaveTypeCode))
                    .ToDictionaryAsync(x => x.LeaveTypeCode, ct);

                foreach (var d in model.Details)
                {
                    if (!leaveTypeMap.TryGetValue(d.LeaveTypeCode, out var lt))
                        return Fail($"Loại nghỉ [{d.LeaveTypeCode}] không tồn tại.");

                    d.DayValue = d.IsHalfDay ? 0.5m : 1m;
                    d.TinhPhep = lt.TinhPhep == true ? 1 : 0;

                    if (!d.IsHalfDay)
                        d.HalfDayOption = null;
                }

                model.CalculateTotals();

                // ================= 3. CHECK OVERLAP (OPTIMIZED) =================
                var start = DateOnly.FromDateTime(model.StartDate);
                var end = DateOnly.FromDateTime(model.EndDate);

                var overlapSet = await _db.F03leaveDayDetails
                    .AsNoTracking()
                    .Where(x =>
                        x.LeaveDays.EmployeeCode == user.EmployeeCode &&
                        x.LeaveDays.IsActive==true &&
                        x.LeaveDays.RequestStatus != "Rejected" &&
                        x.LeaveDate >= start &&
                        x.LeaveDate <= end)
                    .Select(x => new
                    {
                        x.LeaveDate,
                        x.IsHalfDay,
                        x.HalfDayOption
                    })
                    .ToListAsync(ct);

                foreach (var d in model.Details)
                {
                    var exist = overlapSet.FirstOrDefault(o => o.LeaveDate == DateOnly.FromDateTime(d.LeaveDate));

                    if (exist != null)
                    {
                        bool fullConflict =
                            exist.IsHalfDay != true ||
                            d.IsHalfDay != true ||
                            exist.HalfDayOption == d.HalfDayOption;

                        if (fullConflict)
                            return Fail($"Ngày {d.LeaveDate:dd/MM/yyyy} đã tồn tại đơn nghỉ.");
                    }
                }

                // ================= 4. APPROVER VALIDATION =================
                if (user.CVCode == "0003")
                {
                    if (string.IsNullOrWhiteSpace(model.Level1ApproveEmail) ||
                        string.IsNullOrWhiteSpace(model.Level2ApproveEmail) ||
                        string.IsNullOrWhiteSpace(model.Level3ApproveEmail))
                        return Fail("Công nhân phải chọn đủ 3 cấp duyệt.");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(model.Level2ApproveEmail) ||
                        string.IsNullOrWhiteSpace(model.Level3ApproveEmail))
                        return Fail("Văn phòng phải có Manager và GM.");
                }

                // ================= 5. SELF APPROVAL CHECK =================
                var approvers = new[]
                {
                model.Level1ApproveEmail,
                model.Level2ApproveEmail,
                model.Level3ApproveEmail
            };

                if (approvers.Any(x => !string.IsNullOrWhiteSpace(x) && x == user.Email))
                    return Fail("Không thể chọn chính mình làm người duyệt.");

                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail($"Validator error: {ex.Message}");
            }
        }

        private static ServiceResult Fail(string msg) => ServiceResult.Fail(msg);
    }
}
