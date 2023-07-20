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

    private Order _orderToUpdate;
    private ICollection<Product> _seededProducts;
    private ICollection<Discount> _seededDiscounts;

    public OrderUpdaterTests()
    {
        // yes... we resolve dependencies in tests, we don't use mocks (except external services/databases)
        _serviceProvider = TestDependencyResolver.Resolve();

        // we sometimes use an in memory database for testing, it's helps us test when complex queries work
        _database = TestDatabaseCreator.StartSeed(_serviceProvider);

        SetupOrder();
        SetupProducts();
        SetupDiscounts();

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

    private void SetupProducts()
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
        }; _database.Set<Product>().AddRange(_seededProducts);
    }

    private void SetupDiscounts()
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

        
    }

    private void SetupOrder()
    {
        _orderToUpdate = new Order
        {
            OrderId = 10000,
            FirstName = "ORIGINAL_FIRSTNAME",
            LastName = "ORIGINAL_LASTNAME",
            Address = "ORIGINAL_ADDRESS",
            LineItems = new List<LineItem>()
            {
                new()
                {
                    ProductId = 11000,
                    OrderId = 10000,
                    Sku = "SKU3",
                    UnitCost = 150,
                    Quantity = 2,
                    TotalCost = 300,
                },
                new()
                {
                    ProductId = 10100,
                    OrderId = 10000,
                    Sku = "SKU2",
                    UnitCost = 200,
                    Quantity = 1,
                    TotalCost = 400,
                }
            }
        };
    }

    public IUpdateOrders GetContractUnderTest()
    {
        _database.Set<Product>().AddRange(_seededProducts);
        _database.Set<Discount>().AddRange(_seededDiscounts);
        _database.Set<Order>().Add(_orderToUpdate);

        TestDatabaseCreator.EndSeed(_database);

        return _serviceProvider.GetRequiredService<IUpdateOrders>();
    }

    [Fact]
    public void Update_ReturnsCustomerDetails_InDto()
    {
        var contractUnderTest = GetContractUnderTest();

        var responseDto = contractUnderTest.Update(_request);

        Assert.Equal(_request.OrderId, responseDto.OrderId);
        Assert.Equal(_request.FirstName, responseDto.FirstName);
        Assert.Equal(_request.LastName, responseDto.LastName);
        Assert.Equal(_request.Address, responseDto.Address);
    }

    [Fact]
    public void Update_StoresCustomerDetails_InDatabase()
    {
        var contractUnderTest = GetContractUnderTest();

        contractUnderTest.Update(_request);

        var order = _database.ChangeTracker.Entries<Order>()
                             .Select(x => x.Entity)
                             .Single();


        Assert.Equal(_request.OrderId, order.OrderId);
        Assert.Equal(_request.FirstName, order.FirstName);
        Assert.Equal(_request.LastName, order.LastName);
        Assert.Equal(_request.Address, order.Address);
    }

    [Fact]
    public void Update_StoresProductLineItems_InDatabase()
    {
        var contractUnderTest = GetContractUnderTest();

        contractUnderTest.Update(_request);

        var order = _database.ChangeTracker.Entries<Order>()
                             .Select(x => x.Entity)
                             .Single();

        var lineItems = _database.ChangeTracker.Entries<LineItem>()
                                 .Select(x => x.Entity)
                                 .ToList();

        foreach (var requestedItem in _request.LineItems)
        {
            var expectedProduct = _seededProducts.Single(x => x.Sku == requestedItem.Sku);

            var savedLineItem = lineItems.SingleOrDefault(x => x.Sku == requestedItem.Sku);
            Assert.NotNull(savedLineItem);

            Assert.Equal(order.OrderId, savedLineItem.OrderId);
            Assert.Equal(expectedProduct.ProductId, savedLineItem.ProductId);
            Assert.Equal(expectedProduct.Sku, savedLineItem.Sku);

            Assert.Equal(requestedItem.Quantity, savedLineItem.Quantity);

            Assert.Equal(expectedProduct.UnitCost, savedLineItem.UnitCost);
            Assert.Equal(expectedProduct.UnitCost * requestedItem.Quantity, savedLineItem.TotalCost);

            Assert.False(savedLineItem.IsExpired);
        }
    }
}