namespace Client.Dtos;

public class CreateOrderRequestDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Address { get; set; }
    public ICollection<LineItemDto> LineItems { get; set; }
}

public class LineItemDto
{
    public string Sku { get; set; }
    public int Quantity { get; set; }
}