using FVN_REGISTER.Contract.Models;


namespace FVN_REGISTER.Contract.Interfaces.Repositores
{
    public interface ILeaveRepository
    {
        IQueryable<VF03leaveDay> QueryLeaves();
        IQueryable<VF03leaveDayDetail> QueryDetails();

        Task<VF03leaveDayDetail?> GetDetailById(int detailId);
        Task<List<VF03leaveDayDetail>> GetByEmployee(string employeeCode);
        Task<List<VF03leaveDayDetail>> GetByDepartment(string deptCode);
        Task<List<VF03leaveDayDetail>> GetByLeaveId(int leaveId);

        Task<Dictionary<string, string>> GetDepartments();
        Task<Dictionary<string, int>> GetEmployeeCountByDepartment();
    }
}
