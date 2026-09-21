using FVN_REGISTER.Application.Logging;
using FVN_REGISTER.Contract.Dtos.Calendar;
using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Shared.Dialogs;
using FVN_REGISTER.Shared.Services.Leaves;
using FVN_REGISTER.Shared.Services.Users;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FVN_REGISTER.Shared.Pages;

public partial class LeaveCreate : IAsyncDisposable
{
    [SupplyParameterFromQuery(Name = "date")]
    public DateTime? CalendarDate { get; set; }
    [Inject] private ILeaveCreateClientService LeaveService { get; set; } = default!;
    [Inject] private ILeaveTypeClientService LeaveTypeService { get; set; } = default!;
    [Inject] private ICurrentUserClientService CurrentUserService { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private LeaveCalendarDataDto? _model;
    private bool _isLoading = true;
    private readonly CancellationTokenSource _cts = new();

    private List<LeaveCalendarEventDto> LeaveEventsFiltered =>
        _model?.MasterData.LeaveEvents
            .OrderByDescending(x => x.StartDate)
            .ToList() ?? new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            if (!CurrentUserService.IsLoggedIn)
                await CurrentUserService.InitializeAsync();

            if (!CurrentUserService.IsLoggedIn)
            {
                Nav.NavigateTo("/login");
                return;
            }

            await LoadDataAsync();

            if (CalendarDate.HasValue)
                await OpenAddDialog(CalendarDate.Value.ToString("yyyy-MM-dd"), CalendarDate.Value.ToString("yyyy-MM-dd"));
        }
        catch (Exception ex)
        {
            Logger.LogErrorIf(Debug, ex, "[LeaveCreate] Init error");
            Snackbar.Add("Không thể tải dữ liệu lịch nghỉ", Severity.Error);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task LoadDataAsync()
    {
        var user = CurrentUserService.User;
        if (user == null) return;

        var result = await LeaveService.GetCombinedDataAsync(
            user.EmployeeCode ?? string.Empty,
            user.DeptCode ?? string.Empty,
            user.PositionCode ?? string.Empty,
            DateTime.Now.Year,
            _cts.Token);

        if (!result.IsSuccess || result.Data == null)
        {
            Snackbar.Add(result.Message ?? "Lỗi tải dữ liệu", Severity.Error);
            return;
        }

        _model = result.Data;
        var leaveTypes = await LeaveTypeService.GetListAsync(isActive: true, ct: _cts.Token);
        if (leaveTypes.IsSuccess && leaveTypes.Data != null)
            _model.MasterData.LeaveTypes = leaveTypes.Data;
    }

    private Task OnCalendarDateSelected(DateTime date) =>
        OpenAddDialog(date.ToString("yyyy-MM-dd"), date.ToString("yyyy-MM-dd"));

    private async Task OnCalendarEventSelected(CalendarItemDto item)
    {
        if (item.RequiresAction && item.ActionId is not null)
        {
            var route = !string.IsNullOrWhiteSpace(item.DetailRoute)
                ? item.DetailRoute
                : $"/execution?reconciliationId={Uri.EscapeDataString(item.SourceId)}";
            Nav.NavigateTo(route);
            return;
        }

        if (item.ModuleCode == "LEAVE")
        {
            var leave = _model?.MasterData.LeaveEvents
                .FirstOrDefault(x => x.StartDate.Date == item.WorkDate);

            if (leave != null)
            {
                await OpenDetailDialog(leave);
                return;
            }
        }

        Snackbar.Add(item.Summary ?? item.StatusCode, Severity.Info);
    }


    private async Task OpenAddDialog(string? startDate, string? endDate)
    {
        if (_model == null) return;

        var parameters = new DialogParameters
        {
            ["Model"] = _model.MasterData,
            ["DefaultStart"] = startDate,
            ["DefaultEnd"] = endDate,
            ["SelectedYear"] = DateTime.Now.Year,
            ["OnSaved"] = EventCallback.Factory.Create(this, OnLeaveSaved)
        };

        await DialogService.ShowAsync<LeaveAddDialog>(
            "Đăng ký nghỉ phép",
            parameters,
            new DialogOptions { MaxWidth = MaxWidth.Large, FullWidth = true, CloseOnEscapeKey = true });
    }

    private async Task OpenDetailDialog(LeaveCalendarEventDto item)
    {
        var parameters = new DialogParameters
        {
            ["Item"] = item,
            ["OnCancelled"] = EventCallback.Factory.Create(this, OnLeaveCancelled)
        };

        await DialogService.ShowAsync<LeaveDetailDialog>(
            "Chi tiết đơn nghỉ",
            parameters,
            new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseOnEscapeKey = true });
    }

    private async Task OnLeaveSaved()
    {
        await LoadDataAsync();
    }

    private async Task OnLeaveCancelled()
    {
        await LoadDataAsync();
    }

    public ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _cts.Dispose();
        return ValueTask.CompletedTask;
    }
}