using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly DbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(DbContext context)
    {
        _context = context;

        if (_context == null)
            throw new ArgumentNullException(nameof(context));

        _dbSet = _context.Set<T>();
    }

    public IQueryable<T> Get(Expression<Func<T, bool>>? filter)
    {
        IQueryable<T> query = _dbSet;

        if (filter != null)
            query = query.Where(filter);

        return query;
    }

    public T Insert(T entity)
    {
        return _dbSet.Add(entity).Entity;
    }
}