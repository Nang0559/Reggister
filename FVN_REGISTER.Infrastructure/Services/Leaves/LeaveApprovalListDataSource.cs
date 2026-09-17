


using FVN_REGISTER.Application.Interfaces.Approvals;
using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Application.Maps;
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Core.Entities.HR;
using FVN_REGISTER.Core.Entities.Leaves;
using FVN_REGISTER.Core.Entities.Views;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Leaves
{
    public class LeaveApprovalListDataSource : IApprovalListDataSource<LeaveRequestDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IAttachmentService _attachmentService;   // THÊM: thiếu inject

        public LeaveApprovalListDataSource(
            IUnitOfWork uow,
            IAttachmentService attachmentService)                 // THÊM
        {
            _uow = uow;
            _attachmentService = attachmentService;
        }

        public RequestModule RequestType => RequestModule.Leave;

        public async Task<List<LeaveRequestDto>> GetActiveRequestsByIdsAsync(
            List<int> requestIds, CancellationToken ct)
        {
            var entities = await _uow.Repository<F03LeaveDay>().Query()   // SỬA: Uow -> _uow
                .AsNoTracking()
                .Where(x => requestIds.Contains(x.Id) && x.IsActive == true)
                .ToListAsync(ct);

            var empCodes = entities.Select(e => e.EmployeeCode).Distinct().ToList();
            var deptCodes = entities.Select(e => e.DeptCode).Where(d => !string.IsNullOrEmpty(d)).Distinct().ToList();

            var requesters = await _uow.Repository<VF03employee>().Query()   // SỬA: Uow -> _uow
                .AsNoTracking().Where(e => empCodes.Contains(e.EmployeeCode))
                .ToDictionaryAsync(e => e.EmployeeCode, ct);

            var departments = await _uow.Repository<F03Department>().Query()   // SỬA: Uow -> _uow
                .AsNoTracking().Where(d => deptCodes.Contains(d.DeptCode))
                .ToDictionaryAsync(d => d.DeptCode, ct);

            var allDetails = await _uow.Repository<F03LeaveDayDetail>().Query()   // SỬA: Uow -> _uow
                .AsNoTracking()
                .Where(d => requestIds.Contains(d.LeaveDaysId))
                .ToListAsync(ct);

            var dtos = entities.Select(x =>
            {
                requesters.TryGetValue(x.EmployeeCode, out var requester);
                F03Department? dept = null;
                if (!string.IsNullOrEmpty(x.DeptCode)) departments.TryGetValue(x.DeptCode, out dept);

                var dto = LeaveMapper.ToDto(x, requester, dept);
                dto.Details = allDetails
                    .Where(d => d.LeaveDaysId == x.Id)
                    .Select(LeaveMapper.ToDetailDto)
                    .ToList();
                return dto;
            }).ToList();

            await _attachmentService.EnrichAsync(dtos, RequestType, ct);   // giờ đã có _attachmentService
            return dtos;
        }

        // SỬA: gọi lại LeaveMapper.ToPendingItem đã có sẵn, không viết tay lần 2
        // (tránh 2 nơi cùng định nghĩa logic map, dễ lệch field khi sửa sau này)
        public PendingApprovalItemDto ToPendingItem(LeaveRequestDto row, bool canApprove)
            => LeaveMapper.ToPendingItem(row, canApprove);
    }
}
