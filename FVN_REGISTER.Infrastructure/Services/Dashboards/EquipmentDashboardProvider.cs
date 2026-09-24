using FVN_REGISTER.Application.Interfaces.Dashboards;
using FVN_REGISTER.Application.Interfaces.Equipment;
using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Dtos.Dashboard;
using FVN_REGISTER.Contract.Dtos.Equipment;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Infrastructure.Services.Dashboards;

public sealed class EquipmentDashboardProvider : IModuleDashboardProvider
{
    private readonly IEquipmentService _service;
    public RequestModule Module => RequestModule.Equipment;
    public int RequiredFunctionCode => SecurityFunctionCodes.EquipmentView;
    public EquipmentDashboardProvider(IEquipmentService service) => _service = service;

    public async Task<ModuleDashboardContribution> GetContributionAsync(UserIdentityDto user, CancellationToken ct = default)
    {
        var result = await _service.GetMineAsync(ct);
        var rows = result.IsSuccess && result.Data != null
            ? result.Data
            : new List<EquipmentRequestDto>();

        var pending = rows.Count(x =>
            x.RequestStatus == ApprovalStatus.Pending ||
            x.RequestStatus == ApprovalStatus.InProgress);

        var approved = rows.Count(x =>
            x.RequestStatus == ApprovalStatus.Approved);
        var widgets = new List<WidgetCounterDto>
        {
            new() { Title = "Thiết bị chờ duyệt", Value = pending.ToString(), Icon = "Devices", Color = "Info", Link = "/equipment", IsPersonal = true },
            new() { Title = "Thiết bị đã duyệt", Value = approved.ToString(), Icon = "Verified", Color = "Success", Link = "/equipment", IsPersonal = true }
        };
        return new ModuleDashboardContribution { Module = Module, Widgets = widgets, Detail = rows.Take(5).ToList() };
    }
}
