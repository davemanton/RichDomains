namespace Domain;

public class LineItem
{
    private LineItem() {}

    internal LineItem(Order order, SetLineItemInput input)
    {
        Order = order;
        OrderId = order.OrderId;

        Product = input.Product;
        ProductId = input.Product.ProductId;

        Created = DateTime.UtcNow;
        LastModified = DateTime.UtcNow;

        Quantity = input.Quantity;

        Sku = Product.Sku;
        UnitCost = Product.UnitCost;
        TotalCost = UnitCost * Quantity;
    }

    public int OrderId { get; private init; }
    public int ProductId { get; private init; }

    public DateTime Created { get; private init; }
    public DateTime LastModified { get; private set; }

    public string Sku { get; private init; } = default!;

    public bool IsExpired { get; private set; }

    public int Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal TotalCost { get; private set; }

    public Order Order { get; private init; } = default!;
    public Product Product { get; private init; } = default!;

    internal void Update(int quantity)
    {
        if (Quantity == quantity)
            return;

        Quantity = quantity;
        TotalCost = UnitCost * Quantity;

        UpdateLastModified();
    }

    internal void UpdateTotal(decimal totalCost)
    {
        TotalCost = totalCost;

        UpdateLastModified();
    }

    internal void Expire()
    {
        IsExpired = true;

        UpdateLastModified();
    }

    private void UpdateLastModified() => LastModified = DateTime.UtcNow;
}
