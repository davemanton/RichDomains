using Application.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using DataAccess.Infrastructure;
using Domain.Services;

namespace Application.Tests.Infrastructure;

public class TestDependencyResolver
{
    public static IServiceProvider Resolve()
    {
        var services = new ServiceCollection();

        TestDatabaseCreator.SetupDbContext(services);

        ApplicationServices.Resolve(services);
        DataAccessServices.Resolve(services);
        DomainServices.Resolve(services);

        return services.BuildServiceProvider();
    }
}