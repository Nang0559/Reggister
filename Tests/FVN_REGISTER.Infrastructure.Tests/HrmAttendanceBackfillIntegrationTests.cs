using FVN_REGISTER.Contract.Dtos.HrmSync;
using FVN_REGISTER.Infrastructure;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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

        var yesterday = DateTime.Today.AddDays(-1);
        var pDept = new SqlParameter("@DeptCode", SqlDbType.NVarChar, 20)
        {
            Value = DBNull.Value
        };
        var pFrom = new SqlParameter("@FromDate", SqlDbType.Date)
        {
            Value = yesterday.Date
        };
        var pTo = new SqlParameter("@ToDate", SqlDbType.Date)
        {
            Value = yesterday.Date
        };
        var pBy = new SqlParameter("@TriggeredBy", SqlDbType.NVarChar, 100)
        {
            Value = "TEST-ATTENDANCE-BACKFILL"
        };

        var rows = await db.Database.SqlQueryRaw<HrmAttendanceCalculationResultDto>(
            "EXEC dbo.usp_CalculateHrmAttendance @DeptCode,@FromDate,@ToDate,@TriggeredBy",
            pDept, pFrom, pTo, pBy)
            .ToListAsync();

        var result = rows.FirstOrDefault();
        Assert.NotNull(result);
        Assert.True(result!.CalculatedRows >= 0);

        await transaction.RollbackAsync();
    }
}
