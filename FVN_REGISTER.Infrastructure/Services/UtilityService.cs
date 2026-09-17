
using FVN_REGISTER.Application.Interfaces.Common;

namespace FVN_REGISTER.Infrastructure.Utils
{
    public class UtilityService : IUtilityService
    {
        public string GenerateId()
        {
            var bytes = BitConverter.GetBytes(DateTime.Now.Ticks);
            return Convert.ToBase64String(bytes)
                .Replace('+', '_')
                .Replace('/', '-')
                .TrimEnd('=');
        }
    }
}
