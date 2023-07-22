namespace Domain;

public class Order
{
    private HashSet<LineItem> _lineItems;

    private Order()
    {
        _lineItems = new HashSet<LineItem>();
    }

    public Order(string firstName, 
                 string lastName, 
                 string address, 
                 Discount? discount,
                 ICollection<SetLineItemInput> lineItems)
        : this()
    {
        Created = DateTime.UtcNow;
        LastModified = DateTime.UtcNow;

        FirstName = firstName;
        LastName = lastName;
        Address = address;

        _lineItems = lineItems.Select(input => new LineItem(this, input)).ToHashSet();

        DiscountId = discount?.DiscountId;
        Discount = discount;
        Discount?.Calculate(this);
    }

    public int OrderId { get; private init; }

    public DateTime Created { get; private init; }
    public DateTime LastModified { get; private set; }

    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string Address { get; private set; } = default!;

    public int? DiscountId { get; private set; }

    public Discount? Discount { get; private set; }

    public IReadOnlyCollection<LineItem> LineItems => _lineItems;
}

public class SetLineItemInput
{
    public Product Product { get; set; } = default!;
    public int Quantity { get; set; }
}
