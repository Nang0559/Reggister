using FVN_REGISTER.Contract.Dtos.Equipment;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Shared.Handlers;
using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Shared.Services.Equipment;

public sealed class EquipmentClientService : IEquipmentClientService
{
    private readonly IHttpClientWithAuth _http; private readonly ILogger<EquipmentClientService> _logger;
    public EquipmentClientService(IHttpClientWithAuth http, ILogger<EquipmentClientService> logger) { _http = http; _logger = logger; }
    public Task<ApiResponse<bool>> HasAccessAsync(CancellationToken ct = default) => Get<bool>("api/equipment/access", "access", ct);
    public Task<ApiResponse<bool>> HasImportAccessAsync(CancellationToken ct = default) => Get<bool>("api/equipment/import/access", "import access", ct);
    public Task<ApiResponse<List<EquipmentApproverDto>>> GetApproversAsync(string deptCode, CancellationToken ct = default) => Get<List<EquipmentApproverDto>>($"api/equipment/approvers?deptCode={Uri.EscapeDataString(deptCode)}", "approvers", ct);
    public Task<ApiResponse<EquipmentRequestDto>> CreateRegistrationAsync(CreateEquipmentRegistrationDto request, CancellationToken ct = default) => Post<EquipmentRequestDto>("api/equipment/registrations", request, "create registration", ct);
    public Task<ApiResponse<EquipmentRequestDto>> SubmitRegistrationAsync(int id, CancellationToken ct = default) => Post<EquipmentRequestDto>($"api/equipment/registrations/{id}/submit", new { }, "submit registration", ct);
    public Task<ApiResponse<List<EquipmentRequestDto>>> GetMineAsync(CancellationToken ct = default) => Get<List<EquipmentRequestDto>>("api/equipment/registrations/mine", "mine", ct);
    public Task<ApiResponse<EquipmentRequestDto>> CreateRepairAsync(CreateEquipmentRepairDto request, CancellationToken ct = default) => Post<EquipmentRequestDto>("api/equipment/repairs", request, "create repair", ct);
    public Task<ApiResponse<EquipmentRequestDto>> SubmitRepairAsync(int id, CancellationToken ct = default) => Post<EquipmentRequestDto>($"api/equipment/repairs/{id}/submit", new { }, "submit repair", ct);
    public Task<ApiResponse<EquipmentAssetDto>> ScanAsync(string token, CancellationToken ct = default) => Get<EquipmentAssetDto>($"api/equipment/scan/{Uri.EscapeDataString(token)}", "scan", ct);
    public Task<ApiResponse<EquipmentAssetDto>> GetAssetAsync(int id, CancellationToken ct = default) => Get<EquipmentAssetDto>($"api/equipment/assets/{id}", "asset", ct);
    private async Task<ApiResponse<T>> Get<T>(string url, string op, CancellationToken ct) { try { return await _http.GetAsync<T>(url, ct); } catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; } catch (Exception ex) { _logger.LogError(ex, "[EQUIPMENT_CLIENT] {Op}", op); return ApiResponse<T>.Fail("Không thể thực hiện thao tác thiết bị."); } }
    private async Task<ApiResponse<T>> Post<T>(string url, object body, string op, CancellationToken ct) { try { return await _http.PostAsync<T>(url, body, ct); } catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; } catch (Exception ex) { _logger.LogError(ex, "[EQUIPMENT_CLIENT] {Op}", op); return ApiResponse<T>.Fail("Không thể thực hiện thao tác thiết bị."); } }
}
