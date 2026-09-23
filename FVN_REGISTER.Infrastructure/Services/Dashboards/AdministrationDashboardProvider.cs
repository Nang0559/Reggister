using Microsoft.EntityFrameworkCore;
using FVN_REGISTER.Application.Interfaces.Dashboards;
using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Dashboard;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Infrastructure.Services.Dashboards;

public sealed class AdministrationDashboardProvider : IModuleDashboardProvider
{
    private readonly FVNWEBAPPContext _db;
    public AdministrationDashboardProvider(FVNWEBAPPContext db) => _db = db;
    public RequestModule Module => RequestModule.Administration;
    public int RequiredFunctionCode => SecurityFunctionCodes.UserManagementView;

    public async Task<ModuleDashboardContribution> GetContributionAsync(UserIdentityDto user, CancellationToken ct = default)
    {
        var users = await _db.Users.AsNoTracking().CountAsync(x => x.IsActive != false, ct);
        var roles = await _db.Roles.AsNoTracking().CountAsync(x => x.IsActive != false, ct);
        var policies = await _db.ExecutionPolicies.AsNoTracking().CountAsync(x => x.IsActive != false, ct);

        return new()
        {
            Module = Module,
            Widgets =
            [
                new() { Title = "User đang hoạt động", Value = users.ToString(), Icon = "People", Color = "Info", Link = "/admin/users", IsPersonal = false },
                new() { Title = "Role đang hoạt động", Value = roles.ToString(), Icon = "AdminPanelSettings", Color = "Success", Link = "/admin/security", IsPersonal = false },
                new() { Title = "Execution policies", Value = policies.ToString(), Icon = "Rule", Color = "Info", Link = "/admin/security", IsPersonal = false }
            ]
        };
    }
}
