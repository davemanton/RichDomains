using Application.Exceptions;
using DataAccess;
using Domain;

namespace Application.Discounts;

internal class DiscountCalculator : ICalculateOrderDiscounts
{
    private readonly IRepository<Discount> _discountRepo;

    public DiscountCalculator(IRepository<Discount> discountRepo)
    {
        _discountRepo = discountRepo;
    }

    public void ApplyDiscounts(string? discountCode,
                               Order order)
    {
        var discount = _discountRepo.Get(x => x.Code == discountCode).SingleOrDefault();

        if (discount is null)
            throw new ValidationException("Request failed validation",
                                          new Dictionary<string, string>
                                          {
                                              { nameof(discountCode), "Discount code not found" }
                                          });

        order.DiscountId = discount.DiscountId;

        switch (discount.DiscountType)
        {
            case DiscountType.GeneralDiscount:
                ApplyGeneralDiscount(discount, order);
                break;
            case DiscountType.BuyOneGetOneFree:
                ApplyBogofDiscount(discount, order);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ApplyGeneralDiscount(Discount discount,
                                      Order order)
    {
        foreach (var lineItem in order.LineItems)
        {
            lineItem.TotalCost *= 1 - discount.Percentage.GetValueOrDefault();
        }
    }

    private void ApplyBogofDiscount(Discount discount,
                                    Order order)
    {
        foreach (var lineItem in order.LineItems.Where(x => x.Quantity > 1))
        {
            if (lineItem.Quantity % 2 == 0)
                lineItem.TotalCost *= 0.5m;
            else if (lineItem.Quantity > 1)
                lineItem.TotalCost = lineItem.UnitCost + (lineItem.Quantity - 1) * lineItem.UnitCost / 2;
        }
    }
}