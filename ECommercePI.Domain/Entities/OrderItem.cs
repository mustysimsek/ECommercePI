namespace ECommercePI.Domain.Entities;

public class OrderItem
{
    public string OrderItemId { get; set; }
    public string OrderId { get; set; }
    public Order Order { get; set; }
    public string? ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}