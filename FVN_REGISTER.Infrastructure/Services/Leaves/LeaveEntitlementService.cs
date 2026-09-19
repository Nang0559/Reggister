using FVN_REGISTER.Application.Interfaces.Leaves;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Leaves;
using FVN_REGISTER.Core.Entities.HR;
using FVN_REGISTER.Core.Entities.Leaves;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Leaves;

public sealed class LeaveEntitlementService : ILeaveEntitlementService
{
    private const decimal BaseAnnualLeaveDays = 12m;
    private const int SeniorityStepYears = 5;

    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public LeaveEntitlementService(
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<LeaveEntitlementDto> EnsureCalculatedAsync(
        string employeeCode,
        int workYear,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(employeeCode))
            throw new ArgumentException("EmployeeCode không được để trống.", nameof(employeeCode));

        var workYearEntity = await _uow.Repository<F03WorkYear>().Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.WorkYear == workYear && x.IsActive == true,
                ct);

        if (workYearEntity == null)
            throw new InvalidOperationException($"Chưa cấu hình F03WorkYear cho năm {workYear}.");

        var employee = await _uow.Repository<F03Employee>().Query()
            .FirstOrDefaultAsync(
                x => x.EmployeeCode == employeeCode && x.IsActive == true,
                ct);

        if (employee == null)
            throw new InvalidOperationException($"Không tìm thấy nhân viên [{employeeCode}].");

        if (!employee.FirstWorkingDate.HasValue)
            throw new InvalidOperationException(
                $"Nhân viên [{employeeCode}] chưa có FirstWorkingDate.");

        var firstWorkingDate = employee.FirstWorkingDate.Value.Date;

        // Không tính phép cho người bắt đầu làm sau khi năm làm việc đã kết thúc
        // hoặc đã nghỉ trước khi năm bắt đầu.
        var employedInYear = firstWorkingDate <= workYearEntity.EndDate.Date
            && (!employee.EndWorkingDate.HasValue
                || employee.EndWorkingDate.Value.Date >= workYearEntity.StartDate.Date);

        var calculationDate = employee.EndWorkingDate.HasValue
            && employee.EndWorkingDate.Value.Date < workYearEntity.EndDate.Date
                ? employee.EndWorkingDate.Value.Date
                : workYearEntity.EndDate.Date;

        var yearsOfService = employedInYear
            ? CalculateCompletedYears(firstWorkingDate, calculationDate)
            : 0;

        var seniorityLeaveDays = employedInYear
            ? Math.Floor((decimal)yearsOfService / SeniorityStepYears)
            : 0m;

        var totalLeaveDays = employedInYear
            ? BaseAnnualLeaveDays + seniorityLeaveDays
            : 0m;

        var now = DateTime.Now;
        var currentUser = _currentUser.GetCurrentUser();
        var actorId = currentUser?.UserId ?? 0;

        var balance = await _uow.Repository<F03LeaveBalance>().Query()
            .FirstOrDefaultAsync(
                x => x.EmployeeCode == employeeCode && x.WorkYear == workYear,
                ct);

        if (balance == null)
        {
            balance = new F03LeaveBalance
            {
                EmployeeCode = employeeCode,
                WorkYear = workYear,
                BaseLeaveDays = employedInYear ? BaseAnnualLeaveDays : 0m,
                SeniorityLeaveDays = seniorityLeaveDays,
                TotalDays = totalLeaveDays,
                YearsOfService = yearsOfService,
                CalculatedAt = now,
                CreatedBy = actorId,
                CreatedAt = now
            };

            await _uow.Repository<F03LeaveBalance>().AddAsync(balance, ct);
        }
        else
        {
            balance.BaseLeaveDays = employedInYear ? BaseAnnualLeaveDays : 0m;
            balance.SeniorityLeaveDays = seniorityLeaveDays;
            balance.TotalDays = totalLeaveDays;
            balance.YearsOfService = yearsOfService;
            balance.CalculatedAt = now;
            balance.ModifiedBy = actorId;
            balance.ModifiedAt = now;
        }

        // F03Employees.TotalLeaveDays là giá trị entitlement hiện tại
        // để tương thích với master employee. Không dùng nó làm nguồn lịch sử.
        if (workYear == DateTime.Today.Year)
        {
            employee.TotalLeaveDays = totalLeaveDays;
            employee.ModifiedBy = actorId;
            employee.ModifiedAt = now;
        }

        await _uow.SaveChangesAsync(ct);

