using Microsoft.EntityFrameworkCore;
using FVN_REGISTER.Application.Interfaces.Dashboards;
using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Dashboard;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Infrastructure.Services.Dashboards;

public sealed class ITWorkspaceDashboardProvider : IModuleDashboardProvider
{
    private readonly FVNWEBAPPContext _db;
    public ITWorkspaceDashboardProvider(FVNWEBAPPContext db) => _db = db;
    public RequestModule Module => RequestModule.IT;
    public int RequiredFunctionCode => SecurityFunctionCodes.SecurityAudit;

    public async Task<ModuleDashboardContribution> GetContributionAsync(UserIdentityDto user, CancellationToken ct = default)
    {
        var audit24h = await _db.AuditLogs.AsNoTracking()
            .CountAsync(x => x.CreatedAt >= DateTime.Now.AddHours(-24), ct);
        var activeSessions = await _db.UserSessions.AsNoTracking().CountAsync(x => x.IsActive != false, ct);
        var pendingEmail = await _db.EmailQueues.AsNoTracking().CountAsync(x => x.IsActive != false, ct);

        return new()
        {
            Module = Module,
            Widgets =
            [
                new() { Title = "Audit log 24h", Value = audit24h.ToString(), Icon = "Security", Color = "Info", Link = "/admin/security", IsPersonal = false },
                new() { Title = "Phiên người dùng", Value = activeSessions.ToString(), Icon = "Devices", Color = "Success", Link = "/admin/sessions", IsPersonal = false },
                new() { Title = "Email queue", Value = pendingEmail.ToString(), Icon = "Mail", Color = "Warning", Link = "/admin/email-queue", IsPersonal = false }
            ]
        };
    }
}
