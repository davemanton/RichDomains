using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess.Infrastructure;

public class DataAccessServices
{
    public static IServiceCollection Resolve(IServiceCollection services)
    {
        services
            .AddScoped<DbContext, OrderDemoContext>()
            ;

        services
            .AddScoped<IRepository<Order>, Repository<Order>>()
            ;

        services
            .AddScoped<IUnitOfWork, UnitOfWork>()
            ;

        return services;
    }
}