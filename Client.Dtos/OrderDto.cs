namespace Client.Dtos
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Address { get; set; } = default!;
        public ICollection<LineItemDto> LineItems { get; set; }
    }

    public class LineItemDto
    {
        public string Sku { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost { get; set; }
    }
}