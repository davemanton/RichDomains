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

    public OrderUpdater(IRepository<Order> orderRepo)
    {
        _orderRepo = orderRepo;
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

        return new OrderDto
        {
            OrderId = request.OrderId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Address = request.Address,
        };
    }
}