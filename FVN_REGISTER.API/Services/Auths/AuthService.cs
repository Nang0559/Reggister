using FVN_REGISTER.Contract.Interfaces.Auths;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Contract.Util;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Contract.ViewModels;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FVN_REGISTER.API.Services.Auths
{
    public class AuthService : BaseService<AuthService>,IAuthService
    {
        private readonly FVNWEBAPPContext _db;
        private readonly IAuditService _audit;
        private readonly IConfiguration _configuration;

        public AuthService(
       FVNWEBAPPContext db,
       IAuditService audit,
       IConfiguration configuration,
       ILogger<AuthService> logger,
       IOptionsMonitor<AuthDebugOptions> options
       ) : base(logger, options)
        {
            _db = db;
            _audit = audit;
            _configuration = configuration;
        }

        public async Task<ServiceResult<UserSessionDto>> Login(
    string username,
    string password,
    CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[LOGIN] Attempt: {User}", username);

                var user = await _db.F03users
                    .FirstOrDefaultAsync(x => x.UserName == username, ct);

                if (user == null)
                {
                    Logger.LogWarnIf(Debug, "[LOGIN] User not found: {User}", username);

                    await _audit.LogLoginFailed(username);
                    return ServiceResult<UserSessionDto>.Fail("Sai tài khoản hoặc mật khẩu");
                }

                if (user.IsActive == false)
                {
                    Logger.LogWarnIf(Debug, "[LOGIN] Inactive user: {User}", username);
                    return ServiceResult<UserSessionDto>.Fail("Tài khoản chưa kích hoạt");
                }

                if (user.LockoutEndDate.HasValue && user.LockoutEndDate > DateTime.Now)
                {
                    Logger.LogWarnIf(Debug,
                        "[LOGIN] Locked user: {User} until {Time}",
                        username, user.LockoutEndDate);

                    return ServiceResult<UserSessionDto>.Fail(
                        $"Tài khoản bị khóa đến {user.LockoutEndDate:HH:mm}");
                }

                if (!EncryptUtils.PwdCompare(password, user.Password))
                {
                    Logger.LogWarnIf(Debug, "[LOGIN] Wrong password: {User}", username);

                    await HandleLoginFailed(user, ct);
                    await _audit.LogLoginFailed(username);

                    return ServiceResult<UserSessionDto>.Fail("Sai tài khoản hoặc mật khẩu");
                }

                // success
                user.NumLoginFailed = 0;
                user.LastLogin = DateTime.Now;
                user.LockoutEndDate = null;

                await _db.SaveChangesAsync(ct);

                var emp = await _db.VF03employees
                    .FirstOrDefaultAsync(x => x.EmployeeCode == user.EmployeeCode, ct);

                string token = GenerateJwtToken(user, emp);

                var sessionData = new UserSessionDto
                {
                    IsLoggedIn = true,
                    Token = token,
                    UserId = user.IdUser,
                    UserName = user.UserName,
                    FullName = emp?.EmployeeName,
                    Email = emp?.EmailAddress,
                    EmployeeCode = user.EmployeeCode,
                    DeptCode = user.DeptCode ?? emp?.DeptCode,
                    CVCode = user.Cvcode ?? emp?.Cvcode,
                    PermissionCode = user.PermissionCode,
                    LevelApprove = user.LevelApprove,
                    IsAdmin = user.PermissionCode == (int)UserRole.SuperAdmin ||
                              user.PermissionCode == (int)UserRole.Admin
                };

                Logger.LogInfoIf(Debug, "[LOGIN] Success: {User}", username);

                await _audit.LogLoginSuccess(user.IdUser);

                return ServiceResult<UserSessionDto>.Ok(sessionData);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[LOGIN] Exception for {User}", username);
                throw;
            }
        }

        public async Task<ServiceResult<UserSessionDto>> GetProfileAsync(int userId, CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[PROFILE] Get profile: {UserId}", userId);
                var user = await _db.F03users.FirstOrDefaultAsync(x => x.IdUser == userId, ct);

                if (user == null)
                {
                    Logger.LogWarnIf(Debug, "[PROFILE] User not found: {UserId}", userId);
                    return ServiceResult<UserSessionDto>.Fail("Không tìm thấy thông tin tài khoản.");
                }
                var emp = await _db.VF03employees.FirstOrDefaultAsync(x => x.EmployeeCode == user.EmployeeCode, ct);

                var profileData = new UserSessionDto
                {
                    IsLoggedIn = true,
                    UserId = user.IdUser,
                    UserName = user.UserName,
                    FullName = emp?.EmployeeName ?? "Nhân viên FCC",
                    Email = emp?.EmailAddress,
                    EmployeeCode = user.EmployeeCode,
                    DeptCode = user.DeptCode ?? emp?.DeptCode,
                    CVCode = user.Cvcode ?? emp?.Cvcode,
                    PermissionCode = user.PermissionCode,
                    LevelApprove = user.LevelApprove,
                    IsAdmin = user.PermissionCode == (int)UserRole.SuperAdmin ||
                              user.PermissionCode == (int)UserRole.Admin
                };
                Logger.LogDebugIf(Debug, "[PROFILE] Success: {UserId}", userId);
                return ServiceResult<UserSessionDto>.Ok(profileData);
            }
            catch (Exception ex) {
                Logger.LogError(ex, "[PROFILE] Exception for {UserId}", userId);

                return ServiceResult<UserSessionDto>
                    .Fail("Lỗi hệ thống khi lấy thông tin người dùng.");
            }
        }

        public async Task<ServiceResult> UpdateProfileAsync(
    int userId,
    string email,
    string? avatarUrl,
    CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[PROFILE] Update profile: {UserId}", userId);

                var user = await _db.F03users
                    .FirstOrDefaultAsync(x => x.IdUser == userId, ct);

                if (user == null)
                {
                    Logger.LogWarnIf(Debug, "[PROFILE] User not found: {UserId}", userId);
                    return ServiceResult.Fail("Không tìm thấy người dùng.");
                }

                user.Avatar = avatarUrl;

                var emp = await _db.F03employees
                    .FirstOrDefaultAsync(x => x.EmployeeCode == user.EmployeeCode, ct);

                if (emp != null)
                {
                    emp.EmailAddress = email;
                }

                await _db.SaveChangesAsync(ct);

                Logger.LogInfoIf(Debug, "[PROFILE] Updated success: {UserId}", userId);

                return ServiceResult.Ok("Cập nhật thông tin thành công");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[PROFILE] Update error: {UserId}", userId);
                return ServiceResult.Fail("Lỗi hệ thống khi cập nhật thông tin.");
            }
        }

        public async Task<ServiceResult> ChangePassword(
    string username,
    string currentPassword,
    string newPassword,
    CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[PASSWORD] Change attempt: {User}", username);

                var user = await _db.F03users
                    .FirstOrDefaultAsync(x => x.UserName == username, ct);

                if (user == null)
                {
                    Logger.LogWarnIf(Debug, "[PASSWORD] User not found: {User}", username);
                    return ServiceResult.Fail("User không tồn tại");
                }

                if (!EncryptUtils.PwdCompare(currentPassword, user.Password))
                {
                    Logger.LogWarnIf(Debug, "[PASSWORD] Wrong current password: {User}", username);
                    return ServiceResult.Fail("Sai mật khẩu hiện tại");
                }

                user.Password = EncryptUtils.MD5(newPassword);
                await _db.SaveChangesAsync(ct);

                await _audit.LogAction("CHANGE_PASSWORD", user.IdUser, "Đổi mật khẩu thành công");

                Logger.LogInfoIf(Debug, "[PASSWORD] Changed success: {User}", username);

                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[PASSWORD] Error for {User}", username);
                return ServiceResult.Fail("Lỗi hệ thống khi đổi mật khẩu.");
            }
        }

        public async Task<ServiceResult> Logout(int userId, CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[LOGOUT] User: {UserId}", userId);

                await _audit.LogLogout(userId);

                Logger.LogInfoIf(Debug, "[LOGOUT] Success: {UserId}", userId);

                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[LOGOUT] Error: {UserId}", userId);
                return ServiceResult.Fail("Lỗi khi logout.");
            }
        }

        private async Task HandleLoginFailed(F03user user, CancellationToken ct)
        {
            int maxFail = 5;

            user.NumLoginFailed = (user.NumLoginFailed ?? 0) + 1;

            Logger.LogWarnIf(Debug,
                "[LOGIN] Failed attempt {Count} for {User}",
                user.NumLoginFailed, user.UserName);

            if (user.NumLoginFailed >= maxFail)
            {
                user.LockoutEnable = true;
                user.LockoutEndDate = DateTime.UtcNow.AddMinutes(15);

                Logger.LogWarnIf(Debug,
                    "[LOGIN] User locked: {User} until {Time}",
                    user.UserName, user.LockoutEndDate);
            }

            await _db.SaveChangesAsync(ct);
        }

        private string GenerateJwtToken(F03user user, VF03employee? emp)
        {
            // Giữ nguyên logic Sync của bạn vì JWT creation là in-memory, không cần Async/Token
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = jwtSettings["SecretKey"];

            if (string.IsNullOrEmpty(secretKey))
            {
                Logger.LogError("[JWT] SecretKey missing");
                throw new Exception("JWT Secret Key is not configured.");
            }
            var key = Encoding.UTF8.GetBytes(secretKey);
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.IdUser.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("UserId", user.IdUser.ToString()),
            new Claim("FullName", emp?.EmployeeName?.Trim() ?? ""),
            new Claim("EmployeeCode", user.EmployeeCode ?? ""),
            new Claim("DeptCode", user.DeptCode ?? emp?.DeptCode ?? ""),
            new Claim("CvCode", user.Cvcode ?? emp?.Cvcode ?? ""),
            new Claim("LevelApprove", user.LevelApprove.ToString())
        };

            var userRole = (UserRole)user.PermissionCode;
            foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
            {
                if (role != UserRole.None && userRole.HasFlag(role))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
                }
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                NotBefore = DateTime.UtcNow.AddMinutes(-1),
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
