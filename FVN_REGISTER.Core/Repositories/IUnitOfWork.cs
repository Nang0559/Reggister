

namespace FVN_REGISTER.Core.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IBaseRepository<T> Repository<T>() where T : class;

        Task<int> SaveChangesAsync(CancellationToken ct = default);

        Task<IUowTransaction> BeginTransactionAsync(CancellationToken ct = default);
        Task CommitAsync(CancellationToken ct = default);
        Task RollbackAsync(CancellationToken ct = default);
        Task<List<TResult>> SqlQueryRawAsync<TResult>(
          string sql, CancellationToken ct = default, params object[] parameters)
          where TResult : class;

        Task<int> ExecuteSqlRawAsync(
            string sql, CancellationToken ct = default, params object[] parameters);

        void SetCommandTimeout(int seconds);
    }
    public interface IUowTransaction : IAsyncDisposable
    {
        Task CommitAsync(CancellationToken ct = default);
        Task RollbackAsync(CancellationToken ct = default);
    }
}
