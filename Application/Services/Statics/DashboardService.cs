using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Application.Interfaces.Orchestrators;
using FVN_REGISTER.Application.Interfaces.Statics;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Contract.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Application.Services.Statics
{
    /// <summary>
    /// Application facade for Dashboard.
    /// Composition/orchestration is owned by DashboardOrchestrator; this service
    /// keeps the API boundary stable through IDashboardService.
    /// </summary>
    public sealed class DashboardService : BaseApplicationService<DashboardService>, IDashboardService
    {
        private readonly IDashboardOrchestrator _orchestrator;

        public DashboardService(
            IDashboardOrchestrator orchestrator,
            ILogger<DashboardService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _orchestrator = orchestrator;
        }

        public Task<ServiceResult<DashboardResponse>> GetDashboardAsync(
            UserIdentityDto user, CancellationToken ct = default)
            => _orchestrator.BuildAsync(user, ct);
    }
}
