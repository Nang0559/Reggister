using FVN_REGISTER.Contract.Dtos.Equipment;
using FVN_REGISTER.Contract.Utils;

namespace FVN_REGISTER.Application.Interfaces.Equipment;

public interface IEquipmentService
{
    Task<ServiceResult<bool>> HasModuleAccessAsync(CancellationToken ct = default);
    Task<ServiceResult<List<EquipmentApproverDto>>> GetApproversAsync(string deptCode, CancellationToken ct = default);
    Task<ServiceResult<EquipmentRequestDto>> CreateRegistrationDraftAsync(CreateEquipmentRegistrationDto request, CancellationToken ct = default);
    Task<ServiceResult<EquipmentRequestDto>> SubmitRegistrationAsync(int requestId, CancellationToken ct = default);
    Task<ServiceResult<List<EquipmentRequestDto>>> GetMineAsync(CancellationToken ct = default);
    Task<ServiceResult<EquipmentRequestDto>> CreateRepairDraftAsync(CreateEquipmentRepairDto request, CancellationToken ct = default);
    Task<ServiceResult<EquipmentRequestDto>> SubmitRepairAsync(int requestId, CancellationToken ct = default);
    Task<ServiceResult<EquipmentAssetDto>> ScanAsync(string qrToken, CancellationToken ct = default);
    Task<ServiceResult<EquipmentAssetDto>> GetAssetAsync(int assetId, CancellationToken ct = default);
}
