namespace FVN_REGISTER.API.Services.Users
{
    using FVN_REGISTER.Contract.Interfaces.Users;
    using FVN_REGISTER.Contract.Models;
    using FVN_REGISTER.Contract.Util;
    using FVN_REGISTER.Contract.Utils;
    using FVN_REGISTER.Contract.ViewModels;
    using FVN_REGISTER.Core.Configurations;
    using FVN_REGISTER.Core.Logging;
    using FVN_REGISTER.Core.Services;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System.Diagnostics;

    public class UserService
    : BaseService<UserService>, IUserService
    {
        private readonly FVNWEBAPPContext _db;

        public UserService(
            FVNWEBAPPContext db,
            ILogger<UserService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _db = db;
        }

        public async Task<List<VF03user>> GetAllAsync(CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[USER] GetAll");

                return await _db.VF03users
                    .Where(x => x.IsActive == true && x.PermissionCode >= (int)UserRole.Admin)
                    .OrderBy(x => x.IdUser)
                    .ToListAsync(ct);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[USER] GetAll ERROR");
                throw;
            }
        }

        public async Task<F03user?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[USER] GetById: {Id}", id);

                return await _db.F03users
                    .FirstOrDefaultAsync(x => x.IdUser == id, ct);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[USER] GetById ERROR: {Id}", id);
                throw;
            }
        }

        public async Task<ServiceResult> CreateAsync(
            UserAccountViewModel model,
            int currentUserId,
            CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[USER] Create: {User}", model.UserName);

                var exists = await _db.F03users
                    .AnyAsync(x => x.UserName == model.UserName, ct);

                if (exists)
                    return ServiceResult.Fail("Tài khoản đã tồn tại.");

                var entity = new F03user
                {
                    UserName = model.UserName,
                    Password = EncryptUtils.MD5(model.Password),
                    PermissionCode = model.PermissionCode,
                    DeptCode = model.DeptCode,
                    IsActive = true,
                    CreatedBy = currentUserId,
                    CreatedAt = DateTime.Now
                };

                _db.F03users.Add(entity);
                await _db.SaveChangesAsync(ct);

                Logger.LogInfoIf(Debug, "[USER] Created: {User}", model.UserName);

                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[USER] Create ERROR");
                return ServiceResult.Fail("Lỗi hệ thống khi tạo user.");
            }
        }

        public async Task<ServiceResult> UpdateAsync(
            UserAccountViewModel model,
            int currentUserId,
            CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[USER] Update: {Id}", model.IdUser);

                var user = await _db.F03users
                    .FirstOrDefaultAsync(x => x.IdUser == model.IdUser, ct);

                if (user == null)
                    return ServiceResult.Fail("Không tìm thấy người dùng.");

                user.PermissionCode = model.PermissionCode;
                user.DeptCode = model.DeptCode;

                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    user.Password = EncryptUtils.MD5(model.NewPassword);
                }

                user.ModifiedBy = currentUserId;
                user.ModifiedAt = DateTime.Now;

                await _db.SaveChangesAsync(ct);

                Logger.LogInfoIf(Debug, "[USER] Updated: {Id}", model.IdUser);

                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[USER] Update ERROR");
                return ServiceResult.Fail("Lỗi hệ thống khi cập nhật.");
            }
        }

        public async Task<ServiceResult> DeleteAsync(
            int id,
            int currentUserId,
            CancellationToken ct = default)
        {
            try
            {
                Logger.LogDebugIf(Debug, "[USER] Delete: {Id}", id);

                var user = await _db.F03users
                    .FirstOrDefaultAsync(x => x.IdUser == id, ct);

                if (user == null)
                    return ServiceResult.Fail("Người dùng không tồn tại.");

                user.IsActive = false;
                user.ModifiedBy = currentUserId;
                user.ModifiedAt = DateTime.Now;

                await _db.SaveChangesAsync(ct);

                Logger.LogWarnIf(Debug, "[USER] Soft deleted: {Id}", id);

                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[USER] Delete ERROR");
                return ServiceResult.Fail("Lỗi hệ thống khi xóa.");
            }
        }
    }
}