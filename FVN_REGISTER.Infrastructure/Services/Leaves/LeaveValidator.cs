using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Models.Subjects;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Contract.Requests.Leaves;
using FVN_REGISTER.Core.Entities.HR;
using FVN_REGISTER.Core.Entities.Leaves;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Core.Utils;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Leaves
{
    public class LeaveValidator : ILeaveValidator
    {
        private readonly IUnitOfWork _uow;
        private readonly IApprovalProvider<LeaveRequestSubject> _approvalProvider;

        public LeaveValidator(
            IUnitOfWork uow,
            IApprovalProvider<LeaveRequestSubject> approvalProvider)
        {
            _uow = uow;
            _approvalProvider = approvalProvider;
        }

        public async Task<ServiceResult> ValidateAsync(
            LeaveRequestUpsertDto model,
            UserIdentityDto user,
            CancellationToken ct = default)
        {
            try
            {
                if (model.Details == null || model.Details.Count == 0)
                    return Fail("Phải chọn ít nhất 1 ngày nghỉ.");

                model.Details = model.Details
                    .OrderBy(x => x.LeaveDate)
                    .ToList();

                int workYear = model.Details.Min(x => x.LeaveDate).Year;
                int currYear = DateTime.Now.Year;

                if (workYear < currYear || workYear > currYear + 1)
                    return Fail("Năm làm việc không hợp lệ.");

                // ═══════════ 1. LEAVE TYPE — server tự tính lại, KHÔNG tin client ═══════════
                var leaveTypeCodes = model.Details
                    .Select(x => x.LeaveTypeCode)
                    .Distinct()
                    .ToList();

                var leaveTypeMap = await _uow.Repository<F03LeaveType>().Query()
                    .AsNoTracking()
                    .Where(x => leaveTypeCodes.Contains(x.LeaveTypeCode))
                    .ToDictionaryAsync(x => x.LeaveTypeCode, ct);

                foreach (var d in model.Details)
                {
                    if (!leaveTypeMap.TryGetValue(d.LeaveTypeCode, out var lt))
                        return Fail($"Loại nghỉ [{d.LeaveTypeCode}] không tồn tại.");

                    if (!d.IsHalfDay)
                        d.HalfDayOption = null;
                }

                // ═══════════ 2. THÔNG TIN NHÂN VIÊN — query trực tiếp, không tin
                // Email/DeptCode/PositionCode trên UserIdentityDto truyền vào ═══════════
                var emp = await _uow.Repository<F03Employee>().Query()
                    .AsNoTracking()
                    .Where(x => x.EmployeeCode == user.EmployeeCode)
                    .Select(x => new { x.EmailAddress, x.DeptCode, x.PositionCode })
                    .FirstOrDefaultAsync(ct);

                if (emp == null)
                    return Fail("Không tìm thấy thông tin nhân viên.");

                if (string.IsNullOrWhiteSpace(emp.EmailAddress))
                    return Fail("Tài khoản chưa có Email.");

                // ═══════════ 3. OVERLAP — LeaveDate là DateTime, KHÔNG cần DateOnly.FromDateTime ═══════════
                var start = model.Details.Min(x => x.LeaveDate).Date;
                var end = model.Details.Max(x => x.LeaveDate).Date;

                var overlapQuery = _uow.Repository<F03LeaveDayDetail>().Query()
                    .AsNoTracking()
                    .Where(x =>
                        x.LeaveDay.EmployeeCode == user.EmployeeCode &&
                        x.LeaveDay.IsActive == true &&
                        x.LeaveDay.RequestStatus != ApprovalStatus.Rejected &&
                        x.LeaveDate >= start &&
                        x.LeaveDate <= end);

                // Update: loại trừ chính đơn đang sửa khỏi check overlap
                if (model.Id > 0)
                    overlapQuery = overlapQuery.Where(x => x.LeaveDaysId != model.Id);

                var overlapSet = await overlapQuery
                    .Select(x => new { x.LeaveDate, x.IsHalfDay, x.HalfDayOption })
                    .ToListAsync(ct);

                foreach (var d in model.Details)
                {
                    var exist = overlapSet.FirstOrDefault(o => o.LeaveDate.Date == d.LeaveDate.Date);

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

                // ═══════════ 4. APPROVER — build hierarchy thật ═══════════
                var ctx = ApprovalBuildContext.ForLeave(
                    user.EmployeeCode ?? "", emp.DeptCode ?? "", emp.PositionCode ?? "");

                var hierarchy = await _approvalProvider.BuildHierarchyAsync(ctx, ct);

                var missingRequired = hierarchy
                    .Where(s => s.IsRequired && string.IsNullOrWhiteSpace(s.ApproverEmail))
                    .ToList();

                if (missingRequired.Any())
                {
                    var levelNames = string.Join(", ", missingRequired.Select(s => s.LevelName));
                    return Fail(
                        $"Phòng ban {emp.DeptCode} chưa có người duyệt cấp: {levelNames}. " +
                        "Liên hệ Admin để cập nhật danh sách approver.");
                }

                // ═══════════ 5. SELF APPROVAL CHECK ═══════════
                if (hierarchy.Any(s =>
                        s.IsRequired &&
                        !string.IsNullOrEmpty(s.ApproverEmail) &&
                        s.ApproverEmail!.Equals(emp.EmailAddress, StringComparison.OrdinalIgnoreCase)))
                {
                    return Fail("Không thể chọn chính mình làm người duyệt.");
                }

                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                return Fail($"Validator error: {ex.Message}");
            }
        }

        private static ServiceResult Fail(string msg) => ServiceResult.Fail(msg);
    }
}
