using Application.Discounts;
using Application.Exceptions;
using Application.Validation;
using Client.Dtos;
using Client.Dtos.Orders;
using DataAccess;
using Domain;

namespace Application.Orders;

public class OrderCreator : ICreateOrders
{
    private readonly IValidateOrderRequests _requestValidator;
    private readonly IRepository<Order> _orderRepo;
    private readonly IRepository<Product> _productRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICalculateOrderDiscounts _discountCalculator;

    public OrderCreator(IValidateOrderRequests requestValidator,
                        IRepository<Order> orderRepo,
                        IRepository<Product> productRepo,
                        IUnitOfWork unitOfWork,
                        ICalculateOrderDiscounts discountCalculator)
    {
        _requestValidator = requestValidator;

        _orderRepo = orderRepo;
        _productRepo = productRepo;

        _unitOfWork = unitOfWork;

        _discountCalculator = discountCalculator;
    }

    public OrderDto Create(CreateOrderRequestDto request)
    {
        if (!_requestValidator.IsValidRequest(request, out var errors))
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

        if (!string.IsNullOrWhiteSpace(request.DiscountCode))
            _discountCalculator.ApplyDiscounts(request.DiscountCode, order);

        _orderRepo.Insert(order);
        _unitOfWork.Save();

        return new OrderDto
        {
            OrderId = order.OrderId,
            FirstName = order.FirstName,
            LastName = order.LastName,
            Address = order.Address,
            DiscountCode = request.DiscountCode,
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