using FVN_REGISTER.Application.Interfaces.OT;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.OT;
using FVN_REGISTER.Core.Extensions;
using FVN_REGISTER.Core.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;


namespace FVN_REGISTER.Infrastructure.Services.OT
{
    public class OTAttendanceReconciliationService
        : BaseService<OTAttendanceReconciliationService>, IOTAttendanceReconciliationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IOTAttendanceStagingService _staging;

        public OTAttendanceReconciliationService(
            IUnitOfWork uow,
            IOTAttendanceStagingService staging,
            ILogger<OTAttendanceReconciliationService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _uow = uow;
            _staging = staging;
        }

        public async Task<OTReconciliationResultDto> ReconcileActualHoursAsync(
            DateTime date, string? deptCode, CancellationToken ct = default)
        {
            var result = new OTReconciliationResultDto
            {
                WorkDate = date.Date,
                DeptCode = deptCode,
                ReconciledAt = DateTime.Now
            };

            try
            {
                Logger.LogInformation("[OT_RECONCILE] Bắt đầu ngày {Date} dept {Dept}",
                    date.Date, deptCode ?? "ALL");

                var isReady = await _staging.IsStagingReadyAsync(date, ct);
                if (!isReady)
                {
                    Logger.LogWarning("[OT_RECONCILE] Staging chưa có ngày {Date}, tiến hành sync...", date.Date);
                    await _staging.SyncAttendanceStagingAsync(date, ct);
                }

                var dateParam = new SqlParameter("@OTDate", SqlDbType.Date) { Value = date.Date };
                var deptParam = new SqlParameter("@DeptCode", SqlDbType.NVarChar, 30)
                {
                    Value = string.IsNullOrEmpty(deptCode) ? (object)DBNull.Value : deptCode
                };

                _uow.SetCommandTimeout(30);

                // ⚠️ Proc này VỪA UPDATE F03OTEmployee.ActualHours VỪA SELECT báo cáo —
                // đây là điểm ghi dữ liệu duy nhất của toàn bộ method này.
                var rawRows = await _uow.SqlQueryRawAsync<OTReconciliationRawRow>(
                    "EXEC dbo.usp_SyncOTActualHours @OTDate, @DeptCode",
                    ct, dateParam, deptParam);

                result.Items = rawRows.Select(MapToItem).ToList();
                result.TotalRows = result.Items.Count;
                result.ProcessedCount = result.Items.Count(x => x.Status == OTReconciliationStatus.Processed);
                result.PendingConfirmCount = result.Items.Count(x => x.Status == OTReconciliationStatus.PendingConfirm);
                result.PendingApprovalCount = result.Items.Count(x => x.Status == OTReconciliationStatus.RequestNotApproved);
                result.NoRequestCount = result.Items.Count(x => x.Status == OTReconciliationStatus.NoRequest);
                result.ValidCount = result.Items.Count(x => x.ValidationStatus == OTHourValidationStatus.Valid);
                result.WarningCount = result.Items.Count(x => x.ValidationStatus == OTHourValidationStatus.Warning);
                result.IsSuccess = true;

                Logger.LogInformation("[OT_RECONCILE] Xong: {Summary}", result.Summary);
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("[OT_RECONCILE] Bị cancel");
                result.IsSuccess = false;
                result.ErrorMessage = "Tác vụ bị huỷ.";
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[OT_RECONCILE] Lỗi ngày {Date}", date.Date);
                result.IsSuccess = false;
                result.ErrorMessage = Debug ? $"Lỗi: {ex.Message}" : "Lỗi hệ thống khi đối chiếu giờ OT.";
            }

            return result;
        }

        private static OTReconciliationItemDto MapToItem(OTReconciliationRawRow r)
        {
            r.TinhHuong.TryToOTReconciliationStatus(out var status);
            r.ValidationStatus.TryToOTHourValidationStatus(out var validation);

            return new OTReconciliationItemDto
            {
                WorkDate = r.WorkDate,
                EmployeeCode = r.EmployeeCode,
                FullName = r.FullName,
                DeptCode = r.DeptCode,
                DeptName = r.DeptName,
                CheckInText = r.CheckInText,
                CheckOutText = r.CheckOutText,
                OTHoursActual = r.OTHoursActual,
                OTHoursPlanned = r.OTHoursPlanned,
                OTHoursRequest = r.OTHoursRequest,
                IsHoliday = r.IsHoliday,
                HolidayType = r.HolidayType,
                ShiftType = r.ShiftType,
                ShiftCode = r.ShiftCode,
                ShiftName = r.ShiftName,
                OTRequestId = r.OTRequestId,
                OTCode = r.OTCode,
                RequestStatus = r.RequestStatus,
                ValidationStatus = validation,
                ValidationMessage = r.ValidationMessage,
                Status = status
            };
        }

        /// <summary>Raw row khớp 1-1 với SELECT của usp_SyncOTActualHours — chỉ dùng nội bộ để map sang DTO enum-hóa.</summary>
        private class OTReconciliationRawRow
        {
            public DateTime WorkDate { get; set; }
            public string FullName { get; set; } = string.Empty;
            public string EmployeeCode { get; set; } = string.Empty;
            public string DeptCode { get; set; } = string.Empty;
            public string DeptName { get; set; } = string.Empty;
            public string? CheckInText { get; set; }
            public string? CheckOutText { get; set; }
            public decimal OTHoursActual { get; set; }
            public decimal OTHoursPlanned { get; set; }
            public decimal OTHoursRequest { get; set; }
            public bool IsHoliday { get; set; }
            public string? HolidayType { get; set; }
            public string? ShiftType { get; set; }
            public string? ShiftCode { get; set; }
            public string? ShiftName { get; set; }
            public int? OTRequestId { get; set; }
            public string? OTCode { get; set; }
            public string? RequestStatus { get; set; }
            public string ValidationStatus { get; set; } = string.Empty;
            public string ValidationMessage { get; set; } = string.Empty;
            public string TinhHuong { get; set; } = string.Empty;
        }
    }
}
