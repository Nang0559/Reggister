using FVN_REGISTER.Core.Configurations;
using FVN_REGISTER.Core.Logging;
using FVN_REGISTER.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace FVN_REGISTER.Infrastructure.Repositories
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity>
     where TEntity : class
    {
        protected readonly DbContext _db;
        protected readonly DbSet<TEntity> _set;
        protected readonly ILogger<BaseRepository<TEntity>> _logger;
        private readonly IOptionsMonitor<AuthDebugOptions> _options;

        protected bool Debug => _options.CurrentValue.Enabled;

        public BaseRepository(
            DbContext db,
            ILogger<BaseRepository<TEntity>> logger,
            IOptionsMonitor<AuthDebugOptions> options)
        {
            _db = db;
            _set = db.Set<TEntity>();
            _logger = logger;
            _options = options;
        }

        // =============================
        // QUERY
        // =============================
        public IQueryable<TEntity> Query()
        {
            _logger.LogDebugIf(Debug, "[REPO] Query<{Entity}>", typeof(TEntity).Name);
            return _set.AsQueryable();
        }

        // =============================
        // GET BY ID
        // =============================
        public async Task<TEntity?> GetByIdAsync(object id, CancellationToken ct = default)
        {
            _logger.LogDebugIf(Debug,
                "[REPO] GetById<{Entity}> Id={Id}",
                typeof(TEntity).Name,
                id);

            return await _set.FindAsync(new[] { id }, ct);
        }

        // =============================
        // GET ALL
        // =============================
        public async Task<List<TEntity>> GetAllAsync(CancellationToken ct = default)
        {
            _logger.LogDebugIf(Debug,
                "[REPO] GetAll<{Entity}>",
                typeof(TEntity).Name);

            return await _set.ToListAsync(ct);
        }

        // =============================
        // ADD
        // =============================
        public async Task AddAsync(TEntity entity, CancellationToken ct = default)
        {
            _logger.LogDebugIf(Debug,
                "[REPO] Add<{Entity}>",
                typeof(TEntity).Name);

            await _set.AddAsync(entity, ct);
        }

        // =============================
        // UPDATE
        // =============================
        public void Update(TEntity entity)
        {
            _logger.LogDebugIf(Debug,
                "[REPO] Update<{Entity}>",
                typeof(TEntity).Name);

            _set.Update(entity);
        }

        // =============================
        // REMOVE
        // =============================
        public void Remove(TEntity entity)
        {
            _logger.LogDebugIf(Debug,
                "[REPO] Remove<{Entity}>",
                typeof(TEntity).Name);

            _set.Remove(entity);
        }

        // =============================
        // SAVE
        // =============================
        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            _logger.LogDebugIf(Debug, "[REPO] SaveChanges");

            return await _db.SaveChangesAsync(ct);
        }
    }
}
