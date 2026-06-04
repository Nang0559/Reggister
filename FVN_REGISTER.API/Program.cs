using FVN_REGISTER.API.Hubs;
using FVN_REGISTER.API.Repositories;
using FVN_REGISTER.API.Services.Auths;
using FVN_REGISTER.API.Services.Emails;
using FVN_REGISTER.API.Services.Escalations;
using FVN_REGISTER.API.Services.Leaves;
using FVN_REGISTER.API.Services.Leaves.FVN_REGISTER.API.Services.Leaves;
using FVN_REGISTER.API.Services.Notifications;
using FVN_REGISTER.API.Services.OT;
using FVN_REGISTER.API.Services.Reports;
using FVN_REGISTER.API.Services.Statics;
using FVN_REGISTER.API.Services.Users;
using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Interfaces.Auths;
using FVN_REGISTER.Contract.Interfaces.Emails;
using FVN_REGISTER.Contract.Interfaces.Leaves;
using FVN_REGISTER.Contract.Interfaces.OT;
using FVN_REGISTER.Contract.Interfaces.Reports;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Interfaces.Statics;
using FVN_REGISTER.Contract.Interfaces.Users;
using FVN_REGISTER.Contract.Maps;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// 1. CORS — SignalR bắt buộc WithOrigins + AllowCredentials
// =========================================================
var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>()
    ?? new[]
    {
        "https://localhost:7264",  // Blazor Web HTTPS
        "http://localhost:5120",   // Blazor Web HTTP
        "http://localhost:5017",   // API HTTP
        "https://localhost:7135"   // API HTTPS
    };

builder.Services.AddCors(options =>
{
    options.AddPolicy("FccCorsPolicy", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Bắt buộc cho SignalR WebSocket
    });
});

// =========================================================
// 2. INFRASTRUCTURE
// =========================================================
builder.Services.AddMemoryCache();

builder.Services.Configure<AuthDebugOptions>(
    builder.Configuration.GetSection("AuthDebug"));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<FVNWEBAPPContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();

// =========================================================
// 3. REPOSITORIES & SERVICES
// =========================================================
builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

// Infrastructure
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INetworkService, NetworkService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IWorkingDayService, WorkingDayService>();
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Business
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ILeaveRepository, LeaveRepository>();
builder.Services.AddScoped<ILeaveQueryService, LeaveQueryService>();
builder.Services.AddScoped<IEscalationRuleService, EscalationRuleService>();
builder.Services.AddScoped<ILeaveValidator, LeaveValidator>();
builder.Services.AddScoped<ILeaveNotificationService, LeaveNotificationService>();
builder.Services.AddScoped<ILeaveEscalationService, LeaveEscalationService>();
builder.Services.AddScoped<ILeaveService, LeaveService>();
builder.Services.AddScoped<IReportService, LeaveReportService>();
builder.Services.AddScoped<IOTNotificationService, OTNotificationService>();
builder.Services.AddScoped<IOTQueryService, OTQueryService>();
builder.Services.AddScoped<IOTValidator, OTValidator>();
builder.Services.AddScoped<IOTService, OTService>();
// User & Auth
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IUserLogService, UserLogService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISessionService, SessionService>();

