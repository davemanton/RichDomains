using Application.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using DataAccess.Infrastructure;

namespace Application.Tests.Infrastructure;

public class TestDependencyResolver
{
    public static IServiceProvider Resolve()
    {
        var services = new ServiceCollection();

        TestDatabaseCreator.SetupDbContext(services);

        ApplicationServices.Resolve(services);
        DataAccessServices.Resolve(services);

        return services.BuildServiceProvider();
    }
}