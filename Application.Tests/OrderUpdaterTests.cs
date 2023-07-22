using Application.Exceptions;
using Application.Orders;
using Application.Tests.Infrastructure;
using Client.Dtos.Orders;
using Domain;
using Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.Tests;

public class OrderUpdaterTests
{
    private readonly IServiceProvider _serviceProvider;

    private readonly DbContext _database;

    private readonly OrderDto _request;

    private Order? _orderToUpdate;
    private ICollection<Product> _seededProducts;
    private ICollection<Discount> _seededDiscounts;

    public OrderUpdaterTests()
    {
        // yes... we resolve dependencies in tests, we don't use mocks (except external services/databases)
        _serviceProvider = TestDependencyResolver.Resolve();

        // we sometimes use an in memory database for testing, it's helps us test when complex queries work
        _database = TestDatabaseCreator.StartSeed(_serviceProvider);

        SetupProducts();
        SetupDiscounts();
        SetupOrder();

        _request = new OrderDto
        {
            OrderId = 1,
            FirstName = "UPDATED_FIRSTNAME",
            LastName = "UPDATED_LASTNAME",
            Address = "UPDATED_ADDRESS",
            LineItems = new List<LineItemDto>()
            {
                new()
                {
                    ProductId = 10000,
                    Sku = "SKU1",
                    UnitCost = 100,
                    TotalCost = 100,
                    Quantity = 1
                },
                new()
                {
                    ProductId = 10100,
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
                new GeneralDiscount(10000, "TEST-10PERCENT", 0.1m),
                new BuyOneGetOneFreeDiscount(10100, "TEST-BOGOF")
          };
    }

    private void SetupOrder()
    {
        _orderToUpdate = Order.Create("ORIGINAL_FIRSTNAME",
                                      "ORIGINAL_LASTNAME",
                                      "ORIGINAL_ADDRESS",
                                      default,
                                      new List<SetLineItemInput>()
                                      {
                                          new()
                                          {
                                              Quantity = 2,
                                              Product = _seededProducts.Single(x => x.Sku == "SKU3")
                                          },
                                          new()
                                          {
                                              Quantity = 1,
                                              Product = _seededProducts.Single(x => x.Sku == "SKU2")
                                          }
                                      },
                                      _serviceProvider.GetRequiredService<IValidateOrders>(),
                                      out var errors)!;
    }

    public IUpdateOrders GetContractUnderTest()
    {
        _database.Set<Product>().AddRange(_seededProducts);
        _database.Set<Discount>().AddRange(_seededDiscounts);

        if (_orderToUpdate is not null)
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

            var savedLineItem = lineItems.SingleOrDefault(x => x.Sku == requestedItem.Sku && x.OrderId == _request.OrderId);
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
    public void Update_ExpiresOldLineItems_InDatabase()
    {
        var contractUnderTest = GetContractUnderTest();

        contractUnderTest.Update(_request);

        var order = _database.ChangeTracker.Entries<Order>()
                             .Select(x => x.Entity)
                             .Single();

        var lineItems = _database.ChangeTracker.Entries<LineItem>()
                                 .Select(x => x.Entity)
                                 .ToList();

        var itemsToExpire = _orderToUpdate!.LineItems.ExceptBy(_request.LineItems.Select(x => x.Sku), x => x.Sku).ToList();

        Assert.NotEmpty(itemsToExpire);

        foreach (var itemToExpire in itemsToExpire)
        {
            var expiredItem = lineItems.Single(x => x.Sku == itemToExpire.Sku);

            Assert.True(expiredItem.IsExpired);
            Assert.InRange(expiredItem.LastModified, DateTime.UtcNow.AddSeconds(-3), DateTime.UtcNow);
        }
    }

    [Fact]
    public void Update_ReturnsLineItems_InDto()
    {
        var contractUnderTest = GetContractUnderTest();

        var responseDto = contractUnderTest.Update(_request);

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
    public void Update_ThrowsNotFoundException_IfOrderNotPresentInDatabase()
    {
        _orderToUpdate = null;

        var contractUnderTest = GetContractUnderTest();

        Assert.Throws<NotFoundException>(() => { contractUnderTest.Update(_request); });
    }

    [Fact]
    public void Update_Validate_IfCustomerDetailsMissing_ThrowsValidationException()
    {
        var contractUnderTest = GetContractUnderTest();

        _request.FirstName = string.Empty;
        _request.LastName = string.Empty;
        _request.Address = string.Empty;

        var exception = Assert.Throws<ValidationException>(() => contractUnderTest.Update(_request));

        Assert.Contains(exception.Errors, error => error.Key == "firstName");
        Assert.Contains(exception.Errors, error => error.Key == "lastName");
        Assert.Contains(exception.Errors, error => error.Key == "address");
    }

    [Fact]
    public void Update_Validate_IfRequestDoesntContainItems_ThrowsValidationException()
    {
        var contractUnderTest = GetContractUnderTest();

        _request.LineItems.Clear();

        var exception = Assert.Throws<ValidationException>(() => contractUnderTest.Update(_request));

        Assert.Contains(exception.Errors, error => error.Key == "lineItems");
    }

    [Fact]
    public void Update_Discount_ReturnsDiscountCode_InDto()
    {
        _request.DiscountCode = "TEST-10PERCENT";

        var contractUnderTest = GetContractUnderTest();

        var responseDto = contractUnderTest.Update(_request);

        var discount = _seededDiscounts.Single(x => x.Code == _request.DiscountCode);

        Assert.Equal(discount.Code, responseDto.DiscountCode);
    }

    [Fact]
    public void Update_Discount_SavesDiscountOnOrder_ToDatabase()
    {
        _request.DiscountCode = "TEST-10PERCENT";

        var contractUnderTest = GetContractUnderTest();

        contractUnderTest.Update(_request);

        var order = _database.ChangeTracker.Entries<Order>()
                             .Select(x => x.Entity)
                             .Single();

        var discount = _seededDiscounts.Single(x => x.Code == _request.DiscountCode);

        Assert.Equal(discount.DiscountId, order.DiscountId);
    }

    [Fact]
    public void Update_Discount_AppliesGlobalDiscountToLineItems_ToDatabase()
    {
        _request.DiscountCode = "TEST-10PERCENT";

        var contractUnderTest = GetContractUnderTest();

        contractUnderTest.Update(_request);

        var lineItems = _database.ChangeTracker.Entries<LineItem>()
                                 .Select(x => x.Entity)
                                 .ToList();

        var discount = _seededDiscounts.Single(x => x.Code == _request.DiscountCode);

        foreach (var lineItem in lineItems)
        {
            var product = _seededProducts.Single(x => x.Sku == lineItem.Sku);

            var discountedCost = product.UnitCost * lineItem.Quantity * (1 - discount.Percentage);

            Assert.Equal(discountedCost, lineItem.TotalCost);
            Assert.Equal(product.UnitCost, lineItem.UnitCost);
        }
    }

    [Fact]
    public void Update_Discount_AppliesBOGOFDiscountToLineItems_SetValueToHalfIfEven_ToDatabase()
    {
        _request.DiscountCode = "TEST-BOGOF";

        var contractUnderTest = GetContractUnderTest();

        contractUnderTest.Update(_request);

        var lineItems = _database.ChangeTracker.Entries<LineItem>()
                                 .Select(x => x.Entity)
                                 .ToList();

        foreach (var lineItem in lineItems)
        {
            if (lineItem.Quantity % 2 != 0)
                continue;

            var product = _seededProducts.Single(x => x.Sku == lineItem.Sku);

            var discountedCost = product.UnitCost * lineItem.Quantity / 2;

            Assert.Equal(discountedCost, lineItem.TotalCost);
        }
    }

    [Fact]
    public void Update_Discount_AppliesBOGOFDiscountToLineItems_SetValueToHalfIfEven_ToDto()
    {
        _request.DiscountCode = "TEST-BOGOF";

        var contractUnderTest = GetContractUnderTest();

        var responseDto = contractUnderTest.Update(_request);

        foreach (var lineItem in responseDto.LineItems)
        {
            if (lineItem.Quantity % 2 != 0)
                continue;

            var product = _seededProducts.Single(x => x.Sku == lineItem.Sku);

            var discountedCost = product.UnitCost * lineItem.Quantity / 2;

            Assert.Equal(discountedCost, lineItem.TotalCost);
        }
    }

    [Fact]
    public void Update_Discount_AppliesBOGOFDiscountToLineItems_CalculateForEvenAmountWhenOdd_ToDatabase()
    {
        _request.DiscountCode = "TEST-BOGOF";
        _request.LineItems.First().Quantity = 3;

        var contractUnderTest = GetContractUnderTest();

        contractUnderTest.Update(_request);

        var lineItems = _database.ChangeTracker.Entries<LineItem>()
                                 .Select(x => x.Entity)
                                 .ToList();

        foreach (var lineItem in lineItems)
        {
            if (lineItem.Quantity == 1 || lineItem.Quantity % 2 == 0)
                continue;

            var product = _seededProducts.Single(x => x.Sku == lineItem.Sku);

            var discountedCost = (product.UnitCost * (lineItem.Quantity - 1) / 2) + product.UnitCost;

            Assert.Equal(discountedCost, lineItem.TotalCost);
        }
    }

    [Fact]
    public void Update_Discount_AppliesBOGOFDiscountToLineItems_CalculateForEvenAmountWhenOdd_ToDto()
    {
        _request.DiscountCode = "TEST-BOGOF";
        _request.LineItems.First().Quantity = 3;

        var contractUnderTest = GetContractUnderTest();

        var responseDto = contractUnderTest.Update(_request);

        foreach (var responseLineItem in responseDto.LineItems)
        {
            if (responseLineItem.Quantity == 1 || responseLineItem.Quantity % 2 == 0)
                continue;

            var product = _seededProducts.Single(x => x.Sku == responseLineItem.Sku);

            var discountedCost = (product.UnitCost * (responseLineItem.Quantity - 1) / 2) + product.UnitCost;

            Assert.Equal(discountedCost, responseLineItem.TotalCost);
        }
    }
}