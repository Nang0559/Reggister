using Microsoft.EntityFrameworkCore;
using FVN_REGISTER.Application.Interfaces.Dashboards;
using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Dashboard;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Infrastructure.Services.Dashboards;

public sealed class HRWorkspaceDashboardProvider : IModuleDashboardProvider
{
    private readonly FVNWEBAPPContext _db;
    public HRWorkspaceDashboardProvider(FVNWEBAPPContext db) => _db = db;
    public RequestModule Module => RequestModule.HR;
    public int RequiredFunctionCode => SecurityFunctionCodes.HrmSyncViewStatus;

    public async Task<ModuleDashboardContribution> GetContributionAsync(UserIdentityDto user, CancellationToken ct = default)
    {
        var syncReview = await _db.SyncReviewFlags.AsNoTracking().CountAsync(ct);
        var corrections = await _db.ExecutionCorrections.AsNoTracking()
            .CountAsync(x => x.IsActive != false && x.Status == "Pending", ct);
        var headcount = await _db.Employees.AsNoTracking().CountAsync(x => x.IsActive != false, ct);

        return new()
        {
            Module = Module,
            Widgets =
            [
                new() { Title = "HRM Sync cần review", Value = syncReview.ToString(), Icon = "SyncProblem", Color = "Warning", Link = "/hrm-sync", IsPersonal = false },
                new() { Title = "Correction chờ xử lý", Value = corrections.ToString(), Icon = "Rule", Color = "Info", Link = "/execution/hr-review", IsPersonal = false },
                new() { Title = "Headcount", Value = headcount.ToString(), Icon = "Groups", Color = "Success", Link = "/employees", IsPersonal = false }
            ]
        };
    }
}
