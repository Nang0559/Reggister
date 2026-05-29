using FVN_REGISTER.Contract.Interfaces.Repositores;
using FVN_REGISTER.Contract.Models;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.API.Repositories
{
    public class LeaveRepository : ILeaveRepository
    {
        private readonly FVNWEBAPPContext _db;

        public LeaveRepository(FVNWEBAPPContext db)
        {
            _db = db;
        }

        // Giữ nguyên IQueryable để Service có thể thực hiện thêm các lệnh .Where() hoặc .Skip/.Take (Phân trang) trước khi thực thi
        public IQueryable<VF03leaveDay> QueryLeaves()
            => _db.VF03leaveDays.AsNoTracking(); // AsNoTracking giúp tăng tốc độ truy vấn chỉ đọc

        public IQueryable<VF03leaveDayDetail> QueryDetails()
            => _db.VF03leaveDayDetails.AsNoTracking();

        public async Task<VF03leaveDayDetail?> GetDetailById(int detailId)
            => await _db.VF03leaveDayDetails.FirstOrDefaultAsync(x => x.DetailId == detailId);

        public async Task<List<VF03leaveDayDetail>> GetByEmployee(string employeeCode)
            => await _db.VF03leaveDayDetails
                .Where(x => x.EmployeeCode == employeeCode)
                .ToListAsync();

        public async Task<List<VF03leaveDayDetail>> GetByDepartment(string deptCode)
            => await _db.VF03leaveDayDetails
                .Where(x => x.DeptCode == deptCode)
                .ToListAsync();

        public async Task<List<VF03leaveDayDetail>> GetByLeaveId(int leaveId)
            => await _db.VF03leaveDayDetails
                .Where(x => x.LeaveId == leaveId)
                .ToListAsync();

        public async Task<Dictionary<string, string>> GetDepartments()
            => await _db.F03departments
                .ToDictionaryAsync(x => x.DeptCode, x => x.DeptName);

        public async Task<Dictionary<string, int>> GetEmployeeCountByDepartment()
            => await _db.VF03employees
                .GroupBy(x => x.DeptCode)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key ?? "Unknown", x => x.Count);
    }
}
