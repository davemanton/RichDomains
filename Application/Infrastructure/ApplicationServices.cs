using Application.Orders;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Infrastructure;

public class ApplicationServices
{
    public static IServiceCollection Resolve(IServiceCollection services)
    {
        services
            .AddScoped<IReadOrders, OrderReader>()
            .AddScoped<ICreateOrders, OrderCreator>()
            .AddScoped<IUpdateOrders, OrderUpdater>()
            ;

        return services;
    }
}