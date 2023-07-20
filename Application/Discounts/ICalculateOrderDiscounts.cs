using Domain;

namespace Application.Discounts;

public interface ICalculateOrderDiscounts
{
    void ApplyDiscounts(string discountCode,
                        Order order);
}