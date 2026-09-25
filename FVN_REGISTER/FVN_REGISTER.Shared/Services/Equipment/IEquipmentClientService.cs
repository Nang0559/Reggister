using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Dtos.Equipment;
using FVN_REGISTER.Contract.Dtos.EquipmentImport;
using FVN_REGISTER.Contract.Responses;
using Microsoft.AspNetCore.Components.Forms;

namespace FVN_REGISTER.Shared.Services.Equipment;

public interface IEquipmentClientService
{
    Task<ApiResponse<List<DepartmentDto>>> GetEquipmentDepartmentsAsync(CancellationToken ct = default);
    Task<ApiResponse<EquipmentActionAccessDto>> GetActionsAsync(CancellationToken ct = default);
    Task<ApiResponse<bool>> HasAccessAsync(CancellationToken ct = default);
    Task<ApiResponse<bool>> HasImportAccessAsync(CancellationToken ct = default);
    Task<ApiResponse<List<EquipmentHandoverEmployeeOptionDto>>> GetAssignmentEmployeesAsync(string? deptCode = null, CancellationToken ct = default);
    Task<ApiResponse<List<EquipmentApproverDto>>> GetApproversAsync(string deptCode, CancellationToken ct = default);
    Task<ApiResponse<EquipmentRequestDto>> CreateRegistrationAsync(CreateEquipmentRegistrationDto request, CancellationToken ct = default);
    Task<ApiResponse<EquipmentRequestDto>> SubmitRegistrationAsync(int id, CancellationToken ct = default);
    Task<ApiResponse<List<EquipmentRequestDto>>> GetMineAsync(CancellationToken ct = default);
    Task<ApiResponse<List<EquipmentAssetDto>>> GetMyAssignedAssetsAsync(CancellationToken ct = default);
    Task<ApiResponse<EquipmentRequestDto>> CreateRepairAsync(CreateEquipmentRepairDto request, CancellationToken ct = default);
    Task<ApiResponse<List<EquipmentHandoverEmployeeOptionDto>>> GetRepairAssigneesAsync(string? deptCode = null, CancellationToken ct = default);
    Task<ApiResponse<EquipmentRequestDto>> SubmitRepairAsync(int id, CancellationToken ct = default);
    Task<ApiResponse<EquipmentRequestDto>> CompleteRepairAsync(int id, string feedback, CancellationToken ct = default);
    Task<ApiResponse<EquipmentAssetDto>> ScanAsync(string token, CancellationToken ct = default);
    Task<ApiResponse<EquipmentAssetDto>> GetAssetAsync(int id, CancellationToken ct = default);
    Task<ApiResponse<List<EquipmentHandoverEmployeeOptionDto>>> GetHandoverEmployeesAsync(string? deptCode = null, CancellationToken ct = default);
    Task<ApiResponse<List<EquipmentHandoverCandidateDto>>> GetHandoverCandidatesAsync(string oldEmployeeCode, string? deptCode = null, CancellationToken ct = default);
    Task<ApiResponse<EquipmentHandoverResultDto>> HandoverAsync(EquipmentHandoverRequest request, CancellationToken ct = default);
    Task<ApiResponse<List<EquipmentReportRowDto>>> GetReportAsync(EquipmentReportFilterDto filter, CancellationToken ct = default);
    Task<ApiResponse<byte[]>> ExportReportAsync(EquipmentReportFilterDto filter, CancellationToken ct = default);
    Task<ApiResponse<List<EquipmentSchemaSummaryDto>>> GetSchemasAsync(string? deptCode = null, CancellationToken ct = default);
    Task<ApiResponse<EquipmentSchemaDto>> GetSchemaAsync(int schemaId, CancellationToken ct = default);
    Task<ApiResponse<EquipmentSchemaDto>> SaveSchemaAsync(EquipmentSchemaUpsertRequest request, CancellationToken ct = default);
    Task<ApiResponse<EquipmentSchemaDto>> CloneSchemaAsync(int schemaId, EquipmentSchemaCloneRequest request, CancellationToken ct = default);
    Task<ApiResponse<EquipmentFieldDefinitionDto>> SaveSchemaFieldAsync(SaveEquipmentFieldDefinitionRequest request, CancellationToken ct = default);
    Task<ApiResponse<EquipmentImportBatchDto>> StageImportAsync(string deptCode, IBrowserFile file, bool assignToEmployee = false, CancellationToken ct = default);
    Task<ApiResponse<EquipmentImportCommitResultDto>> CommitImportAsync(int batchId, CancellationToken ct = default);
    Task<ApiResponse<List<EquipmentAssetDto>>> GetRegisteredInspectionAssetsAsync(string? deptCode = null, CancellationToken ct = default);
    Task<ApiResponse<List<EquipmentInspectionTemplateDto>>> GetInspectionTemplatesAsync(string? deptCode = null, CancellationToken ct = default);
    Task<ApiResponse<EquipmentInspectionTemplateDto>> GetInspectionTemplateAsync(int id, CancellationToken ct = default);
    Task<ApiResponse<EquipmentInspectionTemplateImportResultDto>> ImportInspectionTemplateExcelAsync(IBrowserFile file, CancellationToken ct = default);
    Task<ApiResponse<EquipmentInspectionTemplateDto>> SaveInspectionTemplateAsync(EquipmentInspectionTemplateUpsertRequest request, CancellationToken ct = default);
    Task<ApiResponse<EquipmentInspectionTemplateDto>> CloneInspectionTemplateAsync(int id, string? name = null, CancellationToken ct = default);
    Task<ApiResponse<List<EquipmentInspectionAssignmentDto>>> GetInspectionAssignmentsAsync(int? equipmentId = null, CancellationToken ct = default);
    Task<ApiResponse<EquipmentInspectionAssignmentDto>> AssignInspectionAsync(EquipmentInspectionAssignmentRequest request, CancellationToken ct = default);
    Task<ApiResponse<List<EquipmentInspectionTaskDto>>> GetInspectionTasksAsync(bool includeCompleted = false, CancellationToken ct = default);
    Task<ApiResponse<EquipmentInspectionTaskDto>> GetInspectionTaskAsync(int id, CancellationToken ct = default);
    Task<ApiResponse<EquipmentInspectionTaskDto>> SubmitInspectionAsync(EquipmentInspectionSubmitRequest request, CancellationToken ct = default);
    Task<ApiResponse<EquipmentInspectionTaskDto>> ApproveInspectionAsync(int id, CancellationToken ct = default);
    Task<ApiResponse<EquipmentInspectionTaskDto>> RejectInspectionAsync(int id, string reason, CancellationToken ct = default);
    Task<ApiResponse<EquipmentInspectionDashboardDto>> GetInspectionDashboardAsync(DateTime from, DateTime to, string? deptCode = null, CancellationToken ct = default);
    Task<ApiResponse<EquipmentInspectionEvidenceDto>> UploadInspectionEvidenceAsync(int taskId, int itemId, IBrowserFile file, CancellationToken ct = default);
}
