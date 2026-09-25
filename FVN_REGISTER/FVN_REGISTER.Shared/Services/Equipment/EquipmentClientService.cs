using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Contract.Dtos.Equipment;
using FVN_REGISTER.Contract.Dtos.EquipmentImport;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace FVN_REGISTER.Shared.Services.Equipment;

public sealed class EquipmentClientService : IEquipmentClientService
{
    private readonly IHttpClientWithAuth _http;
    private readonly ILogger<EquipmentClientService> _logger;
    public EquipmentClientService(IHttpClientWithAuth http, ILogger<EquipmentClientService> logger) { _http = http; _logger = logger; }
    public Task<ApiResponse<List<DepartmentDto>>> GetEquipmentDepartmentsAsync(CancellationToken ct = default) => Get<List<DepartmentDto>>("api/equipment/schemas/departments", "departments", ct);
    public Task<ApiResponse<EquipmentActionAccessDto>> GetActionsAsync(CancellationToken ct = default) => Get<EquipmentActionAccessDto>("api/equipment/actions", "actions", ct);
    public Task<ApiResponse<bool>> HasAccessAsync(CancellationToken ct = default) => Get<bool>("api/equipment/access", "access", ct);
    public Task<ApiResponse<bool>> HasImportAccessAsync(CancellationToken ct = default) => Get<bool>("api/equipment/import/access", "import access", ct);
    public Task<ApiResponse<List<EquipmentHandoverEmployeeOptionDto>>> GetAssignmentEmployeesAsync(string? deptCode = null, CancellationToken ct = default) => Get<List<EquipmentHandoverEmployeeOptionDto>>($"api/equipment/assignment-employees?deptCode={Uri.EscapeDataString(deptCode ?? string.Empty)}", "assignment employees", ct);
    public Task<ApiResponse<List<EquipmentApproverDto>>> GetApproversAsync(string deptCode, CancellationToken ct = default) => Get<List<EquipmentApproverDto>>($"api/equipment/approvers?deptCode={Uri.EscapeDataString(deptCode)}", "approvers", ct);
    public Task<ApiResponse<EquipmentRequestDto>> CreateRegistrationAsync(CreateEquipmentRegistrationDto request, CancellationToken ct = default) => Post<EquipmentRequestDto>("api/equipment/registrations", request, "create registration", ct);
    public Task<ApiResponse<EquipmentRequestDto>> SubmitRegistrationAsync(int id, CancellationToken ct = default) => Post<EquipmentRequestDto>($"api/equipment/registrations/{id}/submit", new { }, "submit registration", ct);
    public Task<ApiResponse<List<EquipmentAssetDto>>> GetMyAssignedAssetsAsync(CancellationToken ct = default) => Get<List<EquipmentAssetDto>>("api/equipment/assigned-to-me", "assigned equipment", ct);
    public Task<ApiResponse<List<EquipmentRequestDto>>> GetMineAsync(CancellationToken ct = default) => Get<List<EquipmentRequestDto>>("api/equipment/registrations/mine", "mine", ct);
    public Task<ApiResponse<EquipmentRequestDto>> CreateRepairAsync(CreateEquipmentRepairDto request, CancellationToken ct = default) => Post<EquipmentRequestDto>("api/equipment/repairs", request, "create repair", ct);
    public Task<ApiResponse<List<EquipmentHandoverEmployeeOptionDto>>> GetRepairAssigneesAsync(string? deptCode = null, CancellationToken ct = default) => Get<List<EquipmentHandoverEmployeeOptionDto>>($"api/equipment/repairs/assignees?deptCode={Uri.EscapeDataString(deptCode ?? string.Empty)}", "repair assignees", ct);
    public Task<ApiResponse<EquipmentRequestDto>> SubmitRepairAsync(int id, CancellationToken ct = default) => Post<EquipmentRequestDto>($"api/equipment/repairs/{id}/submit", new { }, "submit repair", ct);
    public Task<ApiResponse<EquipmentRequestDto>> CompleteRepairAsync(int id, string feedback, CancellationToken ct = default) => Post<EquipmentRequestDto>($"api/equipment/repairs/{id}/complete", feedback, "complete repair", ct);
    public Task<ApiResponse<EquipmentAssetDto>> ScanAsync(string token, CancellationToken ct = default) => Get<EquipmentAssetDto>($"api/equipment/scan/{Uri.EscapeDataString(token)}", "scan", ct);
    public Task<ApiResponse<EquipmentAssetDto>> GetAssetAsync(int id, CancellationToken ct = default) => Get<EquipmentAssetDto>($"api/equipment/assets/{id}", "asset", ct);
    public Task<ApiResponse<List<EquipmentHandoverEmployeeOptionDto>>> GetHandoverEmployeesAsync(string? deptCode = null, CancellationToken ct = default) => Get<List<EquipmentHandoverEmployeeOptionDto>>($"api/equipment/handover/employees?deptCode={Uri.EscapeDataString(deptCode ?? string.Empty)}", "handover employees", ct);
    public Task<ApiResponse<List<EquipmentHandoverCandidateDto>>> GetHandoverCandidatesAsync(string oldEmployeeCode, string? deptCode = null, CancellationToken ct = default) => Get<List<EquipmentHandoverCandidateDto>>($"api/equipment/handover/candidates?oldEmployeeCode={Uri.EscapeDataString(oldEmployeeCode)}&deptCode={Uri.EscapeDataString(deptCode ?? string.Empty)}", "handover candidates", ct);
    public Task<ApiResponse<EquipmentHandoverResultDto>> HandoverAsync(EquipmentHandoverRequest request, CancellationToken ct = default) => Post<EquipmentHandoverResultDto>("api/equipment/handover", request, "equipment handover", ct);
    public Task<ApiResponse<List<EquipmentReportRowDto>>> GetReportAsync(EquipmentReportFilterDto filter, CancellationToken ct = default) => Get<List<EquipmentReportRowDto>>($"api/equipment/reports?DeptCode={Uri.EscapeDataString(filter.DeptCode ?? string.Empty)}&ResponsibleEmployeeCode={Uri.EscapeDataString(filter.ResponsibleEmployeeCode ?? string.Empty)}&RepairResponsibleDeptCode={Uri.EscapeDataString(filter.RepairResponsibleDeptCode ?? string.Empty)}&From={filter.From:O}&To={filter.To:O}", "equipment report", ct);
    public Task<ApiResponse<byte[]>> ExportReportAsync(EquipmentReportFilterDto filter, CancellationToken ct = default) => _http.GetFileAsync($"api/equipment/reports/export?DeptCode={Uri.EscapeDataString(filter.DeptCode ?? string.Empty)}&ResponsibleEmployeeCode={Uri.EscapeDataString(filter.ResponsibleEmployeeCode ?? string.Empty)}&RepairResponsibleDeptCode={Uri.EscapeDataString(filter.RepairResponsibleDeptCode ?? string.Empty)}&From={filter.From:O}&To={filter.To:O}", ct);
    public Task<ApiResponse<List<EquipmentSchemaSummaryDto>>> GetSchemasAsync(string? deptCode = null, CancellationToken ct = default) => Get<List<EquipmentSchemaSummaryDto>>(string.IsNullOrWhiteSpace(deptCode) ? "api/equipment/schemas" : $"api/equipment/schemas?deptCode={Uri.EscapeDataString(deptCode)}", "schemas", ct);
    public Task<ApiResponse<EquipmentSchemaDto>> GetSchemaAsync(int schemaId, CancellationToken ct = default) => Get<EquipmentSchemaDto>($"api/equipment/schemas/{schemaId}", "schema", ct);
    public Task<ApiResponse<EquipmentSchemaDto>> SaveSchemaAsync(EquipmentSchemaUpsertRequest request, CancellationToken ct = default) => Post<EquipmentSchemaDto>("api/equipment/schemas", request, "save schema", ct);
    public Task<ApiResponse<EquipmentSchemaDto>> CloneSchemaAsync(int schemaId, EquipmentSchemaCloneRequest request, CancellationToken ct = default) => Post<EquipmentSchemaDto>($"api/equipment/schemas/{schemaId}/clone", request, "clone schema", ct);
    public Task<ApiResponse<EquipmentFieldDefinitionDto>> SaveSchemaFieldAsync(SaveEquipmentFieldDefinitionRequest request, CancellationToken ct = default) => Put<EquipmentFieldDefinitionDto>("api/equipment/schemas/fields", request, "save schema field", ct);
    public Task<ApiResponse<EquipmentImportCommitResultDto>> CommitImportAsync(int batchId, CancellationToken ct = default) => Post<EquipmentImportCommitResultDto>($"api/equipment/import/{batchId}/commit", new { }, "commit import", ct);

