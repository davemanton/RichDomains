using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;

    public UnitOfWork(DbContext context)
    {
        _context = context;
    }

    public void Save()
    {
        _context.SaveChanges();
    }
}