using Client.Dtos;

namespace Application
{
    public interface ICreateOrders
    {
        OrderDto Create(CreateOrderRequestDto createOrderRequestDto);
    }

    public class OrderCreator : ICreateOrders
    {
        public OrderDto Create(CreateOrderRequestDto createOrderRequestDto)
        {
            return new OrderDto
            {
                FirstName = createOrderRequestDto.FirstName,
                LastName = createOrderRequestDto.LastName,
                Address = createOrderRequestDto.Address,
            };
        }
    }
}