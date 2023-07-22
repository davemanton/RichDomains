using Client.Dtos.Orders;

namespace Application.Orders;

public interface IReadOrders
{
    OrderDto Read(int orderId);
}