using FVN_REGISTER.Contract.Dtos.Security;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Contract.Utils;

namespace FVN_REGISTER.Application.Interfaces.FeatureOperators;

public interface IFeatureOperatorAssignmentService
{
    Task<List<FeatureOperatorAssignmentDto>> GetAsync(int functionCode, string resourceType, int? resourceId, CancellationToken ct = default);
    Task<List<FeatureOperatorAssignmentDto>> GetForResourceIdsAsync(int functionCode, string resourceType, IReadOnlyCollection<int> resourceIds, CancellationToken ct = default);
    Task<bool> CanOperateAsync(int userId, string? employeeCode, int functionCode, string resourceType, int? resourceId, CancellationToken ct = default);
    Task<ServiceResult<FeatureOperatorAssignmentDto>> AddAsync(SaveFeatureOperatorAssignmentRequest request, int actorUserId, CancellationToken ct = default);
    Task<ServiceResult> RemoveAsync(int id, int actorUserId, CancellationToken ct = default);
    Task<List<FeatureOperatorEmployeeDto>> GetEmployeesAsync(string? search, CancellationToken ct = default);
    Task<List<FeatureOperatorResourceDto>> GetResourcesAsync(string resourceType, CancellationToken ct = default);
}
