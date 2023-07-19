using System.Runtime.CompilerServices;
using Application.Exceptions;
using Client.Dtos;
using DataAccess;
using Domain;

namespace Application;

public interface ICreateOrders
{
    OrderDto Create(CreateOrderRequestDto request);
}

public class OrderCreator : ICreateOrders
{
    private readonly IValidateOrderRequests _requestValidator;
    private readonly IRepository<Order> _orderRepo;
    private readonly IRepository<Product> _productRepo;
    private readonly IUnitOfWork _unitOfWork;

    public OrderCreator(IValidateOrderRequests requestValidator,
                        IRepository<Order> orderRepo,
                        IRepository<Product> productRepo,
                        IUnitOfWork unitOfWork)
    {
        _requestValidator = requestValidator;

        _orderRepo = orderRepo;
        _productRepo = productRepo;

        _unitOfWork = unitOfWork;
    }

    public OrderDto Create(CreateOrderRequestDto request)
    {
        if(!_requestValidator.IsValidRequest(request, out var errors))
            throw new ValidationException("Request failed validation", errors);

        var skus = request.LineItems.Select(x => x.Sku).ToList();

        var products = _productRepo.Get(x => skus.Contains(x.Sku));

        var order = new Order
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Address = request.Address,
            LineItems = new List<LineItem>()
        };

        foreach (var requestedItem in request.LineItems)
        {
            var product = products.Single(x => x.Sku == requestedItem.Sku);

            order.LineItems.Add(new LineItem
            {
                ProductId = product.ProductId,
                Sku = product.Sku,

                Quantity = requestedItem.Quantity,
                UnitCost = product.UnitCost,
                TotalCost = requestedItem.Quantity * product.UnitCost,
            });
        }

        _orderRepo.Insert(order);
        _unitOfWork.Save();

        return new OrderDto
        {
            OrderId = order.OrderId,
            FirstName = order.FirstName,
            LastName = order.LastName,
            Address = order.Address,
            LineItems = order.LineItems.Select(x => new LineItemDto
            {
                Sku = x.Sku,
                Quantity = x.Quantity,
                UnitCost = x.UnitCost,
                TotalCost = x.TotalCost,
            }).ToList()
        };
    }
}

public interface IValidateOrderRequests
{
    bool IsValidRequest(CreateOrderRequestDto request, out IDictionary<string, string> errors);
}

internal class OrderRequestValidator : IValidateOrderRequests
{
    public bool IsValidRequest(CreateOrderRequestDto request, out IDictionary<string, string> errors)
    {
        errors = new Dictionary<string, string>();

        if(string.IsNullOrWhiteSpace(request.FirstName))
            errors.Add(nameof(request.FirstName), "First name is required");

        if (string.IsNullOrWhiteSpace(request.FirstName))
            errors.Add(nameof(request.LastName), "Last name is required");

        if (string.IsNullOrWhiteSpace(request.FirstName))
            errors.Add(nameof(request.Address), "Address is required");

        if (!request.LineItems.Any())
            errors.Add(nameof(request.LineItems), "Line Items are required");

        return !errors.Any();
    }
}