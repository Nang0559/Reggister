using FVN_REGISTER.Contract.Dtos.HrmSync;
using FVN_REGISTER.Infrastructure;
using FVN_REGISTER.Infrastructure.Services.HrmSync;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FVN_REGISTER.Infrastructure.Tests;

public sealed class HrmAttendanceBackfillIntegrationTests
{
    [Fact]
    public async Task Company_wide_attendance_calculation_accepts_null_department_scope()
    {
        var connectionString = Environment.GetEnvironmentVariable("FVN_REGISTER_SQL_CONNECTION");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "FVN_REGISTER_SQL_CONNECTION is required for attendance integration tests.");

        var options = new DbContextOptionsBuilder<FVNWEBAPPContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var db = new FVNWEBAPPContext(options);
        await using var transaction = await db.Database.BeginTransactionAsync();

        var service = new HrmAttendanceCalculationService(
            new FVN_REGISTER.Infrastructure.Repositories.UnitOfWork(db),
            NullLogger<HrmAttendanceCalculationService>.Instance,
            db);

        var yesterday = DateTime.Today.AddDays(-1);

        var result = await service.CalculateAsync(
            new HrmAttendanceCalculationRequestDto
            {
                DeptCode = null,
                FromDate = yesterday,
                ToDate = yesterday
            },
            "TEST-ATTENDANCE-BACKFILL");

        Assert.True(result.IsSuccess, result.Message);
        Assert.NotNull(result.Data);
        Assert.True(result.Data!.CalculatedRows >= 0);

        await transaction.RollbackAsync();
    }
}
