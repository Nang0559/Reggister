using FVN_REGISTER.API.Repositories;
using FVN_REGISTER.API.Services.Auths;
using FVN_REGISTER.API.Services.Emails;
using FVN_REGISTER.API.Services.Leaves;
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

// --- 1. CẤU HÌNH CORS (THÊM MỚI) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("FccCorsPolicy", policy =>
    {
        policy.AllowAnyOrigin() // Hoặc chỉ định rõ URL của Blazor Client
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddMemoryCache();
builder.Services.Configure<AuthDebugOptions>(
    builder.Configuration.GetSection("AuthDebug"));
// 1. Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<FVNWEBAPPContext>(options =>
    options.UseSqlServer(connectionString));

// 2. [QUAN TRỌNG] Đăng ký HttpContextAccessor
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
// đăng ký repository
builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
// 3. Đăng ký các Infrastructure Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INetworkService, NetworkService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IWorkingDayService, WorkingDayService>();
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// 4. Đăng ký Business Services (Giữ nguyên của Rotyby)
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ILeaveRepository, LeaveRepository>();
builder.Services.AddScoped<ILeaveQueryService, LeaveQueryService>();
builder.Services.AddScoped<IEscalationRuleService, EscalationRuleService>();
builder.Services.AddScoped<ILeaveValidator, LeaveValidator>();
builder.Services.AddScoped<ILeaveNotificationService, LeaveNotificationService>();
builder.Services.AddScoped<ILeaveEscalationService, LeaveEscalationService>();
builder.Services.AddScoped<ILeaveService, LeaveService>();

// 5. Đăng ký Dịch vụ User
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IUserLogService, UserLogService>();

// 6. Đăng ký Authentication/JWT

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
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

        // --- DÒNG QUAN TRỌNG NHẤT CHO ROTYBY ---
        NameClaimType = "unique_name", // Ánh xạ claim 'unique_name' vào User.Identity.Name
        RoleClaimType = "role",        // Ánh xạ claim 'role' vào Roles
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            // ĐẶT BREAKPOINT TẠI ĐÂY
            // Kiểm tra biến context.Exception để biết lý do (Expired, Invalid Signature...)
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            // ĐẶT BREAKPOINT TẠI ĐÂY
            // Đây là nơi nó quyết định trả về 401
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            // ĐẶT BREAKPOINT TẠI ĐÂY
            // Chuỗi Token nằm ở biến này:
            var rawToken = context.Request.Headers["Authorization"].ToString();

            // Hoặc lấy trực tiếp token đã lọc bỏ chữ "Bearer "
            var tokenOnly = context.Token;

            Console.WriteLine($"[DEBUG] Token nhận được: {tokenOnly}");
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddControllers();
// 7. Đăng ký Worker chạy ngầm
builder.Services.AddHostedService<EmailBackgroundWorker>();

builder.Services.AddOpenApi();
// Lưu ý: Nếu dùng Swagger cũ thì thêm cấu hình SecurityDefinition ở đây

var app = builder.Build();

// --- HTTP request pipeline ---

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// BẬT CORS TRƯỚC AUTHENTICATION
app.UseCors("FccCorsPolicy");

app.UseHttpsRedirection();

app.UseAuthentication(); // Ai là người đang gọi?
app.UseAuthorization();  // Người đó có quyền làm gì?

app.MapControllers();

app.Run();