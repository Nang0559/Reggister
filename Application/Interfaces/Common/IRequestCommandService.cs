using FVN_REGISTER.Contract.Dtos.Authentication;
using FVN_REGISTER.Contract.Requests.Leaves;
using FVN_REGISTER.Contract.Utils;

namespace FVN_REGISTER.Application.Interfaces.Common
{
    /// <summary>
    /// Common command port for request modules (Leave, Overtime, Trip...).
    /// Implementations belong to Infrastructure/application service adapters.
    /// </summary>
    public interface IRequestCommandService<TCreateModel>
    {
        Task<ServiceResult<int>> CreateAsync(
            TCreateModel model, UserIdentityDto user, CancellationToken ct = default);

        Task<ServiceResult> ApproveAsync(
            List<int> ids,
            int level,
            UserIdentityDto user,
            string? comment,
            CancellationToken ct = default);

        Task<ServiceResult> RejectAsync(
            List<int> ids,
            int level,
            UserIdentityDto user,
            string comment,
            CancellationToken ct = default);

        Task<ServiceResult> CancelAsync(
            int requestId,
            string reason,
            UserIdentityDto user,
            CancellationToken ct = default);

        Task<ServiceResult> AttachFilesAsync(
            AttachFilesCommandDto command,
            UserIdentityDto user,
            CancellationToken ct = default);
    }
}
