namespace Domain;

public abstract class Discount
{
    protected Discount() {}

    protected Discount(int discountId,
                       string code,
                       DiscountType type,
                       decimal? percentage = null)
    {
        DiscountId = discountId;
        Code = code;
        DiscountType = type;
        Percentage = percentage;
    }

    public int DiscountId { get; private init; }
    
    public string Code { get; private init; } = default!;

    public DiscountType DiscountType { get; private init; }

    public decimal? Percentage { get; private init; }

    internal abstract void Calculate(Order order);
}

public class GeneralDiscount : Discount
{
    protected GeneralDiscount() { }

    public GeneralDiscount(int discountId,
                           string code,
                           decimal? percentage)
    : base(discountId, code, DiscountType.GeneralDiscount, percentage) {}

    internal override void Calculate(Order order)
    {
        foreach (var lineItem in order.LineItems)
            lineItem.UpdateTotal(lineItem.TotalCost * (1 - Percentage.GetValueOrDefault()));
    }
}

public class BuyOneGetOneFreeDiscount : Discount
{
    protected BuyOneGetOneFreeDiscount() { }

    public BuyOneGetOneFreeDiscount(int discountId,
                                    string code)
        : base(discountId, code, DiscountType.BuyOneGetOneFree) { }

    internal override void Calculate(Order order)
    {
        foreach (var lineItem in order.LineItems.Where(x => x.Quantity > 1))
        {
            if (lineItem.Quantity % 2 == 0)
                lineItem.UpdateTotal(lineItem.TotalCost * 0.5m);
            else if (lineItem.Quantity > 1)
                lineItem.UpdateTotal(lineItem.UnitCost + (lineItem.Quantity - 1) * lineItem.UnitCost / 2);
        }
    }
}

public enum DiscountType
{
    GeneralDiscount = 1,
    BuyOneGetOneFree = 2
}
