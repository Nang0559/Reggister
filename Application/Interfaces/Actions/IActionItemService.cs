using FVN_REGISTER.Contract.Dtos.Actions;

namespace FVN_REGISTER.Application.Interfaces.Actions;

public interface IActionItemService
{
    Task<IReadOnlyList<ActionItemDto>> GetMineAsync(
        string employeeCode,
        int userId,
        bool includeCompleted = false,
        CancellationToken cancellationToken = default);

    Task<ActionCountDto> GetCountAsync(
        string employeeCode,
        int userId,
        CancellationToken cancellationToken = default);

    Task<ActionItemDto?> GetAsync(
        string employeeCode,
        int userId,
        Guid actionId,
        CancellationToken cancellationToken = default);

    Task<bool> CompleteAsync(
        string employeeCode,
        int userId,
        Guid actionId,
        CancellationToken cancellationToken = default);

    Task<bool> DismissAsync(
        string employeeCode,
        int userId,
        Guid actionId,
        CancellationToken cancellationToken = default);
}
