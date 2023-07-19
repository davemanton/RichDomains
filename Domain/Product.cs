namespace Domain;

public class Product
{
    public int ProductId { get; set; }

    public string Sku { get; set; } = null!;

    public string Name { get; set; } = null!;

    public decimal UnitCost { get; set; }

    public virtual ICollection<LineItem> LineItems { get; set; } = new List<LineItem>();
}
