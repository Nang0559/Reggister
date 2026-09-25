using System.Security.Cryptography;
using System.Text;
using FVN_REGISTER.Application.Interfaces.Auths;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Utils;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.Security;
using FVN_REGISTER.Core.Repositories;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QRCoder;

namespace FVN_REGISTER.Infrastructure.Services.Auths;

public sealed class TwoFactorService : ITwoFactorService
{
    private const int ChallengeMinutes = 10;
    private const int MaxAttempts = 5;
    private const string Issuer = "FVN_REGISTER";
    private readonly IUnitOfWork _uow;
    private readonly IDataProtector _protector;
    private readonly ILogger<TwoFactorService> _logger;

    public TwoFactorService(
        IUnitOfWork uow,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<TwoFactorService> logger)
    {
        _uow = uow;
        _protector = dataProtectionProvider.CreateProtector("FVN_REGISTER.TwoFactorSecret.v1");
        _logger = logger;
    }

    public async Task<bool> IsTwoFactorRequiredAsync(int userId, CancellationToken ct = default)
    {
        return await _uow.Repository<F03User>().Query()
            .Where(x => x.Id == userId)
            .Select(x => x.TwoFactorRequired)
            .FirstOrDefaultAsync(ct);
    }

    public Task<bool> IsSensitiveUserAsync(int userId, CancellationToken ct = default)
        => IsTwoFactorRequiredAsync(userId, ct);

    public async Task<string> CreateLoginChallengeAsync(int userId, CancellationToken ct = default)
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        var hash = Hash(raw);
        var repo = _uow.Repository<F03TwoFactorChallenge>();

        var old = await repo.Query()
            .Where(x => x.UserId == userId && !x.IsConsumed && x.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(ct);
        foreach (var item in old) { item.IsConsumed = true; item.ConsumedAt = DateTime.UtcNow; }

        await repo.AddAsync(new F03TwoFactorChallenge
        {
            UserId = userId,
            ChallengeHash = hash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(ChallengeMinutes),
            Purpose = "Login"
        }, ct);
        await _uow.SaveChangesAsync(ct);
        return raw;
    }

    public async Task<ServiceResult<int>> ValidateLoginChallengeAsync(string challengeToken, string code, CancellationToken ct = default)
    {
        var result = await ValidateChallengeAsync(challengeToken, code, "Login", ct);
        if (!result.IsSuccess || result.Data <= 0) return ServiceResult<int>.Fail(result.Message ?? "Mã xác thực không hợp lệ.");
        return ServiceResult<int>.Ok(result.Data);
    }

    public async Task<ServiceResult<TwoFactorSetupDto>> BeginSetupAsync(int userId, CancellationToken ct = default)
    {
        var user = await _uow.Repository<F03User>().Query().FirstOrDefaultAsync(x => x.Id == userId, ct);
        if (user == null) return ServiceResult<TwoFactorSetupDto>.Fail("Không tìm thấy tài khoản.");

        var secret = GenerateSecret();
        user.TwoFactorSecretEncrypted = _protector.Protect(secret);
        await _uow.SaveChangesAsync(ct);
        return ServiceResult<TwoFactorSetupDto>.Ok(BuildSetup(user.EmployeeCode, secret));
    }

    public async Task<ServiceResult> ConfirmSetupAsync(int userId, string code, CancellationToken ct = default)
    {
        var user = await _uow.Repository<F03User>().Query().FirstOrDefaultAsync(x => x.Id == userId, ct);
        if (user == null || string.IsNullOrWhiteSpace(user.TwoFactorSecretEncrypted))
            return ServiceResult.Fail("Chưa có cấu hình 2 lớp.");

        string secret;
        try { secret = _protector.Unprotect(user.TwoFactorSecretEncrypted); }
        catch { return ServiceResult.Fail("Cấu hình 2 lớp không hợp lệ, vui lòng tạo lại."); }

        if (!VerifyCode(secret, code))
            return ServiceResult.Fail("Mã xác thực không đúng.");

        user.TwoFactorEnabled = true;
        user.TwoFactorEnabledAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync(ct);
        return ServiceResult.Ok("Đã bật xác thực 2 lớp.");
    }

    public async Task<ServiceResult<TwoFactorStatusDto>> GetStatusAsync(int userId, CancellationToken ct = default)
    {
        return ServiceResult<TwoFactorStatusDto>.Ok(new TwoFactorStatusDto
        {
            Required = await IsTwoFactorRequiredAsync(userId, ct),
            Enabled = await _uow.Repository<F03User>().Query().Where(x => x.Id == userId).Select(x => x.TwoFactorEnabled).FirstOrDefaultAsync(ct)
        });
    }

    public async Task<ServiceResult<TwoFactorSetupDto>> BeginSetupFromChallengeAsync(string challengeToken, CancellationToken ct = default)
    {
        var userId = await GetChallengeUserAsync(challengeToken, ct);
        if (userId == null) return ServiceResult<TwoFactorSetupDto>.Fail("Phiên xác thực 2 lớp không hợp lệ hoặc đã hết hạn.");
        var user = await _uow.Repository<F03User>().Query().FirstOrDefaultAsync(x => x.Id == userId.Value, ct);
        if (user == null) return ServiceResult<TwoFactorSetupDto>.Fail("Không tìm thấy tài khoản.");