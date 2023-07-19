using Client.Dtos;
using DataAccess;
using Domain;

namespace Application
{
    public interface ICreateOrders
    {
        OrderDto Create(CreateOrderRequestDto createOrderRequestDto);
    }

    public class OrderCreator : ICreateOrders
    {
        private readonly IRepository<Order> _orderRepo;
        private readonly IUnitOfWork _unitOfWork;

        public OrderCreator(IRepository<Order> orderRepo,
                            IUnitOfWork unitOfWork)
        {
            _orderRepo = orderRepo;
            _unitOfWork = unitOfWork;
        }

        public OrderDto Create(CreateOrderRequestDto createOrderRequestDto)
        {
            var order = new Order
            {
                FirstName = createOrderRequestDto.FirstName,
                LastName = createOrderRequestDto.LastName,
                Address = createOrderRequestDto.Address,
            };

            _orderRepo.Insert(order);
            _unitOfWork.Save();

            return new OrderDto
            {
                FirstName = createOrderRequestDto.FirstName,
                LastName = createOrderRequestDto.LastName,
                Address = createOrderRequestDto.Address,
            };
        }
    }
}