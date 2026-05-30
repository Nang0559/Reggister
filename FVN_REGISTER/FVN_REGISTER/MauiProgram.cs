using FVN_REGISTER.Services;
using FVN_REGISTER.Shared.Handlers;
using FVN_REGISTER.Shared.Services.Dashboards;
using FVN_REGISTER.Shared.Services.Leaves;
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
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("Debugging", (handler, view) =>
            {
            #if WINDOWS
                handler.PlatformView.CoreWebView2.Settings.AreDevToolsEnabled = true;
            #endif
            });
            builder.Logging.AddDebug();
            // ✅ Bắt tất cả lỗi unhandled — xem trong Output window (Debug)
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                System.Diagnostics.Debug.WriteLine("=== [UNHANDLED EXCEPTION] ===");
                System.Diagnostics.Debug.WriteLine($"Message : {ex?.Message}");
                System.Diagnostics.Debug.WriteLine($"Type    : {ex?.GetType().FullName}");
                System.Diagnostics.Debug.WriteLine($"Stack   :\n{ex?.StackTrace}");
                if (ex?.InnerException != null)
                    System.Diagnostics.Debug.WriteLine($"Inner   : {ex.InnerException.Message}\n{ex.InnerException.StackTrace}");
            };

            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine("=== [TASK EXCEPTION] ===");
                System.Diagnostics.Debug.WriteLine($"Message : {e.Exception?.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack   :\n{e.Exception?.StackTrace}");
                e.SetObserved();
            };
#endif

            // 1. UI & AUTH CORE
            builder.Services.AddMudServices();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddCascadingAuthenticationState();
            // 2. STORAGE & AUTH PROVIDER
            builder.Services.AddScoped<ITokenStorage, SecureTokenService>();
            builder.Services.AddScoped<CustomAuthStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
                sp.GetRequiredService<CustomAuthStateProvider>());

            // 3. HTTP CLIENT CẤU HÌNH
            builder.Services.AddTransient<AuthHeaderHandler>(); // Fix 1: Đăng ký Handler

            string baseUrl = DeviceInfo.Platform == DevicePlatform.Android
                            ? "https://10.0.2.2:7135/"
                            : "https://localhost:7135/";

            if (!baseUrl.EndsWith("/")) baseUrl += "/";

            // Client dùng cho các API cần Token (Profile, Dashboard...)
            builder.Services.AddHttpClient<IHttpClientWithAuth, AuthorizedHttpClient>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
            })
            .ConfigurePrimaryHttpMessageHandler(() => GetInsecureHandler()) // Tách ra cho sạch code
            .AddHttpMessageHandler<AuthHeaderHandler>();

            // Client dùng cho Login (Fix 2: Đăng ký Typed HttpClient cho Auth)
            builder.Services.AddHttpClient<IAuthClientService, AuthClientService>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
            })
            .ConfigurePrimaryHttpMessageHandler(() => GetInsecureHandler());

            // 4. BUSINESS SERVICES
            builder.Services.AddScoped<ICurrentUserClientService, CurrentUserClientService>();
            builder.Services.AddScoped<IDashboardClientService, DashboardClientService>();
            builder.Services.AddScoped<ILeaveCreateClientService, LeaveCreateClientService>();
            builder.Services.AddScoped<IApproveClientService, ApproveClientService>();
            return builder.Build();

            // Hàm hỗ trợ bypass SSL cho Debug
            static HttpClientHandler GetInsecureHandler()
            {
                var handler = new HttpClientHandler();
#if DEBUG
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif
                return handler;
            }
        }
    }
}
