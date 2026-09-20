using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.Approvers;
using FVN_REGISTER.Core.Entities.HR;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Approvals;

public sealed class ApprovalRouteService : IApprovalRouteService
{
    private readonly IUnitOfWork _uow;

    public ApprovalRouteService(IUnitOfWork uow)
        => _uow = uow;

    public async Task<ServiceResult<ApprovalRoutePreviewDto>> GetPreviewAsync(
        RequestModule requestType,
        string employeeCode,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(employeeCode))
        {
            return ServiceResult<ApprovalRoutePreviewDto>.Fail(
                "Chưa xác định EmployeeCode của người đăng ký.");
        }

        // F03Employee is the only source of requester organization identity.
        // PositionCode/DeptCode must never come from the caller.
        var employee = await _uow.Repository<F03Employee>()
            .Query()
            .AsNoTracking()
            .Where(x => x.IsActive == true && x.EmployeeCode == employeeCode)
            .Select(x => new
            {
                x.EmployeeCode,
                x.DeptCode,
                x.PositionCode
            })
            .FirstOrDefaultAsync(ct);

        if (employee == null)
        {
            return ServiceResult<ApprovalRoutePreviewDto>.Fail(
                $"Không tìm thấy nhân viên HRM {employeeCode}.");
        }

        if (string.IsNullOrWhiteSpace(employee.PositionCode))
        {
            return ServiceResult<ApprovalRoutePreviewDto>.Fail(
                $"Nhân viên {employeeCode} chưa có PositionCode HRM.");
        }

        var position = await _uow.Repository<F03Position>()
            .Query()
            .AsNoTracking()
            .Where(x => x.IsActive == true &&
                        x.PositionCode == employee.PositionCode)
            .Select(x => new
            {
                x.PositionCode,
                x.PositionName
            })
            .FirstOrDefaultAsync(ct);

        if (position == null)
        {
            return ServiceResult<ApprovalRoutePreviewDto>.Fail(
                $"Không tìm thấy chức vụ HRM {employee.PositionCode} trong F03Positions.");
        }

        var resolvedDeptCode = employee.DeptCode ?? string.Empty;

        var policies = await _uow.Repository<F03ApprovalPolicy>()
            .Query()
            .AsNoTracking()
            .Where(x => x.IsActive == true &&
                        x.RequestType == requestType &&
                        x.PositionCode == position.PositionCode)
            .OrderBy(x => x.Sequence)
            .ThenBy(x => x.Level)
            .ToListAsync(ct);

        if (policies.Count == 0)
        {
            return ServiceResult<ApprovalRoutePreviewDto>.Fail(
                $"Chưa cấu hình luồng phê duyệt cho {requestType} / " +
                $"chức vụ {position.PositionCode} - {position.PositionName}.");
        }

        var levels = new List<ApprovalRouteLevelDto>();

        foreach (var policy in policies.Where(x => x.Required))
        {
            var query = _uow.Repository<F03Approver>().Query()
                .AsNoTracking()
                .Where(x => x.IsActive == true &&
                            x.RequestType == requestType &&
                            x.Level == policy.Level &&
                            x.ApproverCode != employeeCode &&
                            (x.ApproveForDeptCode == resolvedDeptCode ||
                             x.ApproveForDeptCode == ApproveForDept.All));

            var candidates = await query
                .Join(
                    _uow.Repository<F03Employee>()
                        .Query()
                        .AsNoTracking()
                        .Where(e => e.IsActive == true),
                    a => a.ApproverCode,
                    e => e.EmployeeCode,
                    (a, e) => new { a, e })
                .Join(
                    _uow.Repository<F03Position>()
                        .Query()
                        .AsNoTracking()
                        .Where(p => p.IsActive == true),
                    x => x.e.PositionCode,
                    p => p.PositionCode,
                    (x, p) => new { x.a, x.e, p })
                // F03Approvers is the assignment source of truth.
                // F03Positions remains the HRM master and must not exclude an
                // explicitly assigned approver because IsApprove/IsAllowApprove
                // are metadata flags, not the assignment itself.
                .Select(x => new ApprovalCandidateDto
                {
                    ApproverCode = x.a.ApproverCode,
                    ApproverName = x.a.ApproverName,
                    PositionCode = x.e.PositionCode,
                    PositionName = x.p.PositionName,
                    ApproverEmail = x.a.ApproverEmail,
                    ApproverDeptCode = x.a.ApproverDeptCode,
                    ApproveForDeptCode = x.a.ApproveForDeptCode
                })
                .ToListAsync(ct);

            var departmentCandidates = candidates
                .Where(x =>
                    string.Equals(
                        x.ApproveForDeptCode,
                        resolvedDeptCode,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (departmentCandidates.Count > 0)
                candidates = departmentCandidates;

            candidates = candidates
                .GroupBy(
                    x => x.ApproverCode,
                    StringComparer.OrdinalIgnoreCase)
                .Select(x => x.First())
                .OrderBy(x => x.ApproverName)
                .ToList();

            if (candidates.Count == 0)
            {
                return ServiceResult<ApprovalRoutePreviewDto>.Fail(
                    $"Chưa cấu hình người phê duyệt cho cấp {policy.Level} " +
                    $"({policy.LevelName}) / chức vụ {position.PositionCode} " +
                    $" / bộ phận {resolvedDeptCode}.");
            }

            levels.Add(new ApprovalRouteLevelDto
            {
                Level = policy.Level,
                Sequence = policy.Sequence,
                LevelName = policy.LevelName,
                RoleName = policy.RoleName,
                Required = policy.Required,
                Candidates = candidates
            });
        }

        var preview = new ApprovalRoutePreviewDto
        {
            RequestType = requestType,
            EmployeeCode = employeeCode,
            DepartmentCode = resolvedDeptCode,
            PositionCode = position.PositionCode,
            Levels = levels
        };

        return ServiceResult<ApprovalRoutePreviewDto>.Ok(preview);
    }
}
