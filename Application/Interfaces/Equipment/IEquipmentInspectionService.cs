using FVN_REGISTER.Contract.Dtos.Equipment;
using FVN_REGISTER.Contract.Utils;

namespace FVN_REGISTER.Application.Interfaces.Equipment;

public interface IEquipmentInspectionService
{
    Task<ServiceResult<List<EquipmentAssetDto>>> GetRegisteredAssetsAsync(string? deptCode, CancellationToken ct = default);
    Task<ServiceResult<List<EquipmentInspectionTemplateDto>>> GetTemplatesAsync(string? deptCode, CancellationToken ct = default);
    Task<ServiceResult<EquipmentInspectionTemplateDto>> GetTemplateAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<EquipmentInspectionTemplateImportResultDto>> ImportTemplateExcelAsync(string fileName, Stream content, CancellationToken ct = default);
    Task<ServiceResult<EquipmentInspectionTemplateDto>> SaveTemplateAsync(EquipmentInspectionTemplateUpsertRequest request, CancellationToken ct = default);
    Task<ServiceResult<EquipmentInspectionTemplateDto>> CloneTemplateAsync(int id, string? name, CancellationToken ct = default);
    Task<ServiceResult<EquipmentInspectionAssignmentDto>> AssignAsync(EquipmentInspectionAssignmentRequest request, CancellationToken ct = default);
    Task<ServiceResult<List<EquipmentInspectionAssignmentDto>>> GetAssignmentsAsync(int? equipmentId, CancellationToken ct = default);
    Task<ServiceResult<List<EquipmentInspectionTaskDto>>> GetMyTasksAsync(bool includeCompleted, CancellationToken ct = default);
    Task<ServiceResult<EquipmentInspectionTaskDto>> GetTaskAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<EquipmentInspectionTaskDto>> SubmitAsync(EquipmentInspectionSubmitRequest request, CancellationToken ct = default);
    Task<ServiceResult<EquipmentInspectionTaskDto>> ApproveAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<EquipmentInspectionTaskDto>> RejectAsync(int id, string reason, CancellationToken ct = default);
    Task<ServiceResult<EquipmentInspectionDashboardDto>> DashboardAsync(DateTime from, DateTime to, string? deptCode, CancellationToken ct = default);
    Task<ServiceResult<EquipmentInspectionEvidenceDto>> AddEvidenceAsync(int taskId, int? itemResultId, int? itemId, string fileName, string contentType, Stream content, CancellationToken ct = default);
    Task<ServiceResult<(string ContentType, Stream Content, string FileName)>> OpenEvidenceAsync(int evidenceId, CancellationToken ct = default);
    Task GenerateScheduledTasksAsync(CancellationToken ct = default);
}