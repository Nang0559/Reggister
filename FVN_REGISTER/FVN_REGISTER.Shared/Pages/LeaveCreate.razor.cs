using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Contract.Dtos.MasterData;
using FVN_REGISTER.Shared.Dialogs;
using FVN_REGISTER.Shared.Services.Leaves;
using FVN_REGISTER.Shared.Services.Users;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor;

namespace FVN_REGISTER.Shared.Pages;

public partial class LeaveCreate : IAsyncDisposable
{
    [Inject] private ILeaveCreateClientService LeaveService { get; set; } = default!;
    [Inject] private ILeaveTypeClientService LeaveTypeService { get; set; } = default!;
    [Inject] private ICurrentUserClientService CurrentUserService { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private ILogger<LeaveCreate> Logger { get; set; } = default!;

    private LeaveCalendarDataDto? _model;
    private bool _isLoading = true;
    private int _selectedYear = DateTime.Now.Year;
    private ElementReference _calendarRef;
    private DotNetObjectReference<LeaveCreate>? _dotNetRef;
    private bool _calendarInitialized;
    private readonly CancellationTokenSource _cts = new();

    private List<LeaveCalendarEventDto> LeaveEventsFiltered => _model?.MasterData.LeaveEvents
        .Where(x => x.StartDate.Year == _selectedYear)
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
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[LeaveCreate] Init error");
            Snackbar.Add("Không thể tải dữ liệu lịch nghỉ", Severity.Error);
        }
        finally { _isLoading = false; }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_calendarInitialized && !_isLoading && _model != null)
        {
            await InitCalendarAsync();
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
            _selectedYear,
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

    private async Task OnYearChanged(int year)
    {
        _selectedYear = year;
        await LoadDataAsync();
        if (_calendarInitialized)
        {
            await JS.InvokeVoidAsync("leaveCalendar.updateEvents", BuildCalendarEvents());
            await JS.InvokeVoidAsync("leaveCalendar.updateHolidays", GetHolidayDates());
        }
    }

    private async Task InitCalendarAsync()
    {
        try
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("leaveCalendar.init", _calendarRef, _dotNetRef, BuildCalendarEvents(), GetHolidayDates());
            _calendarInitialized = true;
        }
        catch (Exception ex) { Logger.LogError(ex, "[LeaveCreate] Calendar init error"); }
    }

    private async Task RefreshCalendarAsync()
    {
        if (_calendarInitialized)
            await JS.InvokeVoidAsync("leaveCalendar.updateEvents", BuildCalendarEvents());
    }

    private List<string> GetHolidayDates() => _model?.MasterData.CompanyHolidays
        .Select(x => x.Start.ToString("yyyy-MM-dd"))
        .Distinct()
        .ToList() ?? new();

    private List<object> BuildCalendarEvents()
    {
        var events = new List<object>();
        foreach (var holiday in _model?.MasterData.CompanyHolidays ?? new())
        {
            events.Add(new
            {
                id = $"H_{holiday.Start:yyyyMMdd}",
                title = string.IsNullOrWhiteSpace(holiday.Title) ? "Nghỉ công ty" : holiday.Title,
                start = holiday.Start.ToString("yyyy-MM-dd"),
                end = holiday.Start.AddDays(1).ToString("yyyy-MM-dd"),
                status = "Holiday",
                extendedProps = new { status = "Holiday" },
                display = "background"
            });
        }
        foreach (var leave in LeaveEventsFiltered)
        {
            events.Add(new
            {
                id = leave.RequestId.ToString(),
                title = string.IsNullOrWhiteSpace(leave.LeaveTypeName) ? "Nghỉ phép" : leave.LeaveTypeName,
                start = leave.StartDate.ToString("yyyy-MM-dd"),
                end = leave.EndDate.AddDays(1).ToString("yyyy-MM-dd"),
                status = leave.Status,
                extendedProps = new { status = leave.Status, APstatus = leave.Status, inforregister = leave.RequestId.ToString() }
            });
        }
        return events;
    }

    [JSInvokable]
    public Task OnRangeSelect(string startStr, string endStr) => OpenAddDialog(startStr, endStr);

    [JSInvokable]
    public async Task OnEventClick(string detailId, string leaveId, string status, string startDate)
    {
        if (status == "Holiday")
        {
            Snackbar.Add("Đây là ngày nghỉ công ty", Severity.Info);
            return;
        }
        if (!int.TryParse(leaveId, out var requestId))
        {
            Snackbar.Add("Không xác định được đơn nghỉ", Severity.Warning);
            return;
        }
        var item = _model?.MasterData.LeaveEvents.FirstOrDefault(x => x.RequestId == requestId);
        if (item != null) await OpenDetailDialog(item);
    }

    private async Task OpenAddDialog(string? startDate, string? endDate)
    {
        if (_model == null) return;
        var parameters = new DialogParameters
        {
            ["Model"] = _model.MasterData,
            ["DefaultStart"] = startDate,
            ["DefaultEnd"] = endDate,
            ["SelectedYear"] = _selectedYear,
            ["OnSaved"] = EventCallback.Factory.Create(this, OnLeaveSaved)
        };
        await DialogService.ShowAsync<LeaveAddDialog>("Đăng ký nghỉ phép", parameters, new DialogOptions { MaxWidth = MaxWidth.Large, FullWidth = true, CloseOnEscapeKey = true });
    }

    private async Task OpenDetailDialog(LeaveCalendarEventDto item)
    {
        var parameters = new DialogParameters
        {
            ["Item"] = item,
            ["OnCancelled"] = EventCallback.Factory.Create(this, OnLeaveCancelled)
        };
        await DialogService.ShowAsync<LeaveDetailDialog>("Chi tiết đơn nghỉ", parameters, new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseOnEscapeKey = true });
    }

    private async Task OnLeaveSaved() { await LoadDataAsync(); await RefreshCalendarAsync(); }
    private async Task OnLeaveCancelled() { await LoadDataAsync(); await RefreshCalendarAsync(); }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _cts.Dispose();
        _dotNetRef?.Dispose();
        if (_calendarInitialized)
        {
            try { await JS.InvokeVoidAsync("leaveCalendar.destroy"); }
            catch { }
        }
    }
}
