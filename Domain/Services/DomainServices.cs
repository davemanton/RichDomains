using Microsoft.Extensions.DependencyInjection;

namespace Domain.Services;

public static class DomainServices
{
    public static IServiceCollection Resolve(IServiceCollection services)
    {
        services
            .AddScoped<IValidateOrders, OrderValidator>()
            ;

        return services;
    }
}