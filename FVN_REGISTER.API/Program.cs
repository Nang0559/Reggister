using FVN_REGISTER.API.Hubs;
using FVN_REGISTER.API.Repositories;
using FVN_REGISTER.API.Services.Auths;
using FVN_REGISTER.API.Services.Emails;
using FVN_REGISTER.API.Services.Leaves;
using FVN_REGISTER.API.Services.Notifications;
using FVN_REGISTER.API.Services.Notifications.FVN_REGISTER.API.Services.Notifications;
using FVN_REGISTER.API.Services.Statics;
using FVN_REGISTER.API.Services.Users;
using FVN_REGISTER.Contract.Interfaces.Auths;
using FVN_REGISTER.Contract.Interfaces.Emails;
using FVN_REGISTER.Contract.Interfaces.Leaves;
using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Interfaces.Statics;
using FVN_REGISTER.Contract.Interfaces.Users;
using FVN_REGISTER.Contract.Maps;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
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
        "http://localhost:5017",   // API HTTP (self-call nếu cần)
        "https://localhost:7135"   // API HTTPS
    };

builder.Services.AddCors(options =>
{
    options.AddPolicy("FccCorsPolicy", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)   // ✅ Không dùng AllowAnyOrigin với SignalR
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();           // ✅ Bắt buộc cho SignalR WebSocket
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

// ✅ SignalR
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
        OnMessageReceived = context =>
        {
            var path = context.HttpContext.Request.Path;
            if (path.StartsWithSegments("/hubs"))
            {
                // Cách 1: Browser JS client gửi qua query string
                var qs = context.Request.Query["access_token"].ToString();
                if (!string.IsNullOrEmpty(qs))
                {
                    context.Token = qs;
                    return Task.CompletedTask;
                }

                // Cách 2: .NET client gửi qua Authorization header
                var header = context.Request.Headers["Authorization"].ToString();
                if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = header["Bearer ".Length..].Trim();
                }
            }
            return Task.CompletedTask;
        },

        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"[AUTH FAILED] {context.Exception.Message}");
            return Task.CompletedTask;
        },

        OnChallenge = context =>
        {
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddControllers();

// =========================================================
// 5. BACKGROUND WORKERS
// =========================================================
builder.Services.AddHostedService<EmailBackgroundWorker>();

builder.Services.AddOpenApi();

// =========================================================
// 6. PIPELINE
// =========================================================
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// ✅ CORS phải đứng TRƯỚC tất cả middleware khác
app.UseCors("FccCorsPolicy");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ✅ MapHub sau MapControllers
app.MapHub<NotificationHub>("/hubs/notification");

app.Run();