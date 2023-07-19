using Application.Exceptions;
using Application.Tests.Infrastructure;
using Client.Dtos;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.Tests;

public class OrderCreatorTests
{
    readonly IServiceProvider _serviceProvider;

    private readonly DbContext _database;

    private CreateOrderRequestDto _request;

    private ICollection<Product> _seededProducts;

    public OrderCreatorTests()
    {
        // yes... we resolve dependencies in tests, we don't use mocks (except external services/databases)
        _serviceProvider = TestDependencyResolver.Resolve();

        // we sometimes use an in memory database for testing, it's helps us test when complex queries work
        _database = TestDatabaseCreator.StartSeed(_serviceProvider);

        SeedProducts();

        // happy path request
        _request = new CreateOrderRequestDto
        {
            FirstName = "FIRSTNAME",
            LastName = "LASTNAME",
            Address = "ADDRESS",
            LineItems = new List<LineItemRequestDto>
            {
                new()
                {
                    Sku = "SKU1",
                    Quantity = 1
                },
                new()
                {
                    Sku = "SKU2",
                    Quantity = 2
                },
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
            }
        };

        _database.Set<Product>().AddRange(_seededProducts);
    }

    public ICreateOrders GetContractUnderTest()
    {
        TestDatabaseCreator.EndSeed(_database);

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

    [Fact]
    public void Create_ReturnsOrderId_InDto()
    {
        var contractUnderTest = GetContractUnderTest();

        var responseDto = contractUnderTest.Create(_request);

        var order = _database.ChangeTracker.Entries<Order>()
                             .Select(x => x.Entity)
                             .Single();

        Assert.Equal(order.OrderId, responseDto.OrderId);
        Assert.NotEqual(default, responseDto.OrderId);
    }

    [Fact]
    public void Create_StoresProductLineItems_InDatabase()
    {
        var contractUnderTest = GetContractUnderTest();

        contractUnderTest.Create(_request);

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

    [Fact]
    public void Create_ReturnsLineItems_InDto()
    {
        var contractUnderTest = GetContractUnderTest();

        var responseDto = contractUnderTest.Create(_request);

        var order = _database.ChangeTracker.Entries<Order>()
                             .Select(x => x.Entity)
                             .Single();

        foreach (var requestedItem in _request.LineItems)
        {
            var expectedProduct = _seededProducts.Single(x => x.Sku == requestedItem.Sku);

            var responseLineItem = responseDto.LineItems.SingleOrDefault(x => x.Sku == requestedItem.Sku);

            Assert.NotNull(responseLineItem);

            Assert.Equal(requestedItem.Sku, responseLineItem.Sku);
            Assert.Equal(requestedItem.Quantity, responseLineItem.Quantity);
            Assert.Equal(expectedProduct.UnitCost, responseLineItem.UnitCost);
            Assert.Equal(expectedProduct.UnitCost * requestedItem.Quantity, responseLineItem.TotalCost);
        }
    }

    [Fact]
    public void Create_Validate_IfCustomerDetailsMissing_ThrowsValidationException()
    {
        var contractUnderTest = GetContractUnderTest();

        _request.FirstName = string.Empty;
        _request.LastName = string.Empty;
        _request.Address = string.Empty;

        var exception = Assert.Throws<ValidationException>(() => contractUnderTest.Create(_request));

        Assert.Contains(exception.Errors, error => error.Key == nameof(CreateOrderRequestDto.FirstName));
        Assert.Contains(exception.Errors, error => error.Key == nameof(CreateOrderRequestDto.LastName));
        Assert.Contains(exception.Errors, error => error.Key == nameof(CreateOrderRequestDto.Address));
    }

    [Fact]
    public void Create_Validate_IfLineItemsNotPresent_ThrowsValidationException()
    {
        var contractUnderTest = GetContractUnderTest();

        _request.LineItems.Clear();

        var exception = Assert.Throws<ValidationException>(() => contractUnderTest.Create(_request));

        Assert.Contains(exception.Errors, error => error.Key == nameof(CreateOrderRequestDto.LineItems));
    }
}