    public Task<ApiResponse<List<EquipmentAssetDto>>> GetRegisteredInspectionAssetsAsync(string? deptCode = null, CancellationToken ct = default) => Get<List<EquipmentAssetDto>>(string.IsNullOrWhiteSpace(deptCode) ? "api/equipment-inspections/registered-assets" : $"api/equipment-inspections/registered-assets?deptCode={Uri.EscapeDataString(deptCode)}", "registered inspection assets", ct);
    public Task<ApiResponse<List<EquipmentInspectionTemplateDto>>> GetInspectionTemplatesAsync(string? deptCode = null, CancellationToken ct = default) => Get<List<EquipmentInspectionTemplateDto>>(string.IsNullOrWhiteSpace(deptCode) ? "api/equipment-inspections/templates" : $"api/equipment-inspections/templates?deptCode={Uri.EscapeDataString(deptCode)}", "inspection templates", ct);
    public Task<ApiResponse<EquipmentInspectionTemplateDto>> GetInspectionTemplateAsync(int id, CancellationToken ct = default) => Get<EquipmentInspectionTemplateDto>($"api/equipment-inspections/templates/{id}", "inspection template", ct);
    public async Task<ApiResponse<EquipmentInspectionTemplateImportResultDto>> ImportInspectionTemplateExcelAsync(IBrowserFile file, CancellationToken ct = default)
    {
        try { await using var stream=file.OpenReadStream(10*1024*1024,ct); using var form=new MultipartFormDataContent(); using var fc=new StreamContent(stream); fc.Headers.ContentType=new MediaTypeHeaderValue("application/octet-stream"); form.Add(fc,"file",file.Name); return await _http.PostMultipartAsync<EquipmentInspectionTemplateImportResultDto>("api/equipment-inspections/templates/import",form,ct); }
        catch(OperationCanceledException) when(ct.IsCancellationRequested){throw;} catch(Exception ex){_logger.LogError(ex,"[EQUIPMENT_CLIENT] checklist import");return ApiResponse<EquipmentInspectionTemplateImportResultDto>.Fail("Không thể import checklist Excel.");}
    }
    public Task<ApiResponse<EquipmentInspectionTemplateDto>> SaveInspectionTemplateAsync(EquipmentInspectionTemplateUpsertRequest request, CancellationToken ct = default) => Post<EquipmentInspectionTemplateDto>("api/equipment-inspections/templates", request, "save inspection template", ct);
    public Task<ApiResponse<EquipmentInspectionTemplateDto>> CloneInspectionTemplateAsync(int id, string? name = null, CancellationToken ct = default) => Post<EquipmentInspectionTemplateDto>($"api/equipment-inspections/templates/{id}/clone", new EquipmentInspectionTemplateCloneRequest { TemplateName = name }, "clone inspection template", ct);
    public Task<ApiResponse<List<EquipmentInspectionAssignmentDto>>> GetInspectionAssignmentsAsync(int? equipmentId = null, CancellationToken ct = default) => Get<List<EquipmentInspectionAssignmentDto>>(equipmentId.HasValue ? $"api/equipment-inspections/assignments?equipmentId={equipmentId}" : "api/equipment-inspections/assignments", "inspection assignments", ct);
    public Task<ApiResponse<EquipmentInspectionAssignmentDto>> AssignInspectionAsync(EquipmentInspectionAssignmentRequest request, CancellationToken ct = default) => Post<EquipmentInspectionAssignmentDto>("api/equipment-inspections/assignments", request, "assign inspection", ct);
    public Task<ApiResponse<List<EquipmentInspectionTaskDto>>> GetInspectionTasksAsync(bool includeCompleted = false, CancellationToken ct = default) => Get<List<EquipmentInspectionTaskDto>>($"api/equipment-inspections/tasks?includeCompleted={includeCompleted}", "inspection tasks", ct);
    public Task<ApiResponse<EquipmentInspectionTaskDto>> GetInspectionTaskAsync(int id, CancellationToken ct = default) => Get<EquipmentInspectionTaskDto>($"api/equipment-inspections/tasks/{id}", "inspection task", ct);
    public Task<ApiResponse<EquipmentInspectionTaskDto>> SubmitInspectionAsync(EquipmentInspectionSubmitRequest request, CancellationToken ct = default) => Post<EquipmentInspectionTaskDto>("api/equipment-inspections/tasks/submit", request, "submit inspection", ct);
    public Task<ApiResponse<EquipmentInspectionTaskDto>> ApproveInspectionAsync(int id, CancellationToken ct = default) => Post<EquipmentInspectionTaskDto>($"api/equipment-inspections/tasks/{id}/approve", new { }, "approve inspection", ct);
    public Task<ApiResponse<EquipmentInspectionTaskDto>> RejectInspectionAsync(int id, string reason, CancellationToken ct = default) => Post<EquipmentInspectionTaskDto>($"api/equipment-inspections/tasks/{id}/reject", reason, "reject inspection", ct);
    public Task<ApiResponse<EquipmentInspectionDashboardDto>> GetInspectionDashboardAsync(DateTime from, DateTime to, string? deptCode = null, CancellationToken ct = default) => Get<EquipmentInspectionDashboardDto>($"api/equipment-inspections/dashboard?from={from:O}&to={to:O}&deptCode={Uri.EscapeDataString(deptCode ?? string.Empty)}", "inspection dashboard", ct);

