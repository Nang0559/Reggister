using FVN_REGISTER.Application.Factories;
using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Auths;
using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Application.Interfaces.Calendar;
using FVN_REGISTER.Application.Interfaces.Companies;
using FVN_REGISTER.Application.Interfaces.EmailTemplates;
using FVN_REGISTER.Application.Interfaces.Emails;
using FVN_REGISTER.Application.Interfaces.Employees;
using FVN_REGISTER.Application.Interfaces.Equipment;
using FVN_REGISTER.Application.Interfaces.Histories;
using FVN_REGISTER.Application.Interfaces.HrmSync;
using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Interfaces.Notifications;
using FVN_REGISTER.Application.Interfaces.OT;
using FVN_REGISTER.Application.Interfaces.OTTypes;
using FVN_REGISTER.Application.Interfaces.PublicInformation;
using FVN_REGISTER.Application.Interfaces.Orchestrators;
using FVN_REGISTER.Application.Interfaces.Reports;
using FVN_REGISTER.Application.Interfaces.Statics;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Dashboards;
using FVN_REGISTER.Application.Interfaces.UserManagers;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Application.Models.Subjects;
using FVN_REGISTER.Application.Orchestrators;
using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Infrastructure.Hubs;
using FVN_REGISTER.Infrastructure.Repositories;
using FVN_REGISTER.Infrastructure.Services.Approvals;
using FVN_REGISTER.Infrastructure.Services.Auths;
using FVN_REGISTER.Infrastructure.Services.Companies;
using FVN_REGISTER.Infrastructure.Services.Calendar;
using FVN_REGISTER.Infrastructure.Services.Common;
using FVN_REGISTER.Infrastructure.Services.Emails;
using FVN_REGISTER.Infrastructure.Services.Employees;
using FVN_REGISTER.Infrastructure.Services.Equipment;
using FVN_REGISTER.Infrastructure.Services.Histories;
using FVN_REGISTER.Infrastructure.Services.HrmSync;
using FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.Importers;
using FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.Readers;
using FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.SourceRows;
using FVN_REGISTER.Infrastructure.Services.HrmSync.SyncJob.Syncs;
using FVN_REGISTER.Infrastructure.Services.Jobs;
using FVN_REGISTER.Infrastructure.Services.Leaves;
using FVN_REGISTER.Infrastructure.Services.Notifications;
using FVN_REGISTER.Infrastructure.Services.OT;
using FVN_REGISTER.Infrastructure.Services.OTs;
using FVN_REGISTER.Infrastructure.Services.PublicInformation;
using FVN_REGISTER.Infrastructure.Services.Reports;
using FVN_REGISTER.Infrastructure.Services.Statics;
using FVN_REGISTER.Infrastructure.Services.Dashboards;
using FVN_REGISTER.Infrastructure.Services.Users;
using FVN_REGISTER.Infrastructure.Services.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Infrastructure;
using FVN_REGISTER.Infrastructure.Utils;
using FVN_REGISTER.Infrastructure.Services;
using FVN_REGISTER.Infrastructure.Services.Departments;
using FVN_REGISTER.Application.Policies;
using FVN_REGISTER.API.Services.OT;
using FVN_REGISTER.API.Services.Histories;
using FVN_REGISTER.Contract.Responses;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[]
{
    "https://localhost:7264", "http://localhost:5120", "http://localhost:5017", "https://localhost:7135"
};

builder.Services.AddCors(options => options.AddPolicy("FccCorsPolicy", policy => policy
    .WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader().AllowCredentials()));
builder.Services.AddMemoryCache();
builder.Services.Configure<AuthDebugOptions>(builder.Configuration.GetSection("AuthDebug"));
builder.Services.Configure<AppOptions>(opts => opts.SiteUrl = builder.Configuration["SiteUrl"] ?? "https://localhost:7264");

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<FVNWEBAPPContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<FVNWEBAPPContext>());
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();

// Common / master data
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INetworkService, NetworkService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IWorkingDayService, WorkingDayService>();
builder.Services.AddScoped<IWorkCalendarService, WorkCalendarService>();
builder.Services.AddScoped<IWorkYearManagementService, WorkYearManagementService>();
builder.Services.AddScoped<ICompanyHolidayManagementService, CompanyHolidayManagementService>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();
builder.Services.AddScoped<IDepartmentLookupService, DepartmentLookupService>();
builder.Services.AddScoped<IDepartmentManagementService, DepartmentManagementService>();

