using ECommercePI.Domain.Enums;

namespace ECommercePI.Domain.Entities;

public class Order
{
    public string? OrderId { get; set; }
    public List<OrderItem> Items { get; set; }
    public decimal Amount { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}