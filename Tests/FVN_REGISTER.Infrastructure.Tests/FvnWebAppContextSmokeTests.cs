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
