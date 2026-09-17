using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Application.Maps;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Interfaces;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Common
{
    public class AttachmentService : IAttachmentService
    {
        private readonly FVNWEBAPPContext _db;

        public AttachmentService(FVNWEBAPPContext db) => _db = db;

        public async Task EnrichAsync<T>(
            IEnumerable<T> items, RequestModule requestType, CancellationToken ct)
            where T : IHasAttachments
        {
            var list = items.ToList();
            var ids = list.Select(x => x.Id).ToList();
            if (ids.Count == 0) return;

            var attachments = await _db.Attachments
                .AsNoTracking()
                .Where(x => ids.Contains(x.RequestId) && x.Module == requestType && x.IsActive == true)
                .ToListAsync(ct);

            var lookup = attachments.ToLookup(a => a.RequestId, a => AttachmentMapper.ToDto(a));

            foreach (var item in list)
            {
                item.Attachments = lookup[item.Id].ToList();
            }
        }

        public async Task<List<AttachmentDto>> GetAttachmentsAsync(
            int requestId, RequestModule requestType, CancellationToken ct)
        {
            var entities = await _db.Attachments
                .AsNoTracking()
                .Where(a => a.RequestId == requestId && a.Module == requestType && a.IsActive == true)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync(ct);

            return AttachmentMapper.ToDtoList(entities);
        }
    }
}
