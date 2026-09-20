using FVN_REGISTER.Application.Models.Actions;
using FVN_REGISTER.Core.Enums;
using FVN_REGISTER.Infrastructure;
using FVN_REGISTER.Infrastructure.Services.Actions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FVN_REGISTER.Infrastructure.Tests;

public sealed class ExecutionLifecycleIntegrationTests
{
    [Fact]
    public async Task Action_writer_is_idempotent_and_preserves_original_due_at()
    {
        var connectionString = Environment.GetEnvironmentVariable("FVN_REGISTER_SQL_CONNECTION");
        if (string.IsNullOrWhiteSpace(connectionString))
            return;

        var options = new DbContextOptionsBuilder<FVNWEBAPPContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var db = new FVNWEBAPPContext(options);
        await using var transaction = await db.Database.BeginTransactionAsync();

        var employee = await db.Employees.AsNoTracking()
            .Where(x => x.IsActive != false)
            .Select(x => new { x.Id, x.EmployeeCode })
            .FirstOrDefaultAsync();

        Assert.NotNull(employee);

        var sourceId = $"TEST-IDEMPOTENT-{Guid.NewGuid():N}";
        var writer = new ActionItemWriter(db);

        var draft = new ActionItemDraft(
            "OT",
            sourceId,
            employee!.Id,
            employee.Id,
            null,
            DateOnly.FromDateTime(DateTime.Today),
            "EXECUTION_CONFIRMATION",
            "Test action",
            "Idempotency test",
            1,
            100,
            null,
            "/execution",
            null,
            "{}",
            "OT_EMPLOYEE",
            employee.EmployeeCode,
            0);

        var firstId = await writer.EnsureOpenAsync(draft);
        var first = await db.ActionItems.SingleAsync(x => x.ActionId == firstId);

        await Task.Delay(20);

        var secondId = await writer.EnsureOpenAsync(draft);
        var second = await db.ActionItems.SingleAsync(x => x.ActionId == secondId);

        Assert.Equal(firstId, secondId);
        Assert.Equal(first.DueAt, second.DueAt);
        Assert.Equal(ActionItemStatus.Open, second.Status);

        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task Action_writer_keeps_participants_isolated()
    {
        var connectionString = Environment.GetEnvironmentVariable("FVN_REGISTER_SQL_CONNECTION");
        if (string.IsNullOrWhiteSpace(connectionString))
            return;

        var options = new DbContextOptionsBuilder<FVNWEBAPPContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var db = new FVNWEBAPPContext(options);
        await using var transaction = await db.Database.BeginTransactionAsync();

        var employee = await db.Employees.AsNoTracking()
            .Where(x => x.IsActive != false)
            .Select(x => new { x.Id, x.EmployeeCode })
            .FirstOrDefaultAsync();

        Assert.NotNull(employee);

        var sourceId = $"TEST-PARTICIPANT-{Guid.NewGuid():N}";
        var writer = new ActionItemWriter(db);

        ActionItemDraft Draft(string participant) => new(
            "OT",
            sourceId,
            employee!.Id,
            employee.Id,
            null,
            DateOnly.FromDateTime(DateTime.Today),
            "EXECUTION_CONFIRMATION",
            "Test participant action",
            "Participant isolation test",
            1,
            100,
            null,
            "/execution",
            null,
            "{}",
            "OT_EMPLOYEE",
            participant,
            0);

        var firstId = await writer.EnsureOpenAsync(Draft("P1"));
        var secondId = await writer.EnsureOpenAsync(Draft("P2"));

        Assert.NotEqual(firstId, secondId);

        var count = await db.ActionItems.CountAsync(x =>
            x.ActionId == firstId || x.ActionId == secondId);

        Assert.Equal(2, count);

        await transaction.RollbackAsync();
    }
}
