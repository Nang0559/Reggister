using FVN_REGISTER.Contract.Dtos.Equipment;

namespace FVN_REGISTER.Application.Interfaces.Equipment;

public interface IEquipmentService
{
    Task<bool> HasModuleAccessAsync(CancellationToken ct = default);
    Task<List<EquipmentApproverDto>> GetApproversAsync(string deptCode, CancellationToken ct = default);
    Task<EquipmentRequestDto> CreateRegistrationDraftAsync(CreateEquipmentRegistrationDto request, CancellationToken ct = default);
    Task<EquipmentRequestDto> SubmitRegistrationAsync(int requestId, CancellationToken ct = default);
    Task<List<EquipmentRequestDto>> GetMineAsync(CancellationToken ct = default);
    Task<EquipmentRequestDto> CreateRepairDraftAsync(CreateEquipmentRepairDto request, CancellationToken ct = default);
    Task<EquipmentRequestDto> SubmitRepairAsync(int requestId, CancellationToken ct = default);
    Task<EquipmentAssetDto> ScanAsync(string qrToken, CancellationToken ct = default);
    Task<EquipmentAssetDto?> GetAssetAsync(int assetId, CancellationToken ct = default);
}
