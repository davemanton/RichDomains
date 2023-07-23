namespace Client.Dtos.Orders;

public class LineItemDto
{

    public int ProductId { get; set; }
    public string Sku { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastModified { get; set; }
    
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    
}