// =========================================================
// 4. AUTHENTICATION / JWT
// =========================================================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(5),
        NameClaimType = "unique_name",
        RoleClaimType = "role",
    };

    options.Events = new JwtBearerEvents
    {
        // ── Đọc token cho SignalR (WebSocket không hỗ trợ custom header) ──
        OnMessageReceived = context =>
        {
            var path = context.HttpContext.Request.Path;
            if (!path.StartsWithSegments("/hubs"))
                return Task.CompletedTask;

            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<Program>>();
            var debug = context.HttpContext.RequestServices
                .GetRequiredService<IOptionsMonitor<AuthDebugOptions>>()
                .CurrentValue.Enabled;

            // Ưu tiên 1: query string (SignalR JS / .NET client dùng AccessTokenProvider)
            var qs = context.Request.Query["access_token"].ToString();
            if (!string.IsNullOrEmpty(qs))
            {
                context.Token = qs;
                logger.LogDebugIf(debug,
                    "[JWT] OnMessageReceived → token from QS | Path={Path} | Len={Len}",
                    path, qs.Length);
                return Task.CompletedTask;
            }

            // Ưu tiên 2: Authorization header (fallback)
            var header = context.Request.Headers["Authorization"].ToString();
            if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Token = header["Bearer ".Length..].Trim();
                logger.LogDebugIf(debug,
                    "[JWT] OnMessageReceived → token from Header | Path={Path} | Len={Len}",
                    path, context.Token.Length);
                return Task.CompletedTask;
            }

            // Không tìm thấy token — log warn (luôn log, không cần flag)
            logger.LogWarning(
                "[JWT] OnMessageReceived → no token found | Path={Path} | QS_keys={Keys}",
                path,
                string.Join(",", context.Request.Query.Keys));

            return Task.CompletedTask;
        },

        // ── Token validate thành công ──
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<Program>>();
            var debug = context.HttpContext.RequestServices
                .GetRequiredService<IOptionsMonitor<AuthDebugOptions>>()
                .CurrentValue.Enabled;

            var claims = string.Join(" | ",
                context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}") ?? []);

            logger.LogDebugIf(debug,
                "[JWT] TokenValidated | Claims={Claims}", claims);

            return Task.CompletedTask;
        },

        // ── Token lỗi — LUÔN log, không cần flag debug ──
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<Program>>();

            logger.LogError(context.Exception,
                "[JWT] AuthenticationFailed | {ExType}: {ExMsg}",
                context.Exception.GetType().Name,
                context.Exception.Message);

            return Task.CompletedTask;
        },

        // ── 401 Challenge ──
        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<Program>>();
            var debug = context.HttpContext.RequestServices
                .GetRequiredService<IOptionsMonitor<AuthDebugOptions>>()
                .CurrentValue.Enabled;

            logger.LogWarnIf(debug,
                "[JWT] Challenge | Path={Path} | Error={Error} | Desc={Desc}",
                context.Request.Path,
                context.Error,
                context.ErrorDescription);

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddControllers();

// =========================================================
// 5. BACKGROUND WORKERS
// =========================================================
builder.Services.AddHostedService<EmailBackgroundWorker>();
builder.Services.AddHostedService<EscalationBackgroundWorker>();
builder.Services.AddOpenApi();

// =========================================================
// 6. BUILD & PIPELINE
// =========================================================
var app = builder.Build();

// Lấy logger + debug flag sau khi build xong
var appLogger = app.Services.GetRequiredService<ILogger<Program>>();
var debugOptions = app.Services.GetRequiredService<IOptionsMonitor<AuthDebugOptions>>();
bool IsDebug() => debugOptions.CurrentValue.Enabled;

appLogger.LogInfoIf(IsDebug(),
    "[STARTUP] AuthDebug.Enabled={Debug} | Env={Env}",
    IsDebug(), app.Environment.EnvironmentName);

// ── OpenAPI (chỉ dev) ──
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Endpoint test notification — CHỈ dùng khi debug
    app.MapGet("/test-notify/{userId}", async (
        int userId,
        INotificationService svc,
        ILogger<Program> logger,
        IOptionsMonitor<AuthDebugOptions> opts) =>
    {
        var debug = opts.CurrentValue.Enabled;
        logger.LogInfoIf(debug,
            "[TEST] Creating test notification for UserId={UserId}", userId);

        await svc.CreateAsync(new CreateNotificationDto
        {
            UserId = userId,
            NotificationType = "TEST",
            Title = "Test thông báo",
            Body = "Nội dung test từ server",
            ActionUrl = "/leave/history"
        });

        logger.LogInfoIf(debug,
            "[TEST] Notification created for UserId={UserId}", userId);

        return Results.Ok(new { message = $"Sent to userId={userId}" });
    });
}

// ── Middleware pipeline (THỨ TỰ QUAN TRỌNG) ──
app.UseCors("FccCorsPolicy");       // 1. CORS trước tất cả
app.UseHttpsRedirection();          // 2. HTTPS redirect
app.UseAuthentication();            // 3. Xác thực (JWT)
app.UseAuthorization();             // 4. Phân quyền

app.MapControllers();               // 5. Controllers
app.MapHub<NotificationHub>("/hubs/notification"); // 6. SignalR Hub

appLogger.LogInfoIf(IsDebug(),
    "[STARTUP] Pipeline ready | Hub=/hubs/notification");

app.Run();