        return new LeaveEntitlementDto
        {
            EmployeeCode = employee.EmployeeCode,
            EmployeeName = employee.EmployeeName,
            WorkYear = workYear,
            FirstWorkingDate = firstWorkingDate,
            CalculationDate = calculationDate,
            YearsOfService = yearsOfService,
            BaseLeaveDays = employedInYear ? BaseAnnualLeaveDays : 0m,
            SeniorityLeaveDays = seniorityLeaveDays,
            TotalLeaveDays = totalLeaveDays
        };
    }

    public async Task EnsureWorkYearCalculatedAsync(
        int workYear,
        CancellationToken ct = default)
    {
        var workYearEntity = await _uow.Repository<F03WorkYear>().Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.WorkYear == workYear && x.IsActive == true,
                ct);

        if (workYearEntity == null)
            throw new InvalidOperationException($"Chưa cấu hình F03WorkYear cho năm {workYear}.");

        var employees = await _uow.Repository<F03Employee>().Query()
            .Where(x => x.IsActive == true && x.FirstWorkingDate.HasValue)
            .ToListAsync(ct);

        if (employees.Count == 0)
            return;

        var codes = employees.Select(x => x.EmployeeCode).ToList();
        var balances = await _uow.Repository<F03LeaveBalance>().Query()
            .Where(x => x.WorkYear == workYear && codes.Contains(x.EmployeeCode))
            .ToDictionaryAsync(x => x.EmployeeCode, ct);

        var today = DateTime.Today;
        var now = DateTime.Now;
        var currentUser = _currentUser.GetCurrentUser();
        var actorId = currentUser?.UserId ?? 0;

        foreach (var employee in employees)
        {
            if (!employee.FirstWorkingDate.HasValue)
                continue;

            var firstWorkingDate = employee.FirstWorkingDate.Value.Date;
            var employedInYear = firstWorkingDate <= workYearEntity.EndDate.Date
                && (!employee.EndWorkingDate.HasValue
                    || employee.EndWorkingDate.Value.Date >= workYearEntity.StartDate.Date);

            var calculationDate = employee.EndWorkingDate.HasValue
                && employee.EndWorkingDate.Value.Date < workYearEntity.EndDate.Date
                    ? employee.EndWorkingDate.Value.Date
                    : workYearEntity.EndDate.Date;

            var yearsOfService = employedInYear
                ? CalculateCompletedYears(firstWorkingDate, calculationDate)
                : 0;
            var seniorityLeaveDays = employedInYear
                ? Math.Floor((decimal)yearsOfService / SeniorityStepYears)
                : 0m;
            var totalLeaveDays = employedInYear
                ? BaseAnnualLeaveDays + seniorityLeaveDays
                : 0m;

            var isNew = false;
            if (!balances.TryGetValue(employee.EmployeeCode, out var balance))
            {
                balance = new F03LeaveBalance
                {
                    EmployeeCode = employee.EmployeeCode,
                    WorkYear = workYear,
                    CreatedBy = actorId,
                    CreatedAt = now
                };
                await _uow.Repository<F03LeaveBalance>().AddAsync(balance, ct);
                balances[employee.EmployeeCode] = balance;
                isNew = true;
            }

            // Không ghi lại balance đã tính trong cùng một ngày nếu entitlement vẫn giống nhau.
            if (isNew
                || balance.CalculatedAt.Date != today
                || balance.TotalDays != totalLeaveDays
                || balance.YearsOfService != yearsOfService)
            {
                balance.BaseLeaveDays = employedInYear ? BaseAnnualLeaveDays : 0m;
                balance.SeniorityLeaveDays = seniorityLeaveDays;
                balance.TotalDays = totalLeaveDays;
                balance.YearsOfService = yearsOfService;
                balance.CalculatedAt = now;
                balance.ModifiedBy = actorId;
                balance.ModifiedAt = now;
            }

            if (workYear == today.Year)
            {
                employee.TotalLeaveDays = totalLeaveDays;
                employee.ModifiedBy = actorId;
                employee.ModifiedAt = now;
            }
        }

        await _uow.SaveChangesAsync(ct);
    }

    private static int CalculateCompletedYears(DateTime firstWorkingDate, DateTime calculationDate)
    {
        if (calculationDate.Date < firstWorkingDate.Date)
            return 0;

        var years = calculationDate.Year - firstWorkingDate.Year;

        if (firstWorkingDate.AddYears(years).Date > calculationDate.Date)
            years--;

        return Math.Max(0, years);
    }
}
