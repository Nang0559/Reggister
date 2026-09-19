using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;


namespace FVN_REGISTER.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _context;
        private readonly IServiceProvider _provider;
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(DbContext context, IServiceProvider provider)
        {
            _context = context;
            _provider = provider;
        }

        public IBaseRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T);

            if (_repositories.TryGetValue(type, out var cached))
                return (IBaseRepository<T>)cached;

            var repo = _provider.GetRequiredService<IBaseRepository<T>>();
            _repositories[type] = repo;

            return repo;
        }

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
            => await _context.SaveChangesAsync(ct);

        // ================= TRANSACTION =================
        // SỬA: trả về IUowTransaction thay vì void — mỗi lần gọi tạo 1 transaction MỚI,
        // không dùng field dùng chung như bản cũ (tránh 2 luồng đè transaction của nhau
        // khi UnitOfWork bị resolve theo Scoped nhưng có 2 nhánh code cùng gọi BeginTransactionAsync).
        public async Task<IUowTransaction> BeginTransactionAsync(CancellationToken ct = default)
        {
            var transaction = await _context.Database.BeginTransactionAsync(ct);
            return new UowTransaction(transaction);
        }

        // Giữ lại 2 method này cho tương thích ngược nếu nơi khác đang gọi trực tiếp
        // _uow.CommitAsync()/_uow.RollbackAsync() mà KHÔNG qua BeginTransactionAsync trả về ở trên.
        // Best practice: nên dùng "await using var tx = await BeginTransactionAsync(...)" thay vì 2 hàm này.
        public async Task CommitAsync(CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);

            var currentTransaction = _context.Database.CurrentTransaction;
            if (currentTransaction != null)
                await currentTransaction.CommitAsync(ct);
        }

        public async Task RollbackAsync(CancellationToken ct = default)
        {
            var currentTransaction = _context.Database.CurrentTransaction;
            if (currentTransaction != null)
                await currentTransaction.RollbackAsync(ct);
        }

        // ================= RAW SQL / STORED PROCEDURE =================
        public async Task<List<TResult>> SqlQueryRawAsync<TResult>(
            string sql, CancellationToken ct = default, params object[] parameters)
            where TResult : class
        {
            return await _context.Database
                .SqlQueryRaw<TResult>(sql, parameters)
                .ToListAsync(ct);
        }

        public async Task<int> ExecuteSqlRawAsync(
            string sql, CancellationToken ct = default, params object[] parameters)
            => await _context.Database.ExecuteSqlRawAsync(sql, parameters, ct);

        public void SetCommandTimeout(int seconds)
            => _context.Database.SetCommandTimeout(seconds);

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
