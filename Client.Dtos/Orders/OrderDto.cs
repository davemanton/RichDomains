namespace Client.Dtos.Orders
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Address { get; set; } = default!;
        public string DiscountCode { get; set; }
        public ICollection<LineItemDto> LineItems { get; set; }
    }
}