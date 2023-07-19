namespace Client.Dtos;

public class CreateOrderRequestDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Address { get; set; }
    public ICollection<LineItemRequestDto> LineItems { get; set; }
}