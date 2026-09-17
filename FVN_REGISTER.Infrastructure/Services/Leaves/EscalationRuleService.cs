
using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.MasterData;

using FVN_REGISTER.Core.Constants;

using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace FVN_REGISTER.Infrastructure.Services.Leaves
{
    public class EscalationRuleService
        : BaseService<EscalationRuleService>, IEscalationRuleService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMemoryCache _cache;

        private const string RULE_CACHE_KEY = "EscalationRules_Cache";
        private const decimal DEFAULT_WARNING_HOURS = 1.0m;
        private const decimal DEFAULT_ESCALATE_HOURS = 1.5m;
        private const int DEFAULT_DEADLINE_HOUR = 16;

        public EscalationRuleService(
            IUnitOfWork uow,
            IMemoryCache cache,
            ILogger<EscalationRuleService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _uow = uow;
            _cache = cache;
        }

        public async Task<EscalationRuleDto?> GetRuleAsync(
            RequestModule requestType, int level, string deptCode, CancellationToken ct = default)
        {
            var rules = await _cache.GetOrCreateAsync(RULE_CACHE_KEY, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromHours(1);

                return await _uow.Repository<F03EscalationRule>()
                    .Query()
                    .AsNoTracking()
                    .ToListAsync(ct);
            });

            var matchedRule = rules?
                .Where(x => x.Level == level && x.RequestModule == requestType)
                .OrderByDescending(x => string.Equals(x.DeptCode, deptCode, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(x => string.IsNullOrEmpty(x.DeptCode) || string.Equals(x.DeptCode, ApproveForDept.All, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (matchedRule != null)
            {
                return new EscalationRuleDto
                {
                    RequestType = matchedRule.RequestModule,
                    Level = matchedRule.Level,
                    DeptCode = matchedRule.DeptCode,
                    WarningHours = matchedRule.WarningHours,
                    EscalateHours = matchedRule.EscalateHours,
                    DeadlineHour = matchedRule.DeadlineHour
                };
            }

            Logger.LogWarnIf(Debug,
                "[ESC-RULE] Không tìm thấy rule cho {Type} Level={Level} Dept={Dept}, dùng mặc định",
                requestType, level, deptCode);

            return new EscalationRuleDto
            {
                RequestType = requestType,
                Level = level,
                DeptCode = deptCode,
                WarningHours = DEFAULT_WARNING_HOURS,
                EscalateHours = DEFAULT_ESCALATE_HOURS,
                DeadlineHour = DEFAULT_DEADLINE_HOUR
            };
        }

        public DateTime GetDeadline(DateTime registerDate, int deadlineHour)
        {
            return new DateTime(
                registerDate.Year, registerDate.Month, registerDate.Day,
                deadlineHour, 0, 0);
        }
    }
}
