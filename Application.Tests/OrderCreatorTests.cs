using Application.Tests.Infrastructure;
using Client.Dtos;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.Tests;

public class OrderCreatorTests
{
    IServiceProvider _serviceProvider;

    private DbContext _database; 

    private CreateOrderRequestDto _request;

    public OrderCreatorTests()
    {
        // yes... we resolve dependencies in tests, we don't use mocks (except external services/databases)
        _serviceProvider = TestDependencyResolver.Resolve();

        // we sometimes use an in memory database for testing, it's helps us test when complex queries work
        _database = TestDatabaseCreator.StartSeed(_serviceProvider);

        // happy path request
        _request = new CreateOrderRequestDto
        {
            FirstName = "FIRSTNAME",
            LastName ="LASTNAME",
            Address = "ADDRESS"
        };
    }

    public ICreateOrders GetContractUnderTest()
    {
        return _serviceProvider.GetRequiredService<ICreateOrders>();
    }

    [Fact]
    public void Create_ReturnsCustomerDetails_InDto()
    {
        var contractUnderTest = GetContractUnderTest();

        var responseDto = contractUnderTest.Create(_request);

        Assert.Equal(_request.FirstName, responseDto.FirstName);
        Assert.Equal(_request.LastName, responseDto.LastName);
        Assert.Equal(_request.Address, responseDto.Address);
    }

    [Fact]
    public void Create_StoresCustomerDetails_InDatabase()
    {
        var contractUnderTest = GetContractUnderTest();

        contractUnderTest.Create(_request);

        var order = _database.ChangeTracker.Entries<Order>()
                              .Select(x => x.Entity)
                              .Single();

        Assert.Equal(_request.FirstName, order.FirstName);
        Assert.Equal(_request.LastName, order.LastName);
        Assert.Equal(_request.Address, order.Address);
    }

}