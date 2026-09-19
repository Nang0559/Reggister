using FVN_REGISTER.Application.Interfaces.Dashboards;
using FVN_REGISTER.Application.Interfaces.Trips;
using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Dashboard;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Infrastructure.Services.Dashboards;

public sealed class TripDashboardProvider : IModuleDashboardProvider
{
    private readonly ITripService _service;
    public RequestModule Module => RequestModule.Trip;
    public int RequiredFunctionCode => SecurityFunctionCodes.TripView;
    public TripDashboardProvider(ITripService service) => _service = service;

    public async Task<ModuleDashboardContribution> GetContributionAsync(UserIdentityDto user, CancellationToken ct = default)
    {
        var rows = await _service.GetMineAsync(ct);
        var pending = rows.Count(x => x.RequestStatus == ApprovalStatus.Pending || x.RequestStatus == ApprovalStatus.InProgress);
        var approved = rows.Count(x => x.RequestStatus == ApprovalStatus.Approved);
        var widgets = new List<WidgetCounterDto>
        {
            new() { Title = "Công tác đang chờ", Value = pending.ToString(), Icon = "FlightTakeoff", Color = "Info", Link = "/trip/history", IsPersonal = true },
            new() { Title = "Công tác đã duyệt", Value = approved.ToString(), Icon = "CheckCircle", Color = "Success", Link = "/trip/history", IsPersonal = true }
        };
        return new ModuleDashboardContribution { Module = Module, Widgets = widgets, Detail = rows.Take(5).ToList() };
    }
}
