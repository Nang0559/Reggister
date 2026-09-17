using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Application.Maps;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Core.Entities.HR;
using FVN_REGISTER.Core.Entities.OT;
using FVN_REGISTER.Core.Entities.Views;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Approvals
{
    public class OTApprovalListDataSource : IApprovalListDataSource<OTRequestDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IAttachmentService _attachmentService;

        public OTApprovalListDataSource(IUnitOfWork uow, IAttachmentService attachmentService)
        {
            _uow = uow;
            _attachmentService = attachmentService;
        }

        public RequestModule RequestType => RequestModule.Overtime;

        public async Task<List<OTRequestDto>> GetActiveRequestsByIdsAsync(
            List<int> requestIds, CancellationToken ct)
        {
            var entities = await _uow.Repository<F03OTRequest>().Query()
                .AsNoTracking()
                .Where(x => requestIds.Contains(x.Id) && x.IsActive == true)
                .ToListAsync(ct);

            var empCodes = entities.Select(e => e.EmployeeCode).Distinct().ToList();
            var deptCodes = entities.Select(e => e.DeptCode).Where(d => !string.IsNullOrEmpty(d)).Distinct().ToList();

            var requesters = await _uow.Repository<VF03employee>().Query()
                .AsNoTracking().Where(e => empCodes.Contains(e.EmployeeCode))
                .ToDictionaryAsync(e => e.EmployeeCode, ct);

            var departments = await _uow.Repository<F03Department>().Query()
                .AsNoTracking().Where(d => deptCodes.Contains(d.DeptCode))
                .ToDictionaryAsync(d => d.DeptCode, ct);

            var allEmployees = await _uow.Repository<F03OTEmployee>().Query()
                .AsNoTracking()
                .Where(e => requestIds.Contains(e.OTRequestId) && e.IsActive == true)
                .ToListAsync(ct);

            var dtos = entities.Select(x =>
            {
                requesters.TryGetValue(x.EmployeeCode, out var requester);
                F03Department? dept = null;
                if (!string.IsNullOrEmpty(x.DeptCode)) departments.TryGetValue(x.DeptCode, out dept);

                var dto = OTMapper.ToDto(x, requester, dept);
                dto.Details = allEmployees
                    .Where(e => e.OTRequestId == x.Id)
                    .Select(OTMapper.ToEmployeeDto)
                    .ToList();
                return dto;
            }).ToList();

            await _attachmentService.EnrichAsync(dtos, RequestType, ct);
            return dtos;
        }

        public PendingApprovalItemDto ToPendingItem(OTRequestDto row, bool canApprove)
            => OTMapper.ToPendingItem(row, canApprove);
    }
}
