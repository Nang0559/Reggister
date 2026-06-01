using Blazored.LocalStorage;
using FVN_REGISTER.API.Services.Auths;
using FVN_REGISTER.API.Services.Notifications;
using FVN_REGISTER.Contract.Interfaces.Auths;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Shared.Handlers;
using FVN_REGISTER.Shared.Services.Dashboards;
using FVN_REGISTER.Shared.Services.Leaves;
using FVN_REGISTER.Shared.Services.Users;
using FVN_REGISTER.Shared.Utils;
using FVN_REGISTER.Web.Components;
using FVN_REGISTER.Web.Services;
using FVN_REGISTER.Web.Services.FVN_REGISTER.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
#region DK

var builder = WebApplication.CreateBuilder(args);

// --- 1. UI ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();
builder.Services.Configure<AuthDebugOptions>(
    builder.Configuration.GetSection("AuthDebug"));
// --- 2. AUTH ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
})
.AddCookie("Cookies", options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/access-denied";
});

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

// 🔥 SINGLE INSTANCE TRONG CIRCUIT
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CustomAuthStateProvider>());

// --- 3. STORAGE ---
builder.Services.AddScoped<ITokenStorage, LocalStorageTokenService>();

// --- 4. HANDLER ---
builder.Services.AddTransient<AuthHeaderHandler>();

// --- 5. HTTP CLIENT ---
var baseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5017/";
if (!baseUrl.EndsWith("/")) baseUrl += "/";

// A. Login client (NO AUTH)
builder.Services.AddHttpClient<IAuthClientService, AuthClientService>(client =>
{
    client.BaseAddress = new Uri(baseUrl);
});

// B. Auth client (🔥 FIX QUAN TRỌNG)
builder.Services.AddHttpClient<IHttpClientWithAuth, AuthorizedHttpClient>(client =>
{
    client.BaseAddress = new Uri(baseUrl);
})
.AddHttpMessageHandler(sp =>
    sp.GetRequiredService<AuthHeaderHandler>()); // 🔥 FIX INSTANCE

// --- 6. BUSINESS ---
builder.Services.AddScoped<ICurrentUserClientService, CurrentUserClientService>();
builder.Services.AddScoped<IDashboardClientService, DashboardClientService>();
builder.Services.AddScoped<ILeaveCreateClientService, LeaveCreateClientService>();
builder.Services.AddScoped<IApproveClientService, ApproveClientService>();
builder.Services.AddScoped<ILeaveHistorysClientService, LeaveHistorysClientService>();

builder.Services.AddScoped<INotificationClientService, NotificationClientService>();


builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options => { options.DetailedErrors = true; });

var app = builder.Build();

// --- PIPELINE ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(FVN_REGISTER.Shared._Imports).Assembly);

app.Run();

#endregion