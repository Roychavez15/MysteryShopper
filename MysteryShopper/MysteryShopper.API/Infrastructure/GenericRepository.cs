using Microsoft.EntityFrameworkCore;
using MysteryShopper.API.Domain;
using System.Linq.Expressions;

namespace MysteryShopper.API.Infrastructure
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T> AddAsync(T entity);
        Task<T?> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] includes);
        IQueryable<T> Query(Expression<Func<T, bool>>? filter = null);
        Task UpdateAsync(T entity);
        Task SoftDeleteAsync(Guid id);
    }

    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly AppDbContext _db;
        protected readonly DbSet<T> _set;
        public GenericRepository(AppDbContext db)
        { _db = db; _set = db.Set<T>(); }

        public async Task<T> AddAsync(T entity)
        {
            await _set.AddAsync(entity);
            return entity;
        }

        public async Task<T?> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> q = _set;
            foreach (var inc in includes) q = q.Include(inc);
            return await q.FirstOrDefaultAsync(x => x.Id == id);
        }

        public IQueryable<T> Query(Expression<Func<T, bool>>? filter = null)
        { return filter is null ? _set.AsQueryable() : _set.Where(filter); }

        public Task UpdateAsync(T entity)
        {
            _set.Update(entity);
            return Task.CompletedTask;
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var e = await _set.FindAsync(id);
            if (e is not null)
            {
                e.IsDeleted = true;
                e.DeletedAt = DateTime.UtcNow;
                _set.Update(e);
            }
        }
    }
}

