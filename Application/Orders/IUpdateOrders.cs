using Client.Dtos.Orders;
using DataAccess;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders;

public interface IUpdateOrders
{
    OrderDto Update(OrderDto request);
}

public class OrderUpdater : IUpdateOrders
{
    private readonly IRepository<Order> _orderRepo;
    private readonly IUnitOfWork _unitOfWork;

    public OrderUpdater(IRepository<Order> orderRepo,
                        IUnitOfWork unitOfWork)
    {
        _orderRepo = orderRepo;
        _unitOfWork = unitOfWork;
    }

    public OrderDto Update(OrderDto request)
    {
        var order = _orderRepo.Get(x => x.OrderId == request.OrderId)
                              .Include(i => i.LineItems)
                              .SingleOrDefault();

        order.OrderId = request.OrderId;
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

        return new OrderDto
        {
            OrderId = request.OrderId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Address = request.Address,
        };
    }
}