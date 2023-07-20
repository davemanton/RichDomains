using Application.Orders;
using Application.Tests.Infrastructure;
using Client.Dtos.Orders;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.Tests;

public class OrderUpdaterTests
{
    private readonly IServiceProvider _serviceProvider;

    private readonly DbContext _database;

    private readonly OrderDto _request;

    private ICollection<Product> _seededProducts;
    private ICollection<Discount> _seededDiscounts;

    public OrderUpdaterTests()
    {
        // yes... we resolve dependencies in tests, we don't use mocks (except external services/databases)
        _serviceProvider = TestDependencyResolver.Resolve();

        // we sometimes use an in memory database for testing, it's helps us test when complex queries work
        _database = TestDatabaseCreator.StartSeed(_serviceProvider);

        SeedProducts();
        SeedDiscounts();

        _request = new OrderDto
        {
            OrderId = 10000,
            FirstName = "UPDATED_FIRSTNAME",
            LastName = "UPDATED_LASTNAME",
            Address = "UPDATED_ADDRESS",
            LineItems = new List<LineItemDto>()
            {
                new()
                {
                    Sku = "SKU1",
                    UnitCost = 100,
                    TotalCost = 100,
                    Quantity = 1
                },
                new()
                {
                    Sku = "SKU2",
                    UnitCost = 200,
                    TotalCost = 400,
                    Quantity = 2
                }
            }
        };
    }

    private void SeedProducts()
    {
        _seededProducts = new List<Product>
        {
            new()
            {
                ProductId = 10000,
                Sku = "SKU1",
                UnitCost = 100,
                Name = "PRODUCT ONE",
            },
            new()
            {
                ProductId = 10100,
                Sku = "SKU2",
                UnitCost = 200,
                Name = "PRODUCT TWO",
            },
            new()
            {
                ProductId = 11000,
                Sku = "SKU3",
                UnitCost = 150,
                Name = "PRODUCT THREE"
            }
        };

        _database.Set<Product>().AddRange(_seededProducts);
    }

    private void SeedDiscounts()
    {
        _seededDiscounts = new List<Discount>
        {
            new()
            {
                DiscountId = 10000,
                Code = "TEST-10PERCENT",
                DiscountType = DiscountType.GeneralDiscount,
                Percentage = 0.1m
            },
            new()
            {
                DiscountId = 10100,
                Code = "TEST-BOGOF",
                DiscountType = DiscountType.BuyOneGetOneFree,
                Percentage = null
            }
        };

        _database.Set<Discount>().AddRange(_seededDiscounts);
    }

    public IUpdateOrders GetContractUnderTest()
    {
        TestDatabaseCreator.EndSeed(_database);

        return _serviceProvider.GetRequiredService<IUpdateOrders>();
    }

    [Fact]
    public void Update_ReturnsCustomerDetails_InDto()
    {
        var contractUnderTest = GetContractUnderTest();

        var responseDto = contractUnderTest.Create(_request);

        Assert.Equal(_request.FirstName, responseDto.FirstName);
        Assert.Equal(_request.LastName, responseDto.LastName);
        Assert.Equal(_request.Address, responseDto.Address);
    }
}