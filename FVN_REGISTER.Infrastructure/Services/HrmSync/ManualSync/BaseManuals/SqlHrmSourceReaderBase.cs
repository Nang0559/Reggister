namespace FVN_REGISTER.Infrastructure.Services.HrmSync.ManualSync.BaseManuals
{
    using Dapper;
    using FVN_REGISTER.Application.Interfaces.HrmSync;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Base cho mọi Reader pull dữ liệu thô từ HRM DB (cross-database, cùng instance
    /// với FVNWEBAPPDB). Domain Reader chỉ cần khai báo câu SQL, không lặp lại
    /// boilerplate mở connection/Dapper.
    /// </summary>
    public abstract class SqlHrmSourceReaderBase<TSourceRow> : IHrmSourceReader<TSourceRow>
        where TSourceRow : class
    {
        private readonly string _connectionString;

        protected SqlHrmSourceReaderBase(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        /// <summary>Câu SELECT thuần trỏ vào [HRM].[dbo].[TableName] — chỉ phần domain cần khai báo.</summary>
        protected abstract string Sql { get; }

        /// <summary>Tham số cho query (nếu cần) — mặc định không có, override khi cần lọc thêm.</summary>
        protected virtual object? Parameters => null;

        public virtual async Task<List<TSourceRow>> ReadAllAsync(CancellationToken ct = default)
        {
            using var conn = new SqlConnection(_connectionString);
            var command = new CommandDefinition(Sql, Parameters, cancellationToken: ct);

            var rows = await conn.QueryAsync<TSourceRow>(command);
            return rows.AsList();
        }
    }
}