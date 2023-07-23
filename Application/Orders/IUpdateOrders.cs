using Client.Dtos.Orders;

namespace Application.Orders;

public interface IUpdateOrders
{
    OrderDto Update(OrderDto request);
}