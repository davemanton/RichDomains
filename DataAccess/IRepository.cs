using System.Linq.Expressions;

namespace DataAccess;

public interface IRepository<T>
{
    IQueryable<T> Get(Expression<Func<T, bool>>? filter);
    T Insert(T entity);
}