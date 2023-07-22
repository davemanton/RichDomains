using Application.Orders;
using Client.Dtos;
using Client.Dtos.Orders;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IReadOrders _orderReader;
        private readonly ICreateOrders _orderCreator;
        private readonly IUpdateOrders _orderUpdater;

        public OrdersController(IReadOrders orderReader,
                                ICreateOrders orderCreator,
                                IUpdateOrders orderUpdater)
        {
            _orderCreator = orderCreator;
            _orderUpdater = orderUpdater;
            _orderReader = orderReader;
        }

        [HttpGet("{orderId}")]
        public IActionResult Index(int orderId)
        {
            var response = _orderReader.Read(orderId);

            return Ok(response);
        }

        [HttpPost]
        public IActionResult Create(CreateOrderRequestDto request)
        {
            var response = _orderCreator.Create(request);

            return Ok(response);
        }

        [HttpPost("[action]")]
        public IActionResult Update(OrderDto request)
        {
            var response = _orderUpdater.Update(request);

            return Ok(response);
        }
    }
}