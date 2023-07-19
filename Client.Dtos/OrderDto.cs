﻿namespace Client.Dtos
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Address { get; set; } = default!;
    }
}