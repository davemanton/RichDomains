using Microsoft.Extensions.DependencyInjection;

namespace Application.Infrastructure;

public class ApplicationServices
{
    public static IServiceCollection Resolve(IServiceCollection services)
    {
        services
            .AddScoped<ICreateOrders, OrderCreator>()
            ;

        return services;
    }
}