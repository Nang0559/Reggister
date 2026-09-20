using FVN_REGISTER.Contract.Dtos.Actions;

namespace FVN_REGISTER.Application.Interfaces.Actions;

public interface IActionItemService
{
    Task<IReadOnlyList<ActionItemDto>> GetMineAsync(
        int employeeId,
        int userId,
        bool includeCompleted = false,
        CancellationToken cancellationToken = default);

    Task<ActionCountDto> GetCountAsync(
        int employeeId,
        int userId,
        CancellationToken cancellationToken = default);

    Task<ActionItemDto?> GetAsync(
        int employeeId,
        int userId,
        Guid actionId,
        CancellationToken cancellationToken = default);

    Task<bool> CompleteAsync(
        int employeeId,
        int userId,
        Guid actionId,
        CancellationToken cancellationToken = default);

    Task<bool> DismissAsync(
        int employeeId,
        int userId,
        Guid actionId,
        CancellationToken cancellationToken = default);
}
