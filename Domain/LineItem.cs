namespace Domain;

public class LineItem
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }

    public DateTime Created { get; set; }
    public DateTime LastModified { get; set; }

    public string Sku { get; set; } = default!;

    public bool IsExpired { get; set; }

    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }

    public Order Order { get; set; } = default!;
    public Product Product { get; set; } = default!;
    
}
