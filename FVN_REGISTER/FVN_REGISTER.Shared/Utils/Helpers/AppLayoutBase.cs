using FVN_REGISTER.Core.Configurations;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace FVN_REGISTER.Shared.Utils.Helpers
{
    public class AppLayoutBase : LayoutComponentBase
    {
        [Inject] protected ILoggerFactory LoggerFactory { get; set; } = default!;
        [Inject] protected IOptionsMonitor<AuthDebugOptions> DebugOptions { get; set; } = default!;

        protected ILogger Logger = default!;
        protected bool Debug => DebugOptions.CurrentValue.Enabled;

        protected string ComponentName => GetType().Name;

        protected override void OnInitialized()
        {
            Logger = LoggerFactory.CreateLogger(GetType());
        }
    }
}
