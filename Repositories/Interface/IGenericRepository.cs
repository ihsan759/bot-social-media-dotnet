using System.Linq.Expressions;

namespace BotSocialMedia.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<List<TResult>> GetAll<TResult>(Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IQueryable<T>>? include = null, Expression<Func<T, TResult>>? selector = null);
        Task<T?> GetById(object id);
        Task<T?> GetByKey(Expression<Func<T, bool>> predicate);
        Task<List<T>> GetManyByKey(Expression<Func<T, bool>> predicate);
        Task<T> Create(T entity);
        Task<T?> Update(T entity);
        Task<T?> Update(object id, Action<T> updateAction);
        Task<bool> Delete(object id);
        Task<bool> Exists(Expression<Func<T, bool>> predicate);
    }
}
