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
        string deptCode,
        string positionCode,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(positionCode))
        {
            return ServiceResult<ApprovalRoutePreviewDto>.Fail(
                "Chưa xác định PositionCode HRM của người đăng ký.");
        }

        // Canonical route resolution:
        // F03Employee.PositionCode -> F03Positions.PositionCode
        // -> F03ApprovalPolicies.PositionCode.
        // No local approval-group translation is used.
        var position = await _uow.Repository<F03Position>()
            .Query()
            .AsNoTracking()
            .Where(x => x.IsActive == true &&
                        x.PositionCode == positionCode)
            .Select(x => new
            {
                x.PositionCode,
                x.PositionName
            })
            .FirstOrDefaultAsync(ct);

        if (position == null)
        {
            return ServiceResult<ApprovalRoutePreviewDto>.Fail(
                $"Không tìm thấy chức vụ HRM {positionCode} trong F03Positions.");
        }

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
                            (x.ApproveForDeptCode == deptCode ||
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
                .Where(x => x.p.IsApprove || x.p.IsAllowApprove)
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
                        deptCode,
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
                    $" / bộ phận {deptCode}.");
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
            DepartmentCode = deptCode,
            PositionCode = position.PositionCode,
            Levels = levels
        };

        return ServiceResult<ApprovalRoutePreviewDto>.Ok(preview);
    }
}
