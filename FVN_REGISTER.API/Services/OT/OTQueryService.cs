using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Contract.Interfaces.OT;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Contract.Utils;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.API.Services.OT
{
    public class OTQueryService : IOTQueryService
    {
        private readonly FVNWEBAPPContext _db;

        public OTQueryService(FVNWEBAPPContext db)
        {
            _db = db;
        }

        // ===== Danh sách chờ duyệt cho Approver =====
        public async Task<ServiceResult<List<VF03OTRequestSummary>>> GetPendingForApproverAsync(
            string approverEmail, CancellationToken ct = default)
        {
            var list = await _db.Set<F03OTRequest>()
                .AsNoTracking()
                .Where(r => r.IsActive &&
                    OTStatus.ActiveStatuses.Contains(r.RequestStatus) &&
                    (
                        // Đang chờ bước 3
                        (r.Lv3ApproveEmail == approverEmail && !r.Lv3Skip && r.Lv3IsApprove == null) ||
                        // Đang chờ bước 4 (bước 3 xong hoặc skip)
                        (r.Lv4ApproveEmail == approverEmail && r.Lv4IsApprove == null &&
                            (r.Lv3Skip || r.Lv3IsApprove == true)) ||
                        // Đang chờ bước 5
                        (r.Lv5ApproveEmail == approverEmail && r.Lv5IsApprove == null &&
                            r.Lv4IsApprove == true) ||
                        // Đang chờ bước 6
                        (r.Lv6ApproveEmail == approverEmail && r.Lv6IsApprove == null &&
                            r.Lv5IsApprove == true) ||
                        // Đang chờ GM
                        (r.Lv7ApproveEmail == approverEmail && r.Lv7IsApprove == null &&
                            r.RequiresGM && r.Lv6IsApprove == true)
                    ))
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(ct);

            var result = list.Select(r => MapToSummary(r)).ToList();
            return ServiceResult<List<VF03OTRequestSummary>>.Ok(result);
        }

        // ===== Lịch sử OT của nhân viên =====
        public async Task<ServiceResult<List<VF03OTRequestSummary>>> GetByEmployeeAsync(
            string employeeCode, int year, CancellationToken ct = default)
        {
            var list = await _db.Set<F03OTRequest>()
                .AsNoTracking()
                .Where(r => r.EmployeeCode == employeeCode &&
                            r.WorkYear == year && r.IsActive)
                .OrderByDescending(r => r.OTDate)
                .ToListAsync(ct);

            return ServiceResult<List<VF03OTRequestSummary>>.Ok(
                list.Select(MapToSummary).ToList());
        }

        // ===== Dropdown chọn người ký theo cấp =====
        public async Task<ServiceResult<List<OTApproverSelectDto>>> GetApproversAsync(
            string deptCode, int level, CancellationToken ct = default)
        {
            // Dùng bảng VF03leaveDaysApprover hoặc bảng approver riêng cho OT
            // Ở đây tái dụng VF03leaveDaysApprover với ApproveLevel = level
            var data = await _db.VF03leaveDaysApprovers
                .AsNoTracking()
                .Where(x => x.ApproveLevel == level &&
                            (x.DeptCode == deptCode || x.DeptCode == "ALL") &&
                            x.IsActive)
                .ToListAsync(ct);

            var result = data.Select(x => new OTApproverSelectDto
            {
                ApproveLevelCode = x.ApproveLevelCode ?? "",
                ApproveLevelName = x.ApproveLevelName,
                ApproveLevelEmail = x.ApproveLevelEmail,
                DeptCode = x.DeptCode,
                DeptName = x.DeptName,
                Role = level switch
                {
                    3 => "SubLeader/Leader",
                    4 => "BCH CĐ",
                    5 => "Ast.Chief/Chief",
                    6 => "A.MG/MG",
                    7 => "GM",
                    _ => "Unknown"
                },
                ApproveLevel = level
            }).ToList();

            return ServiceResult<List<OTApproverSelectDto>>.Ok(result);
        }

        // ===== Tổng giờ OT tháng/năm =====
        public async Task<ServiceResult<OTSummaryDto>> GetSummaryAsync(
            string employeeCode, int year, int month, CancellationToken ct = default)
        {
            var summary = await _db.Set<F03OTSummary>()
                .AsNoTracking()
                .Where(x => x.EmployeeCode == employeeCode &&
                            x.WorkYear == year && x.WorkMonth == month)
                .FirstOrDefaultAsync(ct);

            // Tổng giờ năm = tổng các tháng
            var yearTotal = await _db.Set<F03OTSummary>()
                .AsNoTracking()
                .Where(x => x.EmployeeCode == employeeCode && x.WorkYear == year)
                .SumAsync(x => x.TotalHoursMonth, ct);

            var dto = new OTSummaryDto
            {
                EmployeeCode = employeeCode,
                Year = year,
                Month = month,
                UsedHoursMonth = summary?.TotalHoursMonth ?? 0,
                UsedHoursYear = yearTotal
            };

            return ServiceResult<OTSummaryDto>.Ok(dto);
        }

        // ===== Mapper helper =====
        private static VF03OTRequestSummary MapToSummary(F03OTRequest r)
        {
            string? nextName = null;
            if (!r.Lv3Skip && r.Lv3IsApprove == null) nextName = r.Lv3ApproveName;
            else if (r.Lv4IsApprove == null) nextName = r.Lv4ApproveName;
            else if (r.Lv5IsApprove == null) nextName = r.Lv5ApproveName;
            else if (r.Lv6IsApprove == null) nextName = r.Lv6ApproveName;
            else if (r.RequiresGM && r.Lv7IsApprove == null) nextName = r.Lv7ApproveName;

            return new VF03OTRequestSummary
            {
                Id = r.Id,
                EmployeeCode = r.EmployeeCode,
                OTDate = r.OTDate,
                DayType = r.DayType,
                PlannedHours = r.PlannedHours,
                OTReason = r.OTReason,
                RequiresGM = r.RequiresGM,
                RequestStatus = r.RequestStatus,
                CreatedAt = r.CreatedAt,
                NextApproverName = nextName
            };
        }
    }
}
