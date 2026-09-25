using FVN_REGISTER.Contract.Dtos.Security;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Shared.Handlers;
using Microsoft.Extensions.Logging;

namespace FVN_REGISTER.Shared.Services.Security;

public sealed class AccessChangeClientService : IAccessChangeClientService
{
    private readonly IHttpClientWithAuth _http;
    private readonly ILogger<AccessChangeClientService> _logger;

    public AccessChangeClientService(IHttpClientWithAuth http, ILogger<AccessChangeClientService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public Task<ApiResponse<List<AccessChangeEmployeeOptionDto>>> GetEmployeesAsync(CancellationToken ct = default)
        => Get<List<AccessChangeEmployeeOptionDto>>("api/security/access-change/employees", "employees", ct);

    public Task<ApiResponse<List<AccessChangeFunctionOptionDto>>> GetFunctionOptionsAsync(RequestModule module, string oldEmployeeCode, CancellationToken ct = default)
        => Get<List<AccessChangeFunctionOptionDto>>($"api/security/access-change/functions?businessModule={module}&oldEmployeeCode={Uri.EscapeDataString(oldEmployeeCode)}", "functions", ct);

    public Task<ApiResponse<List<FVN_REGISTER.Contract.Dtos.Equipment.EquipmentHandoverCandidateDto>>> GetEquipmentCandidatesAsync(string oldEmployeeCode, CancellationToken ct = default)
        => Get<List<FVN_REGISTER.Contract.Dtos.Equipment.EquipmentHandoverCandidateDto>>(
            $"api/equipment/handover/candidates?oldEmployeeCode={Uri.EscapeDataString(oldEmployeeCode)}", "equipment candidates", ct);

    public Task<ApiResponse<AccessChangeRequestDto>> CreateAsync(AccessChangeRequestCreateDto request, CancellationToken ct = default)
        => Post<AccessChangeRequestDto>("api/security/access-change", request, "create", ct);

    public Task<ApiResponse<List<AccessChangeRequestDto>>> GetPendingAsync(CancellationToken ct = default)
        => Get<List<AccessChangeRequestDto>>("api/security/access-change/pending", "pending", ct);

    public Task<ApiResponse<List<AccessChangeRequestDto>>> GetMineAsync(CancellationToken ct = default)
        => Get<List<AccessChangeRequestDto>>("api/security/access-change/mine", "mine", ct);

    public Task<ApiResponse<AccessChangeRequestDto>> GetAsync(int id, CancellationToken ct = default)
        => Get<AccessChangeRequestDto>($"api/security/access-change/{id}", "detail", ct);

    public Task<ApiResponse<AccessChangeRequestDto>> ApproveAsync(int id, int level, string? comment, CancellationToken ct = default)
        => Post<AccessChangeRequestDto>($"api/security/access-change/{id}/approve?level={level}", comment ?? string.Empty, "approve", ct);

    public Task<ApiResponse<AccessChangeRequestDto>> RejectAsync(int id, int level, string comment, CancellationToken ct = default)
        => Post<AccessChangeRequestDto>($"api/security/access-change/{id}/reject?level={level}", comment, "reject", ct);

    public Task<ApiResponse<List<AccessChangeItQueueItemDto>>> GetItQueueAsync(CancellationToken ct = default)
        => Get<List<AccessChangeItQueueItemDto>>("api/security/access-change/it/queue", "it queue", ct);

    public Task<ApiResponse<AccessChangeRequestDto>> ExecuteByItAsync(int id, AccessChangeItExecuteDto request, CancellationToken ct = default)
        => Post<AccessChangeRequestDto>($"api/security/access-change/{id}/it-execute", request, "it execute", ct);

    private async Task<ApiResponse<T>> Get<T>(string url, string op, CancellationToken ct)
    {
        try { return await _http.GetAsync<T>(url, ct); }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { _logger.LogError(ex, "[ACCESS_CHANGE_CLIENT] {Op}", op); return ApiResponse<T>.Fail("Không thể tải dữ liệu thay đổi quyền."); }
    }

    private async Task<ApiResponse<T>> Post<T>(string url, object body, string op, CancellationToken ct)
    {
        try { return await _http.PostAsync<T>(url, body, ct); }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { _logger.LogError(ex, "[ACCESS_CHANGE_CLIENT] {Op}", op); return ApiResponse<T>.Fail("Không thể thực hiện thao tác thay đổi quyền."); }
    }
}
