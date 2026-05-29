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
    public class EscalationRuleService
    : BaseService<EscalationRuleService>, IEscalationRuleService
    {
        private readonly FVNWEBAPPContext _db;
        private readonly IMemoryCache _cache;

        private const string RULE_CACHE_KEY = "EscalationRules_Cache";

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

                var rules = await _cache.GetOrCreateAsync(RULE_CACHE_KEY, async entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromHours(1);

                    Logger.LogDebugIf(Debug, "[RULE] Loading rules from DB");

                    return await _db.EscalationRules
                        .AsNoTracking()
                        .Where(x => x.IsActive == true)
                        .ToListAsync(ct);
                });

                if (rules == null || rules.Count == 0)
                {
                    Logger.LogWarnIf(Debug, "[RULE] No escalation rules found");
                    return 2;
                }

                var matchedRule = rules
                    .Where(x => x.Level == level)
                    .OrderByDescending(x => x.DeptCode == deptCode)
                    .ThenByDescending(x => string.IsNullOrEmpty(x.DeptCode) || x.DeptCode == "ALL")
                    .FirstOrDefault();

                int timeout = matchedRule?.TimeoutDays ?? 2;

                Logger.LogDebugIf(Debug,
                    "[RULE] Result Level={Level} Timeout={Timeout}",
                    level, timeout);

                return timeout;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[RULE] Error GetTimeoutDaysAsync");
                return 2;
            }
        }
    }
}
