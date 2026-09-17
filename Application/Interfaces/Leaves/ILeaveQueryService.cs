


using FVN_REGISTER.Application.Interfaces.Common;
using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Contract.Dtos.MasterData;


namespace FVN_REGISTER.Application.Interfaces.Leaves
{
    public interface ILeaveQueryService
     : IRequestQueryService<LeaveSummaryDto, LeaveBalanceDto, LeaveRequestDto>
    {
        /// Dữ liệu tổng hợp cho trang đăng ký nghỉ phép (loại nghỉ, số dư phép, cấp duyệt...).
        Task<SystemMasterDataDto> GetCombinedDataAsync(
            string empCode, string deptCode, string cvCode, int year, CancellationToken ct = default);
    }
}
