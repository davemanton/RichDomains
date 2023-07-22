using Application.Exceptions;
using Client.Dtos;
using Client.Dtos.Orders;
using DataAccess;
using Domain;
using Domain.Services;

namespace Application.Orders;

public class OrderCreator : ICreateOrders
{
    private readonly IValidateOrders _orderValidator;
    private readonly IRepository<Order> _orderRepo;
    private readonly IRepository<Product> _productRepo;
    private readonly IRepository<Discount> _discountRepo;
    private readonly IUnitOfWork _unitOfWork;

    public OrderCreator(IRepository<Order> orderRepo,
                        IRepository<Product> productRepo,
                        IRepository<Discount> discountRepo,
                        IUnitOfWork unitOfWork,
                        IValidateOrders orderValidator)
    {
        _orderValidator = orderValidator;

        _orderRepo = orderRepo;
        _productRepo = productRepo;
        _discountRepo = discountRepo;

        _unitOfWork = unitOfWork;
        
    }

    public OrderDto Create(CreateOrderRequestDto request)
    {
        var skus = request.LineItems.Select(x => x.Sku).ToList();
        var products = _productRepo.Get(x => skus.Contains(x.Sku));

        var setLineItemInputs = request.LineItems.Join(products,
                                                       dto => dto.Sku,
                                                       product => product.Sku,
                                                       (dto, product) => new SetLineItemInput
                                                       {
                                                           Quantity = dto.Quantity,
                                                           Product = product
                                                       }).ToList();

        var discount = _discountRepo.Get(x => x.Code == request.DiscountCode).SingleOrDefault();

        var order = Order.Create(request.FirstName, request.LastName, request.Address,
                                 discount,
                                 setLineItemInputs,
                                 _orderValidator, out var errors);

        if (order is null || errors.Any())
            throw new ValidationException("Order request failed validation", errors);

        _orderRepo.Insert(order);
        _unitOfWork.Save();

        return new OrderDto
        {
            OrderId = order.OrderId,
            Created = order.Created,
            LastModified = order.LastModified,
            FirstName = order.FirstName,
            LastName = order.LastName,
            Address = order.Address,
            DiscountCode = request.DiscountCode,
            LineItems = order.LineItems.Select(x => new LineItemDto
            {
                ProductId = x.ProductId,
                Sku = x.Sku,

                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                
                Quantity = x.Quantity,
                UnitCost = x.UnitCost,
                TotalCost = x.TotalCost,
            }).ToList()
        };
    }
}