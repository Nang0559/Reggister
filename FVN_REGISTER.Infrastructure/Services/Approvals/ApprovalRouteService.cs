using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Contract.Dtos.Approvals;
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

    public ApprovalRouteService(IUnitOfWork uow) => _uow = uow;

    public async Task<ApprovalRoutePreviewDto> GetPreviewAsync(
        RequestModule requestType,
        string employeeCode,
        string deptCode,
        string positionCode,
        CancellationToken ct = default)
    {
        var exact = await _uow.Repository<F03ApprovalPolicy>().Query()
            .AsNoTracking()
            .Where(x => x.IsActive == true &&
                        x.RequestType == requestType &&
                        x.RequesterPositionCode == positionCode)
            .OrderBy(x => x.Sequence)
            .ThenBy(x => x.Level)
            .ToListAsync(ct);

        var policies = exact.Count > 0
            ? exact
            : await _uow.Repository<F03ApprovalPolicy>().Query()
                .AsNoTracking()
                .Where(x => x.IsActive == true &&
                            x.RequestType == requestType &&
                            (x.RequesterPositionCode == null || x.RequesterPositionCode == "*"))
                .OrderBy(x => x.Sequence)
                .ThenBy(x => x.Level)
                .ToListAsync(ct);

        if (policies.Count == 0)
            throw new InvalidOperationException(
                $"Chưa cấu hình luồng phê duyệt cho {requestType} / chức vụ {positionCode}.");

        var levels = new List<ApprovalRouteLevelDto>();

        foreach (var policy in policies.Where(x => x.Required).OrderBy(x => x.Sequence).ThenBy(x => x.Level))
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
                    _uow.Repository<F03Employee>().Query().AsNoTracking().Where(e => e.IsActive == true),
                    a => a.ApproverCode,
                    e => e.EmployeeCode,
                    (a, e) => new { a, e })
                .Join(
                    _uow.Repository<F03Position>().Query().AsNoTracking().Where(p => p.IsActive == true),
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

            // Nếu cấp có cấu hình riêng cho bộ phận thì không lấy thêm ALL.
            var departmentCandidates = candidates
                .Where(x => string.Equals(x.ApproveForDeptCode, deptCode, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (departmentCandidates.Count > 0)
                candidates = departmentCandidates;

            candidates = candidates
                .GroupBy(x => x.ApproverCode, StringComparer.OrdinalIgnoreCase)
                .Select(x => x.First())
                .OrderBy(x => x.ApproverName)
                .ToList();

            levels.Add(new ApprovalRouteLevelDto
            {
                Level = policy.Level,
                Sequence = policy.Sequence,
                LevelName = policy.LevelName,
                RoleName = policy.RoleName,
                Required = true,
                Candidates = candidates
            });
        }

        return new ApprovalRoutePreviewDto
        {
            RequestType = requestType,
            EmployeeCode = employeeCode,
            DepartmentCode = deptCode,
            PositionCode = positionCode,
            Levels = levels
        };
    }
}
