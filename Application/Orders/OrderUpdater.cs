using Application.Discounts;
using Application.Exceptions;
using Client.Dtos.Orders;
using DataAccess;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders;

public class OrderUpdater : IUpdateOrders
{
    private readonly IRepository<Order> _orderRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICalculateOrderDiscounts _discountCalculator;

    public OrderUpdater(IRepository<Order> orderRepo,
                        IUnitOfWork unitOfWork,
                        ICalculateOrderDiscounts discountCalculator)
    {
        _orderRepo = orderRepo;
        _unitOfWork = unitOfWork;
        _discountCalculator = discountCalculator;
    }

    public OrderDto Update(OrderDto request)
    {
        var errors = Validate(request);
        if (errors.Any())
            throw new ValidationException("Validation Failed", errors);
            
        var order = _orderRepo.Get(x => x.OrderId == request.OrderId)
                              .Include(i => i.LineItems)
                              .SingleOrDefault();

        if (order is null)
            throw new NotFoundException("Order not found");

        order.OrderId = request.OrderId;
        order.Created = request.Created;
        order.LastModified = DateTime.Now;
        
        order.FirstName = request.FirstName;
        order.LastName = request.LastName;
        order.Address = request.Address;

        order.LineItems.Clear();

        foreach (var requestedItem in request.LineItems)
        {
            order.LineItems.Add(new LineItem
            {
                ProductId = requestedItem.ProductId,
                Sku = requestedItem.Sku,
                UnitCost = requestedItem.UnitCost,
                Quantity = requestedItem.Quantity,
                TotalCost = requestedItem.TotalCost
            });
        }

        _unitOfWork.Save();

        if(!string.IsNullOrEmpty(request.DiscountCode))
            _discountCalculator.ApplyDiscounts(request.DiscountCode, order);

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

    public IDictionary<string, string> Validate(OrderDto order)
    {
        var errors = new Dictionary<string, string>();

        if(string.IsNullOrEmpty(order.FirstName))
            errors.Add("FIRST NAME", "You must have a first name");

        if (string.IsNullOrEmpty(order.LastName))
            errors.Add("LAST NAME", "You must have a last name");

        if (string.IsNullOrEmpty(order.Address))
            errors.Add("ADDRESS", "You must have an address");

        if(!order.LineItems.Any())
            errors.Add("LINE ITEMS", "There are no items in your order");

        return errors;
    }
}