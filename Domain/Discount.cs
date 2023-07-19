namespace Domain;

public class Discount
{
    public int DiscountId { get; set; }
    
    public string Code { get; set; } = default!;

    public DiscountType DiscountType { get; set; }

    public decimal? Percentage { get; set; }
}

public enum DiscountType
{
    GeneralDiscount = 1,
    BuyOneGetOneFree = 2
}
