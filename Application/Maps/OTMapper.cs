
using FVN_REGISTER.Contract.Dtos.Approvals;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Entities.HR;
using FVN_REGISTER.Core.Entities.OT;
using FVN_REGISTER.Core.Entities.Views;
using FVN_REGISTER.Core.Enums;




namespace FVN_REGISTER.Application.Maps
{
    /// <summary>
    /// Cung cấp các phương thức ánh xạ (Mapping) dữ liệu giữa các Entity Database 
    /// (VF03OTRequest, F03OTRequest, F03OTEmployee...) và các ViewModel hiển thị cho đơn OT.
    /// </summary>
    public static class OTMapper
    {
        // Entity -> DTO (dùng khi đã có đủ header + requester + department)
        public static OTRequestDto ToDto(
            F03OTRequest r,
            VF03employee? requester = null,
            F03Department? department = null,
            List<F03Attachment>? attachments = null)
        {
            return new OTRequestDto
            {
                Id = r.Id,
                RequestStatus = r.RequestStatus,
                Description = r.OTReasonSummary ?? string.Empty,
                RequesterCode = r.EmployeeCode ?? string.Empty,
                RequesterName = requester?.EmployeeName ?? string.Empty,
                DeptCode = r.DeptCode ?? "",
                DeptName = department?.DeptName ?? string.Empty,
                WorkYear = r.OTDate.Year,
                OTDate = r.OTDate,
                OTTypeCode = r.OTTypeCode,
                OtPurpose = r.OTReasonSummary ?? string.Empty,
                ApprovalSteps = new List<ApprovalStepDto>(),
                Attachments = AttachmentMapper.ToDtoList(attachments),
                Details = r.Employees?
                    .Where(e => e.IsActive == true)
                    .Select(ToEmployeeDto)
                    .ToList() ?? new List<OTEmployeeDto>()
            };
        }

        public static OTRequestDto ToDto(VF03OTRequest v)
        {
            return new OTRequestDto
            {
                Id = v.Id ?? 0,
                RequestStatus = v.RequestStatus,
                Description = v.OTReasonSummary ?? string.Empty,
                RequesterCode = v.EmployeeCode ?? string.Empty,
                RequesterName = v.EmployeeName ?? string.Empty,
                DeptCode = v.DeptCode ?? string.Empty,
                DeptName = v.DeptName ?? string.Empty,
                WorkYear = v.OTDate.Year,
                OTTypeCode = v.OTTypeCode,
                OtPurpose = v.OTReasonSummary ?? string.Empty,
                ApprovalSteps = new List<ApprovalStepDto>(),
                Attachments = new List<AttachmentDto>(),
                Details = new List<OTEmployeeDto>()
            };
        }

        public static OTEmployeeDto ToEmployeeDto(F03OTEmployee e) => new()
        {
            EmployeeCode = e.EmployeeCode ?? string.Empty,
            EmployeeName = e.EmployeeName ?? string.Empty,
            DeptCode = e.DeptCode ?? string.Empty,
            DeptName = e.DeptName ?? string.Empty,
            CvCode = e.CvCode ?? string.Empty,
            OTTypeCode = e.OTTypeCode ?? string.Empty,
            StartTime = e.StartTime?.TimeOfDay ?? TimeSpan.Zero,
            EndTime = e.EndTime?.TimeOfDay ?? TimeSpan.Zero,
            OTHours = e.OTHours,
            OTRateMultiplier = e.OTRateMultiplier,
            OTReasonCategoryCode = e.OTReasonCategoryCode ?? string.Empty,
            OTReasonDetail = e.OTReasonDetail ?? string.Empty,
            Note = e.Note ?? string.Empty,
            ValidationStatus = e.ValidationStatus ?? string.Empty,
            ValidationMessage = e.ValidationMessage ?? string.Empty
        };

        public static PendingApprovalItemDto ToPendingItem(OTRequestDto dto, bool canApprove)
        {
            return new PendingApprovalItemDto
            {
                RequestId = dto.Id,
                Kind = RequestModule.Overtime,
                EmployeeCode = dto.RequesterCode,
                EmployeeName = dto.RequesterName,
                DeptCode = dto.DeptCode,
                DeptName = dto.DeptName,
                FromDate = dto.OTDate,
                ToDate = dto.OTDate,
                TotalUnits = dto.TotalOtHours,
                ApprovalSteps = dto.ApprovalSteps,
                CanApprove = canApprove
            };
        }

        // ĐÃ XÓA theo sơ đồ D3 (dead code, thay thế bởi OTReconciliationItemDto):
        //   ToUnconfirmedDto(OTUnconfirmedResult)
        //   ParseCheckStatus(string) -> OtHoursCheckStatus
        //   ParseSituation(string) -> OtSyncSituation
        // Nếu cần logic map tương đương cho Reconciliation, viết trong
        // OTReconciliationMapper riêng (Infrastructure/Mappers), dùng đúng
        // OTReconciliationStatus/OTHourValidationStatus đã chốt ở D3.
    }
}
