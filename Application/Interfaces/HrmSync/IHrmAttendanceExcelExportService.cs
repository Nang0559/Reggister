using FVN_REGISTER.Contract.Utils;

namespace FVN_REGISTER.Application.Interfaces.HrmSync;

public interface IHrmAttendanceExcelExportService
{
    Task<ServiceResult<byte[]>> ExportAttendanceAsync(Guid batchId, CancellationToken ct = default);
    Task<ServiceResult<byte[]>> ExportOtAsync(Guid batchId, CancellationToken ct = default);
}