// Dashboard / Leave
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IDashboardOrchestrator, DashboardOrchestrator>();
builder.Services.AddScoped<IModuleDashboardProvider, LeaveDashboardProvider>();
builder.Services.AddScoped<IModuleDashboardProvider, OTDashboardProvider>();
builder.Services.AddScoped<IModuleDashboardProvider, TripDashboardProvider>();
builder.Services.AddScoped<IModuleDashboardProvider, EquipmentDashboardProvider>();
builder.Services.AddScoped<ILeaveQueryService, LeaveQueryService>();
builder.Services.AddScoped<IEscalationRuleService, EscalationRuleService>();
builder.Services.AddScoped<ILeaveValidator, LeaveValidator>();
builder.Services.AddScoped<ILeaveEscalationService, LeaveEscalationService>();
builder.Services.AddScoped<ILeaveService, LeaveService>();
builder.Services.AddScoped<ILeaveEntitlementService, LeaveEntitlementService>();

// Reports
builder.Services.AddScoped<IReportService, LeaveReportService>();
builder.Services.AddScoped<IReportService, OTReportService>();
builder.Services.AddScoped<IReportService, OperationalReportService>();
builder.Services.AddScoped<IReportDispatcher, ReportDispatcher>();

// OT
builder.Services.AddScoped<OTQueryService>();
builder.Services.AddScoped<IOTQueryService, OTQueryService>();
builder.Services.AddScoped<IOTValidator, OTValidator>();
builder.Services.AddScoped<IOTService, OTService>();
builder.Services.AddScoped<IDepartmentStatusService, DepartmentStatusService>();
builder.Services.AddScoped<IOTTypeManagementService, OTTypeManagementService>();
builder.Services.AddScoped<IOTEscalationService, OTEscalationService>();

// Trip
builder.Services.AddScoped<FVN_REGISTER.Application.Interfaces.Trips.ITripService, FVN_REGISTER.Infrastructure.Services.Trips.TripService>();

// Equipment
builder.Services.AddScoped<IEquipmentService, EquipmentService>();
builder.Services.AddScoped<IEquipmentImportService, EquipmentImportService>();
builder.Services.AddScoped<IEquipmentQrCodeService, QrCodeService>();

// Auth / users
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IUserLogService, UserLogService>();
builder.Services.AddScoped<IEmployeeUserResolver, EmployeeUserResolver>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<ISessionTerminationNotifier, SignalRSessionTerminationNotifier>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddScoped<IPublicInformationService, PublicInformationService>();
builder.Services.AddScoped<IApproverManagementService, ApproverManagementService>();
builder.Services.AddScoped<IApprovalPolicyService, ApprovalPolicyService>();
builder.Services.AddScoped<IApprovalRouteService, ApprovalRouteService>();
builder.Services.AddScoped<IApprovalSelectionService, ApprovalSelectionService>();
builder.Services.AddScoped<IEmployeeManagementService, EmployeeManagementService>();
builder.Services.AddScoped<ILeaveTypeManagementService, LeaveTypeManagementService>();

// History / email / notifications
builder.Services.AddScoped<INotificationFactory, NotificationFactory>();
builder.Services.AddScoped<IApprovalHistoryService, ApprovalHistoryService>();
builder.Services.AddScoped<IHistoryHandler, LeaveHistoryHandler>();
builder.Services.AddScoped<IHistoryHandler, OTHistoryHandler>();
builder.Services.AddScoped<IHistoryHandler, TripHistoryHandler>();
builder.Services.AddScoped<IHistoryHandler, EquipmentHistoryHandler>();
builder.Services.AddScoped<IHistoryDispatcher, HistoryDispatcher>();
builder.Services.AddScoped<IEmailTemplateManagementService, EmailTemplateManagementService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IApprovalNotificationService, ApprovalNotificationService>();

// Approval: provider -> engine -> workflow -> cross-module inbox/resolver
builder.Services.AddScoped<LeaveApprovalProvider>();
builder.Services.AddScoped<OTApprovalProvider>();
builder.Services.AddScoped<TripApprovalProvider>();
builder.Services.AddScoped<EquipmentApprovalProvider>();
builder.Services.AddScoped<IApprovalProvider<LeaveRequestSubject>>(sp => sp.GetRequiredService<LeaveApprovalProvider>());
builder.Services.AddScoped<IApprovalProvider<OTRequestSubject>>(sp => sp.GetRequiredService<OTApprovalProvider>());
builder.Services.AddScoped<IApprovalProvider<TripRequestSubject>>(sp => sp.GetRequiredService<TripApprovalProvider>());
builder.Services.AddScoped<IApprovalProvider<EquipmentRequestSubject>>(sp => sp.GetRequiredService<EquipmentApprovalProvider>());
builder.Services.AddScoped<IApprovalEngine<LeaveRequestSubject>, ApprovalEngine<LeaveRequestSubject>>();
builder.Services.AddScoped<IApprovalEngine<OTRequestSubject>, ApprovalEngine<OTRequestSubject>>();
builder.Services.AddScoped<IApprovalEngine<TripRequestSubject>, ApprovalEngine<TripRequestSubject>>();
builder.Services.AddScoped<IApprovalEngine<EquipmentRequestSubject>, ApprovalEngine<EquipmentRequestSubject>>();
builder.Services.AddScoped<IApprovalWorkflowOrchestrator<LeaveRequestSubject>, ApprovalWorkflowOrchestrator<LeaveRequestSubject>>();
builder.Services.AddScoped<IApprovalWorkflowOrchestrator<OTRequestSubject>, ApprovalWorkflowOrchestrator<OTRequestSubject>>();
builder.Services.AddScoped<IApprovalWorkflowOrchestrator<TripRequestSubject>, ApprovalWorkflowOrchestrator<TripRequestSubject>>();
builder.Services.AddScoped<IApprovalWorkflowOrchestrator<EquipmentRequestSubject>, ApprovalWorkflowOrchestrator<EquipmentRequestSubject>>();
builder.Services.AddScoped<IApprovalEngineResolver, ApprovalEngineResolver>();
builder.Services.AddScoped<IApprovalGroupingPolicy, ApprovalGroupingPolicy>();
// Unified approval inbox: pending items are resolved through the approval workflow/engine path.
builder.Services.AddScoped<IApprovalInboxService, ApprovalInboxService>();

