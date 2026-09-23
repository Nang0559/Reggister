using Microsoft.EntityFrameworkCore;
using FVN_REGISTER.Application.Interfaces.Dashboards;
using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Dashboard;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Infrastructure.Services.Dashboards;

public sealed class ExecutionWorkspaceDashboardProvider : IModuleDashboardProvider
{
    private readonly FVNWEBAPPContext _db;
    public ExecutionWorkspaceDashboardProvider(FVNWEBAPPContext db) => _db = db;
    public RequestModule Module => RequestModule.Execution;
    public int RequiredFunctionCode => SecurityFunctionCodes.ExecutionReview;

    public async Task<ModuleDashboardContribution> GetContributionAsync(UserIdentityDto user, CancellationToken ct = default)
    {
        var unresolved = await _db.ExecutionReconciliations.AsNoTracking()
            .CountAsync(x => x.IsActive != false &&
                (x.ReconciliationStatus == "Mismatch" || x.ReconciliationStatus == "AwaitingConfirmation"), ct);
        var pendingEvidence = await _db.ExecutionConfirmationEvidence.AsNoTracking()
            .CountAsync(x => x.IsActive != false && x.ReviewStatus == "Pending", ct);
        var pendingCorrection = await _db.ExecutionCorrections.AsNoTracking()
            .CountAsync(x => x.IsActive != false && (x.Status == "Pending" || x.Status == "Failed"), ct);

        return new()
        {
            Module = Module,
            Widgets =
            [
                new() { Title = "Execution chưa xử lý", Value = unresolved.ToString(), Icon = "WarningAmber", Color = "Warning", Link = "/execution/reconciliation", IsPersonal = false },
                new() { Title = "Evidence chờ review", Value = pendingEvidence.ToString(), Icon = "FactCheck", Color = "Info", Link = "/execution/hr-review", IsPersonal = false },
                new() { Title = "Correction chờ xử lý", Value = pendingCorrection.ToString(), Icon = "BuildCircle", Color = "Error", Link = "/execution/hr-review", IsPersonal = false }
            ]
        };
    }
}
