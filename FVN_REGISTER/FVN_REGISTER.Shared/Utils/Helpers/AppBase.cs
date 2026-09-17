using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Shared.Utils.Helpers
{
    public abstract class AppBase : ComponentBase
    {
        [Inject] protected ILoggerFactory LoggerFactory { get; set; } = default!;
        [Inject] protected IConfiguration Configuration { get; set; } = default!;

        protected ILogger Logger = default!;

        protected bool Debug => Configuration.GetValue<bool>("AuthDebug:Enabled");

        protected string ComponentName => GetType().Name;

        protected override void OnInitialized()
        {
            Logger = LoggerFactory.CreateLogger(GetType());
        }
    }
}
