

using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Repositories;

namespace FVN_REGISTER.Infrastructure.Services.Users
{
    public class UserLogService : IUserLogService
    {
        private readonly IUnitOfWork _uow;

        public UserLogService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task UpdateLastSeenAsync(
            int userId,
            string action,
            string url,
            string? ipAddress = null,
            string? userAgent = null,
            CancellationToken ct = default)
        {
            var log = new F03UserLog
            {
                UserId = userId,
                ApplicationName = "FVN_REGISTER API",
                ApplicationVersion = "2.0.0",
                WorkstationName = ipAddress ?? "Unknown",
                WorkstationUser = Truncate(userAgent ?? "Unknown", 250),
                LastSeen = action,
                LastSeenUrl = url,
                CreatedAt = DateTime.Now
            };

            await _uow.Repository<F03UserLog>().AddAsync(log, ct);
            await _uow.SaveChangesAsync(ct);
        }

        private static string Truncate(string value, int maxLength)
            => value.Length > maxLength ? value[..maxLength] : value;
    }
}
