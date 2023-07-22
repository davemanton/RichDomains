using Domain.Services;
using System.Net;

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
        throw new NotImplementedException();

        // ensure discount is calculated after line items are set
        // Don't forget last modified
    }

    private void SetCustomerDetails(string firstName,
                                       string lastName,
                                       string address)
    {
        FirstName = firstName;
        LastName = lastName;
        Address = address;
    }

    private void SetLineItems(ICollection<SetLineItemInput> lineItems)
    {
        // TODO: ensure isExpired is set on lineItems that aren't required any more when doing update
        _lineItems = lineItems.Select(input => new LineItem(this, input)).ToHashSet();
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
