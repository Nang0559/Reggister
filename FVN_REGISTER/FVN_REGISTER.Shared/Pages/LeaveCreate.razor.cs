using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Shared.Dialogs;
using FVN_REGISTER.Shared.Utils.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor;


namespace FVN_REGISTER.Shared.Pages
{
    public partial class LeaveCreate : AppComponentBase, IAsyncDisposable
    {
        // ─── State ───────────────────────────────────────────
        private CombinedHolidaysViewModel? _model;
        private bool _isLoading = true;
        private int _selectedYear = DateTime.Now.Year;

        // ─── JS Interop ──────────────────────────────────────
        private ElementReference _calendarRef;
        private DotNetObjectReference<LeaveCreate>? _dotNetRef;
        private bool _calendarInitialized;

        // Khai báo CancellationTokenSource đưa lên đầu phần State cho chuẩn bộ nhớ
        private readonly CancellationTokenSource _cts = new();

        // ─── Lifecycle ───────────────────────────────────────
        protected override async Task OnInitializedAsync()
        {
            try
            {
                _isLoading = true;

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
            finally
            {
                _isLoading = false;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !_isLoading && _model != null)
            {
                await InitCalendarAsync();
            }
        }

        // ─── Data ────────────────────────────────────────────
        private async Task LoadDataAsync()
        {
            var user = CurrentUserService.User;
            if (user == null) return;

            var result = await LeaveService.GetCombinedDataAsync(
                user.EmployeeCode ?? "",
                user.DeptCode ?? "",
                user.CVCode ?? "",
                _selectedYear,
                _cts.Token);

            if (result.IsSuccess && result.Data != null)
            {
                _model = result.Data;
            }
            else
            {
                Snackbar.Add(result.Message ?? "Lỗi tải dữ liệu", Severity.Error);
            }
        }

        private async Task OnYearChanged(int year)
        {
            _selectedYear = year;
            await LoadDataAsync();
            if (_calendarInitialized)
                await RefreshCalendarEventsAsync();
            StateHasChanged();
        }

        // ─── Calendar JS Interop ─────────────────────────────
        private async Task InitCalendarAsync()
        {
            try
            {
                _dotNetRef = DotNetObjectReference.Create(this);
                var events = BuildCalendarEvents();
                var holidayDates = _model?.CompanyHolidays
                    .Where(x => x.Start != null)
                    .Select(x => x.Start!)
                    .ToList() ?? new();

                await JS.InvokeVoidAsync("leaveCalendar.init",
                    _calendarRef,
                    _dotNetRef,
                    events,
                    holidayDates,
                    _selectedYear);

                _calendarInitialized = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[LeaveCreate] Calendar init error");
            }
        }

        private async Task RefreshCalendarEventsAsync()
        {
            if (!_calendarInitialized) return;
            var events = BuildCalendarEvents();
            await JS.InvokeVoidAsync("leaveCalendar.updateEvents", events);
        }

        private List<object> BuildCalendarEvents()
        {
            var events = new List<object>();

            // Ngày nghỉ công ty
            foreach (var h in _model?.CompanyHolidays ?? new())
            {
                events.Add(new
                {
                    id = $"H_{h.Id}",
                    title = "Nghỉ CT",
                    start = h.Start,
                    end = h.End,
                    color = "#ffc107",
                    textColor = "#212529",
                    extendedProps = h.ExtendedProps,
                    status = "Holiday"
                });
            }

            // Đơn cá nhân
            foreach (var d in _model?.LeaveDays ?? new())
            {
                var color = d.Status switch
                {
                    "Approved" => "#28a745",
                    "Rejected" => "#dc3545",
                    _ => "#fd7e14"
                };

                events.Add(new
                {
                    id = d.Id,
                    title = "Nghỉ phép",
                    start = d.Start,
                    end = d.End,
                    color,
                    extendedProps = d.ExtendedProps,
                    status = d.Status
                });
            }

            return events;
        }

        // ─── JS Callbacks ───────────────────────────────────

        [JSInvokable]
        public async Task OnDateClick(string dateStr)
        {
            var isHoliday = _model?.CompanyHolidays.Any(x => x.Start == dateStr) ?? false;
            if (isHoliday)
            {
                Snackbar.Add("Ngày nghỉ công ty, không thể đăng ký", Severity.Warning);
                return;
            }
            await OpenAddDialog(dateStr);
        }

        [JSInvokable]
        public async Task OnEventClick(string eventId, string status)
        {
            if (status == "Holiday")
            {
                Snackbar.Add("Đây là ngày nghỉ công ty", Severity.Info);
                return;
            }

            var leaveItem = _model?.LeaveDays.FirstOrDefault(x => x.Id == eventId);
            if (leaveItem != null)
                await OpenDetailDialog(leaveItem);
        }

        // ─── Dialogs ─────────────────────────────────────────

        private async Task OpenAddDialog(string? defaultDate = null)
        {
            var parameters = new DialogParameters
            {
                ["Model"] = _model!,
                ["DefaultDate"] = defaultDate,
                ["SelectedYear"] = _selectedYear,
                ["OnSaved"] = EventCallback.Factory.Create(this, OnLeaveSaved)
            };

            var options = new DialogOptions
            {
                MaxWidth = MaxWidth.Large,
                FullWidth = true,
                CloseOnEscapeKey = true
            };

            await DialogService.ShowAsync<LeaveAddDialog>("Đăng ký nghỉ phép", parameters, options);
        }

        private async Task OpenDetailDialog(HolidayViewModel item)
        {
            var parameters = new DialogParameters
            {
                ["Item"] = item,
                ["OnCancelled"] = EventCallback.Factory.Create(this, OnLeaveCancelled)
            };

            var options = new DialogOptions
            {
                MaxWidth = MaxWidth.Small,
                FullWidth = true,
                CloseOnEscapeKey = true
            };

            await DialogService.ShowAsync<LeaveDetailDialog>("Chi tiết đơn nghỉ", parameters, options);
        }

        // ─── Callbacks ───────────────────────────────────────

        private async Task OnLeaveSaved()
        {
            await LoadDataAsync();
            await RefreshCalendarEventsAsync();
            StateHasChanged();
            Snackbar.Add("Đã gửi đơn thành công!", Severity.Success);
        }

        private async Task OnLeaveCancelled()
        {
            await LoadDataAsync();
            await RefreshCalendarEventsAsync();
            StateHasChanged();
            Snackbar.Add("Đã hủy đơn", Severity.Info);
        }

        // ─── Cleanup ─────────────────────────────────────────

        public async ValueTask DisposeAsync()
        {
            try
            {
                _cts.Cancel();
                _cts.Dispose();
            }
            catch { /* ignore */ }

            _dotNetRef?.Dispose();

            if (_calendarInitialized)
            {
                try { await JS.InvokeVoidAsync("leaveCalendar.destroy"); }
                catch { /* ignore */ }
            }
        }
    }
}
