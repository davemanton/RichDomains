namespace Domain;

public class Order
{
    public int OrderId { get; set; }

    public string FirstName { get; set; } = default!;

    public string LastName { get; set; } = default!;

    public string Address { get; set; } = default!;

    public int? DiscountId { get; set; }

    public Discount? Discount { get; set; }

    public ICollection<LineItem> LineItems { get; set; } = default!;

}
