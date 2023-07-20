using Client.Dtos;
using Client.Dtos.Orders;

namespace Application.Orders;

public interface ICreateOrders
{
    OrderDto Create(CreateOrderRequestDto request);
}