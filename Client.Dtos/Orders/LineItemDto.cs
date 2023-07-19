namespace Client.Dtos.Orders;

public class LineItemDto
{
    public string Sku { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
}