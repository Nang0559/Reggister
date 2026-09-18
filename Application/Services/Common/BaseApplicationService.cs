using FVN_REGISTER.Application.Configuration;
using FVN_REGISTER.Contract.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace FVN_REGISTER.Application.Services.Common
{
    public abstract class BaseApplicationService<T> : BaseService<T>
    {
        protected BaseApplicationService(
            ILogger<T> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
        }
    }
}
