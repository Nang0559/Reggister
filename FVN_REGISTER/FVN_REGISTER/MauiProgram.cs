using FVN_REGISTER.Services;
using FVN_REGISTER.Shared.Handlers;
using FVN_REGISTER.Shared.Services.Approvals;
using FVN_REGISTER.Shared.Services.Security;
using FVN_REGISTER.Shared.Services.HrmSync;
using FVN_REGISTER.Shared.Services.Calendar;
using FVN_REGISTER.Shared.Services.Dashboards;
using FVN_REGISTER.Shared.Services.Departments;
using FVN_REGISTER.Shared.Services.Emails;
using FVN_REGISTER.Shared.Services.Employees;
using FVN_REGISTER.Shared.Services.Histories;
using FVN_REGISTER.Shared.Services.History;
using FVN_REGISTER.Shared.Services.Leaves;
using FVN_REGISTER.Shared.Services.Notifications;
using FVN_REGISTER.Shared.Services.OTs;
using FVN_REGISTER.Shared.Services.Trips;
using FVN_REGISTER.Shared.Services.Users;
using FVN_REGISTER.Shared.Utils;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;

namespace FVN_REGISTER
{
    public static class MauiProgram
    {
        [System.Runtime.Versioning.SupportedOSPlatform("Android21.0")]
        [System.Runtime.Versioning.SupportedOSPlatform("iOS13.0")]
        [System.Runtime.Versioning.SupportedOSPlatform("Windows10.0.17763.0")]
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>().ConfigureFonts(fonts => fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"));
            builder.Services.AddMauiBlazorWebView();
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
#if WINDOWS
            Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("Debugging", (handler, view) =>
            {
                handler.PlatformView.CoreWebView2Initialized += (sender, args) =>
                {
                    handler.PlatformView.CoreWebView2.Settings.AreDevToolsEnabled = true;
                    handler.PlatformView.CoreWebView2.Profile.PreferredTrackingPreventionLevel = Microsoft.Web.WebView2.Core.CoreWebView2TrackingPreventionLevel.None;
                };
            });
#endif
            builder.Logging.AddDebug();
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => { var ex = e.ExceptionObject as Exception; System.Diagnostics.Debug.WriteLine("=== [UNHANDLED EXCEPTION] ==="); System.Diagnostics.Debug.WriteLine($"Message : {ex?.Message}"); System.Diagnostics.Debug.WriteLine($"Stack   :\n{ex?.StackTrace}"); };
            TaskScheduler.UnobservedTaskException += (sender, e) => { System.Diagnostics.Debug.WriteLine("=== [TASK EXCEPTION] ==="); System.Diagnostics.Debug.WriteLine($"Message : {e.Exception?.Message}"); e.SetObserved(); };
#endif
            builder.Services.AddMudServices(); builder.Services.AddAuthorizationCore(); builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<ITokenStorage, SecureTokenService>(); builder.Services.AddScoped<CustomAuthStateProvider>(); builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());
            builder.Services.AddTransient<AuthHeaderHandler>();
            string baseUrl = "http://localhost:5017/"; if (!baseUrl.EndsWith("/")) baseUrl += "/";
            builder.Services.AddHttpClient<IHttpClientWithAuth, AuthorizedHttpClient>(client => client.BaseAddress = new Uri(baseUrl)).ConfigurePrimaryHttpMessageHandler(() => GetInsecureHandler()).AddHttpMessageHandler<AuthHeaderHandler>();
            builder.Services.AddHttpClient<IAuthClientService, AuthClientService>(client => client.BaseAddress = new Uri(baseUrl)).ConfigurePrimaryHttpMessageHandler(() => GetInsecureHandler());
            builder.Services.AddScoped<ICurrentUserClientService, CurrentUserClientService>(); 
            builder.Services.AddScoped<IDashboardClientService, DashboardClientService>(); builder.Services.AddScoped<ILeaveCreateClientService, LeaveCreateClientService>();
            builder.Services.AddScoped<ILeaveHistorysClientService, LeaveHistorysClientService>();
            builder.Services.AddScoped<IReportClientService, ReportClientService>();
            builder.Services.AddScoped<IApprovalRouteClientService, ApprovalRouteClientService>();
            builder.Services.AddScoped<IApprovalPolicyClientService, ApprovalPolicyClientService>();
            builder.Services.AddScoped<ISecurityClientService, SecurityClientService>();
            builder.Services.AddScoped<IHrmSyncClientService, HrmSyncClientService>();
            builder.Services.AddScoped<IHrmSyncReviewClientService, HrmSyncReviewClientService>();
            builder.Services.AddScoped<IHrmAttendanceCalculationClientService, HrmAttendanceCalculationClientService>();
            builder.Services.AddScoped<IHrmUserRoleRuleClientService, HrmUserRoleRuleClientService>();
            builder.Services.AddScoped<IWorkYearManagementClientService, WorkYearManagementClientService>();
            builder.Services.AddScoped<ICompanyHolidayManagementClientService, CompanyHolidayManagementClientService>();

            builder.Services.AddScoped<INotificationClientService, NotificationClientService>(); 
            builder.Services.AddScoped<IDeptClientService, DeptClientService>(); 
            builder.Services.AddScoped<IOTClientService, OTClientService>();
            builder.Services.AddScoped<IOTLimitRuleManagementClientService, OTLimitRuleManagementClientService>(); 
   
            builder.Services.AddScoped<IDepartmentStatusClientService, DepartmentStatusClientService>(); builder.Services.AddScoped<IUserManagementClientService, UserManagementClientService>(); builder.Services.AddScoped<IApprovalListClientService, ApprovalListClientService>(); builder.Services.AddScoped<IApproverClientService, ApproverClientService>(); builder.Services.AddScoped<IHistoryClientService, HistoryClientService>(); builder.Services.AddScoped<IEmailTemplateClientService, EmailTemplateClientService>(); builder.Services.AddScoped<IEmailQueueClientService, EmailQueueClientService>(); builder.Services.AddScoped<IEmployeeManagementClientService, EmployeeManagementClientService>(); builder.Services.AddScoped<IDepartmentManagementClientService, DepartmentManagementClientService>(); builder.Services.AddScoped<ILeaveTypeClientService, LeaveTypeClientService>(); builder.Services.AddScoped<ITripClientService, TripClientService>(); builder.Services.AddScoped<IWorkCalendarClientService, WorkCalendarClientService>();
            builder.Services.AddScoped<FVN_REGISTER.Shared.Services.Equipment.IEquipmentClientService, FVN_REGISTER.Shared.Services.Equipment.EquipmentClientService>();
            return builder.Build();
            static HttpClientHandler GetInsecureHandler() { var handler = new HttpClientHandler();
#if DEBUG
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif
                return handler; }
        }
    }
}
