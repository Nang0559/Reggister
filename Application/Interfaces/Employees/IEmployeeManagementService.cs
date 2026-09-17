


using FVN_REGISTER.Contract.Dtos.Employees;
using FVN_REGISTER.Contract.Utils;


namespace FVN_REGISTER.Application.Interfaces.Employees
{
    
        public interface IEmployeeManagementService
        {
            // ================= EMPLOYEE (READ) =================

            /// <summary>Cây nhân viên theo phòng ban, kèm phép tồn + OT năm hiện tại.</summary>
            Task<ServiceResult<List<EmployeeDeptTreeDto>>> GetTreeAsync(
                string? searchTerm = null,
                string? deptCode = null,
                CancellationToken ct = default);

            Task<ServiceResult<EmployeeCardDto>> GetByIdAsync(
                int id,
                CancellationToken ct = default);

            Task<ServiceResult<EmployeeOtSummaryDto>> GetOtSummaryAsync(
                string employeeCode,
                int year,
                CancellationToken ct = default);

            // ================= EMPLOYEE (WRITE) =================

            Task<ServiceResult> CreateAsync(
                EmployeeUpsertDto model,
                int currentUserId,
                CancellationToken ct = default);

            Task<ServiceResult> UpdateAsync(
                EmployeeUpsertDto model,
                int currentUserId,
                CancellationToken ct = default);

            /// <summary>Soft delete — set IsActive = false.</summary>
            Task<ServiceResult> DeleteAsync(
                int id,
                int currentUserId,
                CancellationToken ct = default);

            Task<ServiceResult> ToggleActiveAsync(
                int id,
                int currentUserId,
                CancellationToken ct = default);

            
        }
}
