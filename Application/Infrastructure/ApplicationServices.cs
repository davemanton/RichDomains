using Application.Orders;
using Application.Validation;
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

        services
            .AddScoped<IValidateOrderRequests, OrderRequestValidator>()
            ;

        return services;
    }
}