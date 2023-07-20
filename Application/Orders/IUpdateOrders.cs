using Client.Dtos.Orders;

namespace Application.Orders;

public interface IUpdateOrders
{
    OrderDto Create(OrderDto request);
}

public class OrderUpdater : IUpdateOrders
{
    public OrderDto Create(OrderDto request)
    {
        throw new NotImplementedException();
    }
}