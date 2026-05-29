using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _context;
        private IDbContextTransaction? _transaction;
        private readonly IServiceProvider _provider;
        // cache repository để tránh new nhiều lần
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(DbContext context, IServiceProvider provider)
        {
            _context = context;
            _provider = provider;
        }

        public IBaseRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T);

            if (_repositories.ContainsKey(type))
                return (IBaseRepository<T>)_repositories[type];

            var repo = _provider.GetRequiredService<IBaseRepository<T>>();
            _repositories[type] = repo;

            return repo;
        }

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
            => await _context.SaveChangesAsync(ct);

        // ================= TRANSACTION =================

        public async Task BeginTransactionAsync(CancellationToken ct = default)
        {
            if (_transaction != null) return;

            _transaction = await _context.Database.BeginTransactionAsync(ct);
        }

        public async Task CommitAsync(CancellationToken ct = default)
        {
            try
            {
                await _context.SaveChangesAsync(ct);

                if (_transaction != null)
                    await _transaction.CommitAsync(ct);
            }
            catch
            {
                await RollbackAsync(ct);
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackAsync(CancellationToken ct = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(ct);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