// HRM Sync
builder.Services.AddScoped<IHrmSourceReader<HrmDepartmentSourceRow>, HrmDepartmentSourceReader>();
builder.Services.AddScoped<IHrmSourceReader<HrmEmployeeSourceRow>, HrmEmployeeSourceReader>();
builder.Services.AddScoped<IHrmSourceReader<HrmLeaveTypeSourceRow>, HrmLeaveTypeSourceReader>();
builder.Services.AddScoped<IHrmSourceReader<HrmPositionSourceRow>, HrmPositionSourceReader>();
builder.Services.AddScoped<IHrmStagingImporter, DepartmentStagingImporter>();
builder.Services.AddScoped<IHrmStagingImporter, EmployeeStagingImporter>();
builder.Services.AddScoped<IHrmStagingImporter, LeaveTypeStagingImporter>();
builder.Services.AddScoped<IHrmStagingImporter, PositionStagingImporter>();
builder.Services.AddScoped<IHrmSyncJob, DepartmentHrmSyncJob>();
builder.Services.AddScoped<IHrmSyncJob, EmployeeHrmSyncJob>();
builder.Services.AddScoped<IHrmSyncJob, LeaveTypeHrmSyncJob>();
builder.Services.AddScoped<IHrmSyncJob, OTTypeHrmSyncJob>();
builder.Services.AddScoped<IHrmSyncJob, PositionHrmSyncJob>();
builder.Services.AddScoped<IHrmStagingImporterResolver, HrmStagingImporterResolver>();
builder.Services.AddScoped<IHrmSyncJobResolver, HrmSyncJobResolver>();
builder.Services.AddScoped<IHrmSyncReviewQueryService, HrmSyncReviewQueryService>();
builder.Services.AddScoped<IHrmSyncService, HrmSyncService>();
builder.Services.AddScoped<IHrmAttendanceCalculationService, HrmAttendanceCalculationService>();
builder.Services.AddScoped<IHrmAttendanceExcelExportService, HrmAttendanceExcelExportService>();
builder.Services.AddScoped<IHrmUserRoleRuleService, HrmUserRoleRuleService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true, ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
        ValidateLifetime = true, ClockSkew = TimeSpan.FromMinutes(5),
        NameClaimType = "name", RoleClaimType = "role"
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (!context.HttpContext.Request.Path.StartsWithSegments("/hubs")) return Task.CompletedTask;
            var qs = context.Request.Query["access_token"].ToString();
            if (!string.IsNullOrEmpty(qs)) { context.Token = qs; return Task.CompletedTask; }
            var header = context.Request.Headers["Authorization"].ToString();
            if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) context.Token = header["Bearer ".Length..].Trim();
            return Task.CompletedTask;
        },
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
builder.Services.AddHostedService<HrmSyncBackgroundWorker>();
builder.Services.AddHostedService<HrmAttendanceCalculationWorker>();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        var (statusCode, message) = exception switch
        {
            UnauthorizedAccessException ex => (StatusCodes.Status401Unauthorized, ex.Message),
            KeyNotFoundException ex => (StatusCodes.Status404NotFound, ex.Message),
            ArgumentException ex => (StatusCodes.Status400BadRequest, ex.Message),
            InvalidOperationException ex => (StatusCodes.Status400BadRequest, ex.Message),
            _ => (StatusCodes.Status500InternalServerError,
                "Đã xảy ra lỗi hệ thống. Vui lòng thử lại hoặc liên hệ quản trị viên.")
        };

        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        if (statusCode >= 500)
            logger.LogError(exception, "[API] Unhandled exception: {Path}", context.Request.Path);
        else
            logger.LogWarning(exception, "[API] Business exception: {Path}", context.Request.Path);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        await context.Response.WriteAsJsonAsync(
            ApiResponse<object>.Fail(message, statusCode));
    });
});
if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.UseCors("FccCorsPolicy");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notification");
app.Run();