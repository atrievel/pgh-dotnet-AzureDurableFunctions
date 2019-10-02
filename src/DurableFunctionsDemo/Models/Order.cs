using System;
using System.Collections.Generic;

namespace DurableFunctionsDemo.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public Customer Customer { get; set; }
        public List<Product> PurchasedItems { get; set; }
        public float Total { get; set; } = 0f;
        public OrderStatus OrderStatus { get; set; } = OrderStatus.New;
        public string OrchestrationId { get; set; }
    }

    public enum OrderStatus
    {
        New,
        InProgress,
        Charged,
        Shipped,
        Canceled
    }
}
