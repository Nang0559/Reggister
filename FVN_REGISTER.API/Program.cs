using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Auths;
using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Application.Interfaces.Companies;
using FVN_REGISTER.Application.Interfaces.EmailTemplates;
using FVN_REGISTER.Application.Interfaces.Emails;
using FVN_REGISTER.Application.Interfaces.Employees;
using FVN_REGISTER.Application.Interfaces.Histories;
using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Interfaces.Notifications;
using FVN_REGISTER.Application.Interfaces.OT;
using FVN_REGISTER.Application.Interfaces.OTTypes;
using FVN_REGISTER.Application.Interfaces.Reports;
using FVN_REGISTER.Application.Interfaces.Statics;
using FVN_REGISTER.Application.Interfaces.UserManagers;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Interfaces.Jobs;
using FVN_REGISTER.Application.Services.Statics;
using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Maps;
using FVN_REGISTER.Contract.Models.Subjects;
using FVN_REGISTER.Contract.ViewModels.Leaves;
using FVN_REGISTER.Contract.ViewModels.OT;
using FVN_REGISTER.Core.Config;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Infrastructure.Hubs;
using FVN_REGISTER.Infrastructure.Models.Data;
using FVN_REGISTER.Infrastructure.Repositories;
using FVN_REGISTER.Infrastructure.Services.Approvals;
using FVN_REGISTER.Infrastructure.Services.Auths;
using FVN_REGISTER.Infrastructure.Services.Companies;
using FVN_REGISTER.Infrastructure.Services.Emails;
using FVN_REGISTER.Infrastructure.Services.Employees;
using FVN_REGISTER.Infrastructure.Services.Histories;
using FVN_REGISTER.Infrastructure.Services.Jobs;
using FVN_REGISTER.Infrastructure.Services.Leaves;
using FVN_REGISTER.Infrastructure.Services.Notifications;
using FVN_REGISTER.Infrastructure.Services.OT;
using FVN_REGISTER.Infrastructure.Services.Reports;
using FVN_REGISTER.Infrastructure.Services.Statics;
using FVN_REGISTER.Infrastructure.Services.Users;
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
        "https://localhost:7264",
        "http://localhost:5120",
        "http://localhost:5017",
        "https://localhost:7135"
    };

Console.WriteLine($"[STARTUP] CORS AllowedOrigins: {string.Join(", ", allowedOrigins)}");

