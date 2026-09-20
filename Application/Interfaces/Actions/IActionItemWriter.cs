using FVN_REGISTER.Application.Models.Actions;

namespace FVN_REGISTER.Application.Interfaces.Actions;

public interface IActionItemWriter
{
    Task<Guid> EnsureOpenAsync(
        ActionItemDraft draft,
        CancellationToken cancellationToken = default);
}
