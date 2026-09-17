// File: FVN_REGISTER.Infrastructure/Services/Employees/EmployeeManagementService.cs

using FVN_REGISTER.Application.Interfaces.Employees;
using FVN_REGISTER.Application.Services.Common;
using FVN_REGISTER.Contract.Dtos.Employees;
using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Core.Entities.HR;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Repositories;
using FVN_REGISTER.Core.Utils;
using FVN_REGISTER.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.Infrastructure.Services.Employees
{
    public class EmployeeManagementService : BaseService<EmployeeManagementService>, IEmployeeManagementService
    {
        private readonly FVNWEBAPPContext _db;
        private readonly IUnitOfWork _uow;

        public EmployeeManagementService(FVNWEBAPPContext db, IUnitOfWork uow, ILogger<EmployeeManagementService> logger, IOptionsMonitor<AuthDebugOptions> options)
            : base(logger, options)
        {
            _db = db;
            _uow = uow;
        }

        public async Task<ServiceResult<List<EmployeeDeptTreeDto>>> GetTreeAsync(string? searchTerm = null, string? deptCode = null, CancellationToken ct = default)
        {
            try
            {
                var currentYear = DateTime.Now.Year;
                var empQuery = _db.Employees.AsNoTracking().Where(x => x.IsActive == true);
                if (!string.IsNullOrEmpty(deptCode)) empQuery = empQuery.Where(x => x.DeptCode == deptCode);
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var term = searchTerm.Trim().ToLower();
                    empQuery = empQuery.Where(x => x.EmployeeName.ToLower().Contains(term) || x.EmployeeCode.ToLower().Contains(term) || x.EmailAddress.ToLower().Contains(term));
                }

                var joined = await empQuery.Join(_db.Positions.AsNoTracking(), e => e.PositionCode, p => p.PositionCode, (e, p) => new { Employee = e, Position = p })
                    .OrderBy(x => x.Employee.DeptCode).ThenBy(x => x.Employee.EmployeeName).ToListAsync(ct);
                var empCodes = joined.Select(x => x.Employee.EmployeeCode).ToList();
                var balanceDict = await _db.VF03LeaveBalances.AsNoTracking().Where(x => empCodes.Contains(x.EmployeeCode) && x.WorkYear == currentYear).ToDictionaryAsync(x => x.EmployeeCode, ct);
                var startOfYear = new DateTime(currentYear, 1, 1);
                var today = DateTime.Today;
                var shiftRaw = await _db.VwShiftCheckInOuts.AsNoTracking().Where(x => empCodes.Contains(x.EmployeeId) && x.Date >= DateOnly.FromDateTime(startOfYear) && x.Date <= DateOnly.FromDateTime(today)).ToListAsync(ct);
                var otDict = shiftRaw.GroupBy(x => x.EmployeeId).ToDictionary(g => g.Key, g => (TotalDays: g.Count(), TotalMinutes: g.Sum(x => x.CheckOutTime.HasValue && x.CheckInTime.HasValue ? (double)(x.CheckOutTime.Value - x.CheckInTime.Value).TotalMinutes - 480 : 0)));
                var depts = await _db.Departments.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.DeptName).ToListAsync(ct);

                var tree = depts.Select(dept =>
                {
                    var deptEmps = joined.Where(x => x.Employee.DeptCode == dept.DeptCode).Select(x =>
                    {
                        balanceDict.TryGetValue(x.Employee.EmployeeCode, out var balance);
                        otDict.TryGetValue(x.Employee.EmployeeCode, out var ot);
                        var otHours = ot != default ? Math.Max(0, (decimal)(ot.TotalMinutes / 60.0)) : 0;
                        return new EmployeeCardDto
                        {
                            Id = x.Employee.Id, EmployeeCode = x.Employee.EmployeeCode, EmployeeName = x.Employee.EmployeeName,
                            DeptCode = x.Employee.DeptCode, DeptName = dept.DeptName, PositionCode = x.Position.PositionCode,
                            PositionName = x.Position.PositionName, EmailAddress = x.Employee.EmailAddress, PhoneNumber = x.Employee.PhoneNumber,
                            BirthDate = x.Employee.BirthDate, FirstWorkingDate = x.Employee.FirstWorkingDate, EndWorkingDate = x.Employee.EndWorkingDate,
                            IsActive = x.Employee.IsActive ?? false, LevelApprove = x.Employee.LevelApprove,
                            TotalEntitledLeave = balance?.TotalEntitledLeave ?? x.Employee.TotalLeaveDays,
                            LeaveDaysUsed = balance?.LeaveDaysUsed ?? 0, RemainingLeave = balance?.RemainingLeave ?? 0,
                            OtHoursThisYear = Math.Round(otHours, 1), OtDaysThisYear = ot != default ? ot.TotalDays : 0
                        };
                    }).ToList();
                    return new EmployeeDeptTreeDto { DeptCode = dept.DeptCode, DeptName = dept.DeptName, Employees = deptEmps, IsExpanded = deptEmps.Any() };
                }).Where(d => d.Employees.Any() || string.IsNullOrEmpty(searchTerm)).ToList();

                return ServiceResult<List<EmployeeDeptTreeDto>>.Ok(tree);
            }
            catch (Exception ex) { return InternalError<List<EmployeeDeptTreeDto>>(ex, "Lỗi tải danh sách nhân viên"); }
        }

        public async Task<ServiceResult<EmployeeCardDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            try
            {
                var result = await _db.Employees.AsNoTracking().Where(x => x.Id == id)
                    .Join(_db.Positions.AsNoTracking(), e => e.PositionCode, p => p.PositionCode, (e, p) => new { Employee = e, Position = p }).FirstOrDefaultAsync(ct);
                if (result == null) return ServiceResult<EmployeeCardDto>.Fail("Không tìm thấy nhân viên.");
                var dept = await _db.Departments.AsNoTracking().FirstOrDefaultAsync(x => x.DeptCode == result.Employee.DeptCode, ct);
                var balance = await _db.VF03LeaveBalances.AsNoTracking().FirstOrDefaultAsync(x => x.EmployeeCode == result.Employee.EmployeeCode && x.WorkYear == DateTime.Now.Year, ct);
                var dto = new EmployeeCardDto
                {
                    Id = result.Employee.Id, EmployeeCode = result.Employee.EmployeeCode, EmployeeName = result.Employee.EmployeeName,
                    DeptCode = result.Employee.DeptCode, DeptName = dept?.DeptName ?? "", PositionCode = result.Position.PositionCode,
                    PositionName = result.Position.PositionName, EmailAddress = result.Employee.EmailAddress, PhoneNumber = result.Employee.PhoneNumber,
                    BirthDate = result.Employee.BirthDate, FirstWorkingDate = result.Employee.FirstWorkingDate, EndWorkingDate = result.Employee.EndWorkingDate,
                    IsActive = result.Employee.IsActive ?? false, LevelApprove = result.Employee.LevelApprove,
                    TotalEntitledLeave = balance?.TotalEntitledLeave ?? result.Employee.TotalLeaveDays,
                    LeaveDaysUsed = balance?.LeaveDaysUsed ?? 0, RemainingLeave = balance?.RemainingLeave ?? 0
                };
                return ServiceResult<EmployeeCardDto>.Ok(dto);
            }
            catch (Exception ex) { return InternalError<EmployeeCardDto>(ex, "Lỗi lấy thông tin nhân viên"); }
        }

        public async Task<ServiceResult> CreateAsync(EmployeeUpsertDto model, int currentUserId, CancellationToken ct = default)
        {
            try
            {
                if (await _uow.Repository<F03Employee>().Query().AnyAsync(x => x.EmployeeCode == model.EmployeeCode, ct)) return ServiceResult.Fail($"Mã nhân viên '{model.EmployeeCode}' đã tồn tại.");
                if (!await _uow.Repository<F03Position>().Query().AnyAsync(x => x.PositionCode == model.PositionCode, ct)) return ServiceResult.Fail("Chức vụ không tồn tại.");
                var entity = new F03Employee
                {
                    EmployeeCode = model.EmployeeCode, EmployeeName = model.EmployeeName, DeptCode = model.DeptCode, PositionCode = model.PositionCode,
                    EmailAddress = model.EmailAddress, PhoneNumber = model.PhoneNumber, BirthDate = model.BirthDate, GenderCode = model.GenderCode,
                    FirstWorkingDate = model.FirstWorkingDate, EndWorkingDate = model.EndWorkingDate, TotalLeaveDays = model.TotalLeaveDays,
                    IsActive = model.IsActive, LevelApprove = model.LevelApprove, EmployeeNo = model.EmployeeNo,
                    CreatedBy = currentUserId, CreatedAt = DateTime.Now, ModifiedBy = currentUserId, ModifiedAt = DateTime.Now,
                    LastModifiedSource = SyncSourceTags.Manual
                };
                await _uow.Repository<F03Employee>().AddAsync(entity, ct); await _uow.SaveChangesAsync(ct);
                return ServiceResult.Ok("Thêm nhân viên thành công.");
            }
            catch (Exception ex) { return InternalError<object>(ex, "Lỗi thêm nhân viên"); }
        }

        public async Task<ServiceResult> UpdateAsync(EmployeeUpsertDto model, int currentUserId, CancellationToken ct = default)
        {
            try
            {
                var entity = await _uow.Repository<F03Employee>().Query().FirstOrDefaultAsync(x => x.Id == model.Id, ct);
                if (entity == null) return ServiceResult.Fail("Không tìm thấy nhân viên.");
                if (!await _uow.Repository<F03Position>().Query().AnyAsync(x => x.PositionCode == model.PositionCode, ct)) return ServiceResult.Fail("Chức vụ không tồn tại.");
                entity.EmployeeName = model.EmployeeName; entity.DeptCode = model.DeptCode; entity.PositionCode = model.PositionCode;
                entity.EmailAddress = model.EmailAddress; entity.PhoneNumber = model.PhoneNumber; entity.BirthDate = model.BirthDate;
                entity.GenderCode = model.GenderCode; entity.FirstWorkingDate = model.FirstWorkingDate; entity.EndWorkingDate = model.EndWorkingDate;
                entity.TotalLeaveDays = model.TotalLeaveDays; entity.IsActive = model.IsActive; entity.LevelApprove = model.LevelApprove;
                entity.ModifiedBy = currentUserId; entity.ModifiedAt = DateTime.Now; entity.LastModifiedSource = SyncSourceTags.Manual;
                await _uow.SaveChangesAsync(ct); return ServiceResult.Ok("Cập nhật nhân viên thành công.");
            }
            catch (Exception ex) { return InternalError<object>(ex, "Lỗi cập nhật nhân viên"); }
        }

        public async Task<ServiceResult> DeleteAsync(int id, int currentUserId, CancellationToken ct = default)
        {
            try
            {
                var entity = await _uow.Repository<F03Employee>().Query().FirstOrDefaultAsync(x => x.Id == id, ct);
                if (entity == null) return ServiceResult.Fail("Không tìm thấy nhân viên.");
                entity.IsActive = false; entity.EndWorkingDate = DateTime.Now; entity.ModifiedBy = currentUserId; entity.ModifiedAt = DateTime.Now; entity.LastModifiedSource = SyncSourceTags.Manual;
                await _uow.SaveChangesAsync(ct); return ServiceResult.Ok("Đã xóa nhân viên.");
            }
            catch (Exception ex) { return InternalError<object>(ex, "Lỗi xóa nhân viên"); }
        }

        public async Task<ServiceResult> ToggleActiveAsync(int id, int currentUserId, CancellationToken ct = default)
        {
            try
            {
                var entity = await _uow.Repository<F03Employee>().Query().FirstOrDefaultAsync(x => x.Id == id, ct);
                if (entity == null) return ServiceResult.Fail("Không tìm thấy nhân viên.");
                entity.IsActive = !(entity.IsActive ?? false); entity.ModifiedBy = currentUserId; entity.ModifiedAt = DateTime.Now; entity.LastModifiedSource = SyncSourceTags.Manual;
                await _uow.SaveChangesAsync(ct);
                return ServiceResult.Ok($"Đã {(entity.IsActive == true ? "kích hoạt" : "vô hiệu hóa")} nhân viên.");
            }
            catch (Exception ex) { return InternalError<object>(ex, "Lỗi thay đổi trạng thái"); }
        }

        public async Task<ServiceResult<EmployeeOtSummaryDto>> GetOtSummaryAsync(string employeeCode, int year, CancellationToken ct = default)
        {
            try
            {
                var startDate = DateOnly.FromDateTime(new DateTime(year, 1, 1));
                var endDate = DateOnly.FromDateTime(year == DateTime.Now.Year ? DateTime.Today : new DateTime(year, 12, 31));
                var shifts = await _db.VwShiftCheckInOuts.AsNoTracking().Where(x => x.EmployeeId == employeeCode && x.Date >= startDate && x.Date <= endDate && x.CheckInTime.HasValue && x.CheckOutTime.HasValue).ToListAsync(ct);
                var monthly = shifts.GroupBy(x => x.Date!.Value.Month).Select(g =>
                {
                    var totalMinOt = g.Sum(x => Math.Max(0, (x.CheckOutTime!.Value - x.CheckInTime!.Value).TotalMinutes - 480));
                    return new OtMonthlyDto { Month = g.Key, Hours = Math.Round((decimal)(totalMinOt / 60.0), 1), Days = g.Count() };
                }).OrderBy(x => x.Month).ToList();
                var allMonths = Enumerable.Range(1, endDate.Month).Select(m => monthly.FirstOrDefault(x => x.Month == m) ?? new OtMonthlyDto { Month = m }).ToList();
                return ServiceResult<EmployeeOtSummaryDto>.Ok(new EmployeeOtSummaryDto
                {
                    EmployeeCode = employeeCode, Year = year, TotalHours = allMonths.Sum(x => x.Hours), TotalDays = allMonths.Sum(x => x.Days), Monthly = allMonths
                });
            }
            catch (Exception ex) { return InternalError<EmployeeOtSummaryDto>(ex, "Lỗi tính OT"); }
        }
    }
}
