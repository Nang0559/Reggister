using FVN_REGISTER.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FVN_REGISTER.Infrastructure.Tests;

public sealed class FvnWebAppContextSmokeTests
{
    private static FVNWEBAPPContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<FVNWEBAPPContext>()
            .UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;Database=FVN_REGISTER_ModelSmoke;Integrated Security=True;TrustServerCertificate=True;")
            .Options;

        return new FVNWEBAPPContext(options);
    }

    [Fact]
    public void ExecutionReconciliation_service_captures_previous_status_before_mutation()
    {
        var root = FindRepositoryRoot();
        var servicePath = Path.Combine(
            root, "FVN_REGISTER.Infrastructure", "Services", "Execution",
            "ExecutionReconciliationService.cs");

        Assert.True(File.Exists(servicePath), $"Missing service: {servicePath}");

        var source = File.ReadAllText(servicePath);
        var previousIndex = source.IndexOf("var previousStatus = entity?.ReconciliationStatus;", StringComparison.Ordinal);
        var assignmentIndex = source.IndexOf("entity.ReconciliationStatus = effectiveStatus;", StringComparison.Ordinal);

        Assert.True(previousIndex >= 0);
        Assert.True(assignmentIndex >= 0);
        Assert.True(previousIndex < assignmentIndex, "previousStatus must be captured before status mutation.");
        Assert.Contains("ActorUserId = actorUserId", source);
        Assert.Contains("isNew ? \"CREATED\" : \"UPSERT\"", source);
    }

    [Fact]
    public void ExecutionReconciliation_sql_script_has_valid_batch_separators_and_history_fk()
    {
        var root = FindRepositoryRoot();
        var sqlPath = Path.Combine(root, "SQL", "29_ExecutionReconciliation.sql");
        Assert.True(File.Exists(sqlPath), $"Missing SQL script: {sqlPath}");

        var sql = File.ReadAllText(sqlPath);
        Assert.DoesNotContain("GOGO", sql);
        Assert.Contains("GO", sql);
        Assert.Contains("FK_F03ExecutionHistory_Reconciliation", sql, StringComparison.Ordinal);
        Assert.Contains("FK_F03ExecutionEvidence_Confirmation", sql, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Execution_schema_contains_history_fk_when_integration_connection_is_configured()
    {
        var connectionString = Environment.GetEnvironmentVariable("FVN_REGISTER_SQL_CONNECTION");
        if (string.IsNullOrWhiteSpace(connectionString))
            return;

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT COUNT(*)
FROM sys.foreign_keys
WHERE name IN
(
    N'FK_F03ExecutionHistory_Reconciliation',
    N'FK_F03ExecutionEvidence_Confirmation'
);";

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        Assert.Equal(2, count);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "FVN_REGISTER.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root.");
    }

    [Fact]
    public void Model_contains_every_DbSet_entity()
    {
        using var db = CreateContext();

        var missing = typeof(FVNWEBAPPContext)
            .GetProperties()
            .Where(p => p.PropertyType.IsGenericType)
            .Where(p => p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.PropertyType.GetGenericArguments()[0])
            .Distinct()
            .Where(type => db.Model.FindEntityType(type) is null)
            .Select(type => type.FullName)
            .OrderBy(x => x)
            .ToArray();

        Assert.True(
            missing.Length == 0,
            "The EF model is missing DbSet entity mappings:" + Environment.NewLine +
            string.Join(Environment.NewLine, missing));
    }

    [Fact]
    public async Task SqlServer_tables_accept_select_top_zero_when_connection_is_configured()
    {
        var connectionString = Environment.GetEnvironmentVariable("FVN_REGISTER_SQL_CONNECTION");
        if (string.IsNullOrWhiteSpace(connectionString))
            return; // Local smoke run: model test above still executes. CI/integration sets the connection.

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using var db = CreateContext();

        var tables = db.Model
            .GetEntityTypes()
            .Where(e => e.GetTableName() is not null)
            .Select(e => new
            {
                Schema = e.GetSchema() ?? "dbo",
                Table = e.GetTableName()!
            })
            .Distinct()
            .OrderBy(x => x.Schema)
            .ThenBy(x => x.Table)
            .ToArray();

        var failures = new List<string>();

        foreach (var table in tables)
        {
            await using var command = connection.CreateCommand();
            command.CommandText =
                $"SELECT TOP 0 * FROM [{table.Schema.Replace("]", "]]")}].[{table.Table.Replace("]", "]]")}]";

            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                failures.Add($"{table.Schema}.{table.Table}: {ex.Message}");
            }
        }

        Assert.True(
            failures.Count == 0,
            "Mapped SQL tables failed SELECT TOP 0:" + Environment.NewLine +
            string.Join(Environment.NewLine, failures));
    }
}
