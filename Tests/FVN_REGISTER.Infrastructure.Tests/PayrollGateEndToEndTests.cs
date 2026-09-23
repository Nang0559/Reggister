using FVN_REGISTER.Core.Entities.WorkCalendar;
using FVN_REGISTER.Infrastructure;
using FVN_REGISTER.Infrastructure.Services.Payroll;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FVN_REGISTER.Infrastructure.Tests;

public sealed class PayrollGateEndToEndTests
{
    [Fact]
    public async Task Payroll_lock_is_blocked_by_unresolved_execution_then_succeeds_after_resolution()
    {
        var connectionString = Environment.GetEnvironmentVariable("FVN_REGISTER_SQL_CONNECTION");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "FVN_REGISTER_SQL_CONNECTION is required for payroll gate integration tests.");

        var options = new DbContextOptionsBuilder<FVNWEBAPPContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var db = new FVNWEBAPPContext(options);
        await using var transaction = await db.Database.BeginTransactionAsync();

        var period = await db.PayrollCalculationPeriods
            .Where(x => x.IsActive != false
                && x.Status == "Calculated"
                && x.CalculatedAt.HasValue)
            .OrderByDescending(x => x.FromDate)
            .FirstOrDefaultAsync();

        Assert.NotNull(period);

        var inputCount = await db.PayrollInputs.CountAsync(
            x => x.PayrollPeriodId == period!.Id && x.IsActive != false);
        Assert.True(inputCount > 0, "Test requires a Calculated payroll period with Payroll Input.");

        var employee = await db.Employees
            .Where(x => x.IsActive != false)
            .Select(x => new { x.Id, x.EmployeeCode })
            .FirstOrDefaultAsync();
        Assert.NotNull(employee);

        var reconciliation = new F03ExecutionReconciliation
        {
            ModuleCode = "OT",
            SourceType = "PAYROLL_GATE_TEST",
            SourceId = $"PAYROLL-GATE-{Guid.NewGuid():N}",
            ParticipantId = employee!.EmployeeCode,
            EmployeeId = employee.Id,
            WorkDate = period!.FromDate,
            PlannedState = "Registered",
            ActualState = "Mismatch",
            ReconciliationStatus = "Mismatch",
            RequiresConfirmation = true,
            RequiresEvidence = true,
            CreatedBy = 0,
            LastModifiedSource = "TEST_PAYROLL_GATE"
        };

        db.ExecutionReconciliations.Add(reconciliation);
        await db.SaveChangesAsync();

        var service = new PayrollInputService(db);

        var blocked = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.LockAsync(period.Id, 0));

        Assert.Contains("Execution Reconciliation", blocked.Message, StringComparison.OrdinalIgnoreCase);

        reconciliation.ReconciliationStatus = "Resolved";
        reconciliation.ResolvedAt = DateTime.Now;
        reconciliation.ResolvedBy = 0;
        reconciliation.ModifiedBy = 0;
        reconciliation.ModifiedAt = DateTime.Now;
        await db.SaveChangesAsync();

        var locked = await service.LockAsync(period.Id, 0);

        Assert.Equal("Locked", locked.Status);
        Assert.NotNull(locked.LockedAt);

        await transaction.RollbackAsync();
    }
}
