﻿using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess.Infrastructure;

public class DataAccessServices
{
    public static IServiceCollection Resolve(IServiceCollection services)
    {
        services
            .AddScoped<DbContext, OrderDemoContext>(config =>
                                                    {
                                                        var context = config.GetRequiredService<OrderDemoContext>();
                                                        context.Database.EnsureCreated();
                                                        return context;
                                                    })
            ;

        services
            .AddScoped<IRepository<Order>, Repository<Order>>()
            .AddScoped<IRepository<Product>, Repository<Product>>()
            .AddScoped<IRepository<Discount>, Repository<Discount>>()
            ;

        services
            .AddScoped<IUnitOfWork, UnitOfWork>()
            ;

        return services;
    }
}