using Application.Discounts;
using Application.Orders;
using Application.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Infrastructure;

public class ApplicationServices
{
    public static IServiceCollection Resolve(IServiceCollection services)
    {
        services
            .AddScoped<ICreateOrders, OrderCreator>()
            .AddScoped<IUpdateOrders, OrderUpdater>()
            ;

        services
            .AddScoped<IValidateOrderRequests, OrderRequestValidator>()
            ;

        services
            .AddScoped<ICalculateOrderDiscounts, DiscountCalculator>()
            ;

        return services;
    }
}