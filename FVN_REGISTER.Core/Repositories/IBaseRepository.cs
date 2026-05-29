using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVN_REGISTER.Core.Repositories
{
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(object id, CancellationToken ct = default);

        Task<List<TEntity>> GetAllAsync(CancellationToken ct = default);

        IQueryable<TEntity> Query(); // 🔥 quan trọng (cho phép LINQ ngoài)

        Task AddAsync(TEntity entity, CancellationToken ct = default);

        void Update(TEntity entity);

        void Remove(TEntity entity);

        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
