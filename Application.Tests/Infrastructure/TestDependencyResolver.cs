using Application.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Tests.Infrastructure;

public class TestDependencyResolver
{
    public static IServiceProvider Resolve()
    {
        var services = new ServiceCollection();

        ApplicationServices.Resolve(services);

        return services.BuildServiceProvider();
    }
}