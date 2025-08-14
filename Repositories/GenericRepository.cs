using BotSocialMedia.Data;
using BotSocialMedia.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BotSocialMedia.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<List<TResult>> GetAll<TResult>(Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IQueryable<T>>? include = null, Expression<Func<T, TResult>>? selector = null)
        {
            // Shortcut: if no predicate, include, or selector is provided, return all entities as TResult
            if (predicate == null && include == null && selector == null && typeof(TResult) == typeof(T))
            {
                var list = await _dbSet.ToListAsync();
                return list.Cast<TResult>().ToList();
            }

            IQueryable<T> query = _dbSet;

            // Include navigation properties
            if (include != null)
                query = include(query);

            // Filter data
            if (predicate != null)
                query = query.Where(predicate);

            // Projection
            if (selector != null)
                return await query.Select(selector).ToListAsync();

            // Default: return all
            return await query.Cast<TResult>().ToListAsync();
        }

        public async Task<T?> GetById(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T?> GetByKey(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public async Task<List<T>> GetManyByKey(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<T> Create(T entity)
        {
            _dbSet.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<T?> Update(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task<T?> Update(object id, Action<T> updateAction)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null)
                return null; updateAction(entity); // Misalnya: e => e.Name = "New Name"; 
            await _context.SaveChangesAsync();
            return entity;
        }


        public async Task<bool> Delete(object id)
        {
            var entity = await GetById(id);
            if (entity == null) return false;

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Exists(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }
    }
}
