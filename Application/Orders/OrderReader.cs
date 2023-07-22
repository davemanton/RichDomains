using Application.Exceptions;
using Client.Dtos.Orders;
using DataAccess;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders;

public class OrderReader : IReadOrders
{
    private readonly IRepository<Order> _orderRepo;
    public OrderReader(IRepository<Order> orderRepo)
    {
        _orderRepo = orderRepo;
    }

    public OrderDto Read(int orderId)
    {
        var order = _orderRepo.Get(x => x.OrderId == orderId)
                              .Include(i => i.LineItems)
                              .Include(i => i.Discount)
                              .SingleOrDefault();

        if (order == null)
            throw new NotFoundException("Not found");

        return new OrderDto
        {
            OrderId = order.OrderId,
            Created = order.Created,
            LastModified = order.LastModified,
            FirstName = order.FirstName,
            LastName = order.LastName,
            Address = order.Address,
            DiscountCode = order.Discount?.Code,
            LineItems = order.LineItems.Select(x => new LineItemDto
            {
                ProductId = x.ProductId,
                Sku = x.Sku,

                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,

                Quantity = x.Quantity,
                UnitCost = x.UnitCost,
                TotalCost = x.TotalCost,
            }).ToList()
        };
    }
}