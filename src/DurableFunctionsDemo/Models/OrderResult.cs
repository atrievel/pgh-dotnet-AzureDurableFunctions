using System;

namespace DurableFunctionsDemo.Models
{
    public class OrderResult
    {
        public Guid OrderId { get; set; }
        public float OrderTotal { get; set; } = 0f;
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Canceled;
    }
}
