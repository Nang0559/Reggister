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
using FVN_REGISTER.Application.Interfaces.Orchestrators;
using FVN_REGISTER.Application.Interfaces.Reports;
using FVN_REGISTER.Application.Interfaces.Statics;
using FVN_REGISTER.Application.Interfaces.UserManagers;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Interfaces.Jobs;
using FVN_REGISTER.Application.Models.Subjects;
using FVN_REGISTER.Application.Policies;
using FVN_REGISTER.Application.Services.Statics;
using FVN_REGISTER.Contract.Dtos;
using FVN_REGISTER.Contract.Maps;
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
using FVN_REGISTER.Infrastructure.Services.Common;
using FVN_REGISTER.Infrastructure.Services.Emails;
using FVN_REGISTER.Infrastructure.Services.Employees;
using FVN_REGISTER.Infrastructure.Services.Histories;
using FVN_REGISTER.Infrastructure.Services.Jobs;
using FVN_REGISTER.Infrastructure.Services.Leaves;
using FVN_REGISTER.Infrastructure.Services.Notifications;
using FVN_REGISTER.Infrastructure.Services.OT;
using FVN_REGISTER.Infrastructure.Services.OTs;
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

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[]
{
    "https://localhost:7264",
    "http://localhost:5120",
    "http://localhost:5017",
    "https://localhost:7135"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("FccCorsPolicy", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

builder.Services.AddMemoryCache();
builder.Services.Configure<AuthDebugOptions>(builder.Configuration.GetSection("AuthDebug"));
builder.Services.Configure<AppOptions>(opts =>
    opts.SiteUrl = builder.Configuration["SiteUrl"] ?? "https://localhost:7264");

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<FVNWEBAPPContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<FVNWEBAPPContext>());
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Common
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INetworkService, NetworkService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IWorkingDayService, WorkingDayService>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();
builder.Services.AddScoped<IDepartmentLookupService, DepartmentLookupService>();
builder.Services.AddScoped<IDepartmentManagementService, DepartmentManagementService>();

// Dashboard / Leave
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

// OT
builder.Services.AddScoped<OTQueryService>();
builder.Services.AddScoped<IOTQueryService, OTQueryService>();
builder.Services.AddScoped<IOTValidator, OTValidator>();
builder.Services.AddScoped<IOTService, OTService>();
builder.Services.AddScoped<IOTAttendanceStagingService, OTAttendanceStagingService>();
builder.Services.AddScoped<IOTAttendanceReconciliationService, OTAttendanceReconciliationService>();
builder.Services.AddScoped<IDepartmentStatusService, DepartmentStatusService>();
builder.Services.AddScoped<IOTTypeManagementService, OTTypeManagementService>();

// Auth / users
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IUserLogService, UserLogService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<ISessionTerminationNotifier, SignalRSessionTerminationNotifier>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IApproverManagementService, ApproverManagementService>();
builder.Services.AddScoped<IEmployeeManagementService, EmployeeManagementService>();
builder.Services.AddScoped<ILeaveTypeManagementService, LeaveTypeManagementService>();

// History / email / notifications
builder.Services.AddScoped<IHistoryHandler, LeaveHistoryHandler>();
builder.Services.AddScoped<IHistoryHandler, OTHistoryHandler>();
builder.Services.AddScoped<IHistoryDispatcher, HistoryDispatcher>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Approval: provider -> engine -> workflow -> cross-module inbox/resolver
builder.Services.AddScoped<LeaveApprovalProvider>();
builder.Services.AddScoped<OTApprovalProvider>();
builder.Services.AddScoped<IApprovalProvider<LeaveRequestSubject>>(sp => sp.GetRequiredService<LeaveApprovalProvider>());
builder.Services.AddScoped<IApprovalProvider<OTRequestSubject>>(sp => sp.GetRequiredService<OTApprovalProvider>());
builder.Services.AddScoped<IApprovalEngine<LeaveRequestSubject>, ApprovalEngine<LeaveRequestSubject>>();
builder.Services.AddScoped<IApprovalEngine<OTRequestSubject>, ApprovalEngine<OTRequestSubject>>();
builder.Services.AddScoped<IApprovalWorkflowOrchestrator<LeaveRequestSubject>, ApprovalWorkflowOrchestrator<LeaveRequestSubject>>();
builder.Services.AddScoped<IApprovalWorkflowOrchestrator<OTRequestSubject>, ApprovalWorkflowOrchestrator<OTRequestSubject>>();
builder.Services.AddScoped<IApprovalEngineResolver, ApprovalEngineResolver>();
builder.Services.AddScoped<IApprovalGroupingPolicy, ApprovalGroupingPolicy>();
builder.Services.AddScoped<IApprovalInboxService, ApprovalInboxService>();
builder.Services.AddScoped<IApprovalListDataSource<LeaveRequestViewModel>, LeaveApprovalListDataSource>();
builder.Services.AddScoped<IApprovalListDataSource<OTRequestViewModel>, OTApprovalListDataSource>();
builder.Services.AddScoped<ApprovalListService<LeaveRequestViewModel>>();
builder.Services.AddScoped<ApprovalListService<OTRequestViewModel>>();

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
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
            if (!path.StartsWithSegments("/hubs")) return Task.CompletedTask;
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var debug = context.HttpContext.RequestServices.GetRequiredService<IOptionsMonitor<AuthDebugOptions>>().CurrentValue.Enabled;
            var qs = context.Request.Query["access_token"].ToString();
            if (!string.IsNullOrEmpty(qs)) { context.Token = qs; return Task.CompletedTask; }
            var header = context.Request.Headers["Authorization"].ToString();
            if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Token = header["Bearer ".Length..].Trim();
                logger.LogDebugIf(debug, "[JWT] Hub token from Authorization header | Len={Len}", context.Token.Length);
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = context => Task.CompletedTask,
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(context.Exception, "[JWT] AuthenticationFailed");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHostedService<EmailBackgroundWorker>();
builder.Services.AddHostedService<EscalationBackgroundWorker>();
builder.Services.AddSingleton<OTAttendanceStagingWorker>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<OTAttendanceStagingWorker>());
builder.Services.AddSingleton<IOTWorkerStatus>(sp => sp.GetRequiredService<OTAttendanceStagingWorker>());

var app = builder.Build();
app.UsePathBase("/api");
if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.UseCors("FccCorsPolicy");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notification");
app.Run();
