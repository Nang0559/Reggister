using System.Security.Cryptography;
using System.Text;
using FVN_REGISTER.Contract.Dtos.Security;
using FVN_REGISTER.Core.Entities.Security;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Security;

/// <summary>
/// Nhận manifest chức năng từ Web/UI và lưu trạng thái phát hiện.
/// Manifest chỉ tạo/cập nhật candidate; không tự cấp quyền và không tự tạo F03Functions.
/// </summary>
public sealed class SecurityWebManifestService
{
    private readonly FVNWEBAPPContext _db;

    public SecurityWebManifestService(FVNWEBAPPContext db) => _db = db;

    public async Task<WebSecurityManifestSummaryDto> PublishAsync(
        IReadOnlyList<WebSecurityFunctionCandidateDto> entries,
        CancellationToken ct = default)
    {
        var now = DateTime.Now;
        var candidates = entries
            .Where(x => !string.IsNullOrWhiteSpace(x.FunctionKey))
            .GroupBy(x => x.FunctionKey.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .Take(10000)
            .ToList();

        var keys = candidates.Select(x => x.FunctionKey.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existing = await _db.SecurityFunctionRegistry
            .Where(x => x.SourceType == "WebManifest")
            .ToListAsync(ct);
        var existingByKey = existing.ToDictionary(x => x.FunctionKey, StringComparer.OrdinalIgnoreCase);
        var currentByKey = existing.Where(x => keys.Contains(x.FunctionKey))
            .ToDictionary(x => x.FunctionKey, StringComparer.OrdinalIgnoreCase);

        var registered = await _db.Functions
            .Where(x => keys.Contains(x.FunctionKey))
            .Select(x => new { x.FunctionKey, x.FunctionCode, x.LifecycleStatus })
            .ToListAsync(ct);
        var registeredByKey = registered
            .Where(x => !string.IsNullOrWhiteSpace(x.FunctionKey))
            .ToDictionary(x => x.FunctionKey, StringComparer.OrdinalIgnoreCase);

        var added = 0;
        var updated = 0;
        var active = 0;
        var ignored = 0;

        foreach (var candidate in candidates)
        {
            var key = candidate.FunctionKey.Trim();
            var isRegistered = registeredByKey.TryGetValue(key, out var function);
            var status = isRegistered && string.Equals(function!.LifecycleStatus, "Active", StringComparison.OrdinalIgnoreCase)
                ? "Active"
                : "PendingReview";

            if (!currentByKey.TryGetValue(key, out var item))
            {
                item = new F03SecurityFunctionRegistryItem
                {
                    FunctionKey = key,
                    FunctionCode = isRegistered ? function!.FunctionCode : 0,
                    DefinitionName = string.IsNullOrWhiteSpace(candidate.DisplayName) ? key : candidate.DisplayName.Trim(),
                    ModuleCode = string.IsNullOrWhiteSpace(candidate.Module) ? null : candidate.Module.Trim(),
                    ActionCode = string.IsNullOrWhiteSpace(candidate.Action) ? null : candidate.Action.Trim(),
                    LifecycleStatus = status,
                    SourceType = "WebManifest",
                    SourceAssembly = candidate.Source,
                    SourceTypeName = candidate.Component,
                    DefinitionHash = Hash(candidate),
                    FirstDiscoveredAt = now,
                    LastSeenAt = now,
                    IsIgnored = false
                };
                _db.SecurityFunctionRegistry.Add(item);
                currentByKey[key] = item;
                existingByKey[key] = item;
                added++;
            }
            else
            {
                item.LastSeenAt = now;
                item.DefinitionName = string.IsNullOrWhiteSpace(candidate.DisplayName) ? item.DefinitionName : candidate.DisplayName.Trim();
                item.ModuleCode = string.IsNullOrWhiteSpace(candidate.Module) ? item.ModuleCode : candidate.Module.Trim();
                item.ActionCode = string.IsNullOrWhiteSpace(candidate.Action) ? item.ActionCode : candidate.Action.Trim();
                item.SourceAssembly = candidate.Source;
                item.SourceTypeName = candidate.Component;
                item.DefinitionHash = Hash(candidate);

                if (item.IsIgnored)
                {
                    ignored++;
                }
                else if (isRegistered && string.Equals(function!.LifecycleStatus, "Active", StringComparison.OrdinalIgnoreCase))
                {
                    item.FunctionCode = function.FunctionCode;
                    item.LifecycleStatus = "Active";
                    item.ResolvedAt ??= now;
                    active++;
                }
                else if (item.LifecycleStatus is "Retired" or "Replaced" or "PendingRetirement")
                {
                    item.LifecycleStatus = "PendingReview";
                    item.ReplacementFunctionKey = null;
                    item.ResolvedAt = null;
                    updated++;
                }
                else
                {
                    item.LifecycleStatus = "PendingReview";
                    item.ResolvedAt = null;
                    updated++;
                }
            }
        }

        // A Web candidate that disappeared from the next manifest becomes a retirement candidate.
        // We never deactivate/delete F03Functions here; the SuperAdmin must explicitly retire or replace it.
        foreach (var item in existing)
        {
            if (keys.Contains(item.FunctionKey) || item.IsIgnored || item.LifecycleStatus is "Retired" or "Replaced")
                continue;

            item.LifecycleStatus = "PendingRetirement";
            item.ResolvedAt = null;
        }

        await _db.SaveChangesAsync(ct);
        return new WebSecurityManifestSummaryDto(now, candidates.Count, added, updated, active, ignored);
    }

    private static string Hash(WebSecurityFunctionCandidateDto candidate)
    {
        var value = string.Join("|", candidate.FunctionKey, candidate.Module, candidate.Action,
            candidate.Source, candidate.SourceType, candidate.Route, candidate.Component,
            candidate.DisplayName, candidate.Description);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }
}
