using System.Data.Common;
using DataAccess;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Tests.Infrastructure;

public class TestDatabaseCreator
{
    public static IServiceCollection SetupDbContext(IServiceCollection services)
    {
        services.AddDbContext<OrderDemoContext>(options =>
                                                {
                                                    options.UseSqlite(CreateInMemoryDatabase());
                                                    options.EnableSensitiveDataLogging();
                                                });

        return services;
    }

    private static DbConnection CreateInMemoryDatabase()
    {
        var connection = new SqliteConnection("DataSource=file:");

        connection.Open();

        return connection;
    }

    public static DbContext StartSeed(IServiceProvider provider)
    {
        var context = provider.GetRequiredService<DbContext>();

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        return context;
    }

    public static void EndSeed(DbContext context)
    {
        context.SaveChanges();

        context.ChangeTracker.Clear();
    }
}