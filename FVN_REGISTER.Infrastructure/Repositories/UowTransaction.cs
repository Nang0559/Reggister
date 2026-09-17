using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore.Storage;


namespace FVN_REGISTER.Infrastructure.Repositories
{
    internal sealed class UowTransaction : IUowTransaction
    {
        private readonly IDbContextTransaction _transaction;
        private bool _disposed;

        public UowTransaction(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public Task CommitAsync(CancellationToken ct = default)
            => _transaction.CommitAsync(ct);

        public Task RollbackAsync(CancellationToken ct = default)
            => _transaction.RollbackAsync(ct);

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;
            await _transaction.DisposeAsync();
        }
    }
}