builder.Services.AddCors(options =>
{
    options.AddPolicy("FccCorsPolicy", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// =========================================================
// 2. INFRASTRUCTURE
// =========================================================
builder.Services.AddMemoryCache();
builder.Services.Configure<AuthDebugOptions>(
    builder.Configuration.GetSection("AuthDebug"));
builder.Services.Configure<AppOptions>(opts =>
{
    opts.SiteUrl = builder.Configuration["SiteUrl"] ?? "https://localhost:7264";
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<FVNWEBAPPContext>(options =>
    options.UseSqlServer(connectionString));

// UnitOfWork is an Application port implemented by Infrastructure.
// Register DbContext against the same scoped FVNWEBAPPContext instance so
// UnitOfWork never creates a second DbContext in the same request.
builder.Services.AddScoped<DbContext>(sp =>
    sp.GetRequiredService<FVNWEBAPPContext>());
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// =========================================================
// 3. APPLICATION PORTS -> INFRASTRUCTURE ADAPTERS
// =========================================================
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INetworkService, NetworkService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IWorkingDayService, WorkingDayService>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();

// Dashboard/Application orchestration
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ILeaveQueryService, LeaveQueryService>();
builder.Services.AddScoped<IEscalationRuleService, EscalationRuleService>();
builder.Services.AddScoped<ILeaveValidator, LeaveValidator>();
builder.Services.AddScoped<ILeaveEscalationService, LeaveEscalationService>();
builder.Services.AddScoped<ILeaveService, LeaveService>();

// Reports
builder.Services.AddScoped<IReportService, LeaveReportService>();
builder.Services.AddScoped<IReportService, OTReportService>();
builder.Services.AddScoped<IReportDispatcher, ReportDispatcher>();

// Overtime
builder.Services.AddScoped<OTQueryService>();
builder.Services.AddScoped<IOTQueryService, OTQueryService>();
builder.Services.AddScoped<IOTValidator, OTValidator>();
builder.Services.AddScoped<IOTService, OTService>();
builder.Services.AddScoped<IOTSyncService, OTSyncService>();
builder.Services.AddScoped<IDepartmentStatusService, DepartmentStatusService>();
builder.Services.AddScoped<IOTTypeManagementService, OTTypeManagementService>();

// Authentication / users
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IUserLogService, UserLogService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IApproverManagementService, ApproverManagementService>();

// Employees / company master data
builder.Services.AddScoped<IEmployeeManagementService, EmployeeManagementService>();
builder.Services.AddScoped<IDepartmentService, DepartmentManagementService>();
builder.Services.AddScoped<ILeaveTypeManagementService, LeaveTypeManagementService>();

// History
builder.Services.AddScoped<IHistoryHandler, LeaveHistoryHandler>();
builder.Services.AddScoped<IHistoryHandler, OTHistoryHandler>();
builder.Services.AddScoped<IHistoryDispatcher, HistoryDispatcher>();

// Email templates
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();

// Notifications
builder.Services.AddScoped<INotificationService, NotificationService>();

// Approval pipeline
builder.Services.AddScoped<LeaveApprovalProvider>();
builder.Services.AddScoped<IApprovalEngine<LeaveRequestSubject>, ApprovalEngine<LeaveRequestSubject>>();
builder.Services.AddScoped<IApprovalListDataSource<LeaveRequestViewModel>, LeaveApprovalListDataSource>();
builder.Services.AddScoped<IApprovalListDataSource<OTRequestViewModel>, OTApprovalListDataSource>();
builder.Services.AddScoped<ApprovalListService<LeaveRequestViewModel>>();
builder.Services.AddScoped<ApprovalListService<OTRequestViewModel>>();

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
        RoleClaimType = "role"
    };

    options.Events = new JwtBearerEvents
    {
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

            var qs = context.Request.Query["access_token"].ToString();
            if (!string.IsNullOrEmpty(qs))
            {
                context.Token = qs;
                logger.LogDebugIf(debug,
                    "[JWT] OnMessageReceived -> token from QS | Path={Path} | Len={Len}",
                    path, qs.Length);
                return Task.CompletedTask;
            }

            var header = context.Request.Headers["Authorization"].ToString();
            if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Token = header["Bearer ".Length..].Trim();
                logger.LogDebugIf(debug,
                    "[JWT] OnMessageReceived -> token from Header | Path={Path} | Len={Len}",
                    path, context.Token.Length);
                return Task.CompletedTask;
            }

            logger.LogWarning(
                "[JWT] OnMessageReceived -> no token found | Path={Path} | QS_keys={Keys}",
                path,
                string.Join(",", context.Request.Query.Keys));

            return Task.CompletedTask;
        },
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
builder.Services.AddOpenApi();

// =========================================================
// 5. BACKGROUND WORKERS
// =========================================================
builder.Services.AddHostedService<EmailBackgroundWorker>();
builder.Services.AddHostedService<EscalationBackgroundWorker>();
builder.Services.AddSingleton<OTAttendanceStagingWorker>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<OTAttendanceStagingWorker>());
builder.Services.AddSingleton<IOTWorkerStatus>(sp => sp.GetRequiredService<OTAttendanceStagingWorker>());

// =========================================================
// 6. BUILD & PIPELINE
// =========================================================
var app = builder.Build();
app.UsePathBase("/api");

var appLogger = app.Services.GetRequiredService<ILogger<Program>>();
var debugOptions = app.Services.GetRequiredService<IOptionsMonitor<AuthDebugOptions>>();
bool IsDebug() => debugOptions.CurrentValue.Enabled;

appLogger.LogInfoIf(IsDebug(),
    "[STARTUP] AuthDebug.Enabled={Debug} | Env={Env}",
    IsDebug(), app.Environment.EnvironmentName);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

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

app.UseCors("FccCorsPolicy");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notification");

appLogger.LogInfoIf(IsDebug(),
    "[STARTUP] Pipeline ready | Hub=/hubs/notification");

app.Run();
