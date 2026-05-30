
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Shared.Dialogs;
using FVN_REGISTER.Shared.Services.Leaves;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor;

namespace FVN_REGISTER.Shared.Pages;

public partial class LeaveCreate : IAsyncDisposable
{
    private CombinedHolidaysViewModel? _model;
    private bool _isLoading = true;
    private int _selectedYear = DateTime.Now.Year;

    private ElementReference _calendarRef;
    private DotNetObjectReference<LeaveCreate>? _dotNetRef;
    private bool _calendarInitialized;
    private readonly CancellationTokenSource _cts = new();

    // ─── Lifecycle ───────────────────────────────────────
    protected override async Task OnInitializedAsync()
    {
        try
        {
            _isLoading = true;
            if (!CurrentUserService.IsLoggedIn)
                await CurrentUserService.InitializeAsync();

            if (!CurrentUserService.IsLoggedIn) { Nav.NavigateTo("/login"); return; }

            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[LeaveCreate] Init error");
            Snackbar.Add("Không thể tải dữ liệu lịch nghỉ", Severity.Error);
        }
        finally { 
            _isLoading = false;
            StateHasChanged();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // ✅ Không check firstRender — check _calendarInitialized thay thế
        // Mỗi lần render: nếu model đã có, calendar chưa init → init
        if (!_calendarInitialized && !_isLoading && _model != null)
        {
            await InitCalendarAsync();
            StateHasChanged(); // trigger thêm 1 render để cập nhật UI nếu cần
        }
    }

    // ─── Data ────────────────────────────────────────────
    private async Task LoadDataAsync()
    {
        var user = CurrentUserService.User;
        if (user == null) return;

        var result = await LeaveService.GetCombinedDataAsync(
            user.EmployeeCode ?? "", user.DeptCode ?? "", user.CVCode ?? "",
            _selectedYear, _cts.Token);

        if (result.IsSuccess && result.Data != null)
            _model = result.Data;
        else
            Snackbar.Add(result.Message ?? "Lỗi tải dữ liệu", Severity.Error);
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
        StateHasChanged();
    }

    // ─── Calendar ────────────────────────────────────────
    private async Task InitCalendarAsync()
    {
        try
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("leaveCalendar.init",
                _calendarRef, _dotNetRef, BuildCalendarEvents(), GetHolidayDates());
            _calendarInitialized = true;
        }
        catch (Exception ex) { Logger.LogError(ex, "[LeaveCreate] Calendar init error"); }
    }

    private async Task RefreshCalendarAsync()
    {
        if (!_calendarInitialized) return;
        await JS.InvokeVoidAsync("leaveCalendar.updateEvents", BuildCalendarEvents());
    }

    private List<string> GetHolidayDates() =>
        _model?.CompanyHolidays.Where(x => x.Start != null).Select(x => x.Start!).ToList() ?? new();

    private List<object> BuildCalendarEvents()
    {
        var events = new List<object>();

        foreach (var h in _model?.CompanyHolidays ?? new())
        {
            events.Add(new
            {
                id = $"H_{h.Id}",
                title = h.ExtendedProps?.description ?? "Nghỉ CT",
                start = h.Start,
                end = h.End,
                color = "#ffc107",
                textColor = "#212529",
                extendedProps = new { status = "Holiday" },
                display = "background"
            });
        }

        foreach (var d in _model?.LeaveDays ?? new())
        {
            var color = d.Status switch
            {
                "Approved" => "#28a745",
                "Rejected" => "#dc3545",
                "ApprovedLv1" => "#17a2b8",
                "ApprovedLv2" => "#17a2b8",
                _ => "#fd7e14"
            };
            events.Add(new
            {
                id = d.Id,
                title = "Nghỉ phép",
                start = d.Start,
                end = d.End,
                color,
                extendedProps = new { status = d.Status, APstatus = d.Status, inforregister = d.Inforregister }
            });
        }

        return events;
    }

    // ─── JS Callbacks ────────────────────────────────────

    /// <summary>
    /// Callback chung: click 1 ngày (startStr==endStr) HOẶC kéo chọn range.
    /// JS luôn gọi OnRangeSelect, không cần OnDateClick nữa.
    /// </summary>
    [JSInvokable]
    public async Task OnRangeSelect(string startStr, string endStr)
    {
        await OpenAddDialog(startStr, endStr);
    }

    [JSInvokable]
    public async Task OnEventClick(
     string detailId,
     string leaveId,
     string status,
     string startDate)
    {
        if (status == "Holiday")
        {
            Snackbar.Add("Đây là ngày nghỉ công ty", Severity.Info);
            return;
        }

        // Tìm theo Inforregister (leaveId) trước, fallback theo Id
        var leaveItem = _model?.LeaveDays
            .FirstOrDefault(x => x.Inforregister == leaveId)
            ?? _model?.LeaveDays
            .FirstOrDefault(x => x.Id == leaveId || x.Id == detailId);

        if (leaveItem == null)
        {
            Snackbar.Add("Không tìm thấy thông tin đơn", Severity.Warning);
            return;
        }
        int.TryParse(detailId, out var detailIdInt);
        await OpenDetailDialog(leaveItem, detailIdInt);
    }

    // ─── Dialogs ─────────────────────────────────────────
    // LeaveCreate.razor.cs — sửa signature
    private async Task OpenAddDialog(string? startDate, string? endDate)
    {
        if (_model == null) return;

        var parameters = new DialogParameters
        {
            ["Model"] = _model,
            ["DefaultStart"] = startDate ?? "",
            ["DefaultEnd"] = endDate ?? "",
            ["SelectedYear"] = _selectedYear,
            ["OnSaved"] = EventCallback.Factory.Create(this, OnLeaveSaved)
        };

        await DialogService.ShowAsync<LeaveAddDialog>("Đăng ký nghỉ phép",
            parameters,
            new DialogOptions { MaxWidth = MaxWidth.Large, FullWidth = true, CloseOnEscapeKey = true });
    }

    private async Task OpenDetailDialog(HolidayViewModel item, int detailId = 0)
    {
        var parameters = new DialogParameters
        {
            ["Item"] = item,
            ["DetailId"] = detailId,        // ✅ THÊM
            ["OnCancelled"] = EventCallback.Factory.Create(this, OnLeaveCancelled)
        };

        await DialogService.ShowAsync<LeaveDetailDialog>(
            "Chi tiết đơn nghỉ",
            parameters,
            new DialogOptions
            {
                MaxWidth = MaxWidth.Small,
                FullWidth = true,
                CloseOnEscapeKey = true
            });
    }

    private async Task OnLeaveSaved()
    {
        await LoadDataAsync();
        await RefreshCalendarAsync();
        StateHasChanged();
    }

    private async Task OnLeaveCancelled()
    {
        await LoadDataAsync();
        await RefreshCalendarAsync();
        StateHasChanged();
    }

    // ─── Cleanup ─────────────────────────────────────────
    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _cts.Dispose();
        _dotNetRef?.Dispose();
        if (_calendarInitialized)
        {
            try { await JS.InvokeVoidAsync("leaveCalendar.destroy"); } catch { }
        }
    }

    public void Dispose() { _cts.Cancel(); _cts.Dispose(); _dotNetRef?.Dispose(); }
}

