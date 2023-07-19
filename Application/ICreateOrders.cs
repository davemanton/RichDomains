using Client.Dtos;
using DataAccess;
using Domain;

namespace Application
{
    public interface ICreateOrders
    {
        OrderDto Create(CreateOrderRequestDto request);
    }

    public class OrderCreator : ICreateOrders
    {
        private readonly IRepository<Order> _orderRepo;
        private readonly IRepository<Product> _productRepo;
        private readonly IUnitOfWork _unitOfWork;

        public OrderCreator(IRepository<Order> orderRepo,
                            IRepository<Product> productRepo,
                            IUnitOfWork unitOfWork)
        {
            _orderRepo = orderRepo;
            _productRepo = productRepo;

            _unitOfWork = unitOfWork;
        }

        public OrderDto Create(CreateOrderRequestDto request)
        {
            var skus = request.LineItems.Select(x => x.Sku).ToList();

            var products = _productRepo.Get(x => skus.Contains(x.Sku));

            var order = new Order
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Address = request.Address,
            };

            order.LineItems = new List<LineItem>();

            foreach (var requestedItem in request.LineItems)
            {
                var product = products.Single(x => x.Sku == requestedItem.Sku);

                order.LineItems.Add(new LineItem
                {
                    ProductId = product.ProductId,
                    Sku = product.Sku,

                    Quantity = requestedItem.Quantity,
                    UnitCost = product.UnitCost,
                    TotalCost = requestedItem.Quantity * product.UnitCost,

                    IsExpired = false,
                });
            }

            _orderRepo.Insert(order);
            _unitOfWork.Save();

            return new OrderDto
            {
                OrderId = order.OrderId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Address = request.Address,
            };
        }
    }
}