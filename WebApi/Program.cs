using Application.Infrastructure;
using DataAccess;
using DataAccess.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Domain.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

ApplicationServices.Resolve(builder.Services);
DomainServices.Resolve(builder.Services);

builder.Services.AddDbContext<OrderDemoContext>(options =>
                                        {
                                            options.UseSqlite(CreateInMemoryDatabase());
                                        });

DataAccessServices.Resolve(builder.Services);

var app = builder.Build();

//app.Services.CreateScope().ServiceProvider.GetRequiredService<OrderDemoContext>().Database.EnsureCreated();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

static DbConnection CreateInMemoryDatabase()
{
    var connection = new SqliteConnection("DataSource=file:example.sqlite");

    connection.Open();

    return connection;
}