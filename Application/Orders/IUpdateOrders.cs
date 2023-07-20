using Client.Dtos.Orders;
using DataAccess;
using Domain;

namespace Application.Orders;

public interface IUpdateOrders
{
    OrderDto Create(OrderDto request);
}

public class OrderUpdater : IUpdateOrders
{
    private readonly IRepository<Order> _orderRepo;

    public OrderUpdater(IRepository<Order> orderRepo)
    {
        _orderRepo = orderRepo;
    }

    public OrderDto Create(OrderDto request)
    {
        var order = _orderRepo.Get(x => x.OrderId == request.OrderId).SingleOrDefault();

        order.OrderId = request.OrderId;
        order.FirstName = request.FirstName;
        order.LastName = request.LastName;
        order.Address = request.Address;

        return new OrderDto
        {
            OrderId = request.OrderId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Address = request.Address,
        };
    }
}