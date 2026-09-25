using FVN_REGISTER.Contract.Dtos.EquipmentImport;

namespace FVN_REGISTER.Application.Interfaces.Equipment;

public interface IEquipmentImportService
{
    Task<List<EquipmentFieldDefinitionDto>> GetFieldDefinitionsAsync(string deptCode, CancellationToken ct = default);
    Task<EquipmentFieldDefinitionDto> SaveFieldDefinitionAsync(SaveEquipmentFieldDefinitionRequest request, CancellationToken ct = default);
    Task<EquipmentImportBatchDto> StageExcelAsync(string deptCode, string fileName, Stream content, bool assignToEmployee = false, CancellationToken ct = default);
    Task<EquipmentImportBatchDto?> GetBatchAsync(int batchId, CancellationToken ct = default);
    Task<EquipmentImportCommitResultDto> CommitAsync(int batchId, CancellationToken ct = default);
}
