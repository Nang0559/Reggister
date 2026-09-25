using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Auths;
using FVN_REGISTER.Application.Logging;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.HR;
using FVN_REGISTER.Core.Entities.Security;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Core.Utils;
using FVN_REGISTER.Infrastructure.Hubs;
using FVN_REGISTER.Infrastructure.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FVN_REGISTER.Infrastructure.Services.Auths
{
    public class AuthService : BaseService<AuthService>, IAuthService
    {
        private readonly IUnitOfWork _uow;
        private readonly IAuditService _audit;
        private readonly JwtOptions _jwtOptions;
        private readonly ISessionService _sessionService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ITwoFactorService _twoFactor;

        public AuthService(IUnitOfWork uow, IAuditService audit, IOptions<JwtOptions> jwtOptions, ISessionService sessionService, IHubContext<NotificationHub> hubContext, ITwoFactorService twoFactor, ILogger<AuthService> logger, IOptionsMonitor<AuthDebugOptions> options) : base(logger, options)
        {
            _uow = uow; _audit = audit; _jwtOptions = jwtOptions.Value; _sessionService = sessionService; _hubContext = hubContext; _twoFactor = twoFactor;
        }

        public async Task<ServiceResult<AuthResultDto>> Login(string employeeCode,string password,string deviceId,string deviceType,string? deviceName,bool rememberMe,string? ipAddress,string? userAgent,CancellationToken ct=default)
        {
            try { Logger.LogDebugIf(Debug,"[LOGIN] Attempt: {Emp} | Device={Device} | Ip={Ip}",employeeCode,deviceId,ipAddress); var user=await _uow.Repository<F03User>().Query().FirstOrDefaultAsync(x=>x.EmployeeCode==employeeCode,ct); if(user==null){Logger.LogWarnIf(Debug,"[LOGIN] User not found: {Emp}",employeeCode);await _audit.LogLoginFailed(employeeCode,ipAddress,userAgent);return ServiceResult<AuthResultDto>.Fail("Sai mã nhân viên hoặc mật khẩu");} if(user.IsActive==false){Logger.LogWarnIf(Debug,"[LOGIN] Inactive user: {Emp}",employeeCode);return ServiceResult<AuthResultDto>.Fail("Tài khoản chưa kích hoạt");} if(user.LockoutEndDate.HasValue&&user.LockoutEndDate>DateTime.Now){Logger.LogWarnIf(Debug,"[LOGIN] Locked user: {Emp} until {Time}",employeeCode,user.LockoutEndDate);return ServiceResult<AuthResultDto>.Fail($"Tài khoản bị khóa đến {user.LockoutEndDate:HH:mm dd/MM}");} if(!EncryptUtils.PwdCompare(password,user.Password)){Logger.LogWarnIf(Debug,"[LOGIN] Wrong password: {Emp}",employeeCode);await HandleLoginFailedAsync(user,ct);await _audit.LogLoginFailed(employeeCode,ipAddress,userAgent);return ServiceResult<AuthResultDto>.Fail("Sai mã nhân viên hoặc mật khẩu");} user.NumLoginFailed=0;user.LockoutEndDate=null;await _uow.SaveChangesAsync(ct);if(await _twoFactor.IsTwoFactorRequiredAsync(user.Id,ct)){var challenge=await _twoFactor.CreateLoginChallengeAsync(user.Id,ct);await _audit.LogAction("2FA_REQUIRED",user.Id,"Mật khẩu đúng; yêu cầu xác thực 2 lớp");return ServiceResult<AuthResultDto>.Ok(new AuthResultDto{IsSuccess=true,RequiresTwoFactor=true,TwoFactorSetupRequired=!user.TwoFactorEnabled,TwoFactorChallengeToken=challenge,UserId=user.Id,FullName=user.FullName,Avatar=user.Avatar,IsAdmin=user.PermissionCode==UserPermissionCodes.SuperAdmin||user.PermissionCode==UserPermissionCodes.Admin});} return await IssueLoginResultAsync(user,rememberMe,deviceType,deviceId,deviceName,ipAddress,userAgent,ct); }
            catch(Exception ex){Logger.LogError(ex,"[LOGIN] Exception for {Emp}",employeeCode);throw;}
        }

        public async Task<ServiceResult<UserIdentityDto>> GetProfileAsync(int userId,CancellationToken ct=default)
        { try { var user=await _uow.Repository<F03User>().Query().FirstOrDefaultAsync(x=>x.Id==userId,ct);if(user==null)return ServiceResult<UserIdentityDto>.Fail("Không tìm thấy thông tin tài khoản.");var functionIds=await _uow.Repository<F03UserFunction>().Query().Where(x=>x.IdUser==userId).Select(x=>x.IdFunction).ToListAsync(ct);var emp=await _uow.Repository<F03Employee>().Query().Where(x=>x.EmployeeCode==user.EmployeeCode).Select(x=>new{x.EmailAddress,x.PositionCode}).FirstOrDefaultAsync(ct);return ServiceResult<UserIdentityDto>.Ok(new UserIdentityDto{UserId=user.Id,Permission=user.PermissionCode,UserName=user.EmployeeCode,FullName=user.FullName,EmployeeCode=user.EmployeeCode,DeptCode=user.DeptCode,PositionCode=emp?.PositionCode,Email=emp?.EmailAddress,LevelApprove=user.LevelApprove,Functions=functionIds});}catch(Exception ex){Logger.LogError(ex,"[PROFILE] Exception for {UserId}",userId);return ServiceResult<UserIdentityDto>.Fail("Lỗi hệ thống khi lấy thông tin người dùng.");} }
        public async Task<ServiceResult> UpdateProfileAsync(int userId,string email,string? avatarUrl,CancellationToken ct=default){try{var user=await _uow.Repository<F03User>().Query().FirstOrDefaultAsync(x=>x.Id==userId,ct);if(user==null)return ServiceResult.Fail("Không tìm thấy người dùng.");user.Avatar=avatarUrl;await _uow.SaveChangesAsync(ct);Logger.LogInfoIf(Debug,"[PROFILE] Updated success: {UserId}",userId);return ServiceResult.Ok("Cập nhật thông tin thành công");}catch(Exception ex){Logger.LogError(ex,"[PROFILE] Update error: {UserId}",userId);return ServiceResult.Fail("Lỗi hệ thống khi cập nhật thông tin.");}}
        public async Task<ServiceResult> ChangePassword(string employeeCode,string currentPassword,string newPassword,CancellationToken ct=default){try{Logger.LogDebugIf(Debug,"[PASSWORD] Change attempt: {Emp}",employeeCode);var user=await _uow.Repository<F03User>().Query().FirstOrDefaultAsync(x=>x.EmployeeCode==employeeCode,ct);if(user==null)return ServiceResult.Fail("Tài khoản không tồn tại");if(!EncryptUtils.PwdCompare(currentPassword,user.Password)){Logger.LogWarnIf(Debug,"[PASSWORD] Wrong current password: {Emp}",employeeCode);return ServiceResult.Fail("Sai mật khẩu hiện tại");}user.Password=EncryptUtils.MD5(newPassword);await _uow.SaveChangesAsync(ct);await _sessionService.RevokeAllAsync(user.Id,ct);await _audit.LogAction("CHANGE_PASSWORD",user.Id,"Đổi mật khẩu thành công");Logger.LogInfoIf(Debug,"[PASSWORD] Changed success: {Emp}",employeeCode);return ServiceResult.Ok();}catch(Exception ex){Logger.LogError(ex,"[PASSWORD] Error for {Emp}",employeeCode);return ServiceResult.Fail("Lỗi hệ thống khi đổi mật khẩu.");}}
        public async Task<ServiceResult> Logout(int userId,string? refreshToken,string? ipAddress,string? userAgent,CancellationToken ct=default){try{Logger.LogDebugIf(Debug,"[LOGOUT] User: {UserId}",userId);if(!string.IsNullOrEmpty(refreshToken))await _sessionService.RevokeAsync(refreshToken,ct);await _audit.LogLogout(userId,ipAddress,userAgent);Logger.LogInfoIf(Debug,"[LOGOUT] Success: {UserId}",userId);return ServiceResult.Ok();}catch(Exception ex){Logger.LogError(ex,"[LOGOUT] Error: {UserId}",userId);return ServiceResult.Fail("Lỗi khi logout.");}}
        public async Task<ServiceResult<AuthResultDto>> CompleteTwoFactorLoginAsync(string challengeToken,string code,bool rememberMe,string? ipAddress,string? userAgent,CancellationToken ct=default)
        {
            var check=await _twoFactor.ValidateLoginChallengeAsync(challengeToken,code,ct);
            if(!check.IsSuccess||check.Data<=0) return ServiceResult<AuthResultDto>.Fail(check.Message??"Mã xác thực 2 lớp không hợp lệ.");
            var user=await _uow.Repository<F03User>().Query().FirstOrDefaultAsync(x=>x.Id==check.Data,ct);
            if(user==null||user.IsActive==false)return ServiceResult<AuthResultDto>.Fail("Tài khoản không tồn tại hoặc đã bị khóa.");
            await _audit.LogAction("2FA_SUCCESS",user.Id,"Xác thực 2 lớp thành công");
            return await IssueLoginResultAsync(user,rememberMe,"Web",Guid.NewGuid().ToString(),"2FA",ipAddress,userAgent,ct);
        }
        public async Task<ServiceResult<string>> RefreshTokenAsync(string refreshToken,CancellationToken ct=default){try{Logger.LogDebugIf(Debug,"[REFRESH_TOKEN] Attempt");var session=await _sessionService.ValidateSessionAsync(refreshToken,ct);if(session==null){Logger.LogWarnIf(Debug,"[REFRESH_TOKEN] Invalid, inactive or expired session.");return ServiceResult<string>.Fail("Phiên đăng nhập không hợp lệ hoặc đã hết hạn.");}var user=await _uow.Repository<F03User>().Query().FirstOrDefaultAsync(x=>x.Id==session.UserId,ct);if(user==null||user.IsActive==false)return ServiceResult<string>.Fail("Tài khoản không tồn tại hoặc đã bị khóa.");if(await _twoFactor.IsTwoFactorRequiredAsync(user.Id,ct)&&!user.TwoFactorEnabled)return ServiceResult<string>.Fail("Tài khoản có quyền dữ liệu nhạy cảm và chưa hoàn tất xác thực 2 lớp.");var newAccessToken=await GenerateJwtTokenAsync(user,session.RememberMe,ct);Logger.LogInfoIf(Debug,"[REFRESH_TOKEN] Success for UserId: {UserId}",session.UserId);return ServiceResult<string>.Ok(newAccessToken);}catch(Exception ex){Logger.LogError(ex,"[REFRESH_TOKEN] Error checking token");return ServiceResult<string>.Fail("Lỗi hệ thống khi làm mới token.");}}
        private async Task<ServiceResult<AuthResultDto>> IssueLoginResultAsync(F03User user,bool rememberMe,string deviceType,string deviceId,string? deviceName,string? ipAddress,string? userAgent,CancellationToken ct){user.LastLogin=DateTime.Now;await _uow.SaveChangesAsync(ct);var accessToken=await GenerateJwtTokenAsync(user,rememberMe,ct);var refreshTokenRaw=GenerateRefreshTokenRaw();var kickedConnections=await _sessionService.RegisterSessionAsync(user.Id,deviceType,deviceId,deviceName,refreshTokenRaw,rememberMe,ct);foreach(var connId in kickedConnections)await _hubContext.Clients.Client(connId).SendAsync("ForceLogout","Tài khoản vừa đăng nhập từ thiết bị khác",ct);await _audit.LogLoginSuccess(user.Id,ipAddress,userAgent);return ServiceResult<AuthResultDto>.Ok(new AuthResultDto{IsSuccess=true,Token=accessToken,RefreshToken=refreshTokenRaw,UserId=user.Id,FullName=user.FullName,Avatar=user.Avatar,IsAdmin=user.PermissionCode==UserPermissionCodes.SuperAdmin||user.PermissionCode==UserPermissionCodes.Admin});}
        private async Task HandleLoginFailedAsync(F03User user,CancellationToken ct){const int maxFail=5;user.NumLoginFailed++;Logger.LogWarnIf(Debug,"[LOGIN] Failed attempt {Count} for {Emp}",user.NumLoginFailed,user.EmployeeCode);if(user.NumLoginFailed>=maxFail){user.LockoutEndDate=DateTime.Now.AddMinutes(15);user.NumLoginFailed=0;Logger.LogWarnIf(Debug,"[LOGIN] Account locked for {Emp}",user.EmployeeCode);}await _uow.SaveChangesAsync(ct);}
        private async Task<string> GenerateJwtTokenAsync(F03User user,bool rememberMe,CancellationToken ct){var key=_jwtOptions.SecretKey??string.Empty;if(key.Length<32)throw new InvalidOperationException("JWT SecretKey phải có ít nhất 32 ký tự.");var emp=await _uow.Repository<F03Employee>().Query().AsNoTracking().FirstOrDefaultAsync(x=>x.EmployeeCode==user.EmployeeCode,ct);var claims=new List<Claim>{new(JwtRegisteredClaimNames.Sub,user.Id.ToString()),new(JwtRegisteredClaimNames.UniqueName,user.EmployeeCode??string.Empty),new(ClaimTypes.Name,user.FullName??user.EmployeeCode??string.Empty),new("EmployeeCode",user.EmployeeCode??string.Empty),new("DeptCode",user.DeptCode??string.Empty),new("Permission",user.PermissionCode.ToString()),new("LevelApprove",user.LevelApprove.ToString()),new("PositionCode",emp?.PositionCode??string.Empty)};var creds=new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),SecurityAlgorithms.HmacSha256);var token=new JwtSecurityToken(issuer:_jwtOptions.Issuer,audience:_jwtOptions.Audience,claims:claims,expires:DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenHours * 60),signingCredentials:creds);return new JwtSecurityTokenHandler().WriteToken(token);}
        private static string GenerateRefreshTokenRaw()=>Convert.ToBase64String(Guid.NewGuid().ToByteArray()).TrimEnd('=','\n').Replace('+','-').Replace('/','_');
    }
}