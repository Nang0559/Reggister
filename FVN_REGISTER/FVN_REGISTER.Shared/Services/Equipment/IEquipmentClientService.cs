using FVN_REGISTER.Contract.Dtos.Equipment;
using FVN_REGISTER.Contract.Responses;

namespace FVN_REGISTER.Shared.Services.Equipment;

public interface IEquipmentClientService
{
    Task<ApiResponse<bool>> HasAccessAsync(CancellationToken ct = default);
    Task<ApiResponse<bool>> HasImportAccessAsync(CancellationToken ct = default);
    Task<ApiResponse<List<EquipmentApproverDto>>> GetApproversAsync(string deptCode, CancellationToken ct = default);
    Task<ApiResponse<EquipmentRequestDto>> CreateRegistrationAsync(CreateEquipmentRegistrationDto request, CancellationToken ct = default);
    Task<ApiResponse<EquipmentRequestDto>> SubmitRegistrationAsync(int id, CancellationToken ct = default);
    Task<ApiResponse<List<EquipmentRequestDto>>> GetMineAsync(CancellationToken ct = default);
    Task<ApiResponse<EquipmentRequestDto>> CreateRepairAsync(CreateEquipmentRepairDto request, CancellationToken ct = default);
    Task<ApiResponse<EquipmentRequestDto>> SubmitRepairAsync(int id, CancellationToken ct = default);
    Task<ApiResponse<EquipmentAssetDto>> ScanAsync(string token, CancellationToken ct = default);
    Task<ApiResponse<EquipmentAssetDto>> GetAssetAsync(int id, CancellationToken ct = default);
}
