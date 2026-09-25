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

    public async Task<bool> IsTwoFactorRequiredAsync(int userId, CancellationToken ct = default)
    {
        return await _uow.Repository<F03User>().Query()
            .Where(x => x.Id == userId)
            .Select(x => x.TwoFactorRequired)
            .FirstOrDefaultAsync(ct);
    }

    // Backward-compatible alias. The policy is explicit, not inferred from RBAC.
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
        if (!result.IsSuccess || result.Data == null) return ServiceResult<int>.Fail(result.Message ?? "Mã xác thực không hợp lệ.");
        return ServiceResult<int>.Ok(result.Data.Value);
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
        if (user.TwoFactorEnabled) return ServiceResult<TwoFactorSetupDto>.Fail("Tài khoản đã bật 2 lớp; không thể thay thế factor từ phiên đăng nhập.");
        return await BeginSetupAsync(userId.Value, ct);
    }

    public async Task<ServiceResult<int>> ConfirmSetupFromChallengeAsync(string challengeToken, string code, CancellationToken ct = default)
    {
        var userId = await GetChallengeUserAsync(challengeToken, ct);
        if (userId == null) return ServiceResult<int>.Fail("Phiên xác thực 2 lớp không hợp lệ hoặc đã hết hạn.");

        var setup = await ConfirmSetupAsync(userId.Value, code, ct);
        if (!setup.IsSuccess) return ServiceResult<int>.Fail(setup.Message ?? "Không thể bật 2 lớp.");

        var challenge = await FindChallengeAsync(challengeToken, "Login", ct);
        if (challenge == null) return ServiceResult<int>.Fail("Phiên xác thực 2 lớp đã hết hạn.");
        return ServiceResult<int>.Ok(userId.Value);
    }


    public async Task<ServiceResult> SetRequiredAsync(int targetUserId, bool required, int actorUserId, CancellationToken ct = default)
    {
        if (targetUserId == actorUserId)
            return ServiceResult.Fail("Không cho phép tự thay đổi chính sách 2 lớp của chính mình.");

        var user = await _uow.Repository<F03User>().Query()
            .FirstOrDefaultAsync(x => x.Id == targetUserId, ct);
        if (user == null)
            return ServiceResult.Fail("Không tìm thấy tài khoản.");

        user.TwoFactorRequired = required;
        user.TwoFactorRequiredAt = required ? DateTime.UtcNow : null;
        user.TwoFactorRequiredBy = required ? actorUserId : null;
        await _uow.SaveChangesAsync(ct);

        if (!required)
        {
            await _uow.Repository<F03UserSession>().Query()
                .Where(x => x.UserId == targetUserId && x.IsActive == true)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.IsActive, false)
                    .SetProperty(x => x.RevokedAt, DateTime.UtcNow), ct);
        }

        return ServiceResult.Ok(required
            ? "Đã áp dụng bắt buộc xác thực 2 lớp. Người dùng sẽ đăng ký Authenticator ở lần đăng nhập kế tiếp."
            : "Đã tắt yêu cầu xác thực 2 lớp.");
    }

    public async Task<ServiceResult> ResetAsync(int targetUserId, int actorUserId, CancellationToken ct = default)
    {
        if (targetUserId == actorUserId) return ServiceResult.Fail("Không cho phép tự reset 2 lớp bằng quyền quản trị.");
        var user = await _uow.Repository<F03User>().Query().FirstOrDefaultAsync(x => x.Id == targetUserId, ct);
        if (user == null) return ServiceResult.Fail("Không tìm thấy tài khoản.");
        user.TwoFactorEnabled = false;
        user.TwoFactorEnabledAt = null;
        user.TwoFactorSecretEncrypted = null;
        await _uow.SaveChangesAsync(ct);
        await _uow.Repository<F03UserSession>().Query()
             .Where(x => x.UserId == targetUserId && x.IsActive == true)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, false).SetProperty(x => x.RevokedAt, DateTime.UtcNow), ct);
        return ServiceResult.Ok("Đã reset 2 lớp; người dùng sẽ phải đăng ký lại 2 lớp ở lần đăng nhập nhạy cảm tiếp theo.");
    }

    private async Task<ServiceResult<int>> ValidateChallengeAsync(string token, string code, string purpose, CancellationToken ct)
    {
        var challenge = await FindChallengeAsync(token, purpose, ct);
        if (challenge == null || challenge.ExpiresAt <= DateTime.UtcNow || challenge.IsConsumed)
            return ServiceResult<int>.Fail("Phiên xác thực 2 lớp không hợp lệ hoặc đã hết hạn.");

        if (challenge.FailedAttempts >= MaxAttempts)
            return ServiceResult<int>.Fail("Đã vượt quá số lần nhập mã cho phép.");

        var user = await _uow.Repository<F03User>().Query().FirstOrDefaultAsync(x => x.Id == challenge.UserId, ct);
        if (user == null) return ServiceResult<int>.Fail("Tài khoản không tồn tại.");
        if (string.IsNullOrWhiteSpace(user.TwoFactorSecretEncrypted))
            return ServiceResult<int>.Fail("Tài khoản chưa cấu hình 2 lớp.");

        string secret;
        try { secret = _protector.Unprotect(user.TwoFactorSecretEncrypted); }
        catch { return ServiceResult<int>.Fail("Cấu hình 2 lớp không hợp lệ."); }

        if (!VerifyCode(secret, code))
        {
            challenge.FailedAttempts++;
            await _uow.SaveChangesAsync(ct);
            return ServiceResult<int>.Fail("Mã xác thực không đúng.");
        }

        challenge.IsConsumed = true;
        challenge.ConsumedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync(ct);
        return ServiceResult<int>.Ok(challenge.UserId);
    }

    private async Task<F03TwoFactorChallenge?> FindChallengeAsync(string token, string purpose, CancellationToken ct)
    {
        var hash = Hash(token);
        return await _uow.Repository<F03TwoFactorChallenge>().Query()
            .FirstOrDefaultAsync(x => x.ChallengeHash == hash && x.Purpose == purpose, ct);
    }

    private async Task<int?> GetChallengeUserAsync(string token, CancellationToken ct)
    {
        var c = await FindChallengeAsync(token, "Login", ct);
        return c is { IsConsumed: false } && c.ExpiresAt > DateTime.UtcNow ? c.UserId : null;
    }

    private static string GenerateSecret()
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var bytes = RandomNumberGenerator.GetBytes(20);
        var sb = new StringBuilder(32);
        foreach (var b in bytes) sb.Append(alphabet[b & 31]);
        return sb.ToString();
    }

    private static bool VerifyCode(string secret, string code)
    {
        code = new string((code ?? string.Empty).Where(char.IsDigit).ToArray());
        if (code.Length != 6) return false;
        var counter = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
        for (var offset = -1; offset <= 1; offset++)
            if (GenerateCode(secret, counter + offset) == code) return true;
        return false;
    }

    private static string GenerateCode(string secret, long counter)
    {
        var key = Base32Decode(secret);
        Span<byte> bytes = stackalloc byte[8];
        for (var i = 7; i >= 0; i--) { bytes[i] = (byte)(counter & 0xff); counter >>= 8; }
        using var hmac = new HMACSHA1(key);
        var hash = hmac.ComputeHash(bytes.ToArray());
        var index = hash[^1] & 0x0f;
        var binary = ((hash[index] & 0x7f) << 24) | (hash[index + 1] << 16) | (hash[index + 2] << 8) | hash[index + 3];
        return (binary % 1_000_000).ToString("D6");
    }

    private static byte[] Base32Decode(string value)
    {
        var bits = value.TrimEnd('=').ToUpperInvariant();
        var buffer = 0; var bitCount = 0; var output = new List<byte>();
        foreach (var ch in bits)
        {
            var v = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".IndexOf(ch);
            if (v < 0) throw new FormatException("Invalid base32 secret.");
            buffer = (buffer << 5) | v; bitCount += 5;
            if (bitCount >= 8) { bitCount -= 8; output.Add((byte)((buffer >> bitCount) & 0xff)); }
        }
        return output.ToArray();
    }

    private static string Hash(string value)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }

    private static TwoFactorSetupDto BuildSetup(string account, string secret)
    {
        var uri = $"otpauth://totp/{Uri.EscapeDataString(Issuer)}:{Uri.EscapeDataString(account)}?secret={secret}&issuer={Uri.EscapeDataString(Issuer)}&algorithm=SHA1&digits=6&period=30";
        using var generator = new QRCodeGenerator();
        using var qrData = generator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(qrData).GetGraphic(8);

        return new TwoFactorSetupDto
        {
            Required = true,
            Secret = secret,
            Issuer = Issuer,
            Account = account,
            OtpAuthUri = uri,
            QrCodeDataUri = $"data:image/png;base64,{Convert.ToBase64String(png)}"
        };
    }
}
