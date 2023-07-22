using Application.Exceptions;
using Client.Dtos.Orders;
using DataAccess;
using Domain;
using Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders;

public class OrderUpdater : IUpdateOrders
{
    private readonly IValidateOrders _orderValidator;
    private readonly IRepository<Order> _orderRepo;
    private readonly IRepository<Product> _productRepo;
    private readonly IRepository<Discount> _discountRepo;
    private readonly IUnitOfWork _unitOfWork;

    public OrderUpdater(IRepository<Order> orderRepo,
                        IUnitOfWork unitOfWork,
                        IValidateOrders orderValidator,
                        IRepository<Product> productRepo,
                        IRepository<Discount> discountRepo)
    {
        _orderRepo = orderRepo;
        _unitOfWork = unitOfWork;
        _orderValidator = orderValidator;
        _productRepo = productRepo;
        _discountRepo = discountRepo;
    }

    public OrderDto Update(OrderDto request)
    {
        var order = _orderRepo.Get(x => x.OrderId == request.OrderId)
                              .Include(i => i.LineItems)
                              .SingleOrDefault();

        if (order is null)
            throw new NotFoundException("Order not found");

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

        order.Update(request.FirstName, request.LastName, request.Address, 
                     discount, 
                     setLineItemInputs, 
                     _orderValidator, out var errors);

        if (errors.Any())
            throw new ValidationException("Order failed validation", errors);

        _unitOfWork.Save();

        return new OrderDto
        {
            OrderId = request.OrderId,
            Created = request.Created,
            LastModified = request.LastModified,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Address = request.Address,
            DiscountCode = request.DiscountCode,
            LineItems = order.LineItems.Select(x => new LineItemDto
            {
                ProductId = x.ProductId,
                Sku = x.Sku,
                Quantity = x.Quantity,
                UnitCost = x.UnitCost,
                TotalCost = x.TotalCost,
            }).ToList()
        };
    }
}