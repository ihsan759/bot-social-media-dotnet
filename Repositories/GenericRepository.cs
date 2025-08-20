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

        public async Task<List<TResult>> GetAll<TResult>(Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IQueryable<T>>? include = null, Expression<Func<T, TResult>>? selector = null, int pageNumber = 1, int pageSize = 50)
        {
            // Shortcut: if no predicate, include, or selector is provided, return all entities as TResult
            if (predicate == null && include == null && selector == null && typeof(TResult) == typeof(T))
            {
                var list = await _dbSet.ToListAsync();
                return list.Cast<TResult>().ToList();
            }

            IQueryable<T> query = _dbSet.AsNoTracking(); // AsNoTracking for better performance

            // Include navigation properties
            if (include != null)
                query = include(query);

            // Filter data
            if (predicate != null)
                query = query.Where(predicate);

            // Projection
            IQueryable<TResult> projectedQuery;
            if (selector != null)
                projectedQuery = query.Select(selector);
            else
                projectedQuery = query.Cast<TResult>();

            // Apply pagination
            projectedQuery = projectedQuery
                                .Skip((pageNumber - 1) * pageSize)
                                .Take(pageSize);

            // Default: return all
            return await projectedQuery.ToListAsync();
        }

        public async Task<T?> GetById(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<TResult?> GetById<TResult>(object id, Func<IQueryable<T>, IQueryable<T>>? include = null, Expression<Func<T, TResult>>? selector = null)
        {
            IQueryable<T> query = _dbSet.AsNoTracking();

            // Include navigation properties
            if (include != null)
                query = include(query);

            // Cari entity by PK (bisa composite key juga)
            var keyProperty = _context.Model.FindEntityType(typeof(T))!
                .FindPrimaryKey()!
                .Properties
                .Single(); // ambil PK (kalau composite, tinggal diubah ke loop)

            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, keyProperty.Name);
            var equals = Expression.Equal(property, Expression.Constant(id));
            var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);

            query = query.Where(lambda);

            // Projection
            if (selector != null)
                return await query.Select(selector).FirstOrDefaultAsync();

            return (TResult?)(object?)await query.FirstOrDefaultAsync();
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
