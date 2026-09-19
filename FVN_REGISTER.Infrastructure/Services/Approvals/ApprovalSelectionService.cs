using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Contract.Requests.Approvals;
using FVN_REGISTER.Core.Entities.Approvers;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Approvals;

public sealed class ApprovalSelectionService : IApprovalSelectionService
{
    private readonly IUnitOfWork _uow;

    public ApprovalSelectionService(IUnitOfWork uow) => _uow = uow;

    public async Task ReplaceAsync(
        RequestModule requestType,
        int requestId,
        IEnumerable<ApprovalSelectionDto> selections,
        int createdBy,
        CancellationToken ct = default)
    {
        var repo = _uow.Repository<F03ApprovalSelection>();

        var old = await repo.Query()
            .Where(x => x.RequestType == requestType && x.RequestId == requestId)
            .ToListAsync(ct);

        foreach (var row in old)
            repo.Remove(row);

        foreach (var selection in selections
                     .Where(x => x.Level > 0 && !string.IsNullOrWhiteSpace(x.ApproverCode))
                     .GroupBy(x => x.Level)
                     .Select(x => x.Last()))
        {
            await repo.AddAsync(new F03ApprovalSelection
            {
                RequestType = requestType,
                RequestId = requestId,
                Level = selection.Level,
                ApproverCode = selection.ApproverCode.Trim(),
                CreatedBy = createdBy,
                CreatedAt = DateTime.Now,
                IsActive = true
            }, ct);
        }

        await _uow.SaveChangesAsync(ct);
    }

    public async Task<List<ApprovalSelectionDto>> GetAsync(
        RequestModule requestType,
        int requestId,
        CancellationToken ct = default)
    {
        return await _uow.Repository<F03ApprovalSelection>().Query()
            .AsNoTracking()
            .Where(x => x.RequestType == requestType &&
                        x.RequestId == requestId &&
                        x.IsActive == true)
            .OrderBy(x => x.Level)
            .Select(x => new ApprovalSelectionDto
            {
                Level = x.Level,
                ApproverCode = x.ApproverCode
            })
            .ToListAsync(ct);
    }

    public async Task ClearAsync(
        RequestModule requestType,
        int requestId,
        CancellationToken ct = default)
    {
        var repo = _uow.Repository<F03ApprovalSelection>();
        var rows = await repo.Query()
            .Where(x => x.RequestType == requestType && x.RequestId == requestId)
            .ToListAsync(ct);

        foreach (var row in rows)
            repo.Remove(row);

        await _uow.SaveChangesAsync(ct);
    }
}
