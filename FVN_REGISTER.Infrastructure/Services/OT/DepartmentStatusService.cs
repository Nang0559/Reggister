using FVN_REGISTER.Application.Interfaces.OT;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Depts;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;



namespace FVN_REGISTER.Infrastructure.Services.Departments
{
    public class DepartmentStatusService
        : BaseService<DepartmentStatusService>, IDepartmentStatusService
    {
        private readonly IUnitOfWork _uow;   // SỬA: FVNWEBAPPContext -> IUnitOfWork

        public DepartmentStatusService(
            IUnitOfWork uow,
            ILogger<DepartmentStatusService> logger,
            IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _uow = uow;
        }

        public async Task<DepartmentStatusDto> GetDeptStatusAsync(
            string deptCode, DateTime? date = null, CancellationToken ct = default)
        {
            var targetDate = (date ?? DateTime.Today).Date;
            var targetDateOnly = DateOnly.FromDateTime(targetDate);

            Logger.LogDebugIf(Debug, "[DEPT_STATUS] DeptCode={Dept} Date={Date}",
                deptCode, targetDate.ToString("dd/MM/yyyy"));

            var employees = await _uow.Repository<F03Employee>().Query()   // SỬA: F03employee (đúng entity đã chốt)
                .AsNoTracking()
                .Where(e => e.DeptCode == deptCode && e.IsActive == true)
                .Select(e => new { e.EmployeeCode, e.EmployeeName, e.DeptCode })
                .ToListAsync(ct);

            if (employees.Count == 0)
                return new DepartmentStatusDto { DeptCode = deptCode, ReportDate = targetDate };

            var empCodes = employees.Select(e => e.EmployeeCode).ToHashSet();

            var attendanceRaw = await _uow.Repository<VwCurrentlyPresentEmployee>().Query()
                .AsNoTracking()
                .Where(a => a.DeptCode == deptCode && a.Date == targetDateOnly)
                .Select(a => new { a.EmployeeId, CheckIn = a.CheckInTime })
                .ToListAsync(ct);

            var presentSet = attendanceRaw
                .GroupBy(a => a.EmployeeId)
                .ToDictionary(g => g.Key, g => g.Min(x => x.CheckIn));

            // SỬA: F03leaveDays -> F03LeaveDay, LeaveStatus.Rejected/Cancel -> ApprovalStatus.Rejected/Cancelled
            var leaveRaw = await _uow.Repository<F03LeaveDay>().Query()
                .AsNoTracking()
                .Where(l =>
                    empCodes.Contains(l.EmployeeCode) &&
                    l.IsActive == true &&
                    l.RequestStatus == ApprovalStatus.Approved &&   // CHỐT: Approved-only
                    l.StartDate.Date <= targetDate &&
                    l.EndDate.Date >= targetDate)
                .Select(l => new { l.EmployeeCode, l.LeaveTypeCode, l.LeaveReason, l.RequestStatus })
                .ToListAsync(ct);

            var leaveDict = leaveRaw
                .GroupBy(l => l.EmployeeCode)
                .ToDictionary(g => g.Key, g => g.First());

            var deptName = await _uow.Repository<F03Department>().Query()
                .AsNoTracking()
                .Where(d => d.DeptCode == deptCode)
                .Select(d => d.DeptName)
                .FirstOrDefaultAsync(ct) ?? deptCode;

            var leaveTypeCodes = leaveRaw.Select(l => l.LeaveTypeCode).Where(c => c != null).Distinct().ToList();

            var leaveTypeNames = await _uow.Repository<F03LeaveType>().Query()
                .AsNoTracking()
                .Where(t => leaveTypeCodes.Contains(t.LeaveTypeCode))
                .ToDictionaryAsync(t => t.LeaveTypeCode, t => t.LeaveTypeName, ct);

            var rows = employees.Select(emp =>
            {
                string status;
                string? checkInText = null;
                string? leaveTypeCode = null;
                string? leaveTypeName = null;
                string? leaveReason = null;

                if (presentSet.TryGetValue(emp.EmployeeCode, out var checkIn))
                {
                    status = EmployeeStatusConst.Present;
                    checkInText = checkIn?.ToString("HH:mm");
                }
                else if (leaveDict.TryGetValue(emp.EmployeeCode, out var leave))
                {
                    status = EmployeeStatusConst.OnLeave;
                    leaveTypeCode = leave.LeaveTypeCode;
                    leaveTypeName = leave.LeaveTypeCode != null
                        ? leaveTypeNames.GetValueOrDefault(leave.LeaveTypeCode)
                        : null;
                    leaveReason = leave.LeaveReason;
                }
                else
                {
                    status = EmployeeStatusConst.Absent;
                }

                return new EmployeeStatusRow
                {
                    EmployeeCode = emp.EmployeeCode,
                    EmployeeName = emp.EmployeeName,
                    DeptCode = emp.DeptCode,
                    DeptName = deptName,
                    Status = status,
                    StatusDisplay = EmployeeStatusConst.GetDisplay(status),
                    StatusColor = EmployeeStatusConst.GetColor(status),
                    CheckInText = checkInText,
                    LeaveTypeCode = leaveTypeCode,
                    LeaveTypeName = leaveTypeName,
                    LeaveReason = leaveReason
                };
            }).ToList();

            var result = new DepartmentStatusDto
            {
                DeptCode = deptCode,
                DeptName = deptName,
                ReportDate = targetDate,
                TotalEmployees = rows.Count,
                PresentCount = rows.Count(r => r.Status == EmployeeStatusConst.Present),
                OnLeaveCount = rows.Count(r => r.Status == EmployeeStatusConst.OnLeave),
                AbsentNoReasonCount = rows.Count(r => r.Status == EmployeeStatusConst.Absent),
                AbsenceRate = rows.Count == 0 ? 0 :
                    Math.Round((double)rows.Count(r => r.Status != EmployeeStatusConst.Present) / rows.Count * 100, 1),
                Employees = rows.OrderBy(r => r.Status).ThenBy(r => r.EmployeeName).ToList()
            };

            Logger.LogDebugIf(Debug, "[DEPT_STATUS] {Dept}: Total={T} Present={P} OnLeave={L} Absent={A}",
                deptCode, result.TotalEmployees, result.PresentCount, result.OnLeaveCount, result.AbsentNoReasonCount);

            return result;
        }

        public async Task<List<DepartmentStatusDto>> GetAllDeptStatusAsync(
            DateTime? date = null, CancellationToken ct = default)
        {
            var targetDate = (date ?? DateTime.Today).Date;
            var targetDateOnly = DateOnly.FromDateTime(targetDate);

            var employees = await _uow.Repository<F03Employee>().Query()
                .AsNoTracking().Where(e => e.IsActive==true)
                .Select(e => new { e.EmployeeCode, e.DeptCode }).ToListAsync(ct);

            var attendance = (await _uow.Repository<VwCurrentlyPresentEmployee>().Query()
                .AsNoTracking().Where(a => a.Date == targetDateOnly)
                .Select(a => a.EmployeeId).Distinct().ToListAsync(ct)).ToHashSet();

            var onLeave = (await _uow.Repository<F03LeaveDay>().Query()
                 .AsNoTracking()
                 .Where(l => l.IsActive == true &&
                             l.RequestStatus == ApprovalStatus.Approved &&   // CHỐT: đồng bộ 2 hàm
                             l.StartDate.Date <= targetDate &&
                             l.EndDate.Date >= targetDate)
                 .Select(l => l.EmployeeCode).ToListAsync(ct)).ToHashSet();

            var deptMap = (await _uow.Repository<F03Department>().Query()
                .AsNoTracking().Where(d => d.IsActive == true)
                .Select(d => new { d.DeptCode, d.DeptName }).ToListAsync(ct))
                .ToDictionary(d => d.DeptCode, d => d.DeptName);

            var result = employees
                .GroupBy(e => e.DeptCode)
                .Select(g =>
                {
                    var codes = g.Select(e => e.EmployeeCode).ToList();
                    var presentCount = codes.Count(c => attendance.Contains(c));
                    var leaveCount = codes.Count(c => !attendance.Contains(c) && onLeave.Contains(c));
                    var absentCount = codes.Count(c => !attendance.Contains(c) && !onLeave.Contains(c));
                    var total = codes.Count;

                    return new DepartmentStatusDto
                    {
                        DeptCode = g.Key,
                        DeptName = deptMap.GetValueOrDefault(g.Key, g.Key),
                        ReportDate = targetDate,
                        TotalEmployees = total,
                        PresentCount = presentCount,
                        OnLeaveCount = leaveCount,
                        AbsentNoReasonCount = absentCount,
                        AbsenceRate = total == 0 ? 0 : Math.Round((double)(leaveCount + absentCount) / total * 100, 1),
                        Employees = new()
                    };
                })
                .OrderBy(d => d.DeptName)
                .ToList();

            return result;
        }
    }
}
