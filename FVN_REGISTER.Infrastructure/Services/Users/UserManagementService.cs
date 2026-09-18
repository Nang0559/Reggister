using FVN_REGISTER.Application.Interfaces.Auths;
using FVN_REGISTER.Application.Interfaces.UserManagers;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Usermanagers;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace FVN_REGISTER.Infrastructure.Services.Users
{
    public class UserManagementService
        : BaseService<UserManagementService>, IUserManagementService
    {
        private readonly IUnitOfWork _uow;
        private readonly ISessionService _sessionService;

        public UserManagementService(
            IUnitOfWork uow,
            ISessionService sessionService,
            ILogger<UserManagementService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _uow = uow;
            _sessionService = sessionService;
        }

        // ── GET ALL ─────────────────────────────────────────────
        public async Task<List<UserAccountDto>> GetAllAsync(CancellationToken ct = default)
        {
            try
            {
                var joined = await _uow.Repository<F03User>().Query()
                    .AsNoTracking()
                    .OrderBy(x => x.PermissionCode)
                    .ThenBy(x => x.EmployeeCode)
                    .Select(x => new
                    {
                        User = x,
                        PermissionName = x.PermissionCodeNavigation.PermissionName
                    })
                    .ToListAsync(ct);

                var deptCodes = joined.Select(x => x.User.DeptCode).Where(d => d != null).Distinct().ToList();
                var deptNames = await _uow.Repository<F03Department>().Query()
                    .Where(d => deptCodes.Contains(d.DeptCode))
                    .ToDictionaryAsync(d => d.DeptCode, d => d.DeptName, ct);

                var userIds = joined.Select(x => x.User.Id).ToList();
                var functionMap = await _uow.Repository<F03UserFunction>().Query()
                    .Where(f => userIds.Contains(f.IdUser))
                    .GroupBy(f => f.IdUser)
                    .Select(g => new { UserId = g.Key, FunctionIds = g.Select(f => f.IdFunction).ToList() })
                    .ToDictionaryAsync(x => x.UserId, x => x.FunctionIds, ct);

                var list = joined.Select(x => MapToDto(x.User, x.PermissionName, deptNames, functionMap)).ToList();

                Logger.LogDebugIf(Debug, "[USER_MGT] GetAll: {Count} users", list.Count);
                return list;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[USER_MGT] GetAll ERROR");
                throw;
            }
        }

        // ── GET BY ID ────────────────────────────────────────────
        public async Task<UserAccountDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var user = await _uow.Repository<F03User>().Query()
                .AsNoTracking()
                .Include(x => x.PermissionCodeNavigation)
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (user == null) return null;

            var deptName = user.DeptCode == null ? null : await _uow.Repository<F03Department>().Query()
                .Where(d => d.DeptCode == user.DeptCode)
                .Select(d => d.DeptName)
                .FirstOrDefaultAsync(ct);

            var functionIds = await _uow.Repository<F03UserFunction>().Query()
                .Where(f => f.IdUser == id)
                .Select(f => f.IdFunction)
                .ToListAsync(ct);

            return new UserAccountDto
            {
                IdUser = user.Id,
                EmployeeCode = user.EmployeeCode,
                FullName = user.FullName,
                DeptCode = user.DeptCode,
                DeptName = deptName,
                PermissionCode = user.PermissionCode,
                PermissionName = user.PermissionCodeNavigation?.PermissionName,
                LevelApprove = user.LevelApprove,
                Cvcode = user.Cvcode,
                IsActive = user.IsActive ?? false,
                LockoutEnable = user.LockoutEnable,
                LockoutEndDate = user.LockoutEndDate,
                LastLogin = user.LastLogin,
                NumLoginFailed = user.NumLoginFailed,
                Avatar = user.Avatar,
                FunctionIds = functionIds
            };
        }

        // ── CREATE ───────────────────────────────────────────────
        public async Task<ServiceResult> CreateAsync(
            CreateUserRequest request, int currentUserId, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.EmployeeCode))
                    return ServiceResult.Fail("Mã nhân viên không được để trống.");

                if (string.IsNullOrWhiteSpace(request.Password))
                    return ServiceResult.Fail("Mật khẩu không được để trống.");

                var exists = await _uow.Repository<F03User>().Query()
                    .AnyAsync(x => x.EmployeeCode == request.EmployeeCode, ct);
                if (exists)
                    return ServiceResult.Fail($"Nhân viên '{request.EmployeeCode}' đã có tài khoản.");

                var permissionExists = await _uow.Repository<F03Permission>().Query()
                    .AnyAsync(x => x.PermissionCode == request.PermissionCode, ct);
                if (!permissionExists)
                    return ServiceResult.Fail("Quyền không tồn tại.");

                var entity = new F03User
                {
                    EmployeeCode = request.EmployeeCode,
                    Password = EncryptUtils.MD5(request.Password),
                    DeptCode = request.DeptCode,
                    PermissionCode = request.PermissionCode,
                    IsActive = true,
                    LockoutEnable = true,
                    LockoutEndDate = null,
                    NumLoginFailed = 0,
                    CreatedBy = currentUserId,
                    CreatedAt = DateTime.Now,
                    ModifiedBy = currentUserId,
                    ModifiedAt = DateTime.Now
                };

                await _uow.Repository<F03User>().AddAsync(entity, ct);
                await _uow.SaveChangesAsync(ct);

                if (request.FunctionIds is { Count: > 0 })
                    await SyncFunctionsAsync(entity.Id, request.FunctionIds, ct);

                await _uow.SaveChangesAsync(ct);

                Logger.LogInfoIf(Debug, "[USER_MGT] Created user: {Emp}", request.EmployeeCode);
                return ServiceResult.Ok("Tạo tài khoản thành công.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[USER_MGT] Create ERROR");
                return ServiceResult.Fail("Lỗi hệ thống khi tạo tài khoản.");
            }
        }

        // ── UPDATE ───────────────────────────────────────────────
        public async Task<ServiceResult> UpdateAsync(
            UpdateUserRequest request, int currentUserId, CancellationToken ct = default)
        {
            try
            {
                var user = await _uow.Repository<F03User>().Query()
                    .FirstOrDefaultAsync(x => x.Id == request.IdUser, ct);
                if (user == null)
                    return ServiceResult.Fail("Không tìm thấy tài khoản.");

                if (user.PermissionCode == UserPermissionCodes.SuperAdmin && request.IdUser != currentUserId)
                    return ServiceResult.Fail("Không thể sửa tài khoản Super Admin.");

                var permissionExists = await _uow.Repository<F03Permission>().Query()
                    .AnyAsync(x => x.PermissionCode == request.PermissionCode, ct);
                if (!permissionExists)
                    return ServiceResult.Fail("Quyền không tồn tại.");

                bool permissionChanged = user.PermissionCode != request.PermissionCode;

                // EmployeeCode CỐ Ý KHÔNG cập nhật — đã chốt: chỉ set 1 lần lúc Create.
                user.DeptCode = request.DeptCode;
                user.PermissionCode = request.PermissionCode;
                user.ModifiedBy = currentUserId;
                user.ModifiedAt = DateTime.Now;

                bool passwordChanged = false;
                if (!string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    user.Password = EncryptUtils.MD5(request.NewPassword);
                    passwordChanged = true;
                }

                // Resync nếu FunctionIds được gửi lên, HOẶC nếu PermissionCode đổi
                // (tránh IdPermission trên các dòng F03UserFunction cũ bị lệch dữ liệu)
                if (request.FunctionIds is not null)
                {
                    await SyncFunctionsAsync(user.Id, request.FunctionIds, ct);
                }
                else if (permissionChanged)
                {
                    var currentFunctionIds = await _uow.Repository<F03UserFunction>().Query()
                        .Where(f => f.IdUser == user.Id)
                        .Select(f => f.IdFunction)
                        .ToListAsync(ct);

                    await SyncFunctionsAsync(user.Id, currentFunctionIds, ct);
                }

                await _uow.SaveChangesAsync(ct);

                // Đổi quyền/mật khẩu là thay đổi nhạy cảm — JWT cũ đang mang PermissionCode/
                // DeptCode cũ vẫn còn hiệu lực tới khi hết hạn tự nhiên nếu không thu hồi.
                if (permissionChanged || passwordChanged)
                    await _sessionService.RevokeAllAsync(request.IdUser, ct);

                Logger.LogInfoIf(Debug,
                    "[USER_MGT] Updated user: {Id} | PermissionChanged={Perm} | PasswordChanged={Pwd}",
                    request.IdUser, permissionChanged, passwordChanged);

                return ServiceResult.Ok("Cập nhật thành công.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[USER_MGT] Update ERROR");
                return ServiceResult.Fail("Lỗi hệ thống khi cập nhật.");
            }
        }

        // ── DELETE (soft) ────────────────────────────────────────
        public async Task<ServiceResult> DeleteAsync(
            int id, int currentUserId, CancellationToken ct = default)
        {
            try
            {
                var user = await _uow.Repository<F03User>().Query()
                    .FirstOrDefaultAsync(x => x.Id == id, ct);
                if (user == null)
                    return ServiceResult.Fail("Không tìm thấy tài khoản.");

                if (user.PermissionCode == UserPermissionCodes.SuperAdmin)
                    return ServiceResult.Fail("Không thể xóa tài khoản Super Admin.");

                if (id == currentUserId)
                    return ServiceResult.Fail("Không thể tự xóa tài khoản của chính mình.");

                user.IsActive = false;
                user.ModifiedBy = currentUserId;
                user.ModifiedAt = DateTime.Now;

                await _uow.SaveChangesAsync(ct);

                // Thu hồi ngay mọi phiên đang mở — không đợi token hết hạn tự nhiên
                await _sessionService.RevokeAllAsync(id, ct);

                Logger.LogWarnIf(Debug, "[USER_MGT] Soft deleted user: {Id}", id);
                return ServiceResult.Ok("Đã xóa tài khoản.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[USER_MGT] Delete ERROR");
                return ServiceResult.Fail("Lỗi hệ thống khi xóa.");
            }
        }

        // ── REACTIVATE ───────────────────────────────────────────
        public async Task<ServiceResult> ReactivateAsync(
            int id, int currentUserId, CancellationToken ct = default)
        {
            try
            {
                var user = await _uow.Repository<F03User>().Query()
                    .FirstOrDefaultAsync(x => x.Id == id, ct);
                if (user == null)
                    return ServiceResult.Fail("Không tìm thấy tài khoản.");

                user.IsActive = true;
                user.ModifiedBy = currentUserId;
                user.ModifiedAt = DateTime.Now;

                await _uow.SaveChangesAsync(ct);

                Logger.LogInfoIf(Debug, "[USER_MGT] Reactivated user: {Id}", id);
                return ServiceResult.Ok("Đã khôi phục tài khoản.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[USER_MGT] Reactivate ERROR");
                return ServiceResult.Fail("Lỗi hệ thống khi khôi phục tài khoản.");
            }
        }

        // ── TOGGLE LOCK ──────────────────────────────────────────
        public async Task<ServiceResult> ToggleLockAsync(
            int id, int currentUserId, CancellationToken ct = default)
        {
            try
            {
                var user = await _uow.Repository<F03User>().Query()
                    .FirstOrDefaultAsync(x => x.Id == id, ct);
                if (user == null)
                    return ServiceResult.Fail("Không tìm thấy tài khoản.");

                if (user.PermissionCode == UserPermissionCodes.SuperAdmin)
                    return ServiceResult.Fail("Không thể khóa tài khoản Super Admin.");

                if (id == currentUserId)
                    return ServiceResult.Fail("Không thể tự khóa/mở khóa tài khoản của chính mình.");

                // "Đang bị khóa" = còn hạn LockoutEndDate trong tương lai — dùng chung field
                // với auto-lockout (AuthService, +15 phút sau 5 lần sai mật khẩu). Admin khóa
                // dùng +10 năm để phân biệt trực quan trên UI (nếu cần) so với khóa tạm.
                bool isLocked = user.LockoutEndDate.HasValue && user.LockoutEndDate.Value > DateTime.Now;

                if (isLocked)
                {
                    user.LockoutEndDate = null;
                    user.NumLoginFailed = 0;
                }
                else
                {
                    user.LockoutEndDate = DateTime.Now.AddYears(10);
                }

                user.ModifiedBy = currentUserId;
                user.ModifiedAt = DateTime.Now;

                await _uow.SaveChangesAsync(ct);

                // Vừa chuyển sang trạng thái KHÓA — thu hồi ngay mọi phiên đang mở
                if (!isLocked)
                    await _sessionService.RevokeAllAsync(id, ct);

                var action = isLocked ? "Mở khóa" : "Khóa";
                Logger.LogInfoIf(Debug, "[USER_MGT] {Action} user: {Id}", action, id);
                return ServiceResult.Ok($"{action} tài khoản thành công.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[USER_MGT] ToggleLock ERROR");
                return ServiceResult.Fail("Lỗi hệ thống.");
            }
        }

        // ── RESET PASSWORD ───────────────────────────────────────
        public async Task<ServiceResult> ResetPasswordAsync(
            int id, string newPassword, int currentUserId, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newPassword))
                    return ServiceResult.Fail("Mật khẩu mới không được để trống.");

                var user = await _uow.Repository<F03User>().Query()
                    .FirstOrDefaultAsync(x => x.Id == id, ct);
                if (user == null)
                    return ServiceResult.Fail("Không tìm thấy tài khoản.");

                user.Password = EncryptUtils.MD5(newPassword);
                user.NumLoginFailed = 0;
                user.LockoutEndDate = null;
                user.ModifiedBy = currentUserId;
                user.ModifiedAt = DateTime.Now;

                await _uow.SaveChangesAsync(ct);

                // Bắt buộc theo D0b — giống AuthService.ChangePassword, thu hồi mọi phiên cũ
                await _sessionService.RevokeAllAsync(id, ct);

                Logger.LogInfoIf(Debug, "[USER_MGT] Reset password user: {Id}", id);
                return ServiceResult.Ok("Reset mật khẩu thành công.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[USER_MGT] ResetPassword ERROR");
                return ServiceResult.Fail("Lỗi hệ thống khi reset mật khẩu.");
            }
        }

        // ===== HELPER =====
        private UserAccountDto MapToDto(
            F03User user, string? permissionName,
            Dictionary<string, string> deptNames, Dictionary<int, List<int>> functionMap)
        {
            deptNames.TryGetValue(user.DeptCode ?? "", out var deptName);
            functionMap.TryGetValue(user.Id, out var functionIds);

            return new UserAccountDto
            {
                IdUser = user.Id,
                EmployeeCode = user.EmployeeCode,
                FullName = user.FullName,
                DeptCode = user.DeptCode,
                DeptName = deptName,
                PermissionCode = user.PermissionCode,
                PermissionName = permissionName,
                LevelApprove = user.LevelApprove,
                Cvcode = user.Cvcode,
                IsActive = user.IsActive ?? false,
                LockoutEnable = user.LockoutEnable,
                LockoutEndDate = user.LockoutEndDate,
                LastLogin = user.LastLogin,
                NumLoginFailed = user.NumLoginFailed,
                Avatar = user.Avatar,
                FunctionIds = functionIds ?? new List<int>()
            };
        }

        // IdPermission = quyền hạn của user TẠI THỜI ĐIỂM gán function (lineage/audit),
        // không phải khóa logic (PK thật là (IdUser, IdFunction)). Xác nhận theo schema gốc
        // F03userFunction: HasKey(new { IdUser, IdFunction }) — IdPermission chỉ là FK đi kèm.
        private async Task SyncFunctionsAsync(int userId, List<int> functionIds, CancellationToken ct)
        {
            var repo = _uow.Repository<F03UserFunction>();

            var existing = await repo.Query().Where(x => x.IdUser == userId).ToListAsync(ct);
            foreach (var old in existing)
                repo.Remove(old);

            var permissionCode = await _uow.Repository<F03User>().Query()
                .Where(u => u.Id == userId)
                .Select(u => u.PermissionCode)
                .FirstAsync(ct);

            foreach (var functionId in functionIds.Distinct())
            {
                await repo.AddAsync(new F03UserFunction
                {
                    IdUser = userId,
                    IdFunction = functionId,
                    IdPermission = permissionCode
                }, ct);
            }
        }
    }
}
