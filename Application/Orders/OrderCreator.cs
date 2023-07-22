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
    private readonly IRepository<Discount> _discountRepo;
    private readonly IUnitOfWork _unitOfWork;

    public OrderCreator(IValidateOrderRequests requestValidator,
                        IRepository<Order> orderRepo,
                        IRepository<Product> productRepo,
                        IRepository<Discount> discountRepo,
                        IUnitOfWork unitOfWork)
    {
        _requestValidator = requestValidator;

        _orderRepo = orderRepo;
        _productRepo = productRepo;
        _discountRepo = discountRepo;

        _unitOfWork = unitOfWork;
    }

    public OrderDto Create(CreateOrderRequestDto request)
    {
        if (!_requestValidator.IsValidRequest(request, out var errors))
            throw new ValidationException("Request failed validation", errors);

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

        var order = new Order(request.FirstName,
                              request.LastName, 
                              request.Address,
                              discount,
                              setLineItemInputs);

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