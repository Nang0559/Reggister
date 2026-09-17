



using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.ApprovelSnapshotDto;
using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Entities.HR;
using FVN_REGISTER.Core.Entities.Leaves;
using FVN_REGISTER.Core.Entities.Views;
using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Maps
{
    public static class LeaveMapper
    {
        // Entity -> DTO đầy đủ (header, chưa gắn Details/ApprovalSteps)
        public static LeaveRequestDto ToDto(
            F03LeaveDay l,
            VF03employee? requester = null,
            F03Department? department = null,
            List<F03Attachment>? attachments = null)
        {
            return new LeaveRequestDto
            {
                Id = l.Id,
                RequestStatus = l.RequestStatus,
                Description = l.LeaveReason,
                RequesterCode = l.EmployeeCode,
                RequesterName = requester?.EmployeeName ?? string.Empty,
                DeptCode = l.DeptCode ?? string.Empty,          // ⚠️ giả định BaseRequestEntity có DeptCode, giống F03OTRequest
                DeptName = department?.DeptName ?? string.Empty,
                WorkYear = l.WorkYear,
                RegisterDate = l.CreatedAt,                      // ⚠️ TODO: F03LeaveDay không thấy field RegisterDate rõ ràng,
                                                                 // tạm dùng CreatedAt — xác nhận lại nếu có field khác đúng hơn
                ApprovalSteps = new List<ApprovalStepCalculatedDto>(),
                Attachments = AttachmentMapper.ToDtoList(attachments),
                Details = new List<LeaveRequestDetailDto>()      // gắn sau qua AttachDetails
            };
        }

        // Dùng khi query trên VIEW phẳng VF03LeaveRequest (đã có sẵn StartDate/EndDate/TotalDay tính sẵn)
        // Lưu ý: LeaveRequestDto.StartDate/EndDate/TotalDay là computed từ Details,
        // nên map qua view chỉ dùng được cho hiển thị danh sách (không cần Details chi tiết).
        public static LeaveRequestDetailDto ToDetailDtoFromView(VF03LeaveRequest v)
        {
            return new LeaveRequestDetailDto
            {
                LeaveDate = v.StartDate,
                LeaveTypeCode = v.LeaveTypeCode ?? string.Empty,
                LeaveTypeName = v.LeaveTypeName,
                IsCountedAsLeave = true,
                DayValue = v.TotalDay
            };
        }

        public static LeaveRequestDto ToDtoFromView(VF03LeaveRequest v)
        {
            var dto = new LeaveRequestDto
            {
                Id = v.Id,
                RequestStatus = v.RequestStatus,
                Description = v.LeaveReason ?? string.Empty,
                RequesterCode = v.EmployeeCode,
                RequesterName = v.EmployeeName,
                DeptCode = v.DeptCode ?? string.Empty,
                DeptName = v.DeptName ?? string.Empty,
                WorkYear = v.WorkYear ?? DateTime.Now.Year,
                RegisterDate = v.RegisterDate,
                Attachments = new List<AttachmentDto>()
            };

            // Đưa 1 dòng detail tổng hợp từ view để StartDate/EndDate/TotalDay tính đúng
            dto.Details = new List<LeaveRequestDetailDto> { ToDetailDtoFromView(v) };
            return dto;
        }

        public static LeaveRequestDetailDto ToDetailDto(F03LeaveDayDetail d) => new()
        {
            LeaveDate = d.LeaveDate,
            LeaveTypeCode = d.LeaveTypeCode,
            LeaveTypeName = d.LeaveTypeName,
            IsCountedAsLeave = d.IsCountedAsLeave,
            IsHalfDay = d.IsHalfDay,
            HalfDayOption = d.HalfDayOption,
            DayValue = d.DayValue
        };

        public static PendingApprovalItemDto ToPendingItem(LeaveRequestDto dto, bool canApprove)
        {
            return new PendingApprovalItemDto
            {
                RequestId = dto.Id,
                Kind = RequestModule.Leave,
                EmployeeCode = dto.RequesterCode,
                EmployeeName = dto.RequesterName,
                DeptCode = dto.DeptCode,
                DeptName = dto.DeptName,
                FromDate = dto.StartDate,
                ToDate = dto.EndDate,
                TotalUnits = dto.TotalDay,
                ApprovalSteps = dto.ApprovalSteps,
                CanApprove = canApprove
            };
        }
    }
}
