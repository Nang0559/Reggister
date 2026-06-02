using FVN_REGISTER.Contract.Interfaces.Leaves;
using FVN_REGISTER.Contract.Models;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;


namespace FVN_REGISTER.API.Services.Leaves
{
    namespace FVN_REGISTER.API.Services.Leaves
    {
        public class EscalationRuleService
            : BaseService<EscalationRuleService>, IEscalationRuleService
        {
            private readonly FVNWEBAPPContext _db;
            private readonly IMemoryCache _cache;
            private const string RULE_CACHE_KEY = "EscalationRules_Cache";
            private const int DEFAULT_TIMEOUT_DAYS = 2;

            public EscalationRuleService(
                FVNWEBAPPContext db,
                IMemoryCache cache,
                ILogger<EscalationRuleService> logger,
                IOptionsMonitor<AuthDebugOptions> options)
                : base(logger, options)
            {
                _db = db;
                _cache = cache;
            }

            public async Task<int> GetTimeoutDaysAsync(
                F03leaveDay leave,
                int level,
                string deptCode,
                CancellationToken ct = default)
            {
                try
                {
                    Logger.LogDebugIf(Debug,
                        "[RULE] Get timeout Level={Level} Dept={Dept}",
                        level, deptCode);

                    // ✅ Dùng GetOrCreateAsync an toàn với nullable
                    var rules = await _cache.GetOrCreateAsync(
                        RULE_CACHE_KEY,
                        async entry =>
                        {
                            entry.SlidingExpiration = TimeSpan.FromHours(1);
                            Logger.LogDebugIf(Debug, "[RULE] Loading rules from DB");

                            return await _db.EscalationRules
                                .AsNoTracking()
                                .Where(x => x.IsActive == true)
                                .ToListAsync(ct);
                        });

                    // ✅ Xử lý null an toàn
                    if (rules == null || rules.Count == 0)
                    {
                        Logger.LogWarnIf(Debug, "[RULE] No escalation rules found, using default={Default}",
                            DEFAULT_TIMEOUT_DAYS);
                        return DEFAULT_TIMEOUT_DAYS;
                    }

                    // ✅ Ưu tiên: rule khớp dept trước, fallback sang ALL/global
                    var matchedRule = rules
                        .Where(x => x.Level == level)
                        .OrderByDescending(x =>
                            string.Equals(x.DeptCode, deptCode, StringComparison.OrdinalIgnoreCase))
                        .ThenByDescending(x =>
                            string.IsNullOrEmpty(x.DeptCode) ||
                            string.Equals(x.DeptCode, "ALL", StringComparison.OrdinalIgnoreCase))
                        .FirstOrDefault();

                    int timeout = matchedRule?.TimeoutDays ?? DEFAULT_TIMEOUT_DAYS;

                    Logger.LogDebugIf(Debug,
                        "[RULE] Result Level={Level} Dept={Dept} Timeout={Timeout} RuleId={RuleId}",
                        level, deptCode, timeout, matchedRule?.Id);

                    return timeout;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "[RULE] Error GetTimeoutDaysAsync Level={Level}", level);
                    return DEFAULT_TIMEOUT_DAYS;
                }
            }
        }
    }
}