    public async Task<ApiResponse<EquipmentInspectionEvidenceDto>> UploadInspectionEvidenceAsync(int taskId, int itemId, IBrowserFile file, CancellationToken ct = default)
    {
        try
        {
            await using var stream = file.OpenReadStream(10 * 1024 * 1024, ct);
            using var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.Name);
            return await _http.PostMultipartAsync<EquipmentInspectionEvidenceDto>($"api/equipment-inspections/tasks/{taskId}/evidence?itemId={itemId}", content, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { _logger.LogError(ex, "[EQUIPMENT_CLIENT] inspection evidence"); return ApiResponse<EquipmentInspectionEvidenceDto>.Fail("Không thể tải hình ảnh kiểm tra."); }
    }

    public async Task<ApiResponse<EquipmentImportBatchDto>> StageImportAsync(string deptCode, IBrowserFile file, bool assignToEmployee = false, CancellationToken ct = default)
    {
        try
        {
            await using var stream = file.OpenReadStream(25 * 1024 * 1024, ct);
            using var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(fileContent, "file", file.Name);
            return await _http.PostMultipartAsync<EquipmentImportBatchDto>($"api/equipment/import?deptCode={Uri.EscapeDataString(deptCode.Trim().ToUpperInvariant())}&assignToEmployee={assignToEmployee}", content, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { _logger.LogError(ex, "[EQUIPMENT_CLIENT] stage import"); return ApiResponse<EquipmentImportBatchDto>.Fail("Không thể staging file Excel."); }
    }
    private async Task<ApiResponse<T>> Get<T>(string url, string op, CancellationToken ct)
    {
        try { return await _http.GetAsync<T>(url, ct); }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { _logger.LogError(ex, "[EQUIPMENT_CLIENT] {Op}", op); return ApiResponse<T>.Fail("Không thể thực hiện thao tác thiết bị."); }
    }
    private async Task<ApiResponse<T>> Post<T>(string url, object body, string op, CancellationToken ct)
    {
        try { return await _http.PostAsync<T>(url, body, ct); }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { _logger.LogError(ex, "[EQUIPMENT_CLIENT] {Op}", op); return ApiResponse<T>.Fail("Không thể thực hiện thao tác thiết bị."); }
    }
    private async Task<ApiResponse<T>> Put<T>(string url, object body, string op, CancellationToken ct)
    {
        try { return await _http.PutAsync<T>(url, body, ct); }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { _logger.LogError(ex, "[EQUIPMENT_CLIENT] {Op}", op); return ApiResponse<T>.Fail("Không thể thực hiện thao tác thiết bị."); }
    }
}
