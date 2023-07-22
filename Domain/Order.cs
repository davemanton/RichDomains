using Domain.Services;

namespace Domain;

public class Order
{
    private HashSet<LineItem> _lineItems;

    private Order()
    {
        _lineItems = new HashSet<LineItem>();
    }

    private Order(string firstName, 
                  string lastName, 
                  string address, 
                  Discount? discount,
                  ICollection<SetLineItemInput> lineItems)
        : this()
    {
        Created = DateTime.UtcNow;
        LastModified = DateTime.UtcNow;

        SetCustomerDetails(firstName, lastName, address);
        SetLineItems(lineItems);
        SetDiscount(discount);
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
    
    public static Order? Create(string firstName,
                                string lastName,
                                string address,
                                Discount? discount,
                                ICollection<SetLineItemInput> lineItems,
                                IValidateOrders validator,
                                out IDictionary<string, string> errors)
    {
        if (!validator.Validate(firstName, lastName, address, lineItems, out errors))
            return null;

        return new Order(firstName, lastName, address, discount, lineItems);
    }

    public void Update(string firstName,
                       string lastName,
                       string address,
                       Discount? discount,
                       ICollection<SetLineItemInput> lineItems,
                       IValidateOrders validator,
                       out IDictionary<string, string> errors)
    {
        if (!validator.Validate(firstName, lastName, address, lineItems, out errors))
            return;

        SetCustomerDetails(firstName, lastName, address);
        SetLineItems(lineItems);
        SetDiscount(discount);
        
        LastModified = DateTime.UtcNow;
    }

    private void SetCustomerDetails(string firstName,
                                       string lastName,
                                       string address)
    {
        FirstName = firstName;
        LastName = lastName;
        Address = address;
    }

    private void SetLineItems(ICollection<SetLineItemInput> lineItemInputs)
    {
        var lineItemsToExpire = _lineItems.ExceptBy(lineItemInputs.Select(x => x.Product.Sku), x => x.Sku);

        foreach (var lineItem in lineItemsToExpire)
            lineItem.Expire();

        foreach (var lineItemInput in lineItemInputs)
        {
            var lineItem = _lineItems.SingleOrDefault(x => x.Sku == lineItemInput.Product.Sku);

            if (lineItem is null)
                _lineItems.Add(new LineItem(this, lineItemInput));
            else
                lineItem.Update(lineItemInput.Quantity);
        }
    }

    private void SetDiscount(Discount? discount)
    {
        if (discount == null)
            return;

        DiscountId = discount?.DiscountId;
        Discount = discount;
        Discount?.Calculate(this);
    }
}

public class SetLineItemInput
{
    public Product Product { get; set; } = default!;
    public int Quantity { get; set; }
}
