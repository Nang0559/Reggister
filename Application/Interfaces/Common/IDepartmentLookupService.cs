using FVN_REGISTER.Contract.Dtos.Depts;


namespace FVN_REGISTER.Application.Interfaces.Common
{
    /// <summary>Lookup phòng ban dùng chung — Leave, OT, Trip, Dashboard đều cần, không thuộc riêng domain nào.</summary>
    public interface IDepartmentLookupService
    {
        Task<List<DeptOption>> GetActiveDepartmentsAsync(CancellationToken ct = default);
    